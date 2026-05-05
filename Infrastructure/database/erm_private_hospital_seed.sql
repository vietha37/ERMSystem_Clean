SET NOCOUNT ON;
GO

/*
  Du lieu mau nghiep vu cho he thong benh vien tu ERM.
  Uu tien ten goi va noi dung bang tieng Viet, giu nguyen ten thuoc va ma chuan khi can.
*/

INSERT INTO org.Departments (Id, DepartmentCode, Name, Description)
SELECT NEWID(), 'OPD', N'Khối khám ngoại trú', N'Tiếp nhận và điều phối khám ngoại trú'
WHERE NOT EXISTS (SELECT 1 FROM org.Departments WHERE DepartmentCode = 'OPD');

INSERT INTO org.Departments (Id, DepartmentCode, Name, Description)
SELECT NEWID(), 'LAB', N'Trung tâm xét nghiệm', N'Vận hành các dịch vụ xét nghiệm và lấy mẫu'
WHERE NOT EXISTS (SELECT 1 FROM org.Departments WHERE DepartmentCode = 'LAB');

INSERT INTO org.Departments (Id, DepartmentCode, Name, Description)
SELECT NEWID(), 'IMG', N'Chẩn đoán hình ảnh', N'Các dịch vụ X-quang, CT, MRI, siêu âm'
WHERE NOT EXISTS (SELECT 1 FROM org.Departments WHERE DepartmentCode = 'IMG');

INSERT INTO org.Departments (Id, DepartmentCode, Name, Description)
SELECT NEWID(), 'PHA', N'Nhà thuốc bệnh viện', N'Quản lý cấp phát thuốc và tồn kho'
WHERE NOT EXISTS (SELECT 1 FROM org.Departments WHERE DepartmentCode = 'PHA');
GO

DECLARE @OpdDepartmentId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM org.Departments WHERE DepartmentCode = 'OPD');
DECLARE @LabDepartmentId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM org.Departments WHERE DepartmentCode = 'LAB');
DECLARE @ImgDepartmentId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM org.Departments WHERE DepartmentCode = 'IMG');
GO

INSERT INTO org.Specialties (Id, SpecialtyCode, Name, DepartmentId)
SELECT NEWID(), 'CARD', N'Tim mạch', (SELECT TOP 1 Id FROM org.Departments WHERE DepartmentCode = 'OPD')
WHERE NOT EXISTS (SELECT 1 FROM org.Specialties WHERE SpecialtyCode = 'CARD');

INSERT INTO org.Specialties (Id, SpecialtyCode, Name, DepartmentId)
SELECT NEWID(), 'OBGYN', N'Sản phụ khoa', (SELECT TOP 1 Id FROM org.Departments WHERE DepartmentCode = 'OPD')
WHERE NOT EXISTS (SELECT 1 FROM org.Specialties WHERE SpecialtyCode = 'OBGYN');

INSERT INTO org.Specialties (Id, SpecialtyCode, Name, DepartmentId)
SELECT NEWID(), 'PED', N'Nhi khoa', (SELECT TOP 1 Id FROM org.Departments WHERE DepartmentCode = 'OPD')
WHERE NOT EXISTS (SELECT 1 FROM org.Specialties WHERE SpecialtyCode = 'PED');

INSERT INTO org.Specialties (Id, SpecialtyCode, Name, DepartmentId)
SELECT NEWID(), 'GEN', N'Nội tổng quát', (SELECT TOP 1 Id FROM org.Departments WHERE DepartmentCode = 'OPD')
WHERE NOT EXISTS (SELECT 1 FROM org.Specialties WHERE SpecialtyCode = 'GEN');
GO

INSERT INTO org.Clinics (Id, ClinicCode, Name, DepartmentId, FloorLabel, RoomLabel)
SELECT NEWID(), 'CLN-01', N'Phòng khám tổng quát 01', (SELECT TOP 1 Id FROM org.Departments WHERE DepartmentCode = 'OPD'), N'Tầng 2', N'P201'
WHERE NOT EXISTS (SELECT 1 FROM org.Clinics WHERE ClinicCode = 'CLN-01');

INSERT INTO org.Clinics (Id, ClinicCode, Name, DepartmentId, FloorLabel, RoomLabel)
SELECT NEWID(), 'CLN-02', N'Phòng khám tim mạch', (SELECT TOP 1 Id FROM org.Departments WHERE DepartmentCode = 'OPD'), N'Tầng 2', N'P205'
WHERE NOT EXISTS (SELECT 1 FROM org.Clinics WHERE ClinicCode = 'CLN-02');

INSERT INTO org.Clinics (Id, ClinicCode, Name, DepartmentId, FloorLabel, RoomLabel)
SELECT NEWID(), 'LAB-01', N'Khu lấy mẫu xét nghiệm', (SELECT TOP 1 Id FROM org.Departments WHERE DepartmentCode = 'LAB'), N'Tầng 1', N'P103'
WHERE NOT EXISTS (SELECT 1 FROM org.Clinics WHERE ClinicCode = 'LAB-01');

INSERT INTO org.Clinics (Id, ClinicCode, Name, DepartmentId, FloorLabel, RoomLabel)
SELECT NEWID(), 'IMG-01', N'Khu siêu âm và X-quang', (SELECT TOP 1 Id FROM org.Departments WHERE DepartmentCode = 'IMG'), N'Tầng 1', N'P110'
WHERE NOT EXISTS (SELECT 1 FROM org.Clinics WHERE ClinicCode = 'IMG-01');
GO

INSERT INTO billing.ServiceCatalog (Id, ServiceCode, Name, Category, UnitPrice)
SELECT NEWID(), 'CONS-GEN', N'Khám nội tổng quát', N'Consultation', 250000
WHERE NOT EXISTS (SELECT 1 FROM billing.ServiceCatalog WHERE ServiceCode = 'CONS-GEN');

INSERT INTO billing.ServiceCatalog (Id, ServiceCode, Name, Category, UnitPrice)
SELECT NEWID(), 'CONS-CARD', N'Khám tim mạch chuyên sâu', N'Consultation', 450000
WHERE NOT EXISTS (SELECT 1 FROM billing.ServiceCatalog WHERE ServiceCode = 'CONS-CARD');

INSERT INTO billing.ServiceCatalog (Id, ServiceCode, Name, Category, UnitPrice)
SELECT NEWID(), 'LAB-CBC', N'Tổng phân tích tế bào máu ngoại vi', N'Laboratory', 120000
WHERE NOT EXISTS (SELECT 1 FROM billing.ServiceCatalog WHERE ServiceCode = 'LAB-CBC');

INSERT INTO billing.ServiceCatalog (Id, ServiceCode, Name, Category, UnitPrice)
SELECT NEWID(), 'IMG-US-ABD', N'Siêu âm ổ bụng tổng quát', N'Imaging', 280000
WHERE NOT EXISTS (SELECT 1 FROM billing.ServiceCatalog WHERE ServiceCode = 'IMG-US-ABD');
GO

INSERT INTO lab.LabServices (Id, ServiceCode, Name, SampleType, UnitPrice)
SELECT NEWID(), 'LAB-CBC', N'Tổng phân tích tế bào máu ngoại vi', N'Máu toàn phần', 120000
WHERE NOT EXISTS (SELECT 1 FROM lab.LabServices WHERE ServiceCode = 'LAB-CBC');

INSERT INTO lab.LabServices (Id, ServiceCode, Name, SampleType, UnitPrice)
SELECT NEWID(), 'LAB-GLU', N'Đường huyết lúc đói', N'Huyết thanh', 60000
WHERE NOT EXISTS (SELECT 1 FROM lab.LabServices WHERE ServiceCode = 'LAB-GLU');
GO

INSERT INTO imaging.ImagingServices (Id, ServiceCode, Name, Modality, UnitPrice)
SELECT NEWID(), 'IMG-US-ABD', N'Siêu âm ổ bụng tổng quát', N'Ultrasound', 280000
WHERE NOT EXISTS (SELECT 1 FROM imaging.ImagingServices WHERE ServiceCode = 'IMG-US-ABD');

INSERT INTO imaging.ImagingServices (Id, ServiceCode, Name, Modality, UnitPrice)
SELECT NEWID(), 'IMG-XR-CHEST', N'X-quang ngực thẳng', N'XRay', 180000
WHERE NOT EXISTS (SELECT 1 FROM imaging.ImagingServices WHERE ServiceCode = 'IMG-XR-CHEST');
GO

INSERT INTO pharmacy.Medicines (Id, DrugCode, Name, GenericName, Strength, DosageForm, Unit)
SELECT NEWID(), 'MED-PAR-500', N'Paracetamol 500mg', N'Paracetamol', N'500mg', N'Viên nén', N'Viên'
WHERE NOT EXISTS (SELECT 1 FROM pharmacy.Medicines WHERE DrugCode = 'MED-PAR-500');

INSERT INTO pharmacy.Medicines (Id, DrugCode, Name, GenericName, Strength, DosageForm, Unit)
SELECT NEWID(), 'MED-AMO-500', N'Amoxicillin 500mg', N'Amoxicillin', N'500mg', N'Viên nang', N'Viên'
WHERE NOT EXISTS (SELECT 1 FROM pharmacy.Medicines WHERE DrugCode = 'MED-AMO-500');
GO

