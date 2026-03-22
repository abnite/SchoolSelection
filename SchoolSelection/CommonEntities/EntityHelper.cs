using System.ComponentModel.DataAnnotations;
using SchoolSelection.Data;

namespace SchoolSelection.CommonEntities;

public class EntityHelper
{
    [Key]
    public Guid Id { get; set; }
    public DateTime? DateAdded { get; set; }
      public  ApplicationUser? AddedBy { get; set; }
    public Boolean? IsDeleted { get; set; }
    public DateTime? DateDeleted { get; set; }
    public ApplicationUser? DeletedBy { get; set; }
    public string? Token { get; set; }

    public EntityHelper()
    {
        DateAdded = DateTime.UtcNow;
        IsDeleted = false;
    }
}