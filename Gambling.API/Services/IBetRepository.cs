namespace Gambling.API.Services;

public interface IBetRepository
{
    Task<BalanceUpdateResult> UpdateBalance(BalanceUpdateCommand command);
}