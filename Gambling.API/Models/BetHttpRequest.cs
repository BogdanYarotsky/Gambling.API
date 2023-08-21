namespace Gambling.API.Models;

public record BetHttpRequest(int Points, int Number)
{
    private const int MinAllowedNumber = 0;
    private const int MaxAllowedNumber = 9;
    public bool IsNotValid(out Dictionary<string, string[]> problems)
    {
        problems = new Dictionary<string, string[]>();

        if (Points < 0)
        {
            problems.Add(nameof(Points), new[] { "Bet can't be negative" });
        }

        if (Number is < MinAllowedNumber or > MaxAllowedNumber)
        {
            problems.Add(nameof(Number), new[] { $"Winning number must be between {MinAllowedNumber} and {MaxAllowedNumber}" });
        }

        return problems.Any();
    }
}
