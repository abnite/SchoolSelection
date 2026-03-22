using SchoolSelection.Models;

namespace SchoolSelection.ViewModels;

public class EvaluationViewModel
{
    public List<Evaluation> SubmittedEvaluations { get; set; }
    public Guid SchoolId { get; set; }
    public Guid CriteriaId { get; set; }
    public Guid CollegeSelectionId { get; set; }
    public string? Satisfaction { get; set; }
    public string? Notes { get; set; }
    
    public List<SummaryDisplayViewModel> GeneratedSummaries { get; set; } = new();

    
    public List<SchoolEvaluationViewModel> SchoolEvaluations { get; set; }
}

public class SchoolEvaluationViewModel
{
    public string SchoolName { get; set; }
    public List<string> CompletedCriteria { get; set; }
    public bool IsComplete { get; set; }
}