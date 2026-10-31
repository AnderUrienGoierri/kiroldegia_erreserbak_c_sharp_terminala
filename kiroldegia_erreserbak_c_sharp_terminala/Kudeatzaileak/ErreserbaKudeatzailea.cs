using kiroldegia_erreserbak_c_sharp_terminala.Models;
using kiroldegia_erreserbak_c_sharp_terminala.Services;

namespace kiroldegia_erreserbak_c_sharp_terminala.Kudeatzaileak;

public static class ErreserbaKudeatzailea
{
    // =========================================================================
    // 1. ERRESERBA BERRIA EGIN (API TRANSAKZIOA + BONOA & FOR UPDATE)
    // =========================================================================
    public static async Task EginErreserbaFormularioaAsync(KiroldegiaApiClient api)
    {
        KontsolaLaguntzailea.ErakutsiIzenburua("ERRESERBA BERRIAREN FORMULARIOA (REST API TRANSAKZIOA)", ConsoleColor.White);

        // 1. BAZKIDEAK LORTU
        Console.WriteLine("\n[1] Bazkideak kargatzen API-tik...");
        var bazkideakResult = await api.LortuBazkideakAsync(barneEzabatuak: true);
        if (!bazkideakResult.Arrakasta || bazkideakResult.DatuZerrenda == null || bazkideakResult.DatuZerrenda.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROREA] Ezin izan dira bazkideak kargatu: {bazkideakResult.Mezua}");
            Console.ResetColor();
            return;
        }

        Console.WriteLine("\nAukeratu bazkidea:");
        foreach (var b in bazkideakResult.DatuZerrenda)
        {
            string email = string.IsNullOrWhiteSpace(b.Emaila) ? "ez du emailik" : b.Emaila;
            string desgaitua = b.Ezabatua ? " [DESGAITUTA]" : "";
            Console.WriteLine($"  [{b.Id}] {b.Izena} {b.Abizena} ({email}) -> Bono saioak: {b.BonoSaioak}{desgaitua}");
        }

        Console.Write("Sartu bazkidearen ID-a: ");
        if (!int.TryParse(Console.ReadLine(), out int bazkideaId) || bazkideaId <= 0)
        {
            Console.WriteLine("Bazkide baliogabea. Bertan behera utzi da.");
            return;
        }

        // 2. PISTAK LORTU
        Console.WriteLine("\n[2] Pistak kargatzen API-tik...");
        var pistakResult = await api.LortuPistakAsync(barneEzabatuak: true);
        if (!pistakResult.Arrakasta || pistakResult.DatuZerrenda == null || pistakResult.DatuZerrenda.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROREA] Ezin izan dira pistak kargatu: {pistakResult.Mezua}");
            Console.ResetColor();
            return;
        }

        Console.WriteLine("\nAukeratu pista:");
        foreach (var p in pistakResult.DatuZerrenda)
        {
            string egoera = p.Ezabatua ? " [DESGAITUTA/SOFT-DELETE]" : "";
            Console.WriteLine($"  [{p.Id}] {p.Izena} ({p.Kirola}){egoera}");
        }

        Console.Write("Sartu pistaren ID-a: ");
        if (!int.TryParse(Console.ReadLine(), out int pistaId) || pistaId <= 0)
        {
            Console.WriteLine("Pista baliogabea. Bertan behera utzi da.");
            return;
        }

        // 3. DATA AUKERATU
        Console.Write("\n[3] Sartu data (YYYY-MM-DD, adib. 2026-10-25, Enter = gaur): ");
        string? dataInput = Console.ReadLine()?.Trim();
        string data;
        if (string.IsNullOrWhiteSpace(dataInput))
        {
            data = DateTime.Today.ToString("yyyy-MM-dd");
        }
        else if (DateOnly.TryParse(dataInput, out var d))
        {
            data = d.ToString("yyyy-MM-dd");
        }
        else
        {
            Console.WriteLine("Data formatua ez da zuzena. '2026-10-25' lehenetsia erabiliko da.");
            data = "2026-10-25";
        }

        // 4. ORDUA AUKERATU
        Console.WriteLine("\n[4] Aukeratu ordua:");
        Console.WriteLine("  [1] 17:00:00");
        Console.WriteLine("  [2] 18:00:00");
        Console.WriteLine("  [3] 19:00:00");
        Console.WriteLine("  [4] 20:00:00");
        Console.WriteLine("  [5] 21:00:00");
        Console.Write("Aukeratu zenbakia (1-5) edo idatzi ordua eskuz (adib. 18:00): ");
        string orduInput = Console.ReadLine()?.Trim() ?? "2";
        string ordua = orduInput switch
        {
            "1" => "17:00:00",
            "2" => "18:00:00",
            "3" => "19:00:00",
            "4" => "20:00:00",
            "5" => "21:00:00",
            _ => TimeOnly.TryParse(orduInput, out var t) ? t.ToString("HH:mm:ss") : (orduInput.Length == 5 && orduInput.Contains(':') ? orduInput + ":00" : orduInput)
        };

        // 5. BONOA ERABILI NAHI DU?
        Console.Write("\n[5] Erabili bazkidearen bonoa (saio 1 deskontatu)? (b/E): ");
        string? bonoResp = Console.ReadLine()?.Trim().ToLower();
        bool erabiliBonoa = bonoResp is "b" or "bai" or "s" or "y";

        // 6. KONKURRENTZIA SIMULAZIOA
        Console.Write("\n[6] Konkurrentzia blokeoa simulatu nahi duzu (ms)? (Enter = 0): ");
        string? itxaronInput = Console.ReadLine()?.Trim();
        int.TryParse(itxaronInput, out int itxaronMs);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n[DATU HAUTATUAK] Pista: {pistaId} | Bazkidea: {bazkideaId} | Data: {data} | Ordua: {ordua} | Bonoa: {(erabiliBonoa ? "BAI" : "EZ")} | Blokeo simulazioa: {itxaronMs}ms");
        Console.ResetColor();

        Console.WriteLine("-> Eskaera API-ra bidaltzen (POST /api/erreserbak)...");

        var dto = new ErreserbaSortuDTO
        {
            PistaId = pistaId,
            BazkideaId = bazkideaId,
            Data = data,
            Ordua = ordua,
            ErabiliBonoa = erabiliBonoa,
            Itxaron = itxaronMs
        };

        var emaitza = await api.SortuErreserbaAsync(dto, itxaronMs);

        if (emaitza.Arrakasta)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[ARRAKASTA - HTTP {emaitza.StatusCode}] {emaitza.Mezua}");
            Console.WriteLine("-> Transakzioa arrakastaz burutu da backend API-an (COMMIT).");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = emaitza.StatusCode == 409 ? ConsoleColor.Yellow : ConsoleColor.Red;
            Console.WriteLine($"\n[ERROREA / GATAZKA - HTTP {emaitza.StatusCode}]");
            Console.WriteLine($"-> {emaitza.Mezua}");
            if (!string.IsNullOrWhiteSpace(emaitza.Xehetasuna))
            {
                Console.WriteLine($"   Xehetasuna: {emaitza.Xehetasuna}");
            }
            Console.WriteLine("-> Backend API-ak transakzioa desegin du (ROLLBACK).");
            Console.ResetColor();
        }
    }

    // =========================================================================
    // 2. EGUN DAUDEN ERRESERBAK IKUSI
    // =========================================================================
    public static async Task ErakutsiErreserbakAsync(KiroldegiaApiClient api)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("--- EGUNGO ERRESERBAK (AKTIBOAK ETA SOFT-DELETE - REST API) ---");
        Console.ResetColor();

        var res = await api.LortuErreserbakAsync(barneEzabatuak: true);
        if (!res.Arrakasta)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROREA] Ezin izan dira erreserbak eskuratu: {res.Mezua}");
            Console.ResetColor();
            return;
        }

        var zerrenda = res.DatuZerrenda ?? new List<ErreserbaDTO>();
        if (zerrenda.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  (Ez dago erreserbarik taulan une honetan)");
            Console.ResetColor();
            return;
        }

        Console.WriteLine(string.Format("{0,-5} | {1,-12} | {2,-18} | {3,-10} | {4,-8} | {5}",
            "ID", "PISTA", "BAZKIDEA", "DATA", "ORDUA", "EGOERA"));
        Console.WriteLine(new string('-', 85));

        foreach (var e in zerrenda)
        {
            if (e.Ezabatua)
            {
                string ezabData = e.EzabaketaData.HasValue
                    ? $"({e.EzabaketaData.Value:yyyy-MM-dd HH:mm})"
                    : "";
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(string.Format("{0,-5} | {1,-12} | {2,-18} | {3,-10} | {4,-8} | [SOFT-DELETE EZABATUA] {5}",
                    e.Id, e.PistaIzena, e.BazkideaIzena, e.Data, e.Ordua, ezabData));
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(string.Format("{0,-5} | {1,-12} | {2,-18} | {3,-10} | {4,-8} | AKTIBOA",
                    e.Id, e.PistaIzena, e.BazkideaIzena, e.Data, e.Ordua));
                Console.ResetColor();
            }
        }
    }

    // =========================================================================
    // 3 & 4. ERRESERBA EZABATU (SOFT vs HARD DELETE)
    // =========================================================================
    public static async Task EzabatuErreserbaAsync(KiroldegiaApiClient api, bool hard)
    {
        string mota = hard ? "HARD DELETE (Behin betiko ezabatu)" : "SOFT DELETE (Logikoki ezabatu, pista askatu)";
        Console.ForegroundColor = hard ? ConsoleColor.Red : ConsoleColor.Yellow;
        Console.WriteLine($"--- ERRESERBA EZABATU: {mota} (REST API) ---");
        Console.ResetColor();

        await ErakutsiErreserbakAsync(api);

        Console.Write($"\nSartu ezabatu nahi duzun erreserbaren ID-a: ");
        if (!int.TryParse(Console.ReadLine(), out int erreserbaId) || erreserbaId <= 0)
        {
            Console.WriteLine("ID baliogabea. Eragiketa bertan behera utzi da.");
            return;
        }

        if (hard)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"KONTUZ: [{erreserbaId}] erreserba datu-basetik FISIKOKI desagertuko da. Ziur zaude? (b/E): ");
            Console.ResetColor();
            string? berretsi = Console.ReadLine()?.Trim().ToLower();
            if (berretsi is not ("b" or "bai" or "s" or "y"))
            {
                Console.WriteLine("Ezabaketa ezeztatua.");
                return;
            }
        }

        Console.WriteLine($"-> API-ra DELETE eskaera bidaltzen (DELETE /api/erreserbak/{erreserbaId}?hard={hard})...");
        var res = await api.EzabatuErreserbaAsync(erreserbaId, hard);
        KontsolaLaguntzailea.ErakutsiApiEmaitza(res);
    }

    // =========================================================================
    // 5. SOFT DELETE EGITEKO ERRESERBA BERRESKURATU (RESTORE)
    // =========================================================================
    public static async Task BerreskuratuErreserbaAsync(KiroldegiaApiClient api)
    {
        KontsolaLaguntzailea.ErakutsiIzenburua("SOFT-DELETE EGITEKO ERRESERBAK BERRESKURATU (RESTORE - REST API)", ConsoleColor.Cyan);

        var listRes = await api.LortuErreserbakAsync(barneEzabatuak: true);
        if (!listRes.Arrakasta)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROREA] Ezin izan dira erreserbak lortu: {listRes.Mezua}");
            Console.ResetColor();
            return;
        }

        var ezabatuak = listRes.DatuZerrenda?.Where(e => e.Ezabatua).ToList() ?? new List<ErreserbaDTO>();
        if (ezabatuak.Count == 0)
        {
            Console.WriteLine("Ez dago Soft-Delete egindako erreserbarik berreskuratzeko.");
            return;
        }

        Console.WriteLine("SOFT-DELETE ERRESERBEN ZERRENDA:");
        foreach (var e in ezabatuak)
        {
            string ezabData = e.EzabaketaData.HasValue ? e.EzabaketaData.Value.ToString("yyyy-MM-dd HH:mm") : "-";
            Console.WriteLine($"  [{e.Id}] {e.PistaIzena} | {e.BazkideaIzena} | {e.Data} {e.Ordua} (Ezabatua: {ezabData})");
        }

        Console.Write("\nSartu berreskuratu nahi duzun erreserbaren ID-a: ");
        if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
        {
            Console.WriteLine("ID baliogabea.");
            return;
        }

        Console.WriteLine($"-> API-ra PUT eskaera bidaltzen (PUT /api/erreserbak/{id}/restore)...");
        var resRestore = await api.BerreskuratuErreserbaAsync(id);
        KontsolaLaguntzailea.ErakutsiApiEmaitza(resRestore);
    }

    // =========================================================================
    // 8. ERRESERBA TAULA GARBITU
    // =========================================================================
    public static async Task GarbituTaulaAsync(KiroldegiaApiClient api)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Ziur zaude ERRESERBA GUZTIAK ezabatu nahi dituzula (DELETE /api/erreserbak/garbitu)? (b/E): ");
        Console.ResetColor();
        string? resp = Console.ReadLine()?.Trim().ToLower();
        if (resp is "b" or "bai" or "s" or "y")
        {
            Console.WriteLine("-> API-ra DELETE eskaera bidaltzen (/api/erreserbak/garbitu)...");
            var res = await api.GarbituErreserbakAsync();
            KontsolaLaguntzailea.ErakutsiApiEmaitza(res);
        }
    }
}

