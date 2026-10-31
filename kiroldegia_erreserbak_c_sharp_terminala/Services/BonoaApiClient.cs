using System.Net.Http.Json;
using kiroldegia_erreserbak_c_sharp_terminala.Models;

namespace kiroldegia_erreserbak_c_sharp_terminala.Services;

public partial class KiroldegiaApiClient
{
    // =========================================================================
    // BONOAK (GET, PUT/kargatu, POST)
    // =========================================================================

    public async Task<ApiResult<BonoDTO>> LortuBonoakAsync()
    {
        return await EskaeraZerrendaAsync<BonoDTO>("api/bonoak");
    }

    public async Task<ApiResult<object>> KargatuSaioakAsync(string emaila, int saioak)
    {
        try
        {
            var dto = new BonoKargatuDTO { Saioak = saioak };
            var resp = await _http.PutAsJsonAsync($"api/bonoak/{Uri.EscapeDataString(emaila)}/kargatu", dto, JsonOptions);
            return await ProzesatuErantzunaAsync<object>(resp);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<object>.Huts($"Sare-errorea bonoa kargatzean: {ex.Message}", null, 503);
        }
        catch (Exception ex)
        {
            return ApiResult<object>.Huts($"Errorea: {ex.Message}");
        }
    }

    public async Task<ApiResult<object>> SortuBonoaAsync(string emaila, int saioak)
    {
        try
        {
            var dto = new BonoDTO { Emaila = emaila, Saioak = saioak };
            var resp = await _http.PostAsJsonAsync("api/bonoak", dto, JsonOptions);
            return await ProzesatuErantzunaAsync<object>(resp);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<object>.Huts($"Sare-errorea bonoa sortzean: {ex.Message}", null, 503);
        }
        catch (Exception ex)
        {
            return ApiResult<object>.Huts($"Errorea: {ex.Message}");
        }
    }
}

