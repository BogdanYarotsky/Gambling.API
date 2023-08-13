namespace Gambling.API.Interfaces;

public interface IAuthService
{
    Task<string> GetCurrentUserIdAsync();
}