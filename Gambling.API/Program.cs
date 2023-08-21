using Microsoft.AspNetCore.Diagnostics;
using System.Security.Cryptography;

namespace Gambling.API;


public delegate int RngFunc(int fromInclusive, int toExclusive);

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
        app.MapPost("/api/v1/bet", async (BetHttpRequest request, BetHttpHandler handler) => await handler.HandleAsync(request));
        return app;
    }

    public static IServiceCollection AddGamblingServices(this IServiceCollection services)
    {
        services.AddSingleton<RngFunc>(_ => RandomNumberGenerator.GetInt32);
        services.AddScoped<IBetRepository, InMemoryBetRepository>();
        services.AddScoped<IAuthService, CookiesAuthService>();
        services.AddScoped<BetHttpHandler>();
        services.AddHttpContextAccessor();
        services.AddScoped<BetService>();
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