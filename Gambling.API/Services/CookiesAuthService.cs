using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Gambling.API.Services;

public class CookiesAuthService : IAuthService
{
    private readonly IHttpContextAccessor _ctxAccessor;

    public CookiesAuthService(IHttpContextAccessor ctxAccessor)
    {
        _ctxAccessor = ctxAccessor;
    }

    public async Task<string> GetCurrentUserIdAsync()
    {

        throw new NotImplementedException();
        var ctx = _ctxAccessor.HttpContext;

        if (ctx is null)
        {
            throw new InvalidOperationException("Can't use cookies identification without HttpContext");
        }

        var currentUserId = ctx.User.FindFirst(c => c.Type == ClaimTypes.Name)?.Value;
        if (currentUserId is not null) return currentUserId;

        currentUserId = Guid.NewGuid().ToString();
        var claims = new List<Claim> { new(ClaimTypes.Name, currentUserId) };
        var claimsIdentity = new ClaimsIdentity(claims, Cookies.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        await ctx.SignInAsync(Cookies.AuthenticationScheme, claimsPrincipal);

        return currentUserId;
    }
}