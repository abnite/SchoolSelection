using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSelection.CommonEntities;

namespace SchoolSelection.Models;

public class CollegeCriteriaSummary:EntityHelper
{
    [ForeignKey("CollegeSelectionId")]
    public Guid CollegeSelectionId { get; set; }
    public CollegeSelection CollegeSelection { get; set; }
    [Required]
    [ForeignKey("SchoolId")]
    public Guid SchoolId { get; set; }
    public School School { get; set; }

    [Required]
    [ForeignKey("CriteriaId")]
    public Guid CriteriaId { get; set; }
    public Criteria Criteria { get; set; }

    [Required]
    public string Summary { get; set; }

    [Required]
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;

    // Admin manually updated? If true, do not overwrite automatically
    public bool ManuallyUpdated { get; set; } = false;

    // Optional versioning if you want to track changes over time
    public int Version { get; set; } = 1;

    // Custom logic (optional) – you can use this in services/controllers
    [NotMapped]
    public bool NeedsRefresh => DateTime.UtcNow > LastChecked.AddYears(1) && !ManuallyUpdated;

}