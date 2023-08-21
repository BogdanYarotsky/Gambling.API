namespace Gambling.API.Services;

public record BetRequest(string UserId, int Points, int Number);