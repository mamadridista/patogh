using Patogh.Application.Interfaces;
using StackExchange.Redis;
using System.Security.Cryptography;

namespace Patogh.Infrastructure.Services.Otp;

public class RedisOtpService : IOtpService
{
    private readonly IDatabase _db;
    private readonly ISmsSender _smsSender;

    private static readonly TimeSpan OtpExpiry = TimeSpan.FromMinutes(2);

    public RedisOtpService(IConnectionMultiplexer redis, ISmsSender smsSender)
    {
        _db = redis.GetDatabase();
        _smsSender = smsSender;
    }

    public async Task SendOtpAsync(string phoneNumber)
    {
        // FIX: Random is not cryptographically secure. Use RandomNumberGenerator.
        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var key = BuildKey(phoneNumber);

        await _db.StringSetAsync(key, otp, OtpExpiry);
        await _smsSender.SendAsync(phoneNumber, $"کد تأیید پاتوق: {otp}");
    }

    public async Task<bool> VerifyOtpAsync(string phoneNumber, string code)
    {
        var key = BuildKey(phoneNumber);
        var storedOtp = await _db.StringGetAsync(key);

        if (!storedOtp.HasValue)
            return false;

        if (storedOtp != code)
            return false;

        await _db.KeyDeleteAsync(key);
        return true;
    }

    private static string BuildKey(string phoneNumber) => $"otp:{phoneNumber}";
}