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
  $doctor = @($doctors | Where-Object { $_.isBookable -and $_.schedules.Count -gt 0 -and $_.consultationFee -gt 0 }) | Select-Object -First 1
  if ($null -eq $doctor) {
    throw "Khong tim thay bac si co consultation fee va lich bookable de test billing."
  }

  $slots = Get-CandidateScheduleSlots -Doctor $doctor
  $suffix = [DateTime]::Now.ToString("yyyyMMddHHmmss")
  $phone = ("096" + $suffix.Substring($suffix.Length - 7))
  $email = "billingworklist.$suffix@erm.local"
  $booking = $null

  foreach ($slot in $slots) {
    $bookingBody = [ordered]@{
      fullName        = "Benh nhan Billing Worklist $suffix"
      phone           = $phone
      email           = $email
      dateOfBirth     = "1992-11-08"
      gender          = "Nam"
      doctorProfileId = $doctor.doctorProfileId
      specialtyId     = $doctor.specialtyId
      preferredDate   = $slot.PreferredDate
      preferredTime   = $slot.PreferredTime
      chiefComplaint  = "Dau hong"
      notes           = "Smoke test billing worklist"
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

  $adminUsername = "billing_worklist_admin_$suffix"
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
    -Body (@{ counterLabel = "Counter F" } | ConvertTo-Json)

  if ($checkedIn.status -ne "CheckedIn") {
    throw "Check-in cho billing worklist khong thanh cong."
  }

  $encounter = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/hospital-encounters" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body ([ordered]@{
      appointmentId = $booking.appointmentId
      diagnosisName = "Viem hong cap"
      diagnosisCode = "J02.9"
      diagnosisType = "Working"
      encounterStatus = "Finalized"
      summary = "Encounter finalize de lap hoa don"
      subjective = "Dau hong, sot nhe"
      objective = "Hong do, sinh ton on dinh"
      assessment = "Dieu tri ngoai tru"
      carePlan = "Thuoc va nghi ngoi"
    } | ConvertTo-Json)

  if ($encounter.encounterStatus -ne "Finalized") {
    throw "Tao encounter Finalized khong thanh cong."
  }

  $eligible = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-billing/eligible-encounters" `
    -Headers $headers

  $eligibleItem = @($eligible | Where-Object { $_.encounterId -eq $encounter.encounterId }) | Select-Object -First 1
  if ($null -eq $eligibleItem) {
    throw "Encounter moi tao khong xuat hien trong eligible encounters cua billing."
  }

  $invoice = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/hospital-billing" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body (@{
      encounterId = $encounter.encounterId
      discountAmount = 0
      insuranceAmount = 0
    } | ConvertTo-Json)

  if ($invoice.invoiceStatus -ne "Issued") {
    throw "Tao invoice khong thanh cong."
  }

  $issuedList = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-billing?pageNumber=1&pageSize=20&invoiceStatus=Issued&textSearch=$($invoice.invoiceNumber)" `
    -Headers $headers

  $issuedItem = @($issuedList.items | Where-Object { $_.invoiceId -eq $invoice.invoiceId }) | Select-Object -First 1
  if ($null -eq $issuedItem) {
    throw "Invoice moi tao khong xuat hien trong worklist Issued."
  }

  $partialAmount = [Math]::Round([decimal]$invoice.totalAmount / 2, 2)
  if ($partialAmount -le 0) {
    throw "Tong tien invoice khong hop le de test payment."
  }

  $partialPayment = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/hospital-billing/$($invoice.invoiceId)/payments" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body (@{
      paymentMethod = "Cash"
      paymentReference = "PAY-PART-$suffix"
      amount = $partialAmount
    } | ConvertTo-Json)

  if ($partialPayment.invoiceStatus -ne "PartiallyPaid") {
    throw "Invoice sau partial payment khong sang PartiallyPaid."
  }

  $partialList = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-billing?pageNumber=1&pageSize=20&invoiceStatus=PartiallyPaid&textSearch=$($invoice.invoiceNumber)" `
    -Headers $headers

  $partialItem = @($partialList.items | Where-Object { $_.invoiceId -eq $invoice.invoiceId }) | Select-Object -First 1
  if ($null -eq $partialItem) {
    throw "Invoice khong xuat hien trong worklist PartiallyPaid."
  }

  $remainingAmount = [decimal]$partialPayment.balanceAmount
  $finalPayment = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/hospital-billing/$($invoice.invoiceId)/payments" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body (@{
      paymentMethod = "Cash"
      paymentReference = "PAY-FULL-$suffix"
      amount = $remainingAmount
    } | ConvertTo-Json)

  if ($finalPayment.invoiceStatus -ne "Paid") {
    throw "Invoice sau final payment khong sang Paid."
  }

  $paidList = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-billing?pageNumber=1&pageSize=20&invoiceStatus=Paid&textSearch=$($invoice.invoiceNumber)" `
    -Headers $headers

  $paidItem = @($paidList.items | Where-Object { $_.invoiceId -eq $invoice.invoiceId }) | Select-Object -First 1
  if ($null -eq $paidItem) {
    throw "Invoice khong xuat hien trong worklist Paid."
  }

  $refundAmount = [Math]::Round([decimal]$finalPayment.totalAmount / 3, 2)
  if ($refundAmount -le 0) {
    throw "Khong the tao so tien refund hop le."
  }

  $refund = Invoke-RestMethod `
    -Method Post `
    -Uri "$baseUrl/api/hospital-billing/$($invoice.invoiceId)/refunds" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body (@{
      paymentMethod = "Cash"
      paymentReference = "REF-PART-$suffix"
      amount = $refundAmount
      reason = "Smoke test refund"
    } | ConvertTo-Json)

  if ($refund.invoiceStatus -ne "PartiallyPaid") {
    throw "Invoice sau refund khong sang PartiallyPaid."
  }

  $refundPayment = @($refund.payments | Where-Object { $_.paymentStatus -eq "Refunded" } | Sort-Object paidAtLocal -Descending) | Select-Object -First 1
  if ($null -eq $refundPayment) {
    throw "Khong tim thay payment Refund trong invoice detail."
  }

  $postRefundList = Invoke-RestMethod `
    -Method Get `
    -Uri "$baseUrl/api/hospital-billing?pageNumber=1&pageSize=20&invoiceStatus=PartiallyPaid&textSearch=$($invoice.invoiceNumber)" `
    -Headers $headers

  $postRefundItem = @($postRefundList.items | Where-Object { $_.invoiceId -eq $invoice.invoiceId }) | Select-Object -First 1
  if ($null -eq $postRefundItem) {
    throw "Invoice sau refund khong xuat hien lai trong worklist PartiallyPaid."
  }

  Write-Output ("encounter_number=" + $encounter.encounterNumber)
  Write-Output ("invoice_number=" + $invoice.invoiceNumber)
  Write-Output ("eligible_found=" + $true)
  Write-Output ("issued_total=" + $issuedList.totalCount)
  Write-Output ("partial_balance=" + $partialPayment.balanceAmount)
  Write-Output ("paid_total=" + $paidList.totalCount)
  Write-Output ("refund_amount=" + $refundAmount)
  Write-Output ("refund_status=" + $refund.invoiceStatus)
}
finally {
  if ($process -and -not $process.HasExited) {
    Stop-Process -Id $process.Id -Force
    $process.WaitForExit()
  }
}
