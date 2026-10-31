using kiroldegia_erreserbak_c_sharp_terminala.Models;
using kiroldegia_erreserbak_c_sharp_terminala.Services;

namespace kiroldegia_erreserbak_c_sharp_terminala.Kudeatzaileak;

public static class PistaKudeatzailea
{
    public static async Task KudeatuMenuAsync(KiroldegiaApiClient api)
    {
        while (true)
        {
            KontsolaLaguntzailea.GarbituPantaila();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("====================================================================");
            Console.WriteLine("   PISTEN KUDEAKETA: SOFT DELETE (ERRESERBAK DESGAITU) & HARD DELETE");
            Console.WriteLine("====================================================================");
            Console.ResetColor();

            await ErakutsiPistakAsync(api, barneEzabatuak: true);

            Console.WriteLine("\nEkintza:");
            Console.WriteLine("  1. Pista berria sortu (POST /api/pistak)");
            Console.WriteLine("  2. Pista SOFT DELETE egin (Pistarekin erlazionatutako erreserbak desgaitu!)");
            Console.WriteLine("  3. Pista HARD DELETE egin (Pista eta erreserbak behin betiko ezabatu)");
            Console.WriteLine("  4. Pista BERRESKURATU (Restore)");
            Console.WriteLine("  0. Itzuli menu nagusira");
            Console.Write("Aukeratu (0-4): ");

            string? ekintza = Console.ReadLine()?.Trim();
            switch (ekintza)
            {
                case "1":
                    await SortuPistaFormularioaAsync(api);
                    break;
                case "2":
                    await EzabatuPistaAsync(api, hard: false);
                    break;
                case "3":
                    await EzabatuPistaAsync(api, hard: true);
                    break;
                case "4":
                    await BerreskuratuPistaAsync(api);
                    break;
                case "0":
                    return;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Aukera baliogabea.");
                    Console.ResetColor();
                    break;
            }

            KontsolaLaguntzailea.ItxaronEnter();
        }
    }

    public static async Task ErakutsiPistakAsync(KiroldegiaApiClient api, bool barneEzabatuak = true)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("EGUNGO PISTAK:");
        Console.ResetColor();

        var resP = await api.LortuPistakAsync(barneEzabatuak);
        if (!resP.Arrakasta || resP.DatuZerrenda == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROREA] Ezin izan dira pistak kargatu: {resP.Mezua}");
            Console.ResetColor();
            return;
        }

        if (resP.DatuZerrenda.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  (Ez dago pistarik une honetan)");
            Console.ResetColor();
            return;
        }

        Console.WriteLine(string.Format("{0,-5} | {1,-18} | {2,-15} | {3}",
            "ID", "IZENA", "KIROLA", "EGOERA"));
        Console.WriteLine(new string('-', 65));

        foreach (var p in resP.DatuZerrenda)
        {
            string ezabInfo = p.Ezabatua
                ? $"[SOFT-DELETE DESGAITUTA] ({(p.EzabaketaData.HasValue ? p.EzabaketaData.Value.ToString("yyyy-MM-dd HH:mm") : "-")})"
                : "[AKTIBOA]";
            Console.ForegroundColor = p.Ezabatua ? ConsoleColor.DarkGray : ConsoleColor.Green;
            Console.WriteLine(string.Format("{0,-5} | {1,-18} | {2,-15} | {3}",
                p.Id, p.Izena, p.Kirola, ezabInfo));
            Console.ResetColor();
        }
    }

    private static async Task SortuPistaFormularioaAsync(KiroldegiaApiClient api)
    {
        KontsolaLaguntzailea.ErakutsiIzenburua("PISTA BERRIA SORTU (POST /api/pistak)", ConsoleColor.White);

        Console.Write("Pistaren izena (adib. Padel 3): ");
        string? izena = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(izena))
        {
            Console.WriteLine("Izena beharrezkoa da.");
            return;
        }

        Console.Write("Kirola (adib. Padela, Tenisa, Frontoia...): ");
        string? kirola = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(kirola))
        {
            Console.WriteLine("Kirola beharrezkoa da.");
            return;
        }

        var dto = new PistaSortuDTO { Izena = izena, Kirola = kirola };
        Console.WriteLine("\n-> API-ra eskaera bidaltzen (POST /api/pistak)...");
        var res = await api.SortuPistaAsync(dto);
        KontsolaLaguntzailea.ErakutsiApiEmaitza(res);
    }

    private static async Task EzabatuPistaAsync(KiroldegiaApiClient api, bool hard)
    {
        string mota = hard ? "HARD DELETE (Pista eta erreserbak behin betiko ezabatu)" : "SOFT DELETE (Pista eta erreserbak logikoki desgaitu)";
        Console.ForegroundColor = hard ? ConsoleColor.Red : ConsoleColor.Yellow;
        Console.WriteLine($"\n--- PISTA EZABATU: {mota} ---");
        Console.ResetColor();

        Console.Write("Sartu pistaren ID-a: ");
        if (!int.TryParse(Console.ReadLine(), out int pistaId) || pistaId <= 0)
        {
            Console.WriteLine("ID baliogabea.");
            return;
        }

        if (hard)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"KONTUZ: [{pistaId}] pista eta bere erreserba guztiak FISIKOKI ezabatuko dira. Ziur? (b/E): ");
            Console.ResetColor();
            string? b = Console.ReadLine()?.Trim().ToLower();
            if (b is not ("b" or "bai" or "s" or "y"))
            {
                Console.WriteLine("Ezeztatua.");
                return;
            }
        }

        Console.WriteLine($"-> API-ra DELETE eskaera (DELETE /api/pistak/{pistaId}?hard={hard})...");
        var res = await api.EzabatuPistaAsync(pistaId, hard);
        KontsolaLaguntzailea.ErakutsiApiEmaitza(res);
    }

    private static async Task BerreskuratuPistaAsync(KiroldegiaApiClient api)
    {
        KontsolaLaguntzailea.ErakutsiIzenburua("DESGAITUTAKO PISTA BERRESKURATU (PUT /api/pistak/{id}/restore)", ConsoleColor.Cyan);

        Console.Write("Sartu pistaren ID-a: ");
        if (!int.TryParse(Console.ReadLine(), out int pistaId) || pistaId <= 0)
        {
            Console.WriteLine("ID baliogabea.");
            return;
        }

        Console.WriteLine($"-> API-ra PUT eskaera (PUT /api/pistak/{pistaId}/restore)...");
        var res = await api.BerreskuratuPistaAsync(pistaId);
        KontsolaLaguntzailea.ErakutsiApiEmaitza(res);
    }
}

