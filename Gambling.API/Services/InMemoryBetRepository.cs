using System.Collections.Concurrent;

namespace Gambling.API.Services;

public class InMemoryBetRepository : IBetRepository
{
    private static readonly ConcurrentDictionary<string, int> PlayersBalanceDictionary = new();
    public Task<BalanceUpdateResult> UpdateBalance(BalanceUpdateCommand command)
    {
        var enoughCredit = true;

        var balance = PlayersBalanceDictionary.AddOrUpdate(command.UserId,
            command.StartingBalance + command.TheoreticalReward,
            (_, balance) =>
            {
                if (balance - command.BetAmount >= 0)
                {
                    return balance + command.TheoreticalReward;
                }

                enoughCredit = false;
                return balance;
            });

        var result = new BalanceUpdateResult(enoughCredit, balance);
        return Task.FromResult(result);
    }
}