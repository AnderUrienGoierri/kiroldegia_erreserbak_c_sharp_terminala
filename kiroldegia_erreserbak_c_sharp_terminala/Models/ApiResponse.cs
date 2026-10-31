using System.Text.Json.Serialization;

namespace kiroldegia_erreserbak_c_sharp_terminala.Models;

public class ApiResponse<T>
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "success";

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "OK";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "element";

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("dataList")]
    public List<T>? DataList { get; set; }
}

public class ApiError
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "error";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "ERROR";

    [JsonPropertyName("code")]
    public int Code { get; set; } = 500;

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }
}

