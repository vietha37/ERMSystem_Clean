# ERMSystem Backend Rules

## 1. Mục tiêu

Tài liệu này định nghĩa các quy tắc bắt buộc khi phát triển backend cho hệ thống bệnh viện tư `ERMSystem`.

Backend hiện tại đang đi theo Clean Architecture với cấu trúc chính:

```text
ERMSystem.API
ERMSystem.Application
ERMSystem.Domain
ERMSystem.Infrastructure
Infrastructure
```

Mục tiêu của các rule này là giúp Codex hoặc developer:

- Giữ đúng Clean Architecture.
- Phát triển tính năng bệnh viện một cách nhất quán.
- Tích hợp AI an toàn trong bối cảnh y tế.
- Chuẩn bị khả năng mở rộng sang microservice khi cần.
- Tránh đưa business logic vào sai layer.
- Tránh lộ dữ liệu nhạy cảm của bệnh nhân.

---

## 2. Kiến trúc tổng thể

Dự án backend phải được tổ chức theo các layer chính:

```text
ERMSystem.Domain
ERMSystem.Application
ERMSystem.Infrastructure
ERMSystem.API
```

Ý nghĩa từng project:

### `ERMSystem.Domain`

Chứa business core của hệ thống.

Bao gồm:

- Entities
- Value Objects
- Domain Events
- Domain Services
- Repository Interfaces nếu repository thuộc domain contract
- Business Rules
- Domain Exceptions
- Enum nghiệp vụ

Layer này không được phụ thuộc vào framework, database, API, AI SDK hoặc infrastructure.

### `ERMSystem.Application`

Chứa application logic và use case.

Bao gồm:

- Use Cases
- Commands / Queries
- DTOs
- Validators
- Application Services
- Interfaces / Ports
- Authorization Policies
- Transaction orchestration
- AI service contracts

Layer này được phép phụ thuộc vào `ERMSystem.Domain`, nhưng không phụ thuộc trực tiếp vào database, ORM, HTTP client, OpenAI SDK hoặc service bên ngoài.

### `ERMSystem.Infrastructure`

Chứa implementation kỹ thuật.

Bao gồm:

- Database context
- ORM configuration
- Repository implementations
- External API clients
- AI provider implementations
- Cache
- Message broker
- Email/SMS/Zalo notification
- File storage
- Background jobs
- Logging implementation

Layer này implement interface từ `ERMSystem.Application` hoặc `ERMSystem.Domain`.

### `ERMSystem.API`

Chứa presentation layer.

Bao gồm:

- Controllers
- Minimal API endpoints nếu có
- Middlewares
- Filters
- Request models
- Response models
- Authentication setup
- Dependency Injection configuration
- Swagger/OpenAPI configuration

Controller phải mỏng, không chứa business logic.

### `Infrastructure`

Thư mục này chỉ nên dùng cho hạ tầng ngoài source code, ví dụ:

- Docker
- Docker Compose
- Nginx
- Deployment scripts
- CI/CD files
- Terraform
- Kubernetes manifests
- Database seed scripts dùng cho môi trường

Nếu thư mục `Infrastructure` đang chứa code C#, hãy cân nhắc chuyển vào `ERMSystem.Infrastructure`.

---

## 3. Quy tắc dependency

Dependency bắt buộc:

```text
ERMSystem.Domain
  -> không phụ thuộc project nào

ERMSystem.Application
  -> ERMSystem.Domain

ERMSystem.Infrastructure
  -> ERMSystem.Application
  -> ERMSystem.Domain

ERMSystem.API
  -> ERMSystem.Application
  -> ERMSystem.Infrastructure
```

Không được làm:

```text
ERMSystem.Domain -> ERMSystem.Application
ERMSystem.Domain -> ERMSystem.Infrastructure
ERMSystem.Domain -> ERMSystem.API

ERMSystem.Application -> ERMSystem.Infrastructure
ERMSystem.Application -> ERMSystem.API
```

Rule bắt buộc:

- Domain không import Entity Framework.
- Domain không import ASP.NET Core.
- Domain không import AI SDK.
- Application không gọi database trực tiếp.
- Application không gọi HTTP client trực tiếp.
- API không gọi repository trực tiếp nếu có use case phù hợp.
- API không gọi AI provider trực tiếp.
- Infrastructure không được chứa business rule quan trọng.

---

## 4. Quy tắc đặt tên

### Project

Giữ tên project theo pattern:

```text
ERMSystem.API
ERMSystem.Application
ERMSystem.Domain
ERMSystem.Infrastructure
```

Nếu sau này tách service, dùng pattern:

```text
ERMSystem.PatientService.API
ERMSystem.PatientService.Application
ERMSystem.PatientService.Domain
ERMSystem.PatientService.Infrastructure
```

Hoặc nếu service độc lập hoàn toàn:

```text
PatientService.API
PatientService.Application
PatientService.Domain
PatientService.Infrastructure
```

### Use Case

Tên use case nên rõ hành động:

```text
CreatePatientUseCase
UpdatePatientProfileUseCase
BookAppointmentUseCase
CancelAppointmentUseCase
CreateMedicalRecordUseCase
GeneratePatientSummaryUseCase
AnalyzeLabResultUseCase
SuggestDiagnosisUseCase
```

### Repository Interface

```text
IPatientRepository
IDoctorRepository
IAppointmentRepository
IMedicalRecordRepository
IAIAnalysisRepository
```

### Service Interface

```text
IMedicalAIService
INotificationService
ICurrentUserService
IDateTimeProvider
IFileStorageService
IAuditLogService
```

### DTO

```text
CreatePatientRequest
CreatePatientResponse
BookAppointmentCommand
BookAppointmentResult
AnalyzeLabResultInput
AnalyzeLabResultOutput
```

---

## 5. Domain Layer Rules

Domain là nơi chứa nghiệp vụ cốt lõi.

Ví dụ domain chính của hệ thống bệnh viện tư:

- Patient
- Doctor
- Nurse
- Receptionist
- Department
- Appointment
- MedicalRecord
- Prescription
- LabTest
- LabResult
- Invoice
- Payment
- InsuranceClaim
- AIAnalysisResult
- AuditLog

Domain entity nên bảo vệ invariant của chính nó.

Ví dụ:

- Appointment không được book vào slot đã bị khóa.
- MedicalRecord không được chỉnh sửa nếu đã finalized, trừ role có quyền.
- Prescription phải có ít nhất một medication item.
- Patient phải có thông tin định danh hợp lệ.
- AIAnalysisResult phải có trạng thái review nếu ảnh hưởng quy trình y tế.

Không viết domain entity kiểu chỉ có getter/setter nếu entity có business rule.

Không để domain phụ thuộc vào database id nếu có thể dùng value object rõ nghĩa.

Gợi ý value object:

```text
PatientId
DoctorId
AppointmentId
PhoneNumber
EmailAddress
Money
DateRange
TimeSlot
MedicalRecordCode
InsuranceNumber
```

---

## 6. Application Layer Rules

Application layer điều phối use case.

Một use case nên:

- Nhận input rõ ràng.
- Validate nghiệp vụ cấp application.
- Load entity qua repository interface.
- Gọi domain method để xử lý business rule.
- Gọi service interface nếu cần.
- Lưu thay đổi qua repository/unit of work.
- Trả output DTO.

Use case không được:

- Gọi Entity Framework trực tiếp.
- Gọi DbContext trực tiếp.
- Gọi HTTP API trực tiếp.
- Gọi OpenAI SDK trực tiếp.
- Trả ORM entity ra ngoài.
- Chứa logic mapping database.

Ví dụ flow đúng:

```text
Controller
  -> BookAppointmentUseCase
    -> IAppointmentRepository
    -> IDoctorRepository
    -> Appointment.Book(...)
    -> IUnitOfWork.SaveChangesAsync()
```

---

## 7. Infrastructure Layer Rules

Infrastructure chịu trách nhiệm kỹ thuật.

Cho phép:

- Entity Framework Core
- Dapper
- Redis
- RabbitMQ/Kafka
- OpenAI/Azure OpenAI client
- SMTP/SMS/Zalo client
- Cloud storage SDK
- Background worker
- External hospital/lab/pharmacy API

Không được:

- Đặt business rule quan trọng trong repository implementation.
- Để controller gọi thẳng infrastructure service nếu đã có use case.
- Trả ORM model lên application nếu application đang dùng domain entity.
- Hard-code secret, connection string, API key.

Repository implementation phải mapping rõ giữa persistence model và domain entity nếu hai model tách nhau.

---

## 8. API Layer Rules

API layer chỉ xử lý HTTP.

Controller được phép:

- Nhận request.
- Validate request format cơ bản.
- Gọi use case/application service.
- Map result sang response.
- Trả status code phù hợp.

Controller không được:

- Chứa business logic.
- Gọi DbContext.
- Gọi repository trực tiếp.
- Gọi AI SDK.
- Tự xử lý transaction nghiệp vụ.
- Log dữ liệu y tế nhạy cảm.

Response lỗi phải thống nhất.

Ví dụ:

```json
{
  "success": false,
  "error": {
    "code": "APPOINTMENT_SLOT_UNAVAILABLE",
    "message": "Khung giờ khám không còn khả dụng."
  }
}
```

---

## 9. AI Feature Rules

Hệ thống có thể phát triển tính năng AI, nhưng AI trong y tế phải được kiểm soát chặt chẽ.

AI có thể dùng cho:

- Tóm tắt hồ sơ bệnh án.
- Phân tích kết quả xét nghiệm.
- Gợi ý câu hỏi cho bác sĩ.
- Gợi ý chẩn đoán ban đầu.
- Phân loại mức độ ưu tiên bệnh nhân.
- Chatbot hỗ trợ bệnh nhân.
- Trích xuất dữ liệu từ tài liệu y tế.
- Tạo báo cáo nội bộ.
- Tự động hóa chăm sóc khách hàng.

AI không được:

- Đưa ra quyết định y tế cuối cùng.
- Tự động kê đơn.
- Tự động thay đổi hồ sơ bệnh án đã xác nhận.
- Tự động từ chối điều trị.
- Tự động kết luận bệnh nghiêm trọng mà không có bác sĩ xác nhận.

Quy tắc bắt buộc:

- AI output phải được ghi rõ là gợi ý tham khảo.
- Với tác vụ y tế quan trọng, cần có human review.
- Không gửi dữ liệu nhạy cảm sang AI provider nếu chưa có cơ chế bảo vệ.
- Cần masking/anonymization khi phù hợp.
- Phải validate output bằng schema khi AI trả dữ liệu có cấu trúc.
- Phải có fallback khi AI provider lỗi.
- Phải log audit cho AI request quan trọng.

Thông tin nên audit:

- User yêu cầu.
- Thời gian yêu cầu.
- Feature AI được dùng.
- Model/provider.
- Prompt version.
- Input đã được masking nếu có.
- Output.
- Trạng thái review.
- Người xác nhận nếu có.

---

## 10. AI Architecture Rules

Không gọi AI SDK trực tiếp từ use case hoặc controller.

Phải tạo interface trong Application:

```csharp
public interface IMedicalAIService
{
    Task<SummarizePatientHistoryResult> SummarizePatientHistoryAsync(
        SummarizePatientHistoryInput input,
        CancellationToken cancellationToken);

    Task<AnalyzeLabResultResult> AnalyzeLabResultAsync(
        AnalyzeLabResultInput input,
        CancellationToken cancellationToken);

    Task<SuggestDiagnosisResult> SuggestDiagnosisAsync(
        SuggestDiagnosisInput input,
        CancellationToken cancellationToken);
}
```

Implementation nằm ở Infrastructure:

```text
ERMSystem.Infrastructure/AI/OpenAIMedicalAIService.cs
ERMSystem.Infrastructure/AI/AzureOpenAIMedicalAIService.cs
ERMSystem.Infrastructure/AI/PromptTemplateProvider.cs
ERMSystem.Infrastructure/AI/AIOutputValidator.cs
```

Use case chỉ gọi `IMedicalAIService`.

---

## 11. Prompt Rules

Prompt dùng cho y tế phải an toàn.

Prompt nên yêu cầu:

- Chỉ phân tích dựa trên input.
- Không bịa thông tin.
- Không đưa ra kết luận cuối cùng.
- Nêu rõ dữ liệu còn thiếu.
- Trả output theo JSON schema nếu dùng trong hệ thống.
- Nhắc rằng kết quả cần bác sĩ xác nhận.

Ví dụ:

```text
Bạn là trợ lý AI hỗ trợ bác sĩ trong bệnh viện tư.
Bạn không được đưa ra chẩn đoán cuối cùng.
Chỉ phân tích dựa trên dữ liệu được cung cấp.
Nếu thiếu dữ liệu, hãy nêu rõ dữ liệu còn thiếu.
Kết quả chỉ mang tính tham khảo và phải được bác sĩ xác nhận.
Trả về JSON theo schema được yêu cầu.
```

---

## 12. Microservice Rules

Không tách microservice quá sớm.

Chỉ đề xuất microservice khi có một hoặc nhiều điều kiện:

- Domain đủ lớn.
- Team cần deploy độc lập.
- Service cần scale riêng.
- Có boundary nghiệp vụ rõ ràng.
- Có yêu cầu bảo mật hoặc vận hành riêng.
- AI workload cần tách riêng khỏi backend chính.

Service có thể tách trong tương lai:

```text
patient-service
appointment-service
medical-record-service
billing-service
notification-service
ai-service
auth-service
reporting-service
```

Nếu tách microservice:

- Mỗi service sở hữu database/schema riêng.
- Không service nào truy cập trực tiếp database của service khác.
- Dùng API hoặc event để giao tiếp.
- Có timeout/retry/circuit breaker.
- Có idempotency cho command/event quan trọng.
- Có correlation id cho tracing.
- Có versioning cho API/event.
- Có contract rõ ràng.

Event gợi ý:

```text
PatientCreated
AppointmentBooked
AppointmentCancelled
MedicalRecordCreated
PrescriptionCreated
InvoicePaid
LabResultUploaded
AIAnalysisRequested
AIAnalysisCompleted
```

---

## 13. Security Rules

Dữ liệu bệnh viện là dữ liệu nhạy cảm.

Bắt buộc:

- Không log thông tin nhạy cảm nếu không cần.
- Mask số điện thoại, email, mã bệnh nhân trong log nếu phù hợp.
- Không hard-code secret.
- Dùng environment variables hoặc secret manager.
- Validate input.
- Kiểm tra authorization theo role.
- Audit log thao tác với hồ sơ bệnh án.
- Không trả stack trace ra client production.
- Dùng HTTPS.
- Cẩn thận với file upload.
- Kiểm tra phân quyền khi truy cập hồ sơ bệnh nhân.

Role gợi ý:

```text
Admin
HospitalManager
Doctor
Nurse
Receptionist
LabTechnician
Pharmacist
Patient
AIReviewer
```

---

## 14. Testing Rules

Ưu tiên test:

1. Domain unit tests
2. Application use case tests
3. Infrastructure integration tests
4. API tests
5. AI workflow tests

Test bắt buộc cho:

- Business rule quan trọng.
- Use case có nhiều nhánh.
- Permission/authorization.
- AI output schema validation.
- Payment/billing.
- Medical record modification.
- Appointment booking conflict.

AI test không nên assert nội dung tuyệt đối từ model. Nên test:

- Schema hợp lệ.
- Có fallback.
- Có safety disclaimer.
- Không crash khi provider lỗi.
- Không gửi field nhạy cảm khi masking bật.

---

## 15. Database Rules

- Dùng migration có version rõ ràng.
- Không sửa database production thủ công nếu không có quy trình.
- Dùng transaction cho nghiệp vụ cần atomicity.
- Không hard delete dữ liệu y tế quan trọng nếu không có yêu cầu rõ.
- Medical record, invoice, audit log nên có lịch sử thay đổi.
- CreatedAt, UpdatedAt, CreatedBy, UpdatedBy nên có nếu phù hợp.
- Dữ liệu nhạy cảm cần cân nhắc encryption.

---

## 16. Codex Working Rules

Khi Codex sửa hoặc tạo code:

1. Đọc cấu trúc project trước.
2. Xác định đúng layer cần sửa.
3. Không tự ý đổi kiến trúc.
4. Không xóa file nếu không được yêu cầu.
5. Không đổi public contract nếu không cần.
6. Không đưa business logic vào controller.
7. Không đưa infrastructure concern vào domain.
8. Nếu thiếu dependency, tạo interface trước.
9. Nếu thêm AI feature, tạo port ở Application và implementation ở Infrastructure.
10. Nếu thêm API endpoint, phải gọi use case.
11. Nếu thêm business rule, thêm test.
12. Nếu có thay đổi bảo mật, nêu rõ trong summary.

Output sau khi làm task nên gồm:

```text
Summary
Changed Files
Architecture Notes
How to Test
Security/AI Notes
```

---

## 17. Quy uoc tieng Viet

Du an nay uu tien chuan hoa bang tieng Viet cho:

- Giao dien frontend
- Noi dung hien thi
- Tai lieu nghiep vu
- Mo ta API
- Thong bao, validation message, log nghiep vu hien thi cho nguoi dung
- Seed data nghiep vu

Ngoai le:

- Ten thuoc, hoat chat, ky hieu y khoa, ma chuan, ten cong nghe, ten package, ten framework
- Cac ten ky thuat khong nen doi neu se gay vo ecosystem hoac kho bao tri

Mac dinh khi phat trien:

- Text huong nguoi dung phai dung tieng Viet ro nghia, nhat quan
- Uu tien thuat ngu y te pho bien tai Viet Nam
- Neu mot ten khong the Viet hoa hop ly, giu nguyen ten goc
