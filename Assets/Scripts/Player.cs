using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum Player
{
    USER = 0,
    AFURE,
    CHENYI,
    SIGRID,
    VIREO,
}
