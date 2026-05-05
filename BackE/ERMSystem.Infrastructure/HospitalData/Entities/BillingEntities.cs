using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERMSystem.Infrastructure.HospitalData.Entities;

[Table("ServiceCatalog", Schema = "billing")]
public class HospitalServiceCatalogEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string ServiceCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
}
