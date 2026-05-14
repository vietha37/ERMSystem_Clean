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

  $doctors = Invoke-RestMethod -Method Get -Uri "$baseUrl/api/hospital-doctors"
  $doctor = @($doctors | Where-Object { $_.isBookable -and $_.schedules.Count -gt 0 }) | Select-Object -First 1
  if ($null -eq $doctor) {
    throw "Khong tim thay bac si co lich bookable de test encounters worklist."
  }

  $slots = Get-CandidateScheduleSlots -Doctor $doctor
  $suffix = [DateTime]::Now.ToString("yyyyMMddHHmmss")
  $phone = ("095" + $suffix.Substring($suffix.Length - 7))
  $email = "encworklist.$suffix@erm.local"
  $booking = $null

  foreach ($slot in $slots) {
    $bookingBody = [ordered]@{
      fullName        = "Benh nhan Encounter Worklist $suffix"
      phone           = $phone
      email           = $email
      dateOfBirth     = "1994-03-17"
      gender          = "Nam"
      doctorProfileId = $doctor.doctorProfileId
      specialtyId     = $doctor.specialtyId
      preferredDate   = $slot.PreferredDate
      preferredTime   = $slot.PreferredTime
      chiefComplaint  = "Ho, sot nhe"
      notes           = "Smoke test encounters worklist"
    } | ConvertTo-Json

    try {
      $booking = Invoke-RestMethod `
        -Method Post `
        -Uri "$baseUrl/api/hospital-appointments/public-booking" `
        -ContentType "application/json" `
        -Body $bookingBody
      break
    }
    catch {
      continue
    }
  }

  if ($null -eq $booking) {
    throw "Khong dat duoc lich hen sau khi thu cac slot hop le."
  }

  $adminUsername = "enc_worklist_admin_$suffix"
  $password = "123456"

  Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/auth/register" `
    -ContentType "application/json" `
    -Body (@{ username = $adminUsername; password = $password; role = "Admin" } | ConvertTo-Json) | Out-Null

  $login = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/auth/login" `
    -ContentType "application/json" `
    -Body (@{ username = $adminUsername; password = $password } | ConvertTo-Json)

  $token = if ($login.PSObject.Properties.Name -contains "accessToken") { $login.accessToken } else { $login.token }
  $headers = @{ Authorization = "Bearer $token" }

  $checkedIn = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/hospital-appointments/$($booking.appointmentId)/check-in" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body (@{ counterLabel = "Counter E" } | ConvertTo-Json)

  if ($checkedIn.status -ne "CheckedIn") {
    throw "Check-in cho encounters worklist khong thanh cong."
  }

  $eligible = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-encounters/eligible-appointments" `
    -Headers $headers

  $eligibleItem = @($eligible | Where-Object { $_.appointmentId -eq $booking.appointmentId }) | Select-Object -First 1
  if ($null -eq $eligibleItem) {
    throw "Appointment da check-in khong xuat hien trong eligible appointments."
  }

  $encounter = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/hospital-encounters" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body ([ordered]@{
      appointmentId = $booking.appointmentId
      diagnosisName = "Theo doi viem ho hap tren"
      diagnosisCode = "J06.9"
      diagnosisType = "Working"
      encounterStatus = "InProgress"
      summary = "Encounter tao de verify encounter worklist"
      subjective = "Ho, sot nhe"
      objective = "Mach va nhiet do on dinh"
      assessment = "Can theo doi va danh gia them"
      carePlan = "Cho dieu tri ngoai tru"
    } | ConvertTo-Json)

  if ($encounter.encounterStatus -ne "InProgress") {
    throw "Tao encounter InProgress khong thanh cong."
  }

  $appointmentDate = ([DateTime]$booking.appointmentStartLocal).ToString("yyyy-MM-dd")
  $inProgressList = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-encounters?pageNumber=1&pageSize=20&encounterStatus=InProgress&appointmentDate=$appointmentDate&textSearch=$($encounter.encounterNumber)" `
    -Headers $headers

  $inProgressItem = @($inProgressList.items | Where-Object { $_.encounterId -eq $encounter.encounterId }) | Select-Object -First 1
  if ($null -eq $inProgressItem) {
    throw "Encounter moi tao khong xuat hien trong worklist InProgress."
  }

  if ($inProgressItem.primaryDiagnosisName -ne "Theo doi viem ho hap tren") {
    throw "Primary diagnosis trong worklist InProgress khong dung: $($inProgressItem.primaryDiagnosisName)"
  }

  $updated = Invoke-RestMethod `
    -Method Put `
    -Uri "$baseUrl/api/hospital-encounters/$($encounter.encounterId)" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body ([ordered]@{
      diagnosisName = "Theo doi viem ho hap tren"
      diagnosisCode = "J06.9"
      diagnosisType = "Working"
      encounterStatus = "Finalized"
      summary = "Encounter da finalize de smoke test"
      subjective = "Trieu chung giam"
      objective = "Sinh ton on dinh"
      assessment = "Cho ve theo doi"
      carePlan = "Tai kham neu can"
    } | ConvertTo-Json)

  if ($updated.encounterStatus -ne "Finalized") {
    throw "Finalize encounter khong thanh cong."
  }

  $finalizedList = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-encounters?pageNumber=1&pageSize=20&encounterStatus=Finalized&appointmentDate=$appointmentDate&textSearch=Finalize" `
    -Headers $headers

  $finalizedItem = @($finalizedList.items | Where-Object { $_.encounterId -eq $encounter.encounterId }) | Select-Object -First 1
  if ($null -eq $finalizedItem) {
    throw "Encounter da finalize khong xuat hien trong worklist Finalized."
  }

  if ([string]::IsNullOrWhiteSpace($finalizedItem.summary) -or $finalizedItem.summary -notlike "*finalize*") {
    throw "Summary trong worklist Finalized khong dung."
  }

  Write-Output ("appointment_number=" + $booking.appointmentNumber)
  Write-Output ("encounter_number=" + $encounter.encounterNumber)
  Write-Output ("eligible_found=" + $true)
  Write-Output ("inprogress_total=" + $inProgressList.totalCount)
  Write-Output ("finalized_total=" + $finalizedList.totalCount)
}
finally {
  if ($process -and -not $process.HasExited) {
    Stop-Process -Id $process.Id -Force
    $process.WaitForExit()
  }
}
