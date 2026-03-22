namespace SchoolSelection.Interfaces;

public interface IOpenAIService
{
    Task<string> RunChatGPTPromptAsync(string input);
    Task<string> RunChatGPTPromptSummaryAsync(string input);
    Task<string> RunChatGPTGetCompletionAsync(string input);
}