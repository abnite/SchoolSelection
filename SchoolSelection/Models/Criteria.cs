using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSelection.CommonEntities;

namespace SchoolSelection.Models;

public class Criteria:EntityHelper
{
    [ForeignKey("CollegeSelectionId")]
    [Required]
    public Guid? CollegeSelectionId { get; set; }
    public CollegeSelection? CollegeSelection { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Importance { get; set; }
}