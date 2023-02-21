namespace WhatsTheWeather.Models.Domain;

public record Race(
    string Name,
    DateTime Start,
    List<Checkpoint> Checkpoints)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(Start);
        return hash.ToHashCode();
    }
}
