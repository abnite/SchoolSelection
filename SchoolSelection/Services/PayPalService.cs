using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SchoolSelection.ApplicationClass;
using SchoolSelection.Models;

namespace SchoolSelection.Services;

public class PayPalService
{
    
    private readonly HttpClient _httpClient;
    private readonly PayPalEnvironmentConfig _currentConfig;

    public PayPalService(HttpClient httpClient, IOptions<PayPalSettings> payPalSettings)
    {
        _httpClient = httpClient;
        var settings = payPalSettings.Value;
        _currentConfig = settings.GetCurrentEnvironmentConfig();
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var authToken = Encoding.ASCII.GetBytes($"{_currentConfig.ClientId}:{_currentConfig.ClientSecret}");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

        var requestBody = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await _httpClient.PostAsync($"{_currentConfig.ApiUrl}/v1/oauth2/token", requestBody);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseContent);
        return jsonDocument.RootElement.GetProperty("access_token").GetString();
    }

    public async Task<string> CreateOrderAsync(decimal amount, string currency, string returnUrl, string cancelUrl)
    {
        var accessToken = await GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var order = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    amount = new
                    {
                        currency_code = currency,
                        value = amount.ToString("F2")
                    }
                }
            },
            application_context = new
            {
                return_url = returnUrl,
                cancel_url = cancelUrl
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_currentConfig.ApiUrl}/v2/checkout/orders", content);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseContent);
        return jsonDocument.RootElement.GetProperty("id").GetString();
    }
    
    public async Task<GetOrderResponse> GetOrderDetailsAsync(string orderId)
    {
        var accessToken = await GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        //var response = await _httpClient.GetAsync($"{_currentConfig.ApiUrl}/v2/checkout/orders/{orderId}");

        // Explicitly set Content-Type to application/json
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_currentConfig.ApiUrl}/v2/checkout/orders/{orderId}/capture")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json") // Send an empty JSON object
        };

        var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var orderDetails = JsonSerializer.Deserialize<GetOrderResponse>(
            responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        //return JsonSerializer.Deserialize<GetOrderResponse>(responseContent);
        return orderDetails;
    }

}