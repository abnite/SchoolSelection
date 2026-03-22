using System.ComponentModel.DataAnnotations;
using SchoolSelection.CommonEntities;

namespace SchoolSelection.Models;

public class CollegeSelection:EntityHelper
{
    [Required]
    public string CollegeSelectionName { get; set; }
}