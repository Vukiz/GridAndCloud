using Newtonsoft.Json;

namespace Assets.Scripts.GameJson
{
  //This is an format which client sends to server
  public class PlayerMove
  {
    [JsonProperty("PlayerId")]
    public int PlayerId { get; set; }

    [JsonProperty("move")]
    public string Move { get; set; }
  }
}
