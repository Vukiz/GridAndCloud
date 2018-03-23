using Newtonsoft.Json;

namespace Assets.Scripts.GameJson
{
  public static class SerializeExtension
  {
    public static string ToJson(this PlayerMove self) => JsonConvert.SerializeObject(self);
    public static string ToJson(this GameState self) => JsonConvert.SerializeObject(self);
    public static T FromJson<T>(this string json) => JsonConvert.DeserializeObject<T>(json);
  }
}