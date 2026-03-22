using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SchoolSelection.CommonEntities;

namespace SchoolSelection.Models;

public class Evaluation:EntityHelper
{
    [ForeignKey("CollegeSelectionId")]
    [Required]
    public Guid? CollegeSelectionId { get; set; }
    public CollegeSelection? CollegeSelection { get; set; }
    [ForeignKey("SchoolId")]
    [Required]
    public Guid? SchoolId { get; set; }
    public School? School { get; set; }
    [ForeignKey("CriteriaId")]
    [Required]
    public Guid? CriteriaId { get; set; }
    public Criteria? Criteria { get; set; }
    [Required]
    public string? Satisfaction { get; set; }
    public string? Notes { get; set; }
}