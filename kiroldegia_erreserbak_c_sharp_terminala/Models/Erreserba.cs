using System.Text.Json.Serialization;

namespace kiroldegia_erreserbak_c_sharp_terminala.Models;

public class ErreserbaDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("pistaId")]
    public int PistaId { get; set; }

    [JsonPropertyName("pistaIzena")]
    public string PistaIzena { get; set; } = string.Empty;

    [JsonPropertyName("kirola")]
    public string Kirola { get; set; } = string.Empty;

    [JsonPropertyName("bazkideaId")]
    public int BazkideaId { get; set; }

    [JsonPropertyName("bazkideaIzena")]
    public string BazkideaIzena { get; set; } = string.Empty;

    [JsonPropertyName("bazkideaEmaila")]
    public string? BazkideaEmaila { get; set; }

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;

    [JsonPropertyName("ordua")]
    public string Ordua { get; set; } = string.Empty;

    [JsonPropertyName("ezabatua")]
    public bool Ezabatua { get; set; }

    [JsonPropertyName("ezabaketaData")]
    public DateTime? EzabaketaData { get; set; }

    [JsonPropertyName("sortua")]
    public DateTime Sortua { get; set; }
}

public class ErreserbaSortuDTO
{
    [JsonPropertyName("pistaId")]
    public int? PistaId { get; set; }

    [JsonPropertyName("lekua")]
    public string? Lekua { get; set; }

    [JsonPropertyName("bazkideaId")]
    public int? BazkideaId { get; set; }

    [JsonPropertyName("emaila")]
    public string? Emaila { get; set; }

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty; // Formatua: "yyyy-MM-dd"

    [JsonPropertyName("ordua")]
    public string Ordua { get; set; } = string.Empty; // Formatua: "HH:mm:ss"

    [JsonPropertyName("erabiliBonoa")]
    public bool ErabiliBonoa { get; set; } = false;

    [JsonPropertyName("itxaron")]
    public int Itxaron { get; set; } = 0;
}
