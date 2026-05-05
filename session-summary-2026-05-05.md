# Tong ket phien lam viec ngay 2026-05-05

## 1. Muc tieu cua ngay lam viec

Trong ngay hom nay, he thong duoc day tiep theo dung huong trong `plan.md`, tap trung vao 3 nhom chinh:

- Hoan thien nen tang notification theo kieu event-driven.
- Bo sung man hinh noi bo de van hanh va theo doi notification.
- Chuyen `patient portal` sang `ERMSystemHospitalDb` de benh nhan xem du lieu that.

Day la mot lat cat quan trong vi no noi lien 3 lop:

- public booking
- backend event pipeline
- giao dien patient portal/noi bo

Tu diem nay, du an khong con dung o muc demo CRUD co ban, ma da co nhieu luong gan hon voi he thong benh vien tu thuc te.

---

## 2. Tong quan nhung gi da hoan thanh trong ngay

Cac hang muc da lam xong trong ngay:

1. Day luong dat lich cong khai vao `ERMSystemHospitalDb`.
2. Ghi `OutboxMessages` khi tao lich hen.
3. Tao worker publish outbox sang RabbitMQ.
4. Tao worker consumer doc RabbitMQ va sinh `NotificationDeliveries`.
5. Tao worker dispatch gui thong bao qua mock sender Email/SMS.
6. Tao API noi bo de theo doi va retry notification delivery.
7. Tao giao dien noi bo `/notifications` cho admin va le tan.
8. Chuyen patient portal sang hospital DB.
9. Dong bo tai khoan benh nhan tu auth hien tai sang `identity.Users` va `patient.PatientAccounts` trong hospital DB.
10. Verify build va smoke test end-to-end.

---

## 3. Lat cat 1: Dat lich cong khai that tren hospital DB

### Muc tieu

Truoc do giao dien booking moi chi o muc UX/public page. Trong ngay hom nay luong nay da duoc noi vao database dich de co the lam nen cho patient portal va notification pipeline.

### Nhung gi da lam

Da them backend cho phep dat lich cong khai vao bang `scheduling.Appointments` cua `ERMSystemHospitalDb`.

Khi nguoi dung dat lich:

- chon bac si
- chon ngay gio
- nhap thong tin benh nhan

backend se:

- kiem tra bac si ton tai
- kiem tra bac si co cho dat lich online khong
- kiem tra chuyen khoa va bac si co khop khong
- doi chieu lich lam viec cua bac si
- kiem tra trung lich trong khung gio do
- neu benh nhan chua ton tai thi tao patient moi
- tao appointment moi
- tao outbox message de day sang notification pipeline

### File backend lien quan

- `BackE/ERMSystem.API/Controllers/HospitalAppointmentsController.cs`
- `BackE/ERMSystem.API/Controllers/HospitalDoctorsController.cs`
- `BackE/ERMSystem.Application/DTOs/HospitalDoctorDto.cs`
- `BackE/ERMSystem.Application/Interfaces/IHospitalAppointmentRepository.cs`
- `BackE/ERMSystem.Application/Interfaces/IHospitalAppointmentService.cs`
- `BackE/ERMSystem.Application/Interfaces/IHospitalDoctorRepository.cs`
- `BackE/ERMSystem.Application/Interfaces/IHospitalDoctorService.cs`
- `BackE/ERMSystem.Application/Services/HospitalAppointmentService.cs`
- `BackE/ERMSystem.Application/Services/HospitalDoctorService.cs`
- `BackE/ERMSystem.Infrastructure/Repositories/HospitalAppointmentRepository.cs`
- `BackE/ERMSystem.Infrastructure/Repositories/HospitalDoctorRepository.cs`
- `BackE/ERMSystem.Infrastructure/HospitalData/HospitalDbContext.cs`
- `Infrastructure/database/erm_private_hospital_seed.sql`

### File frontend lien quan

- `FontE/services/hospitalDoctorService.ts`
- `FontE/services/hospitalAppointmentService.ts`
- `FontE/components/public/BookingForm.tsx`
- `FontE/app/doctors/page.tsx`
- `FontE/app/booking/page.tsx`

### Y nghia

Day la buoc mo khoa de cac luong sau co du lieu that:

- notification
- patient portal
- worklist cua le tan
- worklist cua bac si

---

## 4. Lat cat 2: Notification pipeline theo kieu event-driven

Phan nay la trong tam ky thuat cua ngay hom nay.

Pipeline hien tai da co du cac tang sau:

1. tao event trong outbox
2. publish ra RabbitMQ
3. consume event tu RabbitMQ
4. tao delivery record trong database
5. dispatch qua sender theo kenh
6. theo doi trang thai va retry

### 4.1. Outbox publisher

#### Chuc nang

Sau khi tao lich hen, he thong ghi 1 ban ghi vao `notification.OutboxMessages` voi event `AppointmentCreated.v1`.

Worker publisher se:

- poll outbox theo chu ky
- mo ket noi RabbitMQ khi broker san sang
- declare exchange va queue can thiet
- publish event len exchange
- danh dau outbox la `Published` neu thanh cong
- retry an toan khi broker chua co

#### File chinh

- `BackE/ERMSystem.Infrastructure/ERMSystem.Infrastructure.csproj`
- `BackE/ERMSystem.Infrastructure/Messaging/RabbitMqOptions.cs`
- `BackE/ERMSystem.Infrastructure/Messaging/OutboxPublisherOptions.cs`
- `BackE/ERMSystem.Infrastructure/Messaging/HospitalOutboxPublisherService.cs`
- `BackE/ERMSystem.API/Program.cs`
- `BackE/ERMSystem.API/appsettings.json`

#### Ghi chu ky thuat

Worker duoc viet theo huong khong lam chet host API khi RabbitMQ down. Neu broker khong san sang thi worker chi log/retry, API booking van chay duoc.

### 4.2. RabbitMQ consumer -> NotificationDeliveries

#### Chuc nang

Worker consumer nghe queue RabbitMQ va map event thanh delivery can gui.

Voi event `AppointmentCreated.v1`, worker se:

- deserialize envelope event
- kiem tra idempotency qua `integration.InboxMessages`
- tim template `APPOINTMENT_CREATED`
- tao `notification.NotificationDeliveries`
  - `Email` neu co email
  - `SMS` neu co phone
- neu thieu template thi danh dau `Skipped`
- neu du dieu kien thi danh dau `Queued`

#### File chinh

- `BackE/ERMSystem.Infrastructure/HospitalData/Entities/NotificationEntities.cs`
- `BackE/ERMSystem.Infrastructure/HospitalData/Entities/IntegrationEntities.cs`
- `BackE/ERMSystem.Infrastructure/HospitalData/HospitalDbContext.cs`
- `BackE/ERMSystem.Infrastructure/Messaging/NotificationConsumerOptions.cs`
- `BackE/ERMSystem.Infrastructure/Messaging/HospitalNotificationConsumerService.cs`
- `BackE/ERMSystem.API/Program.cs`
- `BackE/ERMSystem.API/appsettings.json`
- `Infrastructure/database/erm_private_hospital_seed.sql`

### 4.3. Delivery dispatch worker + mock sender

#### Chuc nang

Sau khi delivery duoc tao va o trang thai `Queued`, worker dispatch se:

- quet danh sach delivery dang cho gui
- chon sender theo `ChannelCode`
- goi mock sender
  - Email -> `MockEmailNotificationSender`
  - SMS -> `MockSmsNotificationSender`
- neu thanh cong:
  - `DeliveryStatus = Delivered`
  - set `ProviderMessageId`
  - set `DeliveredAtUtc`
  - tang `AttemptCount`
- neu loi:
  - `DeliveryStatus = Failed`
  - luu `ErrorMessage`

#### File chinh

- `BackE/ERMSystem.Application/DTOs/NotificationDeliveryDto.cs`
- `BackE/ERMSystem.Application/Interfaces/IHospitalNotificationDeliveryRepository.cs`
- `BackE/ERMSystem.Application/Interfaces/IHospitalNotificationDeliveryService.cs`
- `BackE/ERMSystem.Application/Services/HospitalNotificationDeliveryService.cs`
- `BackE/ERMSystem.Infrastructure/Repositories/HospitalNotificationDeliveryRepository.cs`
- `BackE/ERMSystem.Infrastructure/Messaging/NotificationDispatchOptions.cs`
- `BackE/ERMSystem.Infrastructure/Messaging/INotificationChannelSender.cs`
- `BackE/ERMSystem.Infrastructure/Messaging/MockEmailNotificationSender.cs`
- `BackE/ERMSystem.Infrastructure/Messaging/MockSmsNotificationSender.cs`
- `BackE/ERMSystem.Infrastructure/Messaging/HospitalNotificationDispatchService.cs`
- `BackE/ERMSystem.API/Controllers/HospitalNotificationDeliveriesController.cs`
- `BackE/ERMSystem.API/Program.cs`
- `BackE/ERMSystem.API/appsettings.json`

### 4.4. API da co sau khi lam pipeline

- `GET /api/hospital-notification-deliveries`
- `POST /api/hospital-notification-deliveries/{deliveryId}/retry`

### 4.5. Y nghia cua notification pipeline

Pipeline nay da dua he thong den muc co the mo rong rat de theo huong production:

- thay mock sender bang provider that
- thay RabbitMQ local bang broker dev/staging/prod
- them DLQ, retry policy, backoff, observability
- tach thanh notification microservice trong giai doan sau

---

## 5. Lat cat 3: Giao dien noi bo theo doi notification

### Muc tieu

Sau khi backend da co `NotificationDeliveries`, can co man hinh de van hanh theo doi thay vi chi xem database.

### Nhung gi da lam

Da them route noi bo `/notifications` cho:

- `Admin`
- `Receptionist`

Man hinh nay cho phep:

- xem danh sach delivery
- loc theo trang thai
- phan trang
- tu dong lam moi 15 giay
- xem thong tin:
  - kenh gui
  - nguoi nhan
  - trang thai
  - so lan thu
  - lan thu gan nhat
  - thoi diem gui thanh cong
  - provider message id
  - loi gan nhat
- retry delivery `Failed` hoac `Skipped`

### File frontend lien quan

- `FontE/app/(main)/notifications/page.tsx`
- `FontE/services/hospitalNotificationDeliveryService.ts`
- `FontE/services/types.ts`
- `FontE/components/layout/ProtectedLayout.tsx`
- `FontE/components/layout/Sidebar.tsx`
- `FontE/components/layout/Header.tsx`

### Ghi chu them

Nhan dip nay da chuan hoa them nhan menu/noi dung giao dien noi bo sang Tieng Viet ASCII de tranh loi ma hoa trong moi truong hien tai.

---

## 6. Lat cat 4: Chuyen patient portal sang hospital DB

### Van de truoc khi sua

`/portal` truoc do moi la trang demo:

- doc profile tu schema cu
- chua co lich hen that tren hospital DB
- chua gan duoc voi luong booking moi

### Muc tieu sau khi sua

Portal benh nhan phai doc truc tiep tu `ERMSystemHospitalDb` de tu day co the mo rong thanh:

- lich hen cua toi
- don thuoc cua toi
- ket qua xet nghiem cua toi
- thanh toan cua toi

### Nhung gi da lam o backend

Da tao mot module portal rieng, khong nhung vao `PatientsController` cu.

API moi:

- `GET /api/hospital-patient-portal/me`

API nay doc theo `userId` trong JWT va tra ve:

- profile benh nhan
- danh sach lich hen sap toi
- lich su lich hen gan day

### File backend lien quan

- `BackE/ERMSystem.Application/DTOs/HospitalPatientPortalDto.cs`
- `BackE/ERMSystem.Application/Interfaces/IHospitalPatientPortalRepository.cs`
- `BackE/ERMSystem.Application/Interfaces/IHospitalPatientPortalService.cs`
- `BackE/ERMSystem.Application/Services/HospitalPatientPortalService.cs`
- `BackE/ERMSystem.Infrastructure/Repositories/HospitalPatientPortalRepository.cs`
- `BackE/ERMSystem.API/Controllers/HospitalPatientPortalController.cs`
- `BackE/ERMSystem.API/Program.cs`

### Nhung gi da lam o frontend

Da viet lai trang `/portal` de doc du lieu that tu hospital DB.

Trang moi hien co:

- khoi tong quan tai khoan benh nhan
- ma benh an
- thong tin co ban cua ho so
- lich hen sap toi
- lich su lich hen gan day
- giao dien rieng cho benh nhan, khong dung chung dashboard noi bo

### File frontend lien quan

- `FontE/app/portal/page.tsx`
- `FontE/services/hospitalPatientPortalService.ts`
- `FontE/services/types.ts`

---

## 7. Lat cat 5: Dong bo tai khoan benh nhan sang hospital DB

### Van de kien truc can giai quyet

Auth hien tai van phat JWT tu he thong cu, dua tren `AppUser` o database cu.

Trong khi do, patient portal moi can du lieu o hospital DB:

- `identity.Users`
- `identity.UserRoles`
- `patient.Patients`
- `patient.PatientAccounts`

Neu khong co lop dong bo nay thi:

- benh nhan dang ky moi se khong vao duoc portal moi
- benh nhan cu dang nhap se khong map duoc sang hospital DB

### Cach xu ly da ap dung

Da mo rong `AuthService` de khi:

- `patient-register`
- `login` voi role `Patient`

he thong se tu dong `EnsureHospitalPatientProjectionAsync(...)`.

Ham nay se:

1. tao hoac cap nhat `identity.Users` trong hospital DB voi cung `Id` nhu `AppUser`
2. tao `identity.UserRoles` cho role `Patient` neu chua co
3. tim `patient.PatientAccounts` theo `UserId`
4. neu chua co account:
   - co gang tim 1 hospital patient phu hop theo ho ten + ngay sinh + so dien thoai
   - chi lay patient chua bi account khac so huu
   - neu khong co thi tao patient moi
5. tao `patient.PatientAccounts`
6. cap nhat trang thai portal la `Active`

### Y nghia

Giai phap nay cho phep:

- giu nguyen luong auth hien tai trong giai doan chuyen tiep
- van dua patient portal len hospital DB ngay bay gio
- tranh phai doi toan bo auth stack trong 1 buoc

Day la mot cau noi quan trong giua he thong cu va schema dich moi.

### File chinh

- `BackE/ERMSystem.Infrastructure/Services/AuthService.cs`

---

## 8. Kiem thu va xac minh da thuc hien

### Backend build

Da xac minh:

- `dotnet build BackE\ERMSystem.API\ERMSystem.API.csproj --no-restore`
- ket qua: `Build succeeded` khi chay ngoai sandbox

### Frontend lint/build

Da xac minh:

- `npm.cmd run lint`
- `npm.cmd run build`
- ket qua: pass

### Smoke test notification dispatch

Da xac minh:

- worker dispatch co the xu ly delivery `Queued`
- ket qua mau:
  - `DeliveryStatus = Delivered`
  - `ProviderMessageId = mock-email-...`
  - `AttemptCount = 1`

### Smoke test patient portal end-to-end

Da test luong:

1. `patient-register`
2. dat lich qua `public-booking`
3. goi `GET /api/hospital-patient-portal/me`

Ket qua da xac nhan:

- `portal_profile=Nguyen Thi Portal ...`
- `portal_mrn=MRN-...`
- `upcoming_count=1`
- `recent_count=0`
- `booked_appointment=APT-...`

Dieu nay chung minh:

- tai khoan benh nhan da duoc dong bo sang hospital DB
- patient portal doc dung profile theo JWT
- public booking va portal da noi duoc vao cung mot schema dich

---

## 9. Han che hien tai va nhung gi chua xong

### 9.1. RabbitMQ broker that chua duoc verify full chain tai may nay

Ma code da co:

- publisher
- consumer
- dispatch worker

Tuy nhien may hien tai chua co RabbitMQ broker chay that, nen full chain sau chua verify bang broker song:

- `Outbox -> RabbitMQ -> Consumer -> Delivery -> Dispatch`

Da verify duoc:

- build pass
- worker start an toan
- retry an toan khi broker down
- dispatch worker xu ly duoc queued delivery bang du lieu mau

### 9.2. Portal moi moi chi cover profile + appointments

Chua lam tiep cac muc:

- don thuoc cua toi
- ket qua xet nghiem cua toi
- ket qua chan doan hinh anh cua toi
- hoa don/thanh toan cua toi

### 9.3. He thong van dang o giai doan bridge giua DB cu va DB moi

Mot so module van dang o schema cu:

- auth goc
- patient CRUD cu
- appointment CRUD cu
- dashboard cu

Mot so module moi da chuyen sang hospital DB:

- hospital catalog
- hospital doctors
- public booking
- patient portal moi
- notification pipeline moi

Can tiep tuc refactor theo phase, tranh tron loan 2 he thong qua lau.

---

## 10. Thu tu uu tien de dev tiep tuc

De giu dung huong `plan.md`, thu tu hop ly nhat sau ngay hom nay la:

1. Lam tiep patient portal:
   - don thuoc cua toi
   - ket qua xet nghiem cua toi
   - ket qua chan doan hinh anh cua toi
2. Lam man hinh noi bo hospital appointment management cho le tan.
3. Tich hop provider that cho Email/SMS thay vi mock sender.
4. Dung RabbitMQ broker dev that de verify full chain.
5. Tiep tuc giam phu thuoc vao schema cu bang cach refactor `patient`, `appointment`, `medical record` sang hospital DB.

---

## 11. Ket luan ky thuat

Ngay hom nay la ngay dat nen cho 2 luong rat quan trong:

- he thong thong bao event-driven
- patient portal that tren hospital DB

Sau ngay lam viec nay, du an da co nhung diem moc sau:

- dat lich cong khai khong con la demo, ma da ghi that vao database dich
- notification pipeline da du cach de sau nay tach thanh service rieng
- van hanh noi bo da co man hinh theo doi delivery
- benh nhan da co portal rieng dung du lieu that tu hospital DB
- auth hien tai da co cau noi sang hospital DB cho role benh nhan

Day la mot buoc tien rat quan trong de tu day backend va frontend cung bam vao mot schema dich thong nhat, thay vi tiep tuc phat trien tren schema CRUD cu.
