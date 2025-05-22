namespace Api.Models;

public record Concert(string Band, DateTimeOffset Date, Venue Venue)
{
    public string[]? Genres { get; init; }

    public string? Location { get; init; }

    public string? Description { get; init; }

    // TODO:
    public string[]? Guests { get; init; }

    public string? Url { get; init; }
}

public enum Venue
{
    Rockhal,
    Atelier,
    Luxexpo,
    Neimënster,
    Kulturfabrik,
    Philharmonie,
}
