namespace Gambling.API.Interfaces;

public interface IRandomService
{
    int GetNumber(int min, int max);
}