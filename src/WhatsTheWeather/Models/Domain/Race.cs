using System.Text.Json;

namespace WhatsTheWeather.Models.Domain;

public record Race(
    string Name,
    DateTime Start,
    List<Checkpoint> Checkpoints)
{
    private static JsonSerializerOptions s_opts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name.ToLowerInvariant());
        hash.Add(Start.Year);
        return hash.ToHashCode();
    }

    public static Race? FromJson(string json)
        => JsonSerializer.Deserialize<Race>(json, s_opts);

    public static int MakeHashCode(string name, int year)
        => new Race(name, new DateTime(year, 1, 1), new List<Checkpoint>()).GetHashCode();
}
