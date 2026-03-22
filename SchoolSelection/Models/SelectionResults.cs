using System.ComponentModel.DataAnnotations.Schema;
using SchoolSelection.CommonEntities;

namespace SchoolSelection.Models;

public class SelectionResults:EntityHelper
{
    [ForeignKey("CollegeSelectionId")]
    public Guid? CollegeSelectionId { get; set; }
    public CollegeSelection? CollegeSelection { get; set; }
    public string? Result { get; set; }
}