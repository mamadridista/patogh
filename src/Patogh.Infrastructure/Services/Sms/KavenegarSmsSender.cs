using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Patogh.Application.Interfaces;
using System.Net.Http.Json;

namespace Patogh.Infrastructure.Services.Sms;

public class KavenegarSmsSender : ISmsSender
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _sender;
    private readonly ILogger<KavenegarSmsSender> _logger;

    public KavenegarSmsSender(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<KavenegarSmsSender> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Read from appsettings — never hardcode API keys
        _apiKey = configuration["Kavenegar:ApiKey"]
            ?? throw new InvalidOperationException("Kavenegar:ApiKey is not configured.");
        _sender = configuration["Kavenegar:Sender"] ?? "2000660110";
    }

    public async Task SendAsync(string phoneNumber, string message)
    {
        try
        {
            // Kavenegar REST API v1 — URL-encoded form body
            var url = $"https://api.kavenegar.com/v1/{_apiKey}/sms/send.json";

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("receptor", phoneNumber),
                new KeyValuePair<string, string>("message", message),
                new KeyValuePair<string, string>("sender", _sender)
            });

            var response = await _httpClient.PostAsync(url, formData);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Kavenegar SMS failed for {Phone}. Status: {Status}",
                    phoneNumber, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            // SMS failure must never crash the application.
            // Log and continue — the main operation (confirm/cancel) already succeeded.
            _logger.LogError(ex,
                "Failed to send SMS to {Phone}: {Message}", phoneNumber, ex.Message);
        }
    }
}