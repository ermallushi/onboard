using System.Security.Cryptography;
using System.Text;

namespace Onboard.Web.Infrastructure;

public interface ICallbackSecurityService
{
    bool IsValidSignature(string payload, string providedSignature);
}

public sealed class CallbackSecurityService(IConfiguration configuration) : ICallbackSecurityService
{
    private readonly string _secret = configuration["CallbackSecurity:SharedSecret"] ?? "development-secret";

    public bool IsValidSignature(string payload, string providedSignature)
    {
        var expected = Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(_secret), Encoding.UTF8.GetBytes(payload)));
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(providedSignature ?? string.Empty));
    }
}
