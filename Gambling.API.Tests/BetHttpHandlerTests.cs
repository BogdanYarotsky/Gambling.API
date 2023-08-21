namespace Gambling.API.Tests;

[TestClass]
public class BetHttpHandlerTests
{
    [TestInitialize]
    public void Init()
    {

    }


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
        var service = scope.ServiceProvider.GetService<BetHttpHandler>();
        Assert.IsNotNull(service);
    }

    [TestMethod]
    public async Task BetLessThanZeroNotAllowed()
    {
        var request = new BetHttpRequest(-5, 0);
        var response = await MakeMockBetAsync(request);
        Assert.IsNotNull(response);
        var problemResponse = (ProblemHttpResult) response;
        Assert.AreEqual(StatusCodes.Status400BadRequest, problemResponse.StatusCode);
    }

    [TestMethod]
    public async Task BettingOnNumberLessThanZeroNotAllowed()
    {
        var request = new BetHttpRequest(0, -5);
        var response = await MakeMockBetAsync(request);
        var problemResponse = (ProblemHttpResult)response;
        Assert.AreEqual(StatusCodes.Status400BadRequest, problemResponse.StatusCode);
    }

    [TestMethod]
    public async Task BettingOnNumberBiggerThanNineNotAllowed()
    {
        var request = new BetHttpRequest(0, 10);
        var response = await MakeMockBetAsync(request);
        var problemResponse = (ProblemHttpResult)response;
        Assert.AreEqual(StatusCodes.Status400BadRequest, problemResponse.StatusCode);
    }

    [TestMethod]
    public async Task EmptyBetReturnsOkResponse()
    {
        var request = new BetHttpRequest(0, 0);
        var response = await MakeMockBetAsync(request);
        var okResponse = (Ok<BetHttpResponse>)response;
        Assert.AreEqual(StatusCodes.Status200OK, okResponse.StatusCode);
    }

    [TestMethod]
    public async Task UserHas10KPointsToStartWith()
    {
        var request = new BetHttpRequest(0, 0);
        var response = await MakeMockBetAsyncUnwrap(request);
        Assert.AreEqual(10_000, response.Account);
    }

    [TestMethod]
    public async Task ManyUsersStartWith10K()
    {
        var bet = new BetHttpRequest(0, 0);
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
        var bet = new BetHttpRequest(200, 3);
        var response = await MakeWinningBetAsync(bet, "Alice");
        var expectedWin = bet.Points * 9;
        Assert.AreEqual(10_000 + expectedWin, response.Account);
        Assert.AreEqual($"+{expectedWin}", response.Points);
        Assert.AreEqual("won", response.Status);
    }

    [TestMethod]
    public async Task LosingDeductsPointsFromAccount()
    {
        var bet = new BetHttpRequest(200, 3);
        var response = await MakeLosingBetAsync(bet, "Henry");
        Assert.AreEqual(10_000 - bet.Points, response.Account);
        Assert.AreEqual($"-{bet.Points}", response.Points);
        Assert.AreEqual("lost", response.Status);
    }

    [TestMethod]
    public async Task MultiUserScenarioIsHandled()
    {
        var archieBet = new BetHttpRequest(200, 3);
        var archieResponse = await MakeLosingBetAsync(archieBet, "Archie");
        Assert.AreEqual(10_000 - archieBet.Points, archieResponse.Account);

        var bobBet = new BetHttpRequest(700, 5);
        var bobResponse = await MakeWinningBetAsync(bobBet, "Bob");
        Assert.AreEqual(10_000 + bobBet.Points * 9, bobResponse.Account);

        var charlieBet = new BetHttpRequest(555, 2);
        var charlieResponse = await MakeLosingBetAsync(charlieBet, "Charlie");
        Assert.AreEqual(10_000 - charlieBet.Points, charlieResponse.Account);
    }

    [TestMethod]
    public async Task BalanceIsPreservedBetweenBets()
    {
        var firstBet = new BetHttpRequest(300, 5);
        var secondBet = new BetHttpRequest(50, 6);
        var thirdBet = new BetHttpRequest(800, 7);

        var firstResponse = await MakeLosingBetAsync(firstBet, "Mortimer");
        var secondResponse = await MakeWinningBetAsync(secondBet, "Mortimer");
        var thirdResponse = await MakeWinningBetAsync(thirdBet, "Mortimer");

        var expectedBalanceAfterFirstBet = 10_000 - firstBet.Points;
        Assert.AreEqual(expectedBalanceAfterFirstBet, firstResponse.Account);

        var expectedBalanceAfterSecondBet = expectedBalanceAfterFirstBet + secondBet.Points * 9;
        Assert.AreEqual(expectedBalanceAfterSecondBet, secondResponse.Account);

        var expectedBalanceAfterThirdBet = expectedBalanceAfterSecondBet + thirdBet.Points * 9;
        Assert.AreEqual(expectedBalanceAfterThirdBet, thirdResponse.Account);
    }

    private static async Task<IResult> MakeMockBetAsync(BetHttpRequest httpRequest, string? userId = null, int? winningNumber = null)
    {
        var authService = new MockAuthService(userId ?? "");
        var betService = new BetService((_, _) => winningNumber ?? 0, new InMemoryBetRepository());
        var httpHandler = new BetHttpHandler(authService, betService);
        return await httpHandler.HandleAsync(httpRequest);
    }

    private static async Task<BetHttpResponse> MakeMockBetAsyncUnwrap(BetHttpRequest httpRequest, string? userId = null, int? winningNumber = null)
    {
        var response = await MakeMockBetAsync(httpRequest, userId, winningNumber);
        Assert.IsInstanceOfType(response, typeof(Ok<BetHttpResponse>));
        var okResponse = (Ok<BetHttpResponse>)response;
        return okResponse.Value ?? throw new NullReferenceException("response was null for some reason");
    }

    private static async Task<BetHttpResponse> MakeWinningBetAsync(BetHttpRequest httpRequest, string userId)
    {
        return await MakeMockBetAsyncUnwrap(httpRequest, userId, httpRequest.Number);
    }

    private static async Task<BetHttpResponse> MakeLosingBetAsync(BetHttpRequest httpRequest, string userId)
    {
        return await MakeMockBetAsyncUnwrap(httpRequest, userId, httpRequest.Number == 0 ? 1 : 0);
    }

    private class MockRngService : IRandomService
    {
        private readonly int _number;
        public MockRngService(int number) => _number = number;
        public int GetNumber(int min, int toInclusive) => _number;
    }

    private class MockAuthService : IAuthService
    {
        private readonly string _userId;
        public MockAuthService(string userId) => _userId = userId;
        public Task<string> GetCurrentUserIdAsync() => Task.FromResult(_userId);
    }
}