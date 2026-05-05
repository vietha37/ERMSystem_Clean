using System;
using System.Collections.Generic;

namespace ERMSystem.Application.DTOs
{
    public class HospitalCatalogOverviewDto
    {
        public IReadOnlyList<HospitalDepartmentDto> Departments { get; set; } = Array.Empty<HospitalDepartmentDto>();
        public IReadOnlyList<HospitalSpecialtyDto> Specialties { get; set; } = Array.Empty<HospitalSpecialtyDto>();
        public IReadOnlyList<HospitalClinicDto> Clinics { get; set; } = Array.Empty<HospitalClinicDto>();
        public IReadOnlyList<HospitalServiceCatalogDto> Services { get; set; } = Array.Empty<HospitalServiceCatalogDto>();
    }

    public class HospitalDepartmentDto
    {
        public Guid Id { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class HospitalSpecialtyDto
    {
        public Guid Id { get; set; }
        public string SpecialtyCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public bool IsActive { get; set; }
    }

    public class HospitalClinicDto
    {
        public Guid Id { get; set; }
        public string ClinicCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? FloorLabel { get; set; }
        public string? RoomLabel { get; set; }
        public bool IsActive { get; set; }
    }

    public class HospitalServiceCatalogDto
    {
        public Guid Id { get; set; }
        public string ServiceCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; }
    }
}
