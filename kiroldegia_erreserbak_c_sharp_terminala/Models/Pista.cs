using System.Text.Json.Serialization;

namespace kiroldegia_erreserbak_c_sharp_terminala.Models;

public class PistaDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("izena")]
    public string Izena { get; set; } = string.Empty;

    [JsonPropertyName("kirola")]
    public string Kirola { get; set; } = string.Empty;

    [JsonPropertyName("ezabatua")]
    public bool Ezabatua { get; set; }

    [JsonPropertyName("ezabaketaData")]
    public DateTime? EzabaketaData { get; set; }
}

public class PistaSortuDTO
{
    [JsonPropertyName("izena")]
    public string Izena { get; set; } = string.Empty;

    [JsonPropertyName("kirola")]
    public string Kirola { get; set; } = string.Empty;
}
