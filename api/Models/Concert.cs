namespace Api.Models;

public record Concert(string Band, DateTimeOffset Date, Venue Venue)
{
    public string[]? Genres { get; init; }

    public string? Location { get; init; }

    public string? Description { get; init; }

    public string[]? Guests { get; init; }

    public string? Url { get; init; }
}

public record Venue(string Name, string City, string Country, string? Website = null)
{
    public override string ToString() => Name;
}

public static class Venues
{
    // Luxembourg — Major
    public static readonly Venue Rockhal = new("Rockhal", "Esch-sur-Alzette", "Luxembourg", "https://rockhal.lu");
    public static readonly Venue Atelier = new("den Atelier", "Luxembourg City", "Luxembourg", "https://www.atelier.lu");
    public static readonly Venue Kulturfabrik = new("Kulturfabrik", "Esch-sur-Alzette", "Luxembourg", "https://kulturfabrik.lu");
    public static readonly Venue Philharmonie = new("Philharmonie", "Luxembourg City", "Luxembourg", "https://www.philharmonie.lu");
    public static readonly Venue Neimenster = new("Neimënster", "Luxembourg City", "Luxembourg", "https://www.neimenster.lu");
    public static readonly Venue Luxexpo = new("Luxexpo – The Box", "Kirchberg", "Luxembourg", "https://luxexpo.lu");

    // Luxembourg — Other
    public static readonly Venue Rotondes = new("Rotondes", "Luxembourg City", "Luxembourg", "https://www.rotondes.lu");
    public static readonly Venue Casino2000 = new("Casino 2000", "Mondorf-les-Bains", "Luxembourg", "https://www.casino2000.lu");
    public static readonly Venue Trifolion = new("Trifolion", "Echternach", "Luxembourg", "https://www.trifolion.lu");
    public static readonly Venue Opderschmelz = new("Opderschmelz", "Dudelange", "Luxembourg", "https://www.opderschmelz.lu");
    public static readonly Venue DeGuddeWellen = new("De Gudde Wëllen", "Luxembourg City", "Luxembourg", "https://www.deguddewellen.lu");

    // Germany
    public static readonly Venue TufaTrier = new("Tufa", "Trier", "Germany", "https://www.tufa-trier.de");
    public static readonly Venue EuropahalleTrier = new("Europahalle", "Trier", "Germany", "https://www.europahalle-trier.de");
    public static readonly Venue Saarlandhalle = new("Saarlandhalle", "Saarbrücken", "Germany", "https://www.saarlandhalle.de");

    // France — Cité musicale-Metz (Arsenal, Trinitaires, BAM under one org)
    public static readonly Venue Trinitaires = new("Les Trinitaires", "Metz", "France", "https://www.citemusicale-metz.fr");
    public static readonly Venue Arsenal = new("Arsenal", "Metz", "France", "https://www.citemusicale-metz.fr");
    public static readonly Venue BAM = new("BAM", "Metz", "France", "https://www.citemusicale-metz.fr");

    // Belgium
    public static readonly Venue ReflektorLiege = new("Reflektor", "Liège", "Belgium", "https://reflektor.be");
}
