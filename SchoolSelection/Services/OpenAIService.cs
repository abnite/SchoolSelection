using OpenAI;
using OpenAI.Models;
using OpenAI.Chat;
using SchoolSelection.Interfaces;

namespace SchoolSelection.Services;

public class OpenAIService:IOpenAIService
{
    private readonly string _openAiApiKey;
    private readonly IHttpClientFactory _httpClientFactory;
    
    public HttpClient httpClient { get; }
    public OpenAIService(string openAiApiKey)
    {
        _openAiApiKey = openAiApiKey;
       // _httpClientFactory = httpClientFactory;
        
        
        httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(600);
        
    }
    
    public async Task<string> RunChatGPTPromptAsync(string input)
    {
        try
        {


            string outputResult = "";
            var api = new OpenAIClient(_openAiApiKey, null, httpClient);
            var messages = new List<Message>
            {
                //  new Message(Role.System, "You are an academic Advisor."),
                new Message(Role.System, "\nYou are a College Adviser."),
                new Message(Role.User, "You are an AI College Adviser with the task of helping a student choose the best college based on their preferences. You will receive multiple college names along with specific criteria, the importance of each criterion, and the satisfaction score for each criterion. Based on this information, you will provide a recommendation for the best college, including detailed reasons for your choice. You may also use current college data available to enhance your recommendation. Provide only one choice with a reason for the selection. Ensure that the student's preferences are always considered first, but you may include additional relevant factors in your reasoning"),
                new Message(Role.User, " Find below the colleges and their criteria "),
                new Message(Role.User, input+"\n"),
            };

            var chatRequest = new ChatRequest(messages, Model.GPT3_5_Turbo_16K, number: 1);

            var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
            outputResult = result.FirstChoice.Message.Content.ToString();
            // Replace newlines with <br> or wrap in <p> tags
            outputResult = outputResult.Replace("\n\n", "</p><p>").Replace("\n", "<br>");
            outputResult = "<p>" + outputResult + "</p>"; // Ensure the content is wrapped in <p> tags


            return outputResult;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
    public async Task<string> RunChatGPTPromptSummaryAsync(string input)
    {
        try
        {


            string outputResult = "";
            var api = new OpenAIClient(_openAiApiKey, null, httpClient);
            var messages = new List<Message>
            {
                //  new Message(Role.System, "You are an academic Advisor."),
                new Message(Role.System, "\nYou are a College Adviser."),
                new Message(Role.User, input+"\n"),
            };

            var chatRequest = new ChatRequest(messages, Model.GPT3_5_Turbo_16K, number: 1);

            var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
            outputResult = result.FirstChoice.Message.Content.ToString();
            // Replace newlines with <br> or wrap in <p> tags
            outputResult = outputResult.Replace("\n\n", "</p><p>").Replace("\n", "<br>");
            outputResult = "<p>" + outputResult + "</p>"; // Ensure the content is wrapped in <p> tags


            return outputResult;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
    
    //AI for Recommendation
    public async Task<string> RunChatGPTGetCompletionAsync(string input)
    {
        try
        {


            string outputResult = "";
            var api = new OpenAIClient(_openAiApiKey, null, httpClient);
            var messages = new List<Message>
            {
                //  new Message(Role.System, "You are an academic Advisor."),
                new Message(Role.System, "\nYou are a College Adviser."),
                new Message(Role.User, input+"\n"),
            };

            var chatRequest = new ChatRequest(messages, Model.GPT3_5_Turbo_16K, number: 1);

            var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
            outputResult = result.FirstChoice.Message.Content.ToString();
            // Replace newlines with <br> or wrap in <p> tags
            outputResult = outputResult.Replace("\n\n", "</p><p>").Replace("\n", "<br>");
            outputResult = "<p>" + outputResult + "</p>"; // Ensure the content is wrapped in <p> tags


            return outputResult;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}