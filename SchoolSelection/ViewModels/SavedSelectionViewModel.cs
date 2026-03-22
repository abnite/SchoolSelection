namespace SchoolSelection.ViewModels;

public class SavedSelectionViewModel
{
    public Guid    Id                  { get; set; }
    public string CollegeSelectionName { get; set; }
    public DateTime DateAdded         { get; set; }
    public bool   IsCompleted         { get; set; }
}