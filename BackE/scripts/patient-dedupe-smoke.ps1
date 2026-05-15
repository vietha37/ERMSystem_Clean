param()

$ErrorActionPreference = "Stop"
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

$env:USERPROFILE = "D:\ERMSystem\.localuser"
$env:APPDATA = "D:\ERMSystem\.localuser\AppData\Roaming"
$env:DOTNET_CLI_HOME = "D:\ERMSystem\.dotnet-home"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

$apiProj = "D:\ERMSystem\BackE\ERMSystem.API\ERMSystem.API.csproj"
$out = "D:\ERMSystem\BackE\patient-dedupe-smoke.out.log"
$err = "D:\ERMSystem\BackE\patient-dedupe-smoke.err.log"

if (Test-Path $out) { Remove-Item $out -Force }
if (Test-Path $err) { Remove-Item $err -Force }

$process = Start-Process -FilePath dotnet `
  -ArgumentList @("run", "--no-build", "--project", $apiProj, "--urls", "http://localhost:5219") `
  -PassThru `
  -RedirectStandardOutput $out `
  -RedirectStandardError $err

try {
  Start-Sleep -Seconds 7

  $suffix = [DateTime]::Now.ToString("HHmmss")
  $adminUser = "admin_patient_dedupe_" + $suffix
  $password = "123456"

  $registerBody = @{
    username = $adminUser
    password = $password
    role = "Admin"
  } | ConvertTo-Json

  try {
    Invoke-RestMethod -Method Post -Uri "http://localhost:5219/api/auth/register" -ContentType "application/json" -Body $registerBody | Out-Null
  }
  catch {
  }

  $loginBody = @{
    username = $adminUser
    password = $password
  } | ConvertTo-Json

  $login = Invoke-RestMethod -Method Post -Uri "http://localhost:5219/api/auth/login" -ContentType "application/json" -Body $loginBody
  $headers = @{ Authorization = "Bearer $($login.token)" }

  $doctorBody = @{
    fullName = "Dr Patient Dedupe"
    specialty = "General"
  } | ConvertTo-Json
  $doctor = Invoke-RestMethod -Method Post -Uri "http://localhost:5219/api/doctors" -Headers $headers -ContentType "application/json" -Body $doctorBody

  $sourcePatientBody = @{
    fullName = "Tran Thi Duplicate"
    dateOfBirth = "1988-05-05T00:00:00Z"
    gender = "Female"
    phone = "0912 345 678"
    address = "A Street"
    emergencyContactName = "Nguoi Nha"
    emergencyContactPhone = "0909888777"
    emergencyContactRelationship = "Me"
  } | ConvertTo-Json

  $targetPatientBody = @{
    fullName = "Tran Thi Duplicate"
    dateOfBirth = "1988-05-05T00:00:00Z"
    gender = "Female"
    phone = "0912345678"
    address = "B Street"
    emergencyContactName = $null
    emergencyContactPhone = "0909888777"
    emergencyContactRelationship = $null
  } | ConvertTo-Json

  $sourcePatient = Invoke-RestMethod -Method Post -Uri "http://localhost:5219/api/patients" -Headers $headers -ContentType "application/json" -Body $sourcePatientBody
  $targetPatient = Invoke-RestMethod -Method Post -Uri "http://localhost:5219/api/patients" -Headers $headers -ContentType "application/json" -Body $targetPatientBody

  $duplicates = @(Invoke-RestMethod -Method Get -Uri ("http://localhost:5219/api/patients/" + $sourcePatient.id + "/potential-duplicates") -Headers $headers)
  if ($duplicates.Count -lt 1) {
    throw "Khong tim thay duplicate candidate."
  }

  $appointmentBody = @{
    patientId = $sourcePatient.id
    doctorId = $doctor.id
    appointmentDate = [DateTime]::UtcNow.AddDays(1).ToString("o")
    status = "Pending"
  } | ConvertTo-Json
  $appointment = Invoke-RestMethod -Method Post -Uri "http://localhost:5219/api/appointments" -Headers $headers -ContentType "application/json" -Body $appointmentBody

  $deleteConflict = $null
  try {
    Invoke-RestMethod -Method Delete -Uri ("http://localhost:5219/api/patients/" + $sourcePatient.id) -Headers $headers | Out-Null
    throw "Delete patient dang co appointment dang le phai bi chan."
  }
  catch {
    if (-not $_.Exception.Response -or $_.Exception.Response.StatusCode.value__ -ne 409) {
      throw
    }
    $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
    $deleteConflict = $reader.ReadToEnd()
    $reader.Close()
  }

  $mergeBody = @{
    sourcePatientId = $sourcePatient.id
    targetPatientId = $targetPatient.id
  } | ConvertTo-Json
  $merge = Invoke-RestMethod -Method Post -Uri "http://localhost:5219/api/patients/merge" -Headers $headers -ContentType "application/json" -Body $mergeBody

  $mergedAppointment = Invoke-RestMethod -Method Get -Uri ("http://localhost:5219/api/appointments/" + $appointment.id) -Headers $headers
  $mergedTarget = Invoke-RestMethod -Method Get -Uri ("http://localhost:5219/api/patients/" + $targetPatient.id) -Headers $headers

  $sourceStatus = "unknown"
  try {
    Invoke-RestMethod -Method Get -Uri ("http://localhost:5219/api/patients/" + $sourcePatient.id) -Headers $headers | Out-Null
    $sourceStatus = "still_exists"
  }
  catch {
    if ($_.Exception.Response -and $_.Exception.Response.StatusCode.value__ -eq 404) {
      $sourceStatus = "deleted"
    }
    else {
      throw
    }
  }

  $candidate = $duplicates[0]
  $uniqueReasons = @($candidate.matchReasons | Select-Object -Unique)
  Write-Output ("duplicate_count=" + $duplicates.Count)
  Write-Output ("match_reasons=" + ($uniqueReasons -join ","))
  Write-Output ("delete_conflict_status=409")
  Write-Output ("delete_conflict_body=" + $deleteConflict)
  Write-Output ("reassigned_appointments=" + $merge.reassignedAppointmentCount)
  Write-Output ("appointment_patient_id=" + $mergedAppointment.patientId)
  Write-Output ("target_emergency_name=" + $mergedTarget.emergencyContactName)
  Write-Output ("source_status=" + $sourceStatus)
}
finally {
  if ($process -and -not $process.HasExited) {
    Stop-Process -Id $process.Id -Force
  }
}
