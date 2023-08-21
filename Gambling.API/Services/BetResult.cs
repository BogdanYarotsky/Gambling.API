namespace Gambling.API.Services;

public record BetResult(bool UserHadEnoughCredit, int CurrentBalance, int Reward, bool HasWon);