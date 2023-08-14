# Try it out
This app is hosted on Azure: https://gambling-api.azurewebsites.net/

# Gambling API
Welcome to the Gambling API! Built with ASP.NET Core, this application allows players to bet on a randomly generated number between 0-9. With a starting balance of 10,000 points, players have the opportunity to increase their points by placing bets and predicting the right number.

## Features
A player starts with an initial account of 10,000 points.
The player can place a bet on a randomly generated number between 0-9.
If the player's prediction is correct, they win 9 times their stake.

## API Endpoints
Endpoint: "/"

Method: POST

Request payload example:
{
  "points": 100,
  "number": 3
}

Response payload after a win:
{
  "account": 10900,
  "status": "won",
  "points": "+900"
}

Response payload after a loss:
{
  "account": 9900,
  "status": "lost",
  "points": "-100"
}

## How to reset points balance:
Clear cookies to make server forget your debts

## Setting Up & Running locally
- Clone the repository.
- Navigate to the project directory.
- Use the command dotnet restore to restore the required packages.
- Run the API using the dotnet run command.
- Access the API on [http://localhost:5173](http://localhost:5173) (default configuration).
