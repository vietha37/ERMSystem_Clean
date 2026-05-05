# Tổng kết phiên làm việc ngày 2026-05-04

## 1. Mục tiêu của phiên làm việc

Trong phiên hôm nay, hệ thống được đẩy từ trạng thái demo CRUD cơ bản sang nền tảng gần hơn với một hệ thống bệnh viện tư thực tế. Các hạng mục chính đã làm gồm:

- Mở rộng kiến trúc và lộ trình phát triển tổng thể.
- Nâng cấp xác thực với access token + refresh token.
- Bổ sung luồng tài khoản bệnh nhân độc lập với nội bộ.
- Thiết kế lại website public theo mô hình bệnh viện tư.
- Thiết kế lại database đích cho bệnh viện tư.
- Bootstrap database mới và bắt đầu nối backend/frontend vào database đích.
- Chuẩn hóa quy ước phát triển theo Tiếng Việt ở lớp hiển thị và tài liệu.

Tài liệu này mô tả chi tiết từng phần để dev khác có thể tiếp tục công việc mà không cần dò lại toàn bộ lịch sử chat.

---

## 2. Kế hoạch tổng thể của dự án

### File chính

- `plan.md`

### Nội dung đã làm

Đã tạo một file kế hoạch tổng thể để định hướng phát triển dự án từ hệ thống hiện tại sang hệ thống bệnh viện tư đầy đủ chức năng. Kế hoạch này không phải code thực thi, nhưng là mốc quan trọng để team bám vào.

### Điểm chính trong kế hoạch

- Xác định roadmap chuyển từ mô hình hiện tại sang mô hình có thể tiến tới microservice.
- Liệt kê các module nghiệp vụ cần có của bệnh viện tư:
  - quản lý bệnh nhân
  - đặt lịch và điều phối khám
  - hồ sơ bệnh án điện tử
  - xét nghiệm
  - chẩn đoán hình ảnh
  - nhà thuốc
  - thanh toán, bảo hiểm
  - thông báo, chăm sóc khách hàng
  - audit và tích hợp ngoài
- Bổ sung yêu cầu công nghệ:
  - microservice
  - Redis
  - RabbitMQ
  - refresh token
  - kiến trúc event-driven
  - logging, audit, retry, idempotency

### Ý nghĩa

File này đóng vai trò là backlog định hướng cấp cao. Từ đây, backend và frontend có cơ sở chung để chia phase và không phát triển rời rạc.

---

## 3. Nâng cấp hệ thống xác thực: Access Token + Refresh Token

### File chính

- `BackE/ERMSystem.API/Controllers/AuthController.cs`
- `BackE/ERMSystem.Application/Interfaces/IAuthService.cs`
- `BackE/ERMSystem.Application/DTOs/AuthResponseDto.cs`
- `BackE/ERMSystem.Application/DTOs/RefreshTokenRequestDto.cs`
- `BackE/ERMSystem.Infrastructure/Services/AuthService.cs`
- `BackE/ERMSystem.Domain/Entities/AppUser.cs`
- `BackE/ERMSystem.Infrastructure/Data/Migrations/20260504110000_AddRefreshTokensToAppUsers.cs`
- `BackE/ERMSystem.Infrastructure/Data/Migrations/ApplicationDbContextModelSnapshot.cs`

### Những gì đã thay đổi

Hệ thống auth cũ chỉ trả access token và không có chiến lược làm mới phiên đăng nhập. Trong phiên này đã nâng cấp thành flow production-oriented hơn.

### Chức năng đã có

- Đăng nhập trả về:
  - access token
  - refresh token
- Thêm endpoint:
  - `POST /api/auth/refresh`
  - `POST /api/auth/logout`
- Refresh token được:
  - tạo ngẫu nhiên
  - băm bằng SHA256 trước khi lưu
  - có thời hạn
  - có trạng thái revoke
  - được rotate sau mỗi lần refresh
- Logout sẽ revoke refresh token hiện tại.

### Cách hoạt động

1. User login thành công.
2. Backend tạo access token và refresh token.
3. Refresh token gốc không lưu plain text, chỉ lưu hash.
4. Khi client gọi refresh:
   - backend kiểm tra access token cũ
   - kiểm tra refresh token
   - so hash
   - kiểm tra chưa hết hạn
   - kiểm tra chưa bị revoke
5. Nếu hợp lệ:
   - refresh token cũ bị đánh dấu rotate/revoke
   - access token mới được phát hành
   - refresh token mới được phát hành
6. Khi logout:
   - refresh token hiện tại bị revoke

### Ý nghĩa kỹ thuật

- Tránh giữ access token sống quá lâu.
- Hỗ trợ đăng nhập bền hơn cho frontend.
- Tạo nền tảng để sau này tích hợp Redis blacklist hoặc session management.

### Verify đã chạy

- Build backend pass.
- Smoke test auth:
  - login có token
  - login có refresh token
  - refresh thành công
  - refresh token được rotate
  - logout revoke token thành công

---

## 4. Bổ sung luồng tài khoản bệnh nhân riêng

### File chính

- `BackE/ERMSystem.API/Controllers/AuthController.cs`
- `BackE/ERMSystem.API/Controllers/PatientsController.cs`
- `BackE/ERMSystem.Application/DTOs/PatientRegisterDto.cs`
- `BackE/ERMSystem.Application/Interfaces/IAuthService.cs`
- `BackE/ERMSystem.Application/Interfaces/IPatientRepository.cs`
- `BackE/ERMSystem.Application/Interfaces/IPatientService.cs`
- `BackE/ERMSystem.Application/Services/PatientService.cs`
- `BackE/ERMSystem.Domain/Entities/AppRole.cs`
- `BackE/ERMSystem.Domain/Entities/Patient.cs`
- `BackE/ERMSystem.Infrastructure/Repositories/PatientRepository.cs`
- `BackE/ERMSystem.Infrastructure/Services/AuthService.cs`
- `BackE/ERMSystem.Infrastructure/Data/Migrations/20260504124500_AddPatientUserLink.cs`

### Bài toán trước khi sửa

Hệ thống cũ chỉ có đăng nhập nội bộ. Người dùng kiểu bệnh nhân chưa có tài khoản, chưa có luồng self-register và chưa có cổng truy cập riêng.

### Chức năng đã làm

- Thêm role `Patient`.
- Thêm endpoint:
  - `POST /api/auth/patient-register`
  - `GET /api/patients/me`
- Thêm liên kết giữa `Patient` và `AppUser` qua `AppUserId`.
- Cho phép bệnh nhân tự đăng ký tài khoản.
- Sau khi đăng ký:
  - tạo `AppUser` role `Patient`
  - tạo record `Patient`
  - liên kết hai bảng
- Bệnh nhân đăng nhập xong sẽ không vào dashboard staff mà vào khu vực riêng.

### Logic chính

- `RegisterAsync` được siết lại cho nhóm nội bộ.
- `RegisterPatientAsync` được tách riêng cho bệnh nhân.
- `PatientsController.GetMyProfile()` lấy user id từ claim JWT rồi đọc hồ sơ của chính bệnh nhân đó.

### Quyền truy cập đã chỉnh

- Staff mới được xem danh sách bệnh nhân toàn viện.
- Patient chỉ được đọc profile của chính mình qua `GET /api/patients/me`.

### Ý nghĩa

Đây là bước đầu để hệ thống có 2 không gian người dùng:

- nội bộ: admin, bác sĩ, lễ tân
- bên ngoài: bệnh nhân

Điều này rất quan trọng nếu sau này phát triển patient portal, lịch hẹn của tôi, đơn thuốc của tôi, kết quả xét nghiệm của tôi.

### Verify đã chạy

- `patient-register -> login -> /patients/me` pass
- role trả về là `Patient`

---

## 5. Frontend auth mới và patient portal

### File chính

- `FontE/services/api.ts`
- `FontE/services/authStorage.ts`
- `FontE/services/authService.ts`
- `FontE/services/patientService.ts`
- `FontE/services/types.ts`
- `FontE/hooks/useAuth.ts`
- `FontE/components/layout/ProtectedLayout.tsx`
- `FontE/components/layout/Sidebar.tsx`
- `FontE/app/login/page.tsx`
- `FontE/app/portal/page.tsx`

### Những gì đã làm

Frontend đã được sửa để hiểu mô hình auth mới, bao gồm refresh token và user role `Patient`.

### Chức năng đã có

- Lưu access token và refresh token phía client.
- Interceptor tự refresh token khi gặp `401`.
- Nếu refresh fail thì clear session và quay về login.
- Login page được tách thành 2 luồng:
  - `Nhân sự nội bộ`
  - `Bệnh nhân`
- Bệnh nhân có form `Tạo tài khoản`.
- Sau login:
  - staff vào dashboard
  - patient vào `/portal`

### Patient portal hiện có

Trang `/portal` hiện mới là phiên bản tối thiểu, gồm:

- gọi API `GET /api/patients/me`
- hiển thị hồ sơ bệnh nhân cơ bản
- cho phép logout

### Ý nghĩa

Frontend hiện không còn coi mọi user đều là user nội bộ. Điều này mở đường cho các tính năng:

- lịch hẹn của tôi
- kết quả của tôi
- đơn thuốc của tôi
- thông báo chăm sóc khách hàng

### Verify đã chạy

- `npm run lint` pass
- `npm run build` pass

---

## 6. Thiết kế lại website public theo hướng bệnh viện tư

### File chính

- `FontE/app/page.tsx`
- `FontE/app/services/page.tsx`
- `FontE/app/specialties/page.tsx`
- `FontE/app/doctors/page.tsx`
- `FontE/app/booking/page.tsx`
- `FontE/app/news/page.tsx`
- `FontE/components/public/*`
- `FontE/content/hospitalContent.ts`
- `FontE/app/layout.tsx`
- `FontE/app/globals.css`

### Những gì đã làm

Lớp giao diện public ban đầu được thay bằng một website dạng bệnh viện tư hoàn chỉnh hơn, lấy cảm hứng từ kiểu tổ chức nội dung của các hệ thống như Medlatec nhưng dùng visual riêng của dự án.

### Các trang public đã có

- Trang chủ
- Dịch vụ
- Chuyên khoa
- Bác sĩ
- Đặt lịch
- Tin tức

### Nội dung đã có trên lớp public

- hero section
- quick actions
- nhóm dịch vụ
- hệ chuyên khoa
- đội ngũ bác sĩ
- patient journey
- tin tức
- booking form
- header/footer public

### Ý nghĩa

Hệ thống frontend không còn là một app nội bộ thuần CRUD. Đã có lớp public site đủ để phát triển thành cổng tiếp cận bệnh nhân, marketing và điều hướng booking thực tế.

### Lưu ý

Ở giai đoạn đầu, nhiều nội dung public còn là content tĩnh. Trong phần cuối phiên, một số trang đã bắt đầu được nối vào dữ liệu thật từ database đích.

---

## 7. Thiết kế lại database đích cho bệnh viện tư

### File chính

- `database-design.md`
- `Infrastructure/database/erm_private_hospital_schema.sql`
- `Infrastructure/database/erm_private_hospital_seed.sql`
- `Infrastructure/database/erm_private_hospital_erd.mmd`
- `Infrastructure/database/bootstrap_erm_private_hospital.ps1`

### Mục tiêu

Database cũ chỉ phù hợp với ứng dụng CRUD nhỏ. Mục tiêu của phần này là tạo ra schema đích đủ sức chứa nghiệp vụ của một bệnh viện tư thực tế, đồng thời có ranh giới rõ để sau này tách microservice.

### Thiết kế đã tạo

Schema được tách theo domain:

- `identity`
- `org`
- `patient`
- `scheduling`
- `emr`
- `lab`
- `imaging`
- `pharmacy`
- `billing`
- `notification`
- `integration`
- `audit`

### Một số điểm quan trọng trong thiết kế mới

- Tách `appointment` khỏi `encounter`.
- Có `patient account` riêng cho portal.
- Có `order header` để nối xét nghiệm, chẩn đoán hình ảnh, đơn thuốc.
- Có `invoice`, `payment`, `refund`, `insurance claim`.
- Có `notification outbox`.
- Có `security event` và `audit log`.
- Có mã nghiệp vụ:
  - `MedicalRecordNumber`
  - `AppointmentNumber`
  - `EncounterNumber`
  - `InvoiceNumber`

### Dữ liệu seed

Đã seed dữ liệu nền mẫu cho:

- phòng ban
- chuyên khoa
- phòng khám
- danh mục dịch vụ
- dịch vụ xét nghiệm
- dịch vụ chẩn đoán hình ảnh
- thuốc mẫu
- role

### Bootstrap script

Đã tạo script PowerShell để:

- tạo database
- chạy schema
- chạy seed
- in ra số bảng theo schema để verify

### Vấn đề đã xử lý

Ban đầu schema fail khi import do `identity` là keyword trong SQL Server. Đã sửa bằng cách bracket hóa toàn bộ thành `[identity]`.

### Trạng thái hiện tại

Database mới `ERMSystemHospitalDb` đã được tạo và nạp thành công.

### Verify đã chạy

Số bảng sau bootstrap:

- `identity`: 6
- `org`: 6
- `patient`: 7
- `scheduling`: 4
- `emr`: 8
- `lab`: 4
- `imaging`: 3
- `pharmacy`: 6
- `billing`: 6
- `notification`: 4
- `integration`: 3
- `audit`: 2

---

## 8. Chuẩn hóa ngôn ngữ dự án sang Tiếng Việt

### File chính

- `rule.md`
- `skill.md`
- `database-design.md`
- `Infrastructure/database/bootstrap_erm_private_hospital.ps1`
- `Infrastructure/database/erm_private_hospital_schema.sql`
- `Infrastructure/database/erm_private_hospital_seed.sql`

### Nội dung đã làm

Đã chốt quy ước ngôn ngữ cho dự án:

- ưu tiên Tiếng Việt ở lớp hiển thị
- ưu tiên Tiếng Việt ở tài liệu nghiệp vụ
- ưu tiên Tiếng Việt ở seed data nghiệp vụ
- giữ nguyên tên thuốc, hoạt chất, thuật ngữ không nên dịch, tên công nghệ, package, framework

### Ý nghĩa

Điều này giúp:

- team dev và BA đọc tài liệu dễ hơn
- UI thống nhất hơn với bối cảnh Việt Nam
- seed data mô phỏng sát thực tế hơn

### Lưu ý kỹ thuật

Không refactor toàn bộ tên class/table/route sang Tiếng Việt vì sẽ phá tính ổn định của codebase và tăng chi phí bảo trì. Quy ước hiện tại là:

- text hướng người dùng: Tiếng Việt
- định danh kỹ thuật: chỉ đổi khi thực sự cần

---

## 9. Dựng persistence layer mới bám database đích

### File chính

- `BackE/ERMSystem.Infrastructure/HospitalData/HospitalDbContext.cs`
- `BackE/ERMSystem.Infrastructure/HospitalData/Entities/IdentityEntities.cs`
- `BackE/ERMSystem.Infrastructure/HospitalData/Entities/OrganizationEntities.cs`
- `BackE/ERMSystem.Infrastructure/HospitalData/Entities/PatientEntities.cs`
- `BackE/ERMSystem.Infrastructure/HospitalData/Entities/SchedulingEntities.cs`
- `BackE/ERMSystem.Infrastructure/HospitalData/Entities/BillingEntities.cs`
- `BackE/ERMSystem.API/Program.cs`
- `BackE/ERMSystem.API/appsettings.json`

### Lý do làm phần này

Nếu chỉ có file SQL mà backend không đọc được database mới thì frontend và team dev vẫn không thể bám vào schema đích. Vì vậy đã thêm một `DbContext` mới song song với `ApplicationDbContext` cũ.

### Những gì đã có

- `HospitalDbContext` mới trỏ tới `ERMSystemHospitalDb`
- Entity mapping cho các miền đầu tiên:
  - identity
  - org
  - patient
  - scheduling
  - billing.ServiceCatalog
- Cấu hình quan hệ EF Core cho các bảng nền

### Mục tiêu của cách làm này

- Không phá hệ thống cũ ngay lập tức
- Cho phép refactor theo từng lát cắt
- Giữ được khả năng chạy song song:
  - DB cũ cho flow cũ
  - DB mới cho flow mục tiêu

### Ý nghĩa

Đây là bước chuyển quan trọng nhất về backend trong hôm nay, vì từ thời điểm này hệ thống không còn chỉ “có schema trên giấy”, mà đã có lớp persistence thật để code tiếp.

---

## 10. Tạo API danh mục nền từ database đích

### File chính

- `BackE/ERMSystem.Application/DTOs/HospitalCatalogDto.cs`
- `BackE/ERMSystem.Application/Interfaces/IHospitalCatalogRepository.cs`
- `BackE/ERMSystem.Application/Interfaces/IHospitalCatalogService.cs`
- `BackE/ERMSystem.Application/Services/HospitalCatalogService.cs`
- `BackE/ERMSystem.Infrastructure/Repositories/HospitalCatalogRepository.cs`
- `BackE/ERMSystem.API/Controllers/HospitalCatalogController.cs`

### Chức năng đã làm

Đã mở một lát cắt read-only đầu tiên bám database mới để frontend có thể dùng dữ liệu thật.

### Endpoint đã có

- `GET /api/hospital-catalog/overview`
- `GET /api/hospital-catalog/departments`
- `GET /api/hospital-catalog/specialties`
- `GET /api/hospital-catalog/clinics`
- `GET /api/hospital-catalog/services`

### Dữ liệu trả về

- danh sách phòng ban
- danh sách chuyên khoa
- danh sách phòng khám
- danh mục dịch vụ

### Logic triển khai

- Repository đọc dữ liệu qua `HospitalDbContext`
- Service gom dữ liệu và expose DTO
- Controller mở public để frontend public site có thể đọc

### Ý nghĩa

Thay vì frontend phải dùng content cứng, bây giờ đã có thể đọc danh mục thật từ database đích của bệnh viện.

### Verify đã chạy

Smoke test API:

- `departments=4`
- `specialties=4`
- `clinics=4`
- `services=4`

---

## 11. Nối frontend public vào API danh mục mới

### File chính

- `FontE/services/hospitalCatalogService.ts`
- `FontE/app/services/page.tsx`
- `FontE/app/specialties/page.tsx`
- `FontE/app/booking/page.tsx`
- `FontE/components/public/BookingForm.tsx`

### Những gì đã làm

Frontend public bắt đầu chuyển từ content tĩnh sang đọc dữ liệu thật từ backend mới.

### Trang đã nối

- `/services`
- `/specialties`
- `/booking`

### Cách hoạt động

- Tạo `hospitalCatalogService.ts` để gọi API mới.
- Nếu API sẵn sàng:
  - trang `services` đọc danh mục dịch vụ thật
  - trang `specialties` đọc chuyên khoa thật
  - form booking lấy danh sách dịch vụ từ backend
- Nếu API lỗi hoặc backend chưa chạy:
  - frontend fallback về content tĩnh cũ

### Ý nghĩa

Đây là cách chuyển đổi an toàn:

- không làm vỡ trang public khi backend chưa chạy
- vẫn cho phép môi trường dev dùng dữ liệu thật khi sẵn sàng

---

## 12. Dọn và chuẩn bị hạ tầng local dev

### File chính

- `.gitignore`
- `Infrastructure/docker-compose.dev.yml`

### Những gì đã làm

- Thêm ignore cho local cache và file môi trường dev.
- Thêm docker compose dev cho Redis và RabbitMQ.

### Ý nghĩa

Chuẩn bị môi trường để tiếp tục làm:

- Redis cho revoke/cache/rate limit
- RabbitMQ cho notification/event-driven

Phần RabbitMQ và Redis trong phiên hôm nay mới dừng ở hạ tầng nền, chưa triển khai full business flow.

---

## 13. Những kiểm thử đã chạy trong ngày

### Backend

- `dotnet restore`: pass
- `dotnet build BackE/ERMSystem.sln --no-restore`: pass
- smoke test auth: pass
- smoke test patient register/login/profile: pass
- smoke test `hospital-catalog`: pass

### Frontend

- `npm run lint`: pass
- `npm run build`: pass

### Database

- bootstrap `ERMSystemHospitalDb`: pass
- verify seed role tiếng Việt: pass
- verify số bảng theo schema: pass

---

## 14. Những gì chưa làm xong

Đây là các phần đã có nền nhưng chưa hoàn thiện:

- Patient portal mới chỉ ở mức tối thiểu.
- Booking page mới lấy danh mục dịch vụ thật, chưa ghi lịch hẹn thật vào `scheduling.Appointments`.
- `/doctors` chưa nối thật vào `org.StaffProfiles + org.DoctorProfiles + org.DoctorSchedules`.
- Redis chưa được tích hợp thật vào auth revoke/rate limit.
- RabbitMQ chưa có publisher/consumer nghiệp vụ đầu tiên.
- Hệ DB cũ và DB mới đang chạy song song, chưa refactor hết nghiệp vụ sang `ERMSystemHospitalDb`.

---

## 15. Gợi ý thứ tự làm tiếp cho dev

### Ưu tiên 1: Đặt lịch thật

Nên làm tiếp flow booking thật theo schema mới:

- danh sách chuyên khoa
- danh sách bác sĩ theo chuyên khoa
- lịch bác sĩ
- slot khám
- tạo `scheduling.Appointments`

### Ưu tiên 2: Patient portal thật

- lịch hẹn của tôi
- profile của tôi
- đơn thuốc của tôi
- hồ sơ khám của tôi

### Ưu tiên 3: Danh mục bác sĩ thật

Tạo API thật cho:

- hồ sơ bác sĩ
- lịch làm việc
- phòng khám
- chuyên khoa

### Ưu tiên 4: Dịch chuyển dần sang database mới

Refactor theo thứ tự:

1. identity
2. patient
3. scheduling
4. emr
5. billing

---

## 16. Kết luận ngắn

Phiên hôm nay không chỉ thêm vài endpoint nhỏ, mà đã tạo ra các nền tảng quan trọng:

- auth đúng hướng production hơn
- patient account riêng
- public website đúng chất bệnh viện tư hơn
- database đích đủ rộng cho bài toán thực tế
- backend đã đọc được database đích
- frontend đã bắt đầu dùng dữ liệu thật từ database mới

Nói ngắn gọn: hệ thống hiện đã có “đích kiến trúc”, “đích database” và “lát cắt tích hợp đầu tiên” để tiếp tục phát triển một cách có kiểm soát.
