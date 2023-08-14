using Microsoft.AspNetCore.Diagnostics;

namespace Gambling.API;

public static class Program
{
    private static void Main(string[] args)
    {
        CreateWebApplication(args).Run();
    }

    public static WebApplication CreateWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddGamblingServices();
        builder.Services.AddAuthentication(Cookies.AuthenticationScheme).AddCookie();
        var app = builder.Build();
        app.WrapUnhandledExceptionsInProblemDetails();
        app.UseAuthentication();
        app.MapPost("/", async (BetRequest request, GamblingService service) => await service.BetAsync(request));
        return app;
    }

    public static IServiceCollection AddGamblingServices(this IServiceCollection services)
    {
        services.AddSingleton<IRandomService, CryptoRngService>();
        services.AddScoped<IAuthService, CookiesAuthService>();
        services.AddScoped<GamblingService>();
        services.AddHttpContextAccessor();
        return services;
    }

    private static void WrapUnhandledExceptionsInProblemDetails(this WebApplication app)
    {
        app.UseExceptionHandler(appBuilder =>
        {
            appBuilder.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature?.Error;
                var message = app.Environment.IsDevelopment() ? exception?.ToString() : exception?.Message;
                var problemDetails = new ProblemDetails
                {
                    Type = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/500",
                    Title = "Internal Server Error",
                    Detail = message,
                    Status = 500,
                };
                await context.Response.WriteAsJsonAsync(problemDetails);
            });
        });
    }
}