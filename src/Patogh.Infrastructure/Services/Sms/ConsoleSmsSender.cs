using Microsoft.Extensions.Logging;
using Patogh.Application.Interfaces;

namespace Patogh.Infrastructure.Services.Sms;

/// <summary>
/// Development/staging SMS sender that prints messages to the console
/// with vivid ANSI highlighting so developers immediately see OTP codes
/// without needing a real phone or SMS gateway account.
///
/// HOW TO USE IN PRODUCTION:
/// Replace this class (or add a conditional in DependencyInjection.cs)
/// with a real SMS provider such as:
///   - Twilio (global)
///   - Kavenegar (Iran)
///   - Melipayamak / IPPanel (Iran)
/// and gate the registration behind an environment-specific config key.
/// </summary>
public class ConsoleSmsSender : ISmsSender
{
    private readonly ILogger<ConsoleSmsSender> _logger;

    public ConsoleSmsSender(ILogger<ConsoleSmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string phoneNumber, string message)
    {
        // ── Vivid console output for easy development spotting ────────────────
        var border = new string('═', 60);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine($"  ╔{border}╗");
        Console.WriteLine($"  ║  📱 SMS (Console Sender)  ║");
        Console.WriteLine($"  ╠{border}╣");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ║  To     : {phoneNumber,-48}║");
        Console.WriteLine($"  ║  Message: {TruncateMessage(message, 48),-48}║");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  ╚{border}╝");
        Console.ResetColor();
        Console.WriteLine();

        _logger.LogInformation(
            "[CONSOLE SMS] To: {Phone} | Message: {Message}",
            phoneNumber, message);

        return Task.CompletedTask;
    }

    private static string TruncateMessage(string message, int maxLength)
        => message.Length <= maxLength
            ? message
            : message[..(maxLength - 3)] + "...";
}
