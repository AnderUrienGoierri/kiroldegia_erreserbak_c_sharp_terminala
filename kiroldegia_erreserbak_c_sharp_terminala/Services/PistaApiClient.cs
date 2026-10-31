using System.Net.Http.Json;
using kiroldegia_erreserbak_c_sharp_terminala.Models;

namespace kiroldegia_erreserbak_c_sharp_terminala.Services;

public partial class KiroldegiaApiClient
{
    // =========================================================================
    // PISTAK (GET, POST, DELETE, PUT/restore)
    // =========================================================================

    public async Task<ApiResult<PistaDTO>> LortuPistakAsync(bool barneEzabatuak = true)
    {
        return await EskaeraZerrendaAsync<PistaDTO>($"api/pistak?barneEzabatuak={barneEzabatuak.ToString().ToLower()}");
    }

    public async Task<ApiResult<object>> SortuPistaAsync(PistaSortuDTO dto)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/pistak", dto, JsonOptions);
            return await ProzesatuErantzunaAsync<object>(resp);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<object>.Huts($"Sare-errorea pista sortzean: {ex.Message}", null, 503);
        }
        catch (Exception ex)
        {
            return ApiResult<object>.Huts($"Errorea: {ex.Message}");
        }
    }

    public async Task<ApiResult<object>> EzabatuPistaAsync(int id, bool hard)
    {
        return await EskaeraEzabatuAsync($"api/pistak/{id}?hard={hard.ToString().ToLower()}");
    }

    public async Task<ApiResult<object>> BerreskuratuPistaAsync(int id)
    {
        return await EskaeraPutHutsikAsync($"api/pistak/{id}/restore");
    }
}

