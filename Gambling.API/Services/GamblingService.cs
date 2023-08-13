using System.Collections.Concurrent;

namespace Gambling.API.Services;

public class GamblingService
{
    private static readonly ConcurrentDictionary<string, int> _playersBalanceDictionary = new();

    private readonly IRandomService _random;
    private readonly IAuthService _auth;

    private const int StartingBalance = 10_000;
    private const int WinPointsMultiplier = 9;
    private const int MinWinningNumber = 0;
    private const int MaxWinningNumber = 9;

    public GamblingService(IAuthService auth, IRandomService random)
    {
        _auth = auth;
        _random = random;
    }

    public async Task<IResult> BetAsync(BetRequest request)
    {
        if (IsNotValid(request, out var problemDetails))
        {
            return Results.ValidationProblem(problemDetails);
        }

        var userId = await _auth.GetCurrentUserIdAsync();
        var winningNumber = _random.GetNumber(MinWinningNumber, MaxWinningNumber);
        var hasWon = request.Number == winningNumber;
        var reward = hasWon ? request.Points * WinPointsMultiplier : -request.Points;

        if (!TryUpdateBalance(userId, request.Points, reward, out var balance))
        {
            return Results.BadRequest("Not enough points on the account!");
        };

        var response = MapToResponse(balance, reward, hasWon);
        return Results.Ok(response);
    }

    private static bool TryUpdateBalance(string userId, int bet, int reward, out int balance)
    {
        var enoughCredit = true;

        balance = _playersBalanceDictionary.AddOrUpdate(userId,
            StartingBalance + reward,
            (_, balance) =>
            {
                if (balance - bet >= 0)
                {
                    return balance + reward;
                }

                enoughCredit = false;
                return balance;
            });

        return enoughCredit;
    }

    private static BetResponse MapToResponse(int balance, int points, bool hasWon)
    {
        var statusString = hasWon ? "won" : "lost";
        var rewardString = points >= 0 ? $"+{points}" : points.ToString();
        return new BetResponse(balance, statusString, rewardString);
    }

    private static bool IsNotValid(BetRequest request, out Dictionary<string, string[]> problems)
    {
        problems = new Dictionary<string, string[]>();

        if (request.Points < 0)
        {
            problems.Add(nameof(request.Points), new[] { "Bet can't be negative" });
        }

        if (request.Number is < MinWinningNumber or > MaxWinningNumber)
        {
            problems.Add(nameof(request.Number), new[] { $"Winning number must be between {MinWinningNumber} and {MaxWinningNumber}" });
        }

        return problems.Any();
    }
}
