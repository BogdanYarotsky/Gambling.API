namespace Gambling.API.Services;

public class BetService
{
    private readonly RngFunc _rngFunc;
    private readonly IBetRepository _repository;
    private const int StartingBalance = 10_000;
    private const int WinPointsMultiplier = 9;
    private const int MinWinningNumber = 0;
    private const int MaxWinningNumber = 9;

    public BetService(RngFunc rngFunc, IBetRepository repository)
    {
        _rngFunc = rngFunc;
        _repository = repository;
    }

    public async Task<BetResult> BetAsync(BetRequest request)
    {
        var winningNumber = _rngFunc(MinWinningNumber, MaxWinningNumber + 1);
        var hasWon = request.Number == winningNumber;
        var reward = hasWon ? request.Points * WinPointsMultiplier : -request.Points;
        var updateCommand = new BalanceUpdateCommand(request.UserId, request.Points, reward, StartingBalance);
        var updateResult =  await _repository.UpdateBalance(updateCommand);
        return new BetResult(updateResult.HadEnoughCredit, updateResult.CurrentBalance, reward, hasWon);
    }
}