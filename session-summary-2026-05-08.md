# Tong ket cong viec ngay 2026-05-08

## 1. Muc tieu trong ngay

Trong ngay hom nay, cac hang muc chinh da duoc day tiep theo `plan.md` gom:

1. Dong bo `staff noi bo -> hospital identity` de actor nghiep vu duoc gan dung vao hospital DB.
2. Hoan thien `dispensing flow` cho pharmacy tren hospital DB.
3. Tao `clinical orders flow` cho xet nghiem va chan doan hinh anh.
4. Mo rong `billing flow` tu encounter sang hoa don va thanh toan.
5. Sửa va chuan hoa giao dien frontend do loi ma hoa/UTF-8, uu tien cac man public va cac man noi bo dang van hanh.
6. Don dep workspace: xoa file `.log`, them rule ignore de giu repo sach hon.

---

## 2. Chuc nang 1: Dong bo staff noi bo sang hospital identity

### Muc dich

Auth hien tai van co mot phan du lieu va luong dang nhap o schema cu. Trong khi do cac module hospital moi (`encounter`, `prescription`, `clinical order`) can luu actor vao hospital identity. Vi vay can mot lop cau noi de:

- dong bo user noi bo sang `identity.Users` cua hospital DB,
- gan role hospital phu hop,
- dam bao `RecordedByUserId`, `AuthoredByUserId`, `OrderedByUserId` co gia tri hop le.

### Ket qua dat duoc

- Dong bo user noi bo sang hospital DB khi dang ky/dang nhap.
- Mo API bulk sync de backfill user noi bo da ton tai.
- Service nghiep vu `encounter` va `prescription` resolve actor uu tien theo hospital identity.
- Giai quyet bai toan doctor/staff co username noi bo nhung du lieu van hanh nam o hospital DB.

### File lien quan

#### Backend
- [IHospitalIdentityBridgeService.cs](D:\ERMSystem\BackE\ERMSystem.Application\Interfaces\IHospitalIdentityBridgeService.cs)
- [HospitalInternalUserSyncDto.cs](D:\ERMSystem\BackE\ERMSystem.Application\DTOs\HospitalInternalUserSyncDto.cs)
- [HospitalIdentityBridgeService.cs](D:\ERMSystem\BackE\ERMSystem.Infrastructure\Services\HospitalIdentityBridgeService.cs)
- [AuthService.cs](D:\ERMSystem\BackE\ERMSystem.Infrastructure\Services\AuthService.cs)
- [AdminUsersController.cs](D:\ERMSystem\BackE\ERMSystem.API\Controllers\AdminUsersController.cs)
- [IUserRepository.cs](D:\ERMSystem\BackE\ERMSystem.Application\Interfaces\IUserRepository.cs)
- [UserRepository.cs](D:\ERMSystem\BackE\ERMSystem.Infrastructure\Repositories\UserRepository.cs)
- [HospitalEncounterService.cs](D:\ERMSystem\BackE\ERMSystem.Application\Services\HospitalEncounterService.cs)
- [HospitalPrescriptionService.cs](D:\ERMSystem\BackE\ERMSystem.Application\Services\HospitalPrescriptionService.cs)
- [HospitalEncountersController.cs](D:\ERMSystem\BackE\ERMSystem.API\Controllers\HospitalEncountersController.cs)
- [HospitalPrescriptionsController.cs](D:\ERMSystem\BackE\ERMSystem.API\Controllers\HospitalPrescriptionsController.cs)
- [IHospitalEncounterService.cs](D:\ERMSystem\BackE\ERMSystem.Application\Interfaces\IHospitalEncounterService.cs)
- [IHospitalPrescriptionService.cs](D:\ERMSystem\BackE\ERMSystem.Application\Interfaces\IHospitalPrescriptionService.cs)
- [Program.cs](D:\ERMSystem\BackE\ERMSystem.API\Program.cs)

### API moi
- `POST /api/admin/users/sync-hospital-identity`

### Verify
- `dotnet build`: pass
- Smoke test actor mapping: pass
- Da xac nhan actor trong hospital workflow duoc map dung vao hospital identity.

---

## 3. Chuc nang 2: Dispensing flow cho pharmacy

### Muc dich

Sau khi don thuoc duoc phat hanh, he thong can co buoc cap thuoc that su, khong dung lai o `Issued`.

### Ket qua dat duoc

- Them endpoint cap thuoc cho prescription.
- Tao ban ghi `Dispensings` khi cap thuoc.
- Cap nhat trang thai prescription sang `Dispensed`.
- Cap nhat `OrderHeader` lien quan sang `Completed`.
- Phat sinh outbox event `PrescriptionDispensed.v1`.
- Frontend trang don thuoc co them thao tac `Cap thuoc` va hien metadata dispensing.

### File lien quan

#### Backend
- [HospitalPrescriptionDto.cs](D:\ERMSystem\BackE\ERMSystem.Application\DTOs\HospitalPrescriptionDto.cs)
- [IHospitalPrescriptionRepository.cs](D:\ERMSystem\BackE\ERMSystem.Application\Interfaces\IHospitalPrescriptionRepository.cs)
- [IHospitalPrescriptionService.cs](D:\ERMSystem\BackE\ERMSystem.Application\Interfaces\IHospitalPrescriptionService.cs)
- [HospitalPrescriptionService.cs](D:\ERMSystem\BackE\ERMSystem.Application\Services\HospitalPrescriptionService.cs)
- [HospitalPrescriptionRepository.cs](D:\ERMSystem\BackE\ERMSystem.Infrastructure\Repositories\HospitalPrescriptionRepository.cs)
- [HospitalPrescriptionsController.cs](D:\ERMSystem\BackE\ERMSystem.API\Controllers\HospitalPrescriptionsController.cs)

#### Frontend
- [page.tsx](D:\ERMSystem\FontE\app\(main)\prescriptions\page.tsx)
- [hospitalPrescriptionService.ts](D:\ERMSystem\FontE\services\hospitalPrescriptionService.ts)
- [types.ts](D:\ERMSystem\FontE\services\types.ts)

### API moi
- `POST /api/hospital-prescriptions/{prescriptionId}/dispense`

### Verify
- `dotnet build`: pass
- `npm run lint`: pass
- `npm run build`: pass
- Smoke test dispensing: pass

---

## 4. Chuc nang 3: Clinical orders flow cho xet nghiem va chan doan hinh anh

### Muc dich

Can bo sung luong can lam sang that tren hospital DB, tach ro:

- chi dinh xet nghiem,
- chi dinh chan doan hinh anh,
- nhap ket qua,
- nhap bao cao,
- cap nhat order status,
- day outbox event.

### Ket qua dat duoc

- Tao module `clinical orders` theo category `Lab` va `Imaging`.
- Tao worklist chi dinh.
- Tao chi dinh tu encounter.
- Nhap ket qua xet nghiem.
- Nhap bao cao chan doan hinh anh.
- Cap nhat `OrderHeader` sang `Completed`.
- Phat sinh event cho tung buoc trong pipeline.
- Them trang noi bo `/clinical-orders`.

### File lien quan

#### Backend
- [LabImagingEntities.cs](D:\ERMSystem\BackE\ERMSystem.Infrastructure\HospitalData\Entities\LabImagingEntities.cs)
- [HospitalDbContext.cs](D:\ERMSystem\BackE\ERMSystem.Infrastructure\HospitalData\HospitalDbContext.cs)
- [HospitalClinicalOrderDto.cs](D:\ERMSystem\BackE\ERMSystem.Application\DTOs\HospitalClinicalOrderDto.cs)
- [IHospitalClinicalOrderRepository.cs](D:\ERMSystem\BackE\ERMSystem.Application\Interfaces\IHospitalClinicalOrderRepository.cs)
- [IHospitalClinicalOrderService.cs](D:\ERMSystem\BackE\ERMSystem.Application\Interfaces\IHospitalClinicalOrderService.cs)
- [HospitalClinicalOrderRepository.cs](D:\ERMSystem\BackE\ERMSystem.Infrastructure\Repositories\HospitalClinicalOrderRepository.cs)
- [HospitalClinicalOrderService.cs](D:\ERMSystem\BackE\ERMSystem.Application\Services\HospitalClinicalOrderService.cs)
- [HospitalClinicalOrdersController.cs](D:\ERMSystem\BackE\ERMSystem.API\Controllers\HospitalClinicalOrdersController.cs)
- [Program.cs](D:\ERMSystem\BackE\ERMSystem.API\Program.cs)

#### Frontend
- [page.tsx](D:\ERMSystem\FontE\app\(main)\clinical-orders\page.tsx)
- [hospitalClinicalOrderService.ts](D:\ERMSystem\FontE\services\hospitalClinicalOrderService.ts)
- [types.ts](D:\ERMSystem\FontE\services\types.ts)
- [Sidebar.tsx](D:\ERMSystem\FontE\components\layout\Sidebar.tsx)
- [ProtectedLayout.tsx](D:\ERMSystem\FontE\components\layout\ProtectedLayout.tsx)

### API moi
- `GET /api/hospital-clinical-orders`
- `GET /api/hospital-clinical-orders/{clinicalOrderId}`
- `GET /api/hospital-clinical-orders/eligible-encounters`
- `GET /api/hospital-clinical-orders/catalog`
- `POST /api/hospital-clinical-orders`
- `POST /api/hospital-clinical-orders/{clinicalOrderId}/lab-result`
- `POST /api/hospital-clinical-orders/{clinicalOrderId}/imaging-report`

### Verify
- `dotnet build`: pass
- `npm run lint`: pass
- `npm run build`: pass
- Smoke test end-to-end clinical order: pass

---

## 5. Chuc nang 4: Billing flow cho encounter, hoa don va thanh toan

### Muc dich

Can co buoc tai chinh sau encounter:

- lap hoa don,
- gom phi kham va dich vu hoan thanh,
- thu tien,
- theo doi cong no,
- day event billing.

### Ket qua dat duoc

- Them entity `Invoices`, `InvoiceItems`, `Payments`.
- Lap hoa don tu encounter.
- Tu dong gom:
  - phi kham,
  - xet nghiem hoan thanh,
  - chan doan hinh anh hoan thanh,
  - item thuoc co `UnitPrice`.
- Thu tien va chuyen trang thai:
  - `Issued`
  - `PartiallyPaid`
  - `Paid`
- Day event:
  - `InvoiceIssued.v1`
  - `InvoicePaymentReceived.v1`
- Frontend co man `/billing` de xem worklist hoa don, tao hoa don va thu tien.

### File lien quan

#### Backend
- [BillingEntities.cs](D:\ERMSystem\BackE\ERMSystem.Infrastructure\HospitalData\Entities\BillingEntities.cs)
- [HospitalDbContext.cs](D:\ERMSystem\BackE\ERMSystem.Infrastructure\HospitalData\HospitalDbContext.cs)
- [HospitalBillingDto.cs](D:\ERMSystem\BackE\ERMSystem.Application\DTOs\HospitalBillingDto.cs)
- [IHospitalBillingRepository.cs](D:\ERMSystem\BackE\ERMSystem.Application\Interfaces\IHospitalBillingRepository.cs)
- [IHospitalBillingService.cs](D:\ERMSystem\BackE\ERMSystem.Application\Interfaces\IHospitalBillingService.cs)
- [HospitalBillingRepository.cs](D:\ERMSystem\BackE\ERMSystem.Infrastructure\Repositories\HospitalBillingRepository.cs)
- [HospitalBillingService.cs](D:\ERMSystem\BackE\ERMSystem.Application\Services\HospitalBillingService.cs)
- [HospitalBillingController.cs](D:\ERMSystem\BackE\ERMSystem.API\Controllers\HospitalBillingController.cs)
- [Program.cs](D:\ERMSystem\BackE\ERMSystem.API\Program.cs)

#### Frontend
- [page.tsx](D:\ERMSystem\FontE\app\(main)\billing\page.tsx)
- [hospitalBillingService.ts](D:\ERMSystem\FontE\services\hospitalBillingService.ts)
- [types.ts](D:\ERMSystem\FontE\services\types.ts)
- [Sidebar.tsx](D:\ERMSystem\FontE\components\layout\Sidebar.tsx)
- [ProtectedLayout.tsx](D:\ERMSystem\FontE\components\layout\ProtectedLayout.tsx)

### API moi
- `GET /api/hospital-billing`
- `GET /api/hospital-billing/eligible-encounters`
- `GET /api/hospital-billing/{invoiceId}`
- `POST /api/hospital-billing`
- `POST /api/hospital-billing/{invoiceId}/payments`

### Verify
- `dotnet build`: pass
- `npm run lint`: pass
- `npm run build`: pass
- Smoke test billing: pass

---

## 6. Chuc nang 5: Sua loi ma hoa, chuan hoa UTF-8 cho frontend

### Van de gap phai

Frontend co nhieu literal text bi loi ma hoa, the hien ra cac ky tu vo nghia tren giao dien. Van de nay khong phai do CSS font-family ma do source file dang chua chuoi text bi moji-bake / encoding sai.

### Ket qua dat duoc

Da uu tien sua cac man user-facing va cac man noi bo quan trong:

#### Public / patient-facing
- Trang chu public.
- Trang dang nhap staff/patient.
- Trang bac si.
- Trang booking.
- Trang services.
- Trang specialties.
- Trang patient portal.
- Booking form.
- Footer.
- Sidebar.
- `layout.tsx` duoc cap nhat them `latin-ext` cho Google fonts.

#### Internal pages da sua trong ngay
- Trang `appointments`.
- Trang `notifications`.

### File lien quan

#### Frontend public / shared
- [page.tsx](D:\ERMSystem\FontE\app\page.tsx)
- [layout.tsx](D:\ERMSystem\FontE\app\layout.tsx)
- [login\page.tsx](D:\ERMSystem\FontE\app\login\page.tsx)
- [doctors\page.tsx](D:\ERMSystem\FontE\app\doctors\page.tsx)
- [booking\page.tsx](D:\ERMSystem\FontE\app\booking\page.tsx)
- [services\page.tsx](D:\ERMSystem\FontE\app\services\page.tsx)
- [specialties\page.tsx](D:\ERMSystem\FontE\app\specialties\page.tsx)
- [portal\page.tsx](D:\ERMSystem\FontE\app\portal\page.tsx)
- [BookingForm.tsx](D:\ERMSystem\FontE\components\public\BookingForm.tsx)
- [SiteFooter.tsx](D:\ERMSystem\FontE\components\public\SiteFooter.tsx)
- [Sidebar.tsx](D:\ERMSystem\FontE\components\layout\Sidebar.tsx)

#### Frontend internal pages duoc chuan hoa them
- [appointments\page.tsx](D:\ERMSystem\FontE\app\(main)\appointments\page.tsx)
- [notifications\page.tsx](D:\ERMSystem\FontE\app\(main)\notifications\page.tsx)

### Tinh trang hien tai

Da xu ly xong loi hien thi nghiem trong o cac man public va mot phan man noi bo.

Van con mot so man legacy chua duoc chuan hoa 100% trong ngay nay:
- [medical-records\page.tsx](D:\ERMSystem\FontE\app\(main)\medical-records\page.tsx)
- [clinical-orders\page.tsx](D:\ERMSystem\FontE\app\(main)\clinical-orders\page.tsx)
- [doctor-worklist\page.tsx](D:\ERMSystem\FontE\app\(main)\doctor-worklist\page.tsx)
- [prescriptions\page.tsx](D:\ERMSystem\FontE\app\(main)\prescriptions\page.tsx)
- [Header.tsx](D:\ERMSystem\FontE\components\layout\Header.tsx)

### Verify
- `npm run lint`: pass
- `npm run build`: pass
- Frontend production build tao route thanh cong.

---

## 7. Chuc nang 6: Don dep workspace va giu repo sach hon

### Ket qua dat duoc

- Xoa toan bo file `*.log` trong workspace.
- Them rule ignore de file log va build artifact khong quay lai.
- Khong dong vao `.vs/` local state de tranh anh huong IDE cua nguoi dung.

### File lien quan
- [.gitignore](D:\ERMSystem\.gitignore)

### Noi dung da bo sung vao `.gitignore`
- `*.log`
- `FontE/.next/`

---

## 8. Kiem thu da chay trong ngay

### Backend
- `dotnet build D:\ERMSystem\BackE\ERMSystem.sln --no-restore`: pass

### Frontend
- `npm.cmd run lint`: pass
- `npm.cmd run build`: pass

### Smoke tests da xac nhan trong ngay
- actor mapping cho hospital identity: pass
- dispensing: pass
- clinical orders end-to-end: pass
- billing end-to-end: pass

---

## 9. Luu y cho dev tiep quan

1. Hospital workflow da du tuyen hon truoc: auth bridge, encounter actor, prescription dispensing, clinical orders, billing.
2. Frontend da sach hon nhieu o lop public va mot phan lop noi bo, nhung chua xong 100% pass Viet hoa / UTF-8 cho toan bo `FontE`.
3. Khong stage `.vs/` va khong stage file local khong lien quan.
4. Trong workspace van con file summary cu khong lien quan den code hien tai:
   - [session-summary-2026-05-06.md](D:\ERMSystem\session-summary-2026-05-06.md)
5. Neu tiep tuc frontend cleanup, uu tien theo thu tu:
   - `medical-records`
   - `clinical-orders`
   - `doctor-worklist`
   - `prescriptions`
   - `Header`

---

## 10. Thu tu uu tien de lam tiep

1. Chuan hoa not cac man noi bo con loi ma hoa.
2. Day tiep pharmacy / lab / imaging / billing sang UI production-grade dong nhat.
3. Mo rong patient portal de xem don thuoc, ket qua can lam sang va hoa don.
4. Neu can release on dinh hon, can bo sung them e2e test cho cac route hospital moi.
