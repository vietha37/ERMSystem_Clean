$ErrorActionPreference = "Stop"

function Stop-ApiProcess {
  param([System.Diagnostics.Process]$Process)

  if ($null -ne $Process -and -not $Process.HasExited) {
    Stop-Process -Id $Process.Id -Force
    $Process.WaitForExit()
  }
}

function Invoke-Json {
  param(
    [string]$Method,
    [string]$Uri,
    [object]$Body = $null,
    [hashtable]$Headers = @{}
  )

  if ($null -eq $Body) {
    return Invoke-RestMethod -Method $Method -Uri $Uri -Headers $Headers
  }

  return Invoke-RestMethod `
    -Method $Method `
    -Uri $Uri `
    -Headers $Headers `
    -ContentType "application/json" `
    -Body ($Body | ConvertTo-Json -Depth 10)
}

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
          ClinicId = $schedule.clinicId
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

$apiProj = "D:\ERMSystem\BackE\ERMSystem.API\ERMSystem.API.csproj"
$baseUrl = "http://localhost:5219"
$apiProcess = $null

try {
  $apiProcess = Start-Process `
    -FilePath "dotnet" `
    -ArgumentList @("run", "--no-build", "--project", $apiProj, "--urls", $baseUrl) `
    -PassThru

  Start-Sleep -Seconds 7

  $suffix = Get-Date -Format "yyyyMMddHHmmss"
  $adminUser = "worklist_admin_$suffix"
  $password = "123456"

  Invoke-Json `
    -Method Post `
    -Uri "$baseUrl/api/auth/register" `
    -Body @{
      username = $adminUser
      password = $password
      role = "Admin"
    } | Out-Null

  $login = Invoke-Json `
    -Method Post `
    -Uri "$baseUrl/api/auth/login" `
    -Body @{
      username = $adminUser
      password = $password
    }

  $headers = @{ Authorization = "Bearer $($login.token)" }

  $doctors = Invoke-RestMethod -Method Get -Uri "$baseUrl/api/hospital-doctors"
  $doctor = $doctors | Where-Object {
    $_.isBookable -and
    $_.doctorProfileId -and
    $_.schedules -and
    $_.schedules.Count -gt 0
  } | Select-Object -First 1

  if ($null -eq $doctor) {
    throw "Khong tim thay bac si co lich bookable de test worklist."
  }

  $slots = Get-CandidateScheduleSlots -Doctor $doctor
  $booking = $null
  $selectedSlot = $null
  foreach ($slot in $slots) {
    try {
      $booking = Invoke-Json `
        -Method Post `
        -Uri "$baseUrl/api/hospital-appointments/public-booking" `
        -Body @{
          doctorProfileId = $doctor.doctorProfileId
          specialtyId = $doctor.specialtyId
          fullName = "Benh nhan Worklist $suffix"
          dateOfBirth = "1992-05-11"
          gender = "Nam"
          phone = ("093" + $suffix.Substring($suffix.Length - 7))
          email = "worklist.$suffix@erm.local"
          preferredDate = $slot.PreferredDate
          preferredTime = $slot.PreferredTime
          serviceCode = "FOLLOWUP"
          chiefComplaint = "Smoke test doctor worklist"
          notes = "Smoke test doctor worklist"
        }
      $selectedSlot = $slot
      break
    }
    catch {
      continue
    }
  }

  if ($null -eq $booking -or $null -eq $selectedSlot) {
    throw "Khong dat duoc lich hen de test doctor worklist."
  }

  $workDate = ([DateTime]$booking.appointmentStartLocal).ToString("yyyy-MM-dd")
  $worklistBeforeCheckIn = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-doctor-worklist?workDate=$workDate&doctorProfileId=$($doctor.doctorProfileId)" `
    -Headers $headers

  $bookedItem = $worklistBeforeCheckIn.items | Where-Object { $_.appointmentId -eq $booking.appointmentId } | Select-Object -First 1
  if ($null -eq $bookedItem) {
    throw "Appointment moi tao khong xuat hien trong worklist."
  }

  if ($bookedItem.workflowStage -ne "Cho tiep don") {
    throw "Workflow stage truoc check-in khong dung: $($bookedItem.workflowStage)"
  }

  $checkIn = Invoke-Json `
    -Method Post `
    -Uri "$baseUrl/api/hospital-appointments/$($booking.appointmentId)/check-in" `
    -Headers $headers `
    -Body @{ counterLabel = "Counter C" }

  if ($checkIn.status -ne "CheckedIn") {
    throw "Check-in cho worklist khong thanh cong."
  }

  $worklistAfterCheckIn = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-doctor-worklist?workDate=$workDate&doctorProfileId=$($doctor.doctorProfileId)" `
    -Headers $headers

  $checkedInItem = $worklistAfterCheckIn.items | Where-Object { $_.appointmentId -eq $booking.appointmentId } | Select-Object -First 1
  if ($null -eq $checkedInItem) {
    throw "Appointment sau check-in bien mat khoi worklist."
  }

  if ($checkedInItem.appointmentStatus -ne "CheckedIn") {
    throw "Appointment status sau check-in khong dung: $($checkedInItem.appointmentStatus)"
  }

  if ($checkedInItem.workflowStage -ne "Cho mo ho so") {
    throw "Workflow stage sau check-in khong dung: $($checkedInItem.workflowStage)"
  }

  $encounter = Invoke-Json `
    -Method Post `
    -Uri "$baseUrl/api/hospital-encounters" `
    -Headers $headers `
    -Body @{
      appointmentId = $booking.appointmentId
      diagnosisName = "Theo doi worklist smoke"
      diagnosisCode = "Z09"
      diagnosisType = "Working"
      encounterStatus = "InProgress"
      summary = "Encounter tao de verify doctor worklist."
      subjective = "Benh nhan dang cho kham."
      objective = "Sinh ton on dinh."
      assessment = "Can theo doi them."
      carePlan = "Tiep tuc quy trinh kham."
    }

  if ($encounter.encounterStatus -ne "InProgress") {
    throw "Tao encounter cho worklist khong thanh cong."
  }

  $worklistAfterEncounter = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-doctor-worklist?workDate=$workDate&doctorProfileId=$($doctor.doctorProfileId)" `
    -Headers $headers

  $encounterItem = $worklistAfterEncounter.items | Where-Object { $_.appointmentId -eq $booking.appointmentId } | Select-Object -First 1
  if ($null -eq $encounterItem) {
    throw "Appointment sau khi mo encounter khong con trong worklist."
  }

  if ($encounterItem.encounterId -ne $encounter.encounterId) {
    throw "Encounter trong worklist khong khop encounter vua tao."
  }

  if ($encounterItem.encounterStatus -ne "InProgress") {
    throw "Encounter status trong worklist khong dung: $($encounterItem.encounterStatus)"
  }

  if ($encounterItem.workflowStage -ne "Dang kham") {
    throw "Workflow stage sau khi mo encounter khong dung: $($encounterItem.workflowStage)"
  }

  Write-Output ("appointment_number=" + $booking.appointmentNumber)
  Write-Output ("work_date=" + $workDate)
  Write-Output ("doctor_name=" + $doctor.fullName)
  Write-Output ("checked_in_count=" + $worklistAfterCheckIn.checkedInAppointments)
  Write-Output ("encounter_number=" + $encounter.encounterNumber)
  Write-Output ("in_progress_count=" + $worklistAfterEncounter.inProgressEncounters)
}
finally {
  Stop-ApiProcess -Process $apiProcess
}
