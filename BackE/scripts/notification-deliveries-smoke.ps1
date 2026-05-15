$ErrorActionPreference = "Stop"

$env:USERPROFILE = "D:\ERMSystem\.localuser"
$env:APPDATA = "D:\ERMSystem\.localuser\AppData\Roaming"
$env:DOTNET_CLI_HOME = "D:\ERMSystem\.dotnet-home"
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

$apiProj = "D:\ERMSystem\BackE\ERMSystem.API\ERMSystem.API.csproj"
$baseUrl = "http://localhost:5219"
$hospitalSqlServer = "VietHa\MSSQLSERVER01"
$hospitalDb = "ERMSystemHospitalDb"

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
  $adminUsername = "notif_delivery_admin_$suffix"
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

  $allDeliveries = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-notification-deliveries?pageNumber=1&pageSize=20" `
    -Headers $headers

  if ($allDeliveries.totalCount -lt 1 -or $allDeliveries.items.Count -lt 1) {
    throw "Khong co notification delivery nao de smoke test."
  }

  $invalidDelivery = @($allDeliveries.items | Where-Object { $_.deliveryStatus -notin @("Failed", "Skipped", "Queued") }) | Select-Object -First 1
  if ($null -ne $invalidDelivery) {
    try {
      Invoke-RestMethod `
        -Method Post `
        -Uri "$baseUrl/api/hospital-notification-deliveries/$($invalidDelivery.id)/retry" `
        -Headers $headers | Out-Null
      throw "Retry delivery khong hop le dang le phai tra ve 409."
    }
    catch {
      if (-not $_.Exception.Response -or $_.Exception.Response.StatusCode.value__ -ne 409) {
        throw
      }
    }
  }

  $delivery = @($allDeliveries.items | Where-Object { $_.deliveryStatus -in @("Failed", "Skipped") }) | Select-Object -First 1
  if ($null -eq $delivery) {
    $delivery = @($allDeliveries.items | Where-Object { $_.deliveryStatus -notin @("Queued") }) | Select-Object -First 1
    if ($null -eq $delivery) {
      throw "Khong co delivery nao de force sang Failed phuc vu smoke test."
    }

    $deliveryIdSql = [string]$delivery.id
    & sqlcmd -S $hospitalSqlServer -E -d $hospitalDb -Q @"
SET NOCOUNT ON;
UPDATE [notification].[NotificationDeliveries]
SET [DeliveryStatus] = N'Failed',
    [ErrorMessage] = N'Smoke forced failure',
    [DeliveredAtUtc] = NULL,
    [ProviderMessageId] = NULL
WHERE [Id] = '$deliveryIdSql';
"@ | Out-Null

    $delivery.deliveryStatus = "Failed"
  }

  $preRetryStatus = $delivery.deliveryStatus
  $retryResponse = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/hospital-notification-deliveries/$($delivery.id)/retry" `
    -Headers $headers

  $queuedDeliveries = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-notification-deliveries?status=Queued&channelCode=$($delivery.channelCode)&recipient=$([uri]::EscapeDataString($delivery.recipient))&pageNumber=1&pageSize=20" `
    -Headers $headers

  $queuedItem = @($queuedDeliveries.items | Where-Object { $_.id -eq $delivery.id }) | Select-Object -First 1
  if ($null -eq $queuedItem) {
    throw "Delivery vua retry khong xuat hien trong danh sach Queued."
  }

  if ($queuedItem.deliveryStatus -ne "Queued") {
    throw "Status sau retry khong dung: $($queuedItem.deliveryStatus)"
  }

  if (-not $retryResponse.message) {
    throw "Retry response khong tra ve message xac nhan."
  }

  Write-Output ("delivery_id=" + $delivery.id)
  Write-Output ("previous_status=" + $preRetryStatus)
  Write-Output ("queued_count=" + $queuedDeliveries.totalCount)
  Write-Output ("channel_code=" + $queuedItem.channelCode)
  Write-Output ("recipient=" + $queuedItem.recipient)
}
finally {
  if ($process -and -not $process.HasExited) {
    Stop-Process -Id $process.Id -Force
    $process.WaitForExit()
  }
}
