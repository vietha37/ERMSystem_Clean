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
    throw "Khong tim thay bac si co lich bookable de test appointments worklist."
  }

  $slots = Get-CandidateScheduleSlots -Doctor $doctor
  $suffix = [DateTime]::Now.ToString("yyyyMMddHHmmss")
  $phone = ("094" + $suffix.Substring($suffix.Length - 7))
  $email = "apptworklist.$suffix@erm.local"
  $booking = $null

  foreach ($slot in $slots) {
    $bookingBody = [ordered]@{
      fullName        = "Benh nhan Appt Worklist $suffix"
      phone           = $phone
      email           = $email
      dateOfBirth     = "1993-07-21"
      gender          = "Nam"
      doctorProfileId = $doctor.doctorProfileId
      specialtyId     = $doctor.specialtyId
      preferredDate   = $slot.PreferredDate
      preferredTime   = $slot.PreferredTime
      chiefComplaint  = "Dau bung nhe"
      notes           = "Smoke test appointments worklist"
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

  $adminUsername = "appt_worklist_admin_$suffix"
  $password = "123456"

  $registerBody = @{
    username = $adminUsername
    password = $password
    role = "Admin"
  } | ConvertTo-Json

  Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/auth/register" `
    -ContentType "application/json" `
    -Body $registerBody | Out-Null

  $loginBody = @{
    username = $adminUsername
    password = $password
  } | ConvertTo-Json

  $login = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/auth/login" `
    -ContentType "application/json" `
    -Body $loginBody

  $token = if ($login.PSObject.Properties.Name -contains "accessToken") { $login.accessToken } else { $login.token }
  $headers = @{
    Authorization = "Bearer $token"
  }

  $appointmentDate = ([DateTime]$booking.appointmentStartLocal).ToString("yyyy-MM-dd")
  $patientKeyword = "Appt Worklist $suffix"

  $scheduledList = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-appointments?pageNumber=1&pageSize=20&status=Scheduled&appointmentDate=$appointmentDate&textSearch=$patientKeyword" `
    -Headers $headers

  $scheduledItem = @($scheduledList.items | Where-Object { $_.appointmentId -eq $booking.appointmentId }) | Select-Object -First 1
  if ($null -eq $scheduledItem) {
    throw "Appointment moi tao khong xuat hien trong worklist Scheduled."
  }

  if ($scheduledItem.status -ne "Scheduled") {
    throw "Status trong worklist Scheduled khong dung: $($scheduledItem.status)"
  }

  if ($scheduledItem.patientPhone -ne $phone) {
    throw "Patient phone trong worklist khong khop booking."
  }

  $checkIn = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/hospital-appointments/$($booking.appointmentId)/check-in" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body (@{ counterLabel = "Counter D" } | ConvertTo-Json)

  if ($checkIn.status -ne "CheckedIn") {
    throw "Check-in cho appointments worklist khong thanh cong."
  }

  $checkedInList = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-appointments?pageNumber=1&pageSize=20&status=CheckedIn&appointmentDate=$appointmentDate&textSearch=$patientKeyword" `
    -Headers $headers

  $checkedInItem = @($checkedInList.items | Where-Object { $_.appointmentId -eq $booking.appointmentId }) | Select-Object -First 1
  if ($null -eq $checkedInItem) {
    throw "Appointment sau check-in khong xuat hien trong worklist CheckedIn."
  }

  if ($checkedInItem.counterLabel -ne "Counter D") {
    throw "Counter label trong worklist sau check-in khong dung: $($checkedInItem.counterLabel)"
  }

  if ([string]::IsNullOrWhiteSpace($checkedInItem.queueNumber)) {
    throw "Queue number trong worklist sau check-in dang rong."
  }

  $singleSearch = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-appointments?pageNumber=1&pageSize=5&textSearch=$($booking.appointmentNumber)" `
    -Headers $headers

  $searchItem = @($singleSearch.items | Where-Object { $_.appointmentId -eq $booking.appointmentId }) | Select-Object -First 1
  if ($null -eq $searchItem) {
    throw "Tim theo appointment number khong ra lich hen vua tao."
  }

  Write-Output ("appointment_number=" + $booking.appointmentNumber)
  Write-Output ("appointment_date=" + $appointmentDate)
  Write-Output ("scheduled_total=" + $scheduledList.totalCount)
  Write-Output ("checked_in_queue=" + $checkedInItem.queueNumber)
  Write-Output ("doctor_name=" + $checkedInItem.doctorName)
}
finally {
  if ($process -and -not $process.HasExited) {
    Stop-Process -Id $process.Id -Force
    $process.WaitForExit()
  }
}
