namespace Gambling.API.Services;

public class BetHttpRequestValidator
{
    private const int MinAllowedNumber = 0;
    private const int MaxAllowedNumber = 9;
    public bool IsNotValid(BetHttpRequest httpRequest, out Dictionary<string, string[]> problems)
    {
        problems = new Dictionary<string, string[]>();

        if (httpRequest.Points < 0)
        {
            problems.Add(nameof(httpRequest.Points), new[] { "Bet can't be negative" });
        }

        if (httpRequest.Number is < MinAllowedNumber or > MaxAllowedNumber)
        {
            problems.Add(nameof(httpRequest.Number), new[] { $"Winning number must be between {MinAllowedNumber} and {MaxAllowedNumber}" });
        }

        return problems.Any();
    }
}