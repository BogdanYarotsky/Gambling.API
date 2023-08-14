namespace Gambling.API.Tests;

[TestClass]
public class GamblingTests
{
    [TestMethod]
    public void WebApplicationIsCreatedWithoutExceptions()
    {
        var app = Program.CreateWebApplication(Array.Empty<string>());
        Assert.IsNotNull(app);
    }

    [TestMethod]
    public void GamblingServiceResolvedFromServiceProvider()
    {
        var serviceProvider = new ServiceCollection().AddGamblingServices().BuildServiceProvider(true);
        using var scope = serviceProvider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetService<GamblingService>();
        Assert.IsNotNull(service);
    }

    [TestMethod]
    public async Task BetLessThanZeroNotAllowed()
    {
        var request = new BetRequest(-5, 0);
        var response = await MakeMockBetAsync(request);
        Assert.IsNotNull(response);
        var problemResponse = (ProblemHttpResult) response;
        Assert.AreEqual(StatusCodes.Status400BadRequest, problemResponse.StatusCode);
    }

    [TestMethod]
    public async Task BettingOnNumberLessThanZeroNotAllowed()
    {
        var request = new BetRequest(0, -5);
        var response = await MakeMockBetAsync(request);
        var problemResponse = (ProblemHttpResult)response;
        Assert.AreEqual(StatusCodes.Status400BadRequest, problemResponse.StatusCode);
    }

    [TestMethod]
    public async Task BettingOnNumberBiggerThanNineNotAllowed()
    {
        var request = new BetRequest(0, 10);
        var response = await MakeMockBetAsync(request);
        var problemResponse = (ProblemHttpResult)response;
        Assert.AreEqual(StatusCodes.Status400BadRequest, problemResponse.StatusCode);
    }

    [TestMethod]
    public async Task EmptyBetReturnsOkResponse()
    {
        var request = new BetRequest(0, 0);
        var response = await MakeMockBetAsync(request);
        var okResponse = (Ok<BetResponse>)response;
        Assert.AreEqual(StatusCodes.Status200OK, okResponse.StatusCode);
    }

    [TestMethod]
    public async Task UserHas10KPointsToStartWith()
    {
        var request = new BetRequest(0, 0);
        var response = await MakeMockBetAsyncUnwrap(request);
        Assert.AreEqual(10_000, response.Account);
    }

    [TestMethod]
    public async Task ManyUsersStartWith10K()
    {
        var bet = new BetRequest(0, 0);
        var aliceResponse = await MakeMockBetAsyncUnwrap(bet, "Alice");
        var bobResponse = await MakeMockBetAsyncUnwrap(bet, "Bob");
        var charlieResponse = await MakeMockBetAsyncUnwrap(bet, "Charlie");
        Assert.AreEqual(10_000, aliceResponse.Account);
        Assert.AreEqual(10_000, bobResponse.Account);
        Assert.AreEqual(10_000, charlieResponse.Account);
    }

    [TestMethod]
    public async Task WinGives9TimesTheBetAsReward()
    {
        var bet = new BetRequest(200, 3);
        var response = await MakeWinningBetAsync(bet, "Alice");
        var expectedWin = bet.Points * 9;
        Assert.AreEqual(10_000 + expectedWin, response.Account);
        Assert.AreEqual($"+{expectedWin}", response.Points);
        Assert.AreEqual("won", response.Status);
    }

    [TestMethod]
    public async Task LosingDeductsPointsFromAccount()
    {
        var bet = new BetRequest(200, 3);
        var response = await MakeLosingBetAsync(bet, "Alice");
        Assert.AreEqual(10_000 - bet.Points, response.Account);
        Assert.AreEqual($"-{bet.Points}", response.Points);
        Assert.AreEqual("lost", response.Points);
    }

    [TestMethod]
    public async Task MultiUserScenarioIsHandled()
    {
        var aliceBet = new BetRequest(200, 3);
        var aliceResponse = await MakeLosingBetAsync(aliceBet, "Alice");
        Assert.AreEqual(10_000 - aliceBet.Points, aliceResponse.Account);

        var bobBet = new BetRequest(700, 5);
        var bobResponse = await MakeWinningBetAsync(bobBet, "Bob");
        Assert.AreEqual(10_000 + bobBet.Points * 9, bobResponse.Account);

        var charlieBet = new BetRequest(555, 2);
        var charlieResponse = await MakeLosingBetAsync(charlieBet, "Charlie");
        Assert.AreEqual(10_000 - charlieBet.Points, charlieResponse.Account);
    }

    [TestMethod]
    public async Task BalanceIsPreservedBetweenBets()
    {
        var firstBet = new BetRequest(300, 5);
        var secondBet = new BetRequest(50, 6);
        var thirdBet = new BetRequest(800, 7);

        var firstResponse = await MakeLosingBetAsync(firstBet, "Alice");
        var secondResponse = await MakeWinningBetAsync(secondBet, "Alice");
        var thirdResponse = await MakeWinningBetAsync(thirdBet, "Alice");

        var expectedBalanceAfterFirstBet = 10_000 - firstBet.Points;
        Assert.AreEqual(expectedBalanceAfterFirstBet, firstResponse.Account);

        var expectedBalanceAfterSecondBet = expectedBalanceAfterFirstBet + secondBet.Points * 9;
        Assert.AreEqual(expectedBalanceAfterSecondBet, secondResponse.Account);

        var expectedBalanceAfterThirdBet = expectedBalanceAfterSecondBet + thirdBet.Points * 9;
        Assert.AreEqual(expectedBalanceAfterThirdBet, thirdResponse.Account);
    }

    private static async Task<IResult> MakeMockBetAsync(BetRequest request, string? userId = null, int? winningNumber = null)
    {
        var authService = new MockAuthService(userId ?? "");
        var rngService = new MockRngService(winningNumber ?? 0);
        var gamblingService = new GamblingService(authService, rngService);
        return await gamblingService.BetAsync(request);
    }

    private static async Task<BetResponse> MakeMockBetAsyncUnwrap(BetRequest request, string? userId = null, int? winningNumber = null)
    {
        var response = await MakeMockBetAsync(request, userId, winningNumber);
        var okResponse = (Ok<BetResponse>)response;
        return okResponse.Value ?? throw new NullReferenceException("response was null for some reason");
    }
    private static async Task<BetResponse> MakeWinningBetAsync(BetRequest request, string userId)
    {
        return await MakeMockBetAsyncUnwrap(request, userId, request.Number);
    }

    private static async Task<BetResponse> MakeLosingBetAsync(BetRequest request, string userId)
    {
        return await MakeMockBetAsyncUnwrap(request, userId, request.Number == 0 ? 1 : 0);
    }

    private class MockRngService : IRandomService
    {
        private readonly int _number;
        public MockRngService(int number) => _number = number;
        public int GetNumber(int min, int max) => _number;
    }

    private class MockAuthService : IAuthService
    {
        private readonly string _userId;
        public MockAuthService(string userId) => _userId = userId;
        public Task<string> GetCurrentUserIdAsync() => Task.FromResult(_userId);
    }
}