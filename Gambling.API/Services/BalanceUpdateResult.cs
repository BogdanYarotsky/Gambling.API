namespace Gambling.API.Services;

public record BalanceUpdateResult(bool HadEnoughCredit, int CurrentBalance);