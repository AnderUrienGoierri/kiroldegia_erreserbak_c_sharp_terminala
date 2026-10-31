using System.Net.Http.Json;
using kiroldegia_erreserbak_c_sharp_terminala.Models;

namespace kiroldegia_erreserbak_c_sharp_terminala.Services;

public partial class KiroldegiaApiClient
{
    // =========================================================================
    // ERRESERBAK (GET, POST, DELETE, PUT/restore, DELETE garbitu)
    // =========================================================================

    public async Task<ApiResult<ErreserbaDTO>> LortuErreserbakAsync(bool barneEzabatuak = true)
    {
        return await EskaeraZerrendaAsync<ErreserbaDTO>($"api/erreserbak?barneEzabatuak={barneEzabatuak.ToString().ToLower()}");
    }

    public async Task<ApiResult<object>> SortuErreserbaAsync(ErreserbaSortuDTO dto, int itxaron = 0)
    {
        try
        {
            string url = itxaron > 0 ? $"api/erreserbak?itxaron={itxaron}" : "api/erreserbak";
            var resp = await _http.PostAsJsonAsync(url, dto, JsonOptions);
            return await ProzesatuErantzunaAsync<object>(resp);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<object>.Huts($"Sare-errorea erreserba egitean: {ex.Message}", null, 503);
        }
        catch (Exception ex)
        {
            return ApiResult<object>.Huts($"Errorea: {ex.Message}");
        }
    }

    public async Task<ApiResult<object>> EzabatuErreserbaAsync(int id, bool hard)
    {
        return await EskaeraEzabatuAsync($"api/erreserbak/{id}?hard={hard.ToString().ToLower()}");
    }

    public async Task<ApiResult<object>> BerreskuratuErreserbaAsync(int id)
    {
        return await EskaeraPutHutsikAsync($"api/erreserbak/{id}/restore");
    }

    public async Task<ApiResult<object>> GarbituErreserbakAsync()
    {
        return await EskaeraEzabatuAsync("api/erreserbak/garbitu");
    }
}

