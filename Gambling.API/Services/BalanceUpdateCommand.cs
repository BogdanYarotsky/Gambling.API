namespace Gambling.API.Services;

public record BalanceUpdateCommand(string UserId, int BetAmount, int TheoreticalReward, int StartingBalance);