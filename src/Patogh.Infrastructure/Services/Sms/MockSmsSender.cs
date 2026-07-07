using Microsoft.Extensions.Logging;
using Patogh.Application.Interfaces;

namespace Patogh.Infrastructure.Services.Sms;

public class MockSmsSender : ISmsSender
{
    private readonly ILogger<MockSmsSender> _logger;

    public MockSmsSender(ILogger<MockSmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("[MOCK SMS] To: {Phone} | Message: {Message}",
            phoneNumber, message);
        return Task.CompletedTask;
    }
}