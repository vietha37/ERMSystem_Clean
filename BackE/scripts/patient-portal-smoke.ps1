$ErrorActionPreference = "Stop"

$env:USERPROFILE = "D:\ERMSystem\.localuser"
$env:APPDATA = "D:\ERMSystem\.localuser\AppData\Roaming"
$env:DOTNET_CLI_HOME = "D:\ERMSystem\.dotnet-home"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

$apiProj = "D:\ERMSystem\BackE\ERMSystem.API\ERMSystem.API.csproj"
$baseUrl = "http://localhost:5219"
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
  $username = "portal_patient_$suffix"
  $password = "123456"

  $registerBody = @{
    username    = $username
    password    = $password
    fullName    = "Benh nhan Portal $suffix"
    dateOfBirth = "1995-05-20"
    gender      = "Nam"
    phone       = "090000$suffix".Substring(0, 10)
    address     = "123 Duong Portal, Quan 1"
  } | ConvertTo-Json

  Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/auth/patient-register" `
    -ContentType "application/json" `
    -Body $registerBody | Out-Null

  $loginBody = @{
    username = $username
    password = $password
  } | ConvertTo-Json

  $login = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/auth/login" `
    -ContentType "application/json" `
    -Body $loginBody

  $headers = @{
    Authorization = "Bearer $($login.token)"
  }

  $portal = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-patient-portal/me" `
    -Headers $headers

  if (-not $portal.profile) {
    throw "Portal response khong co profile."
  }

  if ($portal.profile.fullName -ne "Benh nhan Portal $suffix") {
    throw "Portal profile fullName khong khop."
  }

  foreach ($property in @("upcomingAppointments", "recentAppointments", "recentPrescriptions", "recentClinicalOrders", "recentInvoices")) {
    if (-not ($portal.PSObject.Properties.Name -contains $property)) {
      throw "Portal response thieu truong $property."
    }
  }

  Write-Output ("portal_username=" + $username)
  Write-Output ("portal_patient=" + $portal.profile.fullName)
  Write-Output ("upcoming_count=" + $portal.upcomingAppointments.Count)
  Write-Output ("recent_appointments_count=" + $portal.recentAppointments.Count)
  Write-Output ("recent_prescriptions_count=" + $portal.recentPrescriptions.Count)
  Write-Output ("recent_clinical_orders_count=" + $portal.recentClinicalOrders.Count)
  Write-Output ("recent_invoices_count=" + $portal.recentInvoices.Count)
}
finally {
  if ($process -and -not $process.HasExited) {
    Stop-Process -Id $process.Id -Force
  }
}
