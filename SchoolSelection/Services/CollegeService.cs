using Microsoft.EntityFrameworkCore;
using SchoolSelection.Data;
using SchoolSelection.Interfaces;
using SchoolSelection.Models;

namespace SchoolSelection.Services;

public class CollegeService
{
    private readonly CollegeDbContext _context;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<CollegeService> _logger;
    
    public CollegeService(CollegeDbContext context, IOpenAIService openAIService, ILogger<CollegeService> logger)
    {
        _context = context;
        _openAIService = openAIService;
        _logger = logger;
    }
    
    public async Task<string> GenerateRecommendationAsync(Guid collegeSelectionId)
    {
        try
        {
            // Get all colleges for this selection
            var colleges = await _context.Schools.Where(c => c.CollegeSelectionId == collegeSelectionId).ToListAsync();

            // Get all criteria for this selection
            var criteria = await _context.Criteria
                .Where(c => c.CollegeSelectionId == collegeSelectionId)
                .ToListAsync();

            if (!colleges.Any())
            {
                return "Error: No colleges found for this selection.";
            }

            if (!criteria.Any())
            {
                return "Error: No criteria found for this selection.";
            }

            // Prepare the prompt for AI
            var prompt = BuildPrompt(colleges, criteria);
            
            // Call AI service
            var recommendation = await _openAIService.RunChatGPTGetCompletionAsync(prompt);
            
            return recommendation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendation");
            return "Sorry, we encountered an error while generating your recommendation. Please try again later.";
        }
    }
    
    private string BuildPrompt(List<School> colleges, List<Criteria> criteria)
    {
        var collegeNames = string.Join(", ", colleges.Select(c => c.Name));
        var orderedCriteria = criteria
            .OrderByDescending(c => GetImportanceWeight(c.Importance))
            .Select(c => $"{c.Name} ({c.Importance} importance)");
        
        var criteriaList = string.Join("\n- ", orderedCriteria);

        return
            "**Role**: You are an expert college admissions counselor with 15+ years experience helping students find their ideal college match.\n" +
            "**Task**: Analyze the following colleges against the student's prioritized criteria to generate a personalized recommendation report.\n" +
            "**Student's College Options**:\n" +
            string.Join("\n", collegeNames) + "\n" +
            "**Student's Priority Criteria (Ranked)**:\n" +
            string.Join("\n", orderedCriteria.Select((c, i) => $"{i + 1}. {c}")) + "\n" +
            "**Required Analysis Format**:\n" +
            "1. **Comparative Analysis**:\n" +
            "   - For each top criterion, compare how each college performs\n" +
            "   - Use specific data points where available\n" +
            "   - Highlight notable differences\n\n" +
            "2. **Top Recommendations**:\n" +
            "   - Recommend 2-3 best matches\n" +
            "   - For each recommendation:\n" +
            "     * Explain how it aligns with top priorities\n" +
            "     * Note any compromise areas\n" +
            "     * Include one \"dark horse\" option if appropriate\n\n" +
            "3. **Decision Factors**:\n" +
            "   - 3 most important differentiating factors\n" +
            "   - 1 potential surprise consideration\n\n" +
            "4. **Next Steps**:\n" +
            "   - 2-3 actionable follow-up items\n" +
            "   - 1 question the student should ask themselves\n\n" +
            "**Tone Guidelines**:\n" +
            "- Professional but approachable\n" +
            "- Data-informed but not dry\n" +
            "- Balanced perspective (highlight pros/cons)\n" +
            "- Approximately 4-6 paragraphs\n\n" +
            "**Special Instructions**:\n" +
            "- Weight analysis heavily toward top 3 criteria\n" +
            "- Flag any criteria where no college stands out\n" +
            "- Include 1 unique insight about the selection\n";

    }

    private int GetImportanceWeight(string importance)
    {
        return importance switch
        {
            "Extremely" => 5,
            "Very" => 4,
            "Neutral" => 3,
            "Slightly" => 2,
            "Low" => 1,
            _ => 3
        };
    }
}