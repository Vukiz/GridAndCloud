using Newtonsoft.Json;

namespace IslandServer.GameJson
{
  public class GameState
  {
    [JsonProperty("currentGameState")]
    public int CurrentGameState { get; set; }

    [JsonProperty("player_one")]
    public Player PlayerOne { get; set; }

    [JsonProperty("player_two")]
    public Player PlayerTwo { get; set; }
  }

  public class Player
  {
    [JsonProperty("x")]
    public int X { get; set; }

    [JsonProperty("y")]
    public int Y { get; set; }
  }
}
