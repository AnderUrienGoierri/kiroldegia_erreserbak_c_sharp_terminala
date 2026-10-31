using System.Net.Http.Json;
using kiroldegia_erreserbak_c_sharp_terminala.Models;

namespace kiroldegia_erreserbak_c_sharp_terminala.Services;

public partial class KiroldegiaApiClient
{
    // =========================================================================
    // BAZKIDEAK (GET, POST, DELETE, PUT/restore)
    // =========================================================================

    public async Task<ApiResult<BazkideaDTO>> LortuBazkideakAsync(bool barneEzabatuak = true)
    {
        return await EskaeraZerrendaAsync<BazkideaDTO>($"api/bazkideak?barneEzabatuak={barneEzabatuak.ToString().ToLower()}");
    }

    public async Task<ApiResult<BazkideaDTO>> LortuBazkideaAsync(int id)
    {
        return await EskaeraBakarraAsync<BazkideaDTO>($"api/bazkideak/{id}");
    }

    public async Task<ApiResult<object>> SortuBazkideaAsync(BazkideaSortuDTO dto)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/bazkideak", dto, JsonOptions);
            return await ProzesatuErantzunaAsync<object>(resp);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<object>.Huts($"Sare-errorea bazkidea sortzean: {ex.Message}", null, 503);
        }
        catch (Exception ex)
        {
            return ApiResult<object>.Huts($"Errorea: {ex.Message}");
        }
    }

    public async Task<ApiResult<object>> EzabatuBazkideaAsync(int id, bool hard)
    {
        return await EskaeraEzabatuAsync($"api/bazkideak/{id}?hard={hard.ToString().ToLower()}");
    }

    public async Task<ApiResult<object>> BerreskuratuBazkideaAsync(int id)
    {
        return await EskaeraPutHutsikAsync($"api/bazkideak/{id}/restore");
    }
}

