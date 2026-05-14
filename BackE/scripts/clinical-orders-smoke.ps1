$ErrorActionPreference = "Stop"

$env:USERPROFILE = "D:\ERMSystem\.localuser"
$env:APPDATA = "D:\ERMSystem\.localuser\AppData\Roaming"
$env:DOTNET_CLI_HOME = "D:\ERMSystem\.dotnet-home"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

$apiProj = "D:\ERMSystem\BackE\ERMSystem.API\ERMSystem.API.csproj"
$baseUrl = "http://localhost:5219"

function Get-CandidateScheduleSlots {
  param(
    [Parameter(Mandatory = $true)]
    $Doctor
  )

  $today = (Get-Date).Date
  $minDate = $today.AddDays(14)
  $candidates = New-Object System.Collections.Generic.List[object]

  foreach ($schedule in @($Doctor.schedules | Sort-Object dayOfWeek, startTime)) {
    for ($offset = 0; $offset -lt 120; $offset++) {
      $candidate = $minDate.AddDays($offset)
      if ([int]$candidate.DayOfWeek -ne [int]$schedule.dayOfWeek) {
        continue
      }

      $validFrom = [DateTime]::Parse($schedule.validFrom.ToString()).Date
      if ($candidate -lt $validFrom) {
        continue
      }

      if ($null -ne $schedule.validTo) {
        $validTo = [DateTime]::Parse($schedule.validTo.ToString()).Date
        if ($candidate -gt $validTo) {
          continue
        }
      }

      $startTime = [TimeSpan]::Parse($schedule.startTime.ToString())
      $endTime = [TimeSpan]::Parse($schedule.endTime.ToString())
      $slotMinutes = [int]$schedule.slotMinutes
      $availableSlots = [Math]::Max(1, [int][Math]::Floor((($endTime - $startTime).TotalMinutes) / $slotMinutes))

      for ($slotIndex = 0; $slotIndex -lt $availableSlots; $slotIndex++) {
        $preferredTime = $startTime.Add([TimeSpan]::FromMinutes($slotIndex * $slotMinutes))
        $candidates.Add([pscustomobject]@{
          PreferredDate = $candidate.ToString("yyyy-MM-dd")
          PreferredTime = ([DateTime]::Today + $preferredTime).ToString("HH:mm:ss")
        })

        if ($candidates.Count -ge 80) {
          return $candidates
        }
      }
    }
  }

  if ($candidates.Count -eq 0) {
    throw "Khong tim thay lich lam viec hop le de dat lich."
  }

  return $candidates
}

$startInfo = New-Object System.Diagnostics.ProcessStartInfo
$startInfo.FileName = "dotnet"
$startInfo.Arguments = "run --no-build --project `"$apiProj`" --urls `"$baseUrl`""
$startInfo.UseShellExecute = $false
$startInfo.WorkingDirectory = "D:\ERMSystem\BackE"

$process = New-Object System.Diagnostics.Process
$process.StartInfo = $startInfo
$process.Start() | Out-Null

try {
  Start-Sleep -Seconds 8

  $suffix = [DateTime]::Now.ToString("yyyyMMddHHmmss")
  $adminUser = "clinical_admin_$suffix"
  $password = "123456"

  Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/auth/register" `
    -ContentType "application/json" `
    -Body (@{ username = $adminUser; password = $password; role = "Admin" } | ConvertTo-Json) | Out-Null

  $login = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/auth/login" `
    -ContentType "application/json" `
    -Body (@{ username = $adminUser; password = $password } | ConvertTo-Json)

  $headers = @{ Authorization = "Bearer $($login.accessToken)" }

  $doctors = Invoke-RestMethod -Method Get -Uri "$baseUrl/api/hospital-doctors"
  $doctor = @($doctors | Where-Object { $_.isBookable -and $_.schedules.Count -gt 0 }) | Select-Object -First 1
  if ($null -eq $doctor) {
    throw "Khong tim thay bac si co lich bookable de test clinical orders."
  }

  $slots = Get-CandidateScheduleSlots -Doctor $doctor
  $booking = $null
  foreach ($slot in $slots) {
    try {
      $booking = Invoke-RestMethod `
        -Method Post `
        -Uri "$baseUrl/api/hospital-appointments/public-booking" `
        -ContentType "application/json" `
        -Body ([ordered]@{
          fullName = "Benh nhan Clinical $suffix"
          phone = ("092" + $suffix.Substring($suffix.Length - 7))
          email = "clinical.$suffix@erm.local"
          dateOfBirth = "1990-10-10"
          gender = "Nam"
          doctorProfileId = $doctor.doctorProfileId
          specialtyId = $doctor.specialtyId
          preferredDate = $slot.PreferredDate
          preferredTime = $slot.PreferredTime
          chiefComplaint = "Sot va met moi"
          notes = "Smoke test clinical orders"
        } | ConvertTo-Json)
      break
    }
    catch {
      continue
    }
  }

  if ($null -eq $booking) {
    throw "Khong dat duoc lich hen de test clinical orders."
  }

  $checkedIn = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/hospital-appointments/$($booking.appointmentId)/check-in" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body (@{ counterLabel = "Counter B" } | ConvertTo-Json)

  if ($checkedIn.status -ne "CheckedIn") {
    throw "Check-in cho clinical orders khong thanh cong."
  }

  $encounter = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/hospital-encounters" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body ([ordered]@{
      appointmentId = $booking.appointmentId
      diagnosisName = "Theo doi nhiem trung ho hap"
      diagnosisCode = "J06.9"
      diagnosisType = "Working"
      encounterStatus = "InProgress"
      summary = "Can lam sang de danh gia them"
      subjective = "Sot nhe, met moi"
      objective = "Sinh ton on dinh"
      assessment = "Can CBC va imaging"
      carePlan = "Chi dinh can lam sang"
    } | ConvertTo-Json)

  $catalog = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-clinical-orders/catalog" `
    -Headers $headers

  $labService = @($catalog | Where-Object { $_.category -eq "Lab" }) | Select-Object -First 1
  $imagingService = @($catalog | Where-Object { $_.category -eq "Imaging" }) | Select-Object -First 1

  if ($null -eq $labService -or $null -eq $imagingService) {
    throw "Khong tim thay du catalog Lab/Imaging de test."
  }

  $labOrder = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/hospital-clinical-orders" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body (@{
      encounterId = $encounter.encounterId
      category = "Lab"
      serviceId = $labService.serviceId
      priorityCode = "STAT"
    } | ConvertTo-Json)

  $labCompleted = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/hospital-clinical-orders/$($labOrder.clinicalOrderId)/lab-result" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body (@{
      specimenCode = "SPC-$suffix"
      resultItems = @(
        @{
          analyteCode = "WBC"
          analyteName = "Bach cau"
          resultValue = "9.8"
          unit = "G/L"
          referenceRange = "4.0-10.0"
          abnormalFlag = "Normal"
        }
      )
    } | ConvertTo-Json -Depth 5)

  if ($labCompleted.status -ne "Completed") {
    throw "Lab order khong chuyen sang Completed."
  }

  $imagingOrder = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/hospital-clinical-orders" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body (@{
      encounterId = $encounter.encounterId
      category = "Imaging"
      serviceId = $imagingService.serviceId
      priorityCode = "Routine"
    } | ConvertTo-Json)

  $imagingCompleted = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/hospital-clinical-orders/$($imagingOrder.clinicalOrderId)/imaging-report" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body (@{
      findings = "Khong thay ton thuong cap tinh"
      impression = "Khong ghi nhan bat thuong ro"
      reportUri = "https://example.local/report/$suffix"
    } | ConvertTo-Json)

  if ($imagingCompleted.status -ne "Completed") {
    throw "Imaging order khong chuyen sang Completed."
  }

  Write-Output ("encounter_number=" + $encounter.encounterNumber)
  Write-Output ("lab_order_number=" + $labCompleted.orderNumber)
  Write-Output ("lab_result_items=" + $labCompleted.resultItems.Count)
  Write-Output ("imaging_order_number=" + $imagingCompleted.orderNumber)
  Write-Output ("imaging_impression=" + $imagingCompleted.impression)
}
finally {
  if ($process -and -not $process.HasExited) {
    Stop-Process -Id $process.Id -Force
  }
}
