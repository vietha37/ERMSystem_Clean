# Báo Cáo Kiến Trúc & Luồng Hoạt Động Backend (ERMSystem)

Dựa trên mã nguồn của dự án thư mục `ERMSystem`, backend đang được xây dựng theo mô hình **Clean Architecture** (Kiến trúc Sạch) kết hợp với **ASP.NET Core Web API**. 

Dưới đây là phần giải thích chi tiết từ cấu trúc thư mục, sự liên kết giữa các file đến luồng hoạt động cụ thể của một request từ lúc nhận vào cho đến khi trả về lại cho người dùng.

---

## 1. Tổng quan Cấu trúc Thư mục & Sự Liên Quan

Dự án được chia thành 4 dự án con (tương ứng với 4 layer của Clean Architecture). Việc chia nhỏ này giúp tách biệt rõ ràng Business Logic (logic nghiệp vụ) khỏi các công nghệ cụ thể như Database hay UI.

### 1.1. `ERMSystem.Domain` (Tầng Cốt Lõi)
Tầng này chứa các thực thể (Entities) định nghĩa dữ liệu cốt lõi của hệ thống. Nó **không phụ thuộc vào bất kỳ tầng nào khác**.
- **Thư mục liên quan:** `Entities`
- **File tiêu biểu:** `Patient.cs`, `Doctor.cs`, `Appointment.cs`, `Medicine.cs`, v.v.
- **Vai trò:** Định nghĩa các đối tượng thực tế trong bệnh viện (Bệnh nhân, Bác sĩ, Đơn thuốc, Lịch hẹn...).

### 1.2. `ERMSystem.Application` (Tầng Ứng Dụng)
Tầng này chứa các logic nghiệp vụ (Business Rules). Nó phụ thuộc vào `ERMSystem.Domain` nhưng không biết các dữ liệu được lưu ở đâu hay trả về cho client như thế nào.
- **Thư mục liên quan:**
  - `DTOs/` (Data Transfer Objects): Định nghĩa các model dùng để nhận dữ liệu từ Client hoặc trả về cho Client (ví dụ: `CreatePatientDto`, `PatientDto`). Giúp ẩn đi cấu trúc thực sự của Table trong DB.
  - `Interfaces/`: Chứa các Abstract/Interface cho Service và Repository (`IPatientService`, `IPatientRepository`). Việc dùng interface giúp dự án dễ dàng viết mock test và hỗ trợ Dependency Injection.
  - `Services/`: Chứa các class thực thi logic nghiệp vụ (ví dụ: `PatientService`). Nó sẽ gọi đến các Interfaces của Repository để lấy/xử lý dữ liệu.

### 1.3. `ERMSystem.Infrastructure` (Tầng Cơ Sở Hạ Tầng)
Tầng này chịu trách nhiệm giao tiếp với thế giới bên ngoài, cụ thể ở đây là thao tác trực tiếp với Database (SQL Server thông qua Entity Framework Core) hoặc các dịch vụ bên thứ ba.
- **Thư mục liên quan:**
  - `Data/`: Chứa `ApplicationDbContext.cs` – là cấu hình Entity Framework Core kết nối code và database.
  - `Repositories/`: Chứa các class thực thi (`PatientRepository`, `DoctorRepository`...) implements lại các Interfaces (`IPatientRepository`) được định nghĩa ở tầng Application. Các file này sẽ trực tiếp gọi cơ sở dữ liệu để thêm/sửa/xóa/với DbContext.

### 1.4. `ERMSystem.API` (Tầng Giao Diện/Trình Diễn - Presentation)
Đây là cổng giao tiếp của toàn bộ Backend với Client (Web, Mobile). Nó chỉ phụ thuộc vào `ERMSystem.Application` và dùng để nhận các HTTP Request.
- **Thư mục liên quan:**
  - `Controllers/`: Chứa các file như `PatientsController.cs`, `AuthController.cs`. Controller sẽ bắt các Endpoint (API URL), kiểm tra tính hợp lệ cơ bản của dữ liệu và đẩy xuống cho Service (tầng Application) xử lý.
  - `Program.cs`: Nơi khởi chạy ứng dụng, đăng ký Dependency Injection (DI), cấu hình Authentication (JWT), kết nối DB và thiết lập cấu hình Middleware Pipeline.

---

## 2. Luồng Hoạt Động Của Hệ Thống (Request Flow)

Khi một Client (frontend) gọi tới một API, ví dụ `GET /api/patients`, hệ thống sẽ xử lý theo một đường thẳng (Pipeline) theo trình tự dưới đây.

**Sơ đồ luồng đi cơ bản:**
`Client` ➔ `API Controller` ➔ `Application Service` ➔ `Infrastructure Repository` ➔ `Database` (và ngược lại lúc trả kết quả).

### Bước 1: Routing & Middleware (`Program.cs`)
- Request HTTP gửi lên server.
- Trong `Program.cs`, middleware sẽ kiểm tra xem request này có hợp lệ không (ví dụ kiểm tra Token JWT có chính xác không tại `app.UseAuthentication()`).
- Nếu cấu trúc đúng, hệ thống sẽ định tuyến (Route) request đó đến đúng Controller.

### Bước 2: Request vào tới `API Controller`
- Giả sử Client gọi API `GET /api/patients`. Code chạy vào file `PatientsController.cs`.
- Controller sẽ nhận Request và các tham số (ví dụ truyền lên page, pageSize thông qua `PaginationRequest`).
- Controller không chứa logic tính toán. Nó gọi tới một phương thức tương ứng của `IPatientService` (đã được Inject vào Controller thông qua constructor).
- *Ví dụ code:* `var result = await _patientService.GetAllPatientsAsync(request, ct);`

### Bước 3: Xử lý Logic tại `Application Service`
- Request đi tới `PatientService.cs` nằm trong `ERMSystem.Application/Services`.
- Service xử lý các luật logic nghiệp vụ. Tại đây, Service cần dữ liệu nên nó sẽ gọi Interface `IPatientRepository` (cũng được Inject sẵn).
- Service chỉ biết gọi Interface, nó không quan tâm dữ liệu lấy ra từ SQL, RAM hay file text.
- *Ví dụ code:* `return await _patientRepository.GetAllAsync(request, ct);`

### Bước 4: Tương tác Database tại `Infrastructure Repository`
- Implementation thực sự là file `PatientRepository.cs` nằm ở `ERMSystem.Infrastructure/Repositories/`.
- File này có chứa biến `_context` (tức `ApplicationDbContext`). Nó sẽ gọi thư viện Entity Framework Core đẩy câu query xuống thẳng SQL Server.
- *Hành động:* Lấy các bản ghi dạng Entity (`Patient`) từ Database lên server.

### Bước 5: Phản hồi ngược (Response)
- **Từ DbContext ➔ Repository:** Trả về danh sách Entity `Patient`.
- **Từ Repository ➔ Service:** Service nhận Object `Patient`, sau đó mapping (chuyển đổi) object này sang một `PatientDto` (ẩn đi những trường tĩnh không cần thiết). Trả về cho Controller.
- **Thuận Controller ➔ Client:** Controller gói `PatientDto` lại vào trong 1 HTTP Response (như `Ok(result)` tức là HTTP Code `200`). Trả kết quả JSON về cho Client/Mobile.

---

## 3. Quản Lý Phụ Thuộc (Dependency Injection ngầm)

Hệ thống kết nối các file lại với nhau như thế nào nếu `Controller` ở tầng API chỉ biết `Interface` ở tầng Application mà không biết code `Service`/`Repository` chi tiết ở đâu?

Cấu hình bí mật nằm ở file **`Program.cs`** trong tầng `ERMSystem.API`:
```csharp
// ── DI – Patient ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();
```
Khi ASP.NET chạy Controller, nó thấy bạn yêu cầu `IPatientService`. Nó sẽ nhìn vào config này và tự động "bơm" (Inject) cái ruột thật sự là class `PatientService` vào. Điều này giúp các file độc lập và cực kỳ ít dính chặt (Coupling) với nhau.

---

## Tóm Kết

Backend `ERMSystem` là một hệ thống chặt chẽ, an toàn và dễ bảo trì nhờ quy tắc phân định ranh giới Clean Architecture:
1. **API** nhận giao tiếp HTTP.
2. **Application** làm não bộ xử lý nghiệp vụ, kiểm soát DTOs.
3. **Domain** là các mẫu Data đại diện cho cốt lõi hệ thống Y tế.
4. **Infrastructure** chuyên tâm làm bốc vác, giao tiếp với các dịch vụ Database, File IO hay Email bên dưới.

Request --------> Controller (API) --------> Interfaces (Application) --------> Services (Application) --------> Repositories (Infrastructure) --------> Database