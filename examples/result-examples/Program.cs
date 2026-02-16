/*
This example shows usage of the result monad in a scenario when composing data from different external sources.

Using real world football standings provided by Azhari Muhammad Marzan - https://github.com/azharimm/football-standings-api.
*/

using Svan.Monads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


// Firing off a range of queries with various outcomes
WhichTeamWon("eng.1", 2020);
WhichTeamWon("fra.1", 2021);
WhichTeamWon("eng.1", 2022);
WhichTeamWon("invalid league id", 2020);


static void WhichTeamWon(string leagueId, int season)
{
    var result = GetLeague(new Query(leagueId, season))
                    .Bind(GetSeason)
                    .Bind(GetParticipants)
                    .Bind(GetWinner)
                    .Map(winner => winner.Name)
                    .DefaultWith(error => error.Reason);

    Console.WriteLine($"Winner of {season} {leagueId} is:");
    Console.WriteLine(result);
}

static Result<LookupError, League> GetLeague(Query query)
    => TryCallApi($"leagues/{query.LeagueId}")
            .Fold<Result<LookupError, League>>(
                error => new LookupError($"Could not get league: {error.Message}"),
                success => new League(success["data"]!["id"]!.ToString(), query));

static Result<LookupError, Season> GetSeason(League league)
    => TryCallApi($"leagues/{league.Id}/standings?season={league.Query.Season}&sort=asc")
            .Fold<Result<LookupError, Season>>(
                error => new LookupError($"Could not get league: {error.Message}"),
                success => new Season(Convert.ToInt32(success["data"]!["season"]), league.Query));

static Result<LookupError, IEnumerable<Team>> GetParticipants(Season season)
    => TryCallApi($"leagues/{season.Query.LeagueId}/standings?season={season.Year}&sort=asc")
            .Fold<Result<LookupError, IEnumerable<Team>>>(
                error => new LookupError($"Could not get participants: {error.Message}"),
                success =>
                {
                    var participants = success["data"]!["standings"]!
                                .Select((item, rank) => new Team(
                                    item["team"]!["name"]!.ToString(),
                                    rank + 1,
                                    season.Query));

                    return Result<LookupError, IEnumerable<Team>>.Success(participants);
                });

static Result<LookupError, Team> GetWinner(IEnumerable<Team> teams)
{
    var winner = teams.FirstOrDefault(team => team.Rank == 1);
    if (winner != null)
    {
        return winner;
    }
    else
    {
        return new LookupError("No team with rank 1 found");
    }
}

static Result<Exception, JObject> TryCallApi(string path)
{
    const string RootUrl = "https://api-football-standings.azharimm.site/";

    try
    {
        var url = string.Concat(RootUrl, path);
        var client = new HttpClient();
        var response = client.GetAsync(url).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var parsed = JsonConvert.DeserializeObject<JObject>(json);

        return parsed;
    }
    catch (Exception ex)
    {
        return ex;
    }
}

record Query(string LeagueId, int Season);
record League(string Id, Query Query);
record Team(string Name, int Rank, Query Query);
record Season(int Year, Query Query);
record LookupError(string Reason);
