using System.Net.Http.Json;
using System.Text.Json;
using kiroldegia_erreserbak_c_sharp_terminala.Models;

namespace kiroldegia_erreserbak_c_sharp_terminala.Services;

public partial class KiroldegiaApiClient
{
    public string BaseUrl { get; }
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public KiroldegiaApiClient(string? baseUrl = null)
    {
        BaseUrl = baseUrl 
            ?? Environment.GetEnvironmentVariable("KIROLDEGIA_API_URL") 
            ?? "http://localhost:5089";

        if (!BaseUrl.EndsWith("/"))
            BaseUrl += "/";

        _http = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// APIa martxan dagoen eta erantzuten duen egiaztatzen du (GET /).
    /// </summary>
    public async Task<(bool Konektatuta, string Mezua)> EgiaztatuKonexioaAsync()
    {
        try
        {
            var erantzuna = await _http.GetAsync("");
            if (erantzuna.IsSuccessStatusCode)
            {
                return (true, "API zerbitzaria martxan dago eta konexioa egokia da.");
            }
            return (false, $"API zerbitzariak egoera kode okerra itzuli du: {(int)erantzuna.StatusCode} {erantzuna.ReasonPhrase}");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Ezin izan da API zerbitzariarekin konektatu ({BaseUrl}): {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Errore ezezaguna API konexioan: {ex.Message}");
        }
    }

    // =========================================================================
    // LAGUNTZAILE OROKORRAK (HTTP & JSON)
    // =========================================================================

    private async Task<ApiResult<T>> EskaeraZerrendaAsync<T>(string bidea)
    {
        try
        {
            var resp = await _http.GetAsync(bidea);
            if (resp.IsSuccessStatusCode)
            {
                var apiResp = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
                return ApiResult<T>.OkList(apiResp?.DataList ?? new List<T>(), apiResp?.Message ?? "OK", (int)resp.StatusCode);
            }

            return await EraikiAkatsaAsync<T>(resp);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<T>.Huts($"API konexio errorea ({BaseUrl}{bidea}): {ex.Message}", null, 503);
        }
        catch (Exception ex)
        {
            return ApiResult<T>.Huts($"Errorea: {ex.Message}");
        }
    }

    private async Task<ApiResult<T>> EskaeraBakarraAsync<T>(string bidea)
    {
        try
        {
            var resp = await _http.GetAsync(bidea);
            if (resp.IsSuccessStatusCode)
            {
                var apiResp = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
                return ApiResult<T>.Ok(apiResp != null ? apiResp.Data : default, apiResp?.Message ?? "OK", (int)resp.StatusCode);
            }

            return await EraikiAkatsaAsync<T>(resp);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<T>.Huts($"API konexio errorea ({BaseUrl}{bidea}): {ex.Message}", null, 503);
        }
        catch (Exception ex)
        {
            return ApiResult<T>.Huts($"Errorea: {ex.Message}");
        }
    }

    private async Task<ApiResult<object>> EskaeraEzabatuAsync(string bidea)
    {
        try
        {
            var resp = await _http.DeleteAsync(bidea);
            return await ProzesatuErantzunaAsync<object>(resp);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<object>.Huts($"Sare-errorea ezabatzean: {ex.Message}", null, 503);
        }
        catch (Exception ex)
        {
            return ApiResult<object>.Huts($"Errorea: {ex.Message}");
        }
    }

    private async Task<ApiResult<object>> EskaeraPutHutsikAsync(string bidea)
    {
        try
        {
            var resp = await _http.PutAsync(bidea, null);
            return await ProzesatuErantzunaAsync<object>(resp);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<object>.Huts($"Sare-errorea bidaltzean: {ex.Message}", null, 503);
        }
        catch (Exception ex)
        {
            return ApiResult<object>.Huts($"Errorea: {ex.Message}");
        }
    }

    private static async Task<ApiResult<T>> ProzesatuErantzunaAsync<T>(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode)
        {
            var apiResp = await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
            return ApiResult<T>.Ok(apiResp != null ? apiResp.Data : default, apiResp?.Message ?? "Eragiketa arrakastatsua", (int)resp.StatusCode);
        }

        return await EraikiAkatsaAsync<T>(resp);
    }

    private static async Task<ApiResult<T>> EraikiAkatsaAsync<T>(HttpResponseMessage resp)
    {
        int kodea = (int)resp.StatusCode;
        try
        {
            var erroreGorputza = await resp.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(erroreGorputza))
            {
                var errObj = JsonSerializer.Deserialize<ApiError>(erroreGorputza, JsonOptions);
                if (errObj != null && !string.IsNullOrWhiteSpace(errObj.Message))
                {
                    return ApiResult<T>.Huts(errObj.Message, errObj.Detail, kodea);
                }
            }
        }
        catch
        {
            // Errore deserializazioak huts egiten badu, arrazoi estandarra erabili
        }

        return ApiResult<T>.Huts($"Errorea HTTP {kodea} ({resp.ReasonPhrase})", null, kodea);
    }
}
