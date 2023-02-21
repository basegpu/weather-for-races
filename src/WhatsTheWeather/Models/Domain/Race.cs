namespace WhatsTheWeather.Models.Domain;

public record Race(string Name, DateTime Start, List<Checkpoint> Checkpoints);
