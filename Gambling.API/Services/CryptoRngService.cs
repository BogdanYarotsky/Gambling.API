using System.Security.Cryptography;

namespace Gambling.API.Services;

public class CryptoRngService : IRandomService
{
    public int GetNumber(int min, int max) => RandomNumberGenerator.GetInt32(min, max);
}