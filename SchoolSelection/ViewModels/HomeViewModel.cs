namespace SchoolSelection.ViewModels;

public class HomeViewModel
{
    public string  PlanType { get; set; }
    public int RemainingSelections { get; set; }
    public string CollegeSelectionId  { get; set; }
    public List<SavedSelectionViewModel> SavedSelections  { get; set; } = new();
}