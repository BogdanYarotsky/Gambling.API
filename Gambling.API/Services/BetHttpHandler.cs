
namespace Gambling.API.Services;

public class BetHttpHandler
{
    private readonly IAuthService _auth;
    private readonly BetService _service;

    public BetHttpHandler(IAuthService auth, BetService service)
    {
        _auth = auth;
        _service = service;
    }

    public async Task<IResult> HandleAsync(BetHttpRequest httpRequest)
    {
        if (httpRequest.IsNotValid(out var problemDetails))
        {
            return Results.ValidationProblem(problemDetails);
        }

        var userId = await _auth.GetCurrentUserIdAsync();
        var request = new BetRequest(userId, httpRequest.Points, httpRequest.Number);
        var betResult =  await _service.BetAsync(request);
        if (!betResult.UserHadEnoughCredit)
        {
            return Results.BadRequest("Not enough points on the account!");
        };

        var response = MapToResponse(betResult);
        return Results.Ok(response);
    }

    private static BetHttpResponse MapToResponse(BetResult betResult)
    {
        var statusString = betResult.HasWon ? "won" : "lost";
        var rewardString = betResult.Reward >= 0 ? $"+{betResult.Reward}" : betResult.Reward.ToString();
        return new BetHttpResponse(betResult.CurrentBalance, statusString, rewardString);
    }
}
