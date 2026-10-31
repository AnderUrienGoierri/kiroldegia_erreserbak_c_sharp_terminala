using System.Text.Json.Serialization;

namespace kiroldegia_erreserbak_c_sharp_terminala.Models;

public class BonoDTO
{
    [JsonPropertyName("emaila")]
    public string Emaila { get; set; } = string.Empty;

    [JsonPropertyName("saioak")]
    public int Saioak { get; set; }

    [JsonPropertyName("ezabatua")]
    public bool Ezabatua { get; set; }

    [JsonPropertyName("ezabaketaData")]
    public DateTime? EzabaketaData { get; set; }
}

public class BonoKargatuDTO
{
    [JsonPropertyName("saioak")]
    public int Saioak { get; set; }
}
