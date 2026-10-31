using System.Text.Json.Serialization;

namespace kiroldegia_erreserbak_c_sharp_terminala.Models;

public class BazkideaDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("izena")]
    public string Izena { get; set; } = string.Empty;

    [JsonPropertyName("abizena")]
    public string Abizena { get; set; } = string.Empty;

    [JsonPropertyName("emaila")]
    public string? Emaila { get; set; }

    [JsonPropertyName("bonoSaioak")]
    public int BonoSaioak { get; set; } = 0;

    [JsonPropertyName("ezabatua")]
    public bool Ezabatua { get; set; }

    [JsonPropertyName("ezabaketaData")]
    public DateTime? EzabaketaData { get; set; }
}

public class BazkideaSortuDTO
{
    [JsonPropertyName("izena")]
    public string Izena { get; set; } = string.Empty;

    [JsonPropertyName("abizena")]
    public string Abizena { get; set; } = string.Empty;

    [JsonPropertyName("emaila")]
    public string Emaila { get; set; } = string.Empty;

    [JsonPropertyName("hasierakoBonoa")]
    public int HasierakoBonoa { get; set; } = 0;
}

