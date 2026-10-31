namespace kiroldegia_erreserbak_c_sharp_terminala.Services;

public class ApiResult<T>
{
    public bool Arrakasta { get; set; }
    public int StatusCode { get; set; }
    public string Mezua { get; set; } = string.Empty;
    public string? Xehetasuna { get; set; }
    public T? Datuak { get; set; }
    public List<T>? DatuZerrenda { get; set; }

    public static ApiResult<T> Ok(T? datuak, string mezua = "OK", int code = 200) =>
        new() { Arrakasta = true, StatusCode = code, Datuak = datuak, Mezua = mezua };

    public static ApiResult<T> OkList(List<T>? zerrenda, string mezua = "OK", int code = 200) =>
        new() { Arrakasta = true, StatusCode = code, DatuZerrenda = zerrenda, Mezua = mezua };

    public static ApiResult<T> Huts(string mezua, string? xehetasuna = null, int code = 500) =>
        new() { Arrakasta = false, StatusCode = code, Mezua = mezua, Xehetasuna = xehetasuna };
}

