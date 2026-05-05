# Thiet ke lai co so du lieu ERMSystem

## 1. Muc dich

Tai lieu nay dinh nghia mo hinh du lieu dich cho ERMSystem khi nang cap thanh he thong benh vien tu co day du nghiep vu thuc te.

Thiet ke moi phai du suc phuc vu:

- van hanh ngoai tru
- cong benh nhan
- van hanh noi bo cho nhan su
- benh an dien tu va luong kham
- xet nghiem, chan doan hinh anh, nha thuoc
- vien phi, thanh toan, bao hiem
- thong bao va audit
- tach dan sang microservice sau nay

Schema hien tai qua nho, moi aggregate moi chi moi dung o muc demo. Tai lieu nay tro thanh blueprint chuan de backend va frontend bam theo.

## 2. Nguyen tac thiet ke

1. Tach domain theo schema tu bay gio, tach service sau.
2. Dung business identifier cho nghiep vu quan trong, khong chi dua vao GUID noi bo.
3. Moi du lieu y te va tai chinh quan trong phai audit duoc.
4. Encounter, order, result, invoice, notification la aggregate hang mot.
5. Ho tro song song tai khoan benh nhan va tai khoan nhan su.
6. Uu tien lich su y khoa dang append, tranh cap nhat de mat dau vet.
7. Chi dung soft delete khi an toan nghiep vu; khong soft delete cac bang dang ledger.

## 3. Phan tach schema muc tieu

Giai doan modular monolith su dung mot database SQL Server, nhung tach bang theo schema:

- `identity`
  Xac thuc, phan quyen, session, refresh token, tai khoan nhan su va benh nhan.
- `org`
  Cau truc benh vien, khoa phong, chuyen khoa, phong kham, ho so nhan su, lich bac si.
- `patient`
  Ho so benh nhan, dinh danh, lien he, bao hiem, nguoi lien he khan cap, consent.
- `scheduling`
  Slot dat lich, lich hen, check-in, so thu tu, tiep don.
- `emr`
  Encounter, dau hieu sinh ton, chan doan, ghi chu, document, order.
- `lab`
  Danh muc xet nghiem, chi dinh, specimen, ket qua.
- `imaging`
  Danh muc chan doan hinh anh, chi dinh, bao cao.
- `pharmacy`
  Thuoc, don thuoc, cap phat, batch ton kho, giao dich kho.
- `billing`
  Danh muc dich vu, hoa don, dong hoa don, thanh toan, hoan tien, claim bao hiem.
- `notification`
  Template, preference, outbox, log gui.
- `integration`
  Mapping he thong ngoai, inbox, webhook, idempotency.
- `audit`
  Audit log va access log.

## 4. Aggregate nghiep vu cot loi

### Identity

- `identity.Users`
- `identity.Roles`
- `identity.UserRoles`
- `identity.RefreshTokens`
- `identity.UserSessions`
- `identity.SecurityEvents`

Ghi chu:

- Ca nhan su va benh nhan deu song trong `identity.Users`.
- Role he thong du kien: `Admin`, `Doctor`, `Receptionist`, `Nurse`, `Pharmacist`, `LabTech`, `Cashier`, `Patient`.

### To chuc

- `org.Departments`
- `org.Specialties`
- `org.Clinics`
- `org.StaffProfiles`
- `org.DoctorProfiles`
- `org.DoctorSchedules`

Ghi chu:

- `StaffProfiles` lien ket mot-mot voi `identity.Users`.
- `DoctorProfiles` mo rong tu `StaffProfiles`, khong dung mot bang doctor tach biet khong context.

### Benh nhan

- `patient.Patients`
- `patient.PatientAccounts`
- `patient.PatientIdentifiers`
- `patient.PatientContacts`
- `patient.PatientEmergencyContacts`
- `patient.PatientInsurancePolicies`
- `patient.PatientConsents`

Ghi chu:

- `PatientAccounts` map ho so benh nhan voi tai khoan portal.
- `MedicalRecordNumber` la business identifier bat buoc va unique.

### Dat lich va tiep don

- `scheduling.AppointmentSlots`
- `scheduling.Appointments`
- `scheduling.CheckIns`
- `scheduling.QueueTickets`

Ghi chu:

- `Appointment` khong dong nghia voi `Encounter`.
- Lich hen co the no-show, huy, doi lich ma khong tao encounter.

### Benh an dien tu

- `emr.Encounters`
- `emr.VitalSigns`
- `emr.Diagnoses`
- `emr.ClinicalNotes`
- `emr.Allergies`
- `emr.ChronicConditions`
- `emr.ClinicalDocuments`
- `emr.OrderHeaders`

Ghi chu:

- `Encounter` la trung tam nghiep vu lam sang.
- `OrderHeaders` la diem vao chung cho lab, imaging va pharmacy.

### Xet nghiem

- `lab.LabServices`
- `lab.LabOrders`
- `lab.Specimens`
- `lab.LabResultItems`

### Chan doan hinh anh

- `imaging.ImagingServices`
- `imaging.ImagingOrders`
- `imaging.ImagingReports`

### Nha thuoc

- `pharmacy.Medicines`
- `pharmacy.Prescriptions`
- `pharmacy.PrescriptionItems`
- `pharmacy.Dispensings`
- `pharmacy.InventoryBatches`
- `pharmacy.InventoryTransactions`

### Vien phi

- `billing.ServiceCatalog`
- `billing.Invoices`
- `billing.InvoiceItems`
- `billing.Payments`
- `billing.Refunds`
- `billing.InsuranceClaims`

### Thong bao va tich hop

- `notification.NotificationTemplates`
- `notification.NotificationPreferences`
- `notification.OutboxMessages`
- `notification.NotificationDeliveries`
- `integration.InboxMessages`
- `integration.ExternalMappings`
- `integration.WebhookLogs`

### Audit

- `audit.AuditLogs`
- `audit.EntityAccessLogs`

## 5. Quan he nghiep vu chinh

- Mot `identity.User` co the map toi mot `patient.PatientAccount`.
- Mot `identity.User` co the map toi mot `org.StaffProfile`.
- Mot `patient.Patient` co nhieu `scheduling.Appointments`.
- Mot `scheduling.Appointment` co the tao ra mot `emr.Encounter`.
- Mot `emr.Encounter` co the sinh ra nhieu order xuong lab, imaging, pharmacy.
- Mot `emr.Encounter` co the co nhieu chan doan va ghi chu.
- Mot `emr.Encounter` co the tao ra mot hoac nhieu don thuoc.
- Mot `billing.Invoice` co nhieu dong hoa don va nhieu lan thanh toan.

## 6. Quy uoc dinh danh va timestamp

Dung thong nhat:

- Khoa chinh noi bo: `Id UNIQUEIDENTIFIER`
- Timestamp nghiep vu:
  - `CreatedAtUtc`
  - `UpdatedAtUtc`
  - tuy bang co the co `DeletedAtUtc`
- Nguoi tac dong:
  - `CreatedByUserId`
  - `UpdatedByUserId`
- Trang thai dung string code, khong dung so magic.
- Bang cap nhat nhieu nen co `rowversion`.

Business identifier can co:

- `Patients.MedicalRecordNumber`
- `Appointments.AppointmentNumber`
- `Encounters.EncounterNumber`
- `Invoices.InvoiceNumber`
- `Payments.PaymentReference`
- `Prescriptions.PrescriptionNumber`

## 7. Yeu cau bao mat va compliance trong schema

Schema phai ho tro:

- consent tracking
- audit truy cap benh an
- hash refresh token
- security event cho login/session
- tach benh nhan va nhan su o tang app access
- lich su y khoa va tai chinh co the doi chieu

## 8. Huong mapping cho backend

Backend nen duoc to chuc xoay quanh cac module:

1. `Identity`
2. `Organization`
3. `Patient`
4. `Scheduling`
5. `Encounter/EMR`
6. `Lab`
7. `Imaging`
8. `Pharmacy`
9. `Billing`
10. `Notification`
11. `Audit`

Thu tu tach service sau nay nen la:

1. `Identity`
2. `Patient`
3. `Scheduling`
4. `Notification`
5. `EMR`
6. `Pharmacy`
7. `Billing`

## 9. Huong mapping cho frontend

Mat public va patient side bam theo:

- dang ky/dang nhap benh nhan
- ho so benh nhan
- lich hen
- ket qua xet nghiem
- don thuoc
- hoa don/thanh toan
- thong bao

Mat noi bo bam theo:

- tiep don va xep hang
- bang lich hen
- doctor worklist
- encounter/EMR editor
- lab/imaging worklist
- pharmacy dispensing
- cashier desk
- admin/staff management

## 10. Chien luoc chuyen doi tu schema hien tai

Cac bang hien tai nhu `Patients`, `Appointments`, `MedicalRecords`, `Prescriptions`, `Medicines`, `AppUsers` chi duoc xem la buoc dem migration, khong phai dich den cuoi cung.

Thu tu migration nen la:

1. On dinh `identity`.
2. Dua `org` vao va lam day profile nhan su.
3. Tach ro tai khoan benh nhan va ho so benh nhan.
4. Thay `MedicalRecords` bang `Encounters + ClinicalNotes + Diagnoses`.
5. Dua order-driven flow vao lab/imaging/pharmacy.
6. Thay invoice don gian bang mo hinh billing ledger dung nghia.

## 11. Artifact chuan

Hai artifact chuan backend/frontend phai bam theo:

- `Infrastructure/database/erm_private_hospital_schema.sql`
- `Infrastructure/database/erm_private_hospital_erd.mmd`

Day la baseline de cac buoc tiep theo cua backend va frontend xay dung tren cung mot ngon ngu du lieu.
