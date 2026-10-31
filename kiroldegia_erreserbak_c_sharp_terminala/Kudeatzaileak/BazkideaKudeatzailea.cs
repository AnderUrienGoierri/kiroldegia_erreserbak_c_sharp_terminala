using kiroldegia_erreserbak_c_sharp_terminala.Models;
using kiroldegia_erreserbak_c_sharp_terminala.Services;

namespace kiroldegia_erreserbak_c_sharp_terminala.Kudeatzaileak;

public static class BazkideaKudeatzailea
{
    public static async Task KudeatuMenuAsync(KiroldegiaApiClient api)
    {
        while (true)
        {
            KontsolaLaguntzailea.GarbituPantaila();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("====================================================================");
            Console.WriteLine("        BAZKIDEEN KUDEAKETA: ZERRENDATU, SORTU, EZABATU & RESTORE   ");
            Console.WriteLine("====================================================================");
            Console.ResetColor();

            await ErakutsiBazkideakAsync(api, barneEzabatuak: true);

            Console.WriteLine("\nEkintza:");
            Console.WriteLine("  1. Bazkide berria sortu (Izena, Abizena, Emaila, Hasierako bonoa)");
            Console.WriteLine("  2. Bazkidea SOFT DELETE egin (Logikoki desgaitu)");
            Console.WriteLine("  3. Bazkidea HARD DELETE egin (Fisikoki ezabatu - erreserbarik ez badu)");
            Console.WriteLine("  4. Bazkidea BERRESKURATU (Restore)");
            Console.WriteLine("  0. Itzuli menu nagusira");
            Console.Write("Aukeratu (0-4): ");

            string? auk = Console.ReadLine()?.Trim();
            switch (auk)
            {
                case "1":
                    await SortuBazkideaFormularioaAsync(api);
                    break;
                case "2":
                    await EzabatuBazkideaAsync(api, hard: false);
                    break;
                case "3":
                    await EzabatuBazkideaAsync(api, hard: true);
                    break;
                case "4":
                    await BerreskuratuBazkideaAsync(api);
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

    public static async Task ErakutsiBazkideakAsync(KiroldegiaApiClient api, bool barneEzabatuak = true)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("EGUNGO BAZKIDEAK:");
        Console.ResetColor();

        var res = await api.LortuBazkideakAsync(barneEzabatuak);
        if (!res.Arrakasta || res.DatuZerrenda == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROREA] Ezin izan dira bazkideak lortu: {res.Mezua}");
            Console.ResetColor();
            return;
        }

        if (res.DatuZerrenda.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  (Ez dago bazkiderik une honetan)");
            Console.ResetColor();
            return;
        }

        Console.WriteLine(string.Format("{0,-5} | {1,-20} | {2,-25} | {3,-12} | {4}",
            "ID", "IZEN-ABIZENAK", "EMAILA", "BONO SAIOAK", "EGOERA"));
        Console.WriteLine(new string('-', 85));

        foreach (var b in res.DatuZerrenda)
        {
            string email = string.IsNullOrWhiteSpace(b.Emaila) ? "-" : b.Emaila;
            string izenaOsoa = $"{b.Izena} {b.Abizena}";
            if (b.Ezabatua)
            {
                string ezabData = b.EzabaketaData.HasValue
                    ? $"({b.EzabaketaData.Value:yyyy-MM-dd HH:mm})"
                    : "";
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(string.Format("{0,-5} | {1,-20} | {2,-25} | {3,-12} | [SOFT-DELETE] {4}",
                    b.Id, izenaOsoa, email, b.BonoSaioak, ezabData));
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(string.Format("{0,-5} | {1,-20} | {2,-25} | {3,-12} | AKTIBOA",
                    b.Id, izenaOsoa, email, b.BonoSaioak));
                Console.ResetColor();
            }
        }
    }

    private static async Task SortuBazkideaFormularioaAsync(KiroldegiaApiClient api)
    {
        KontsolaLaguntzailea.ErakutsiIzenburua("BAZKIDE BERRIA SORTU (POST /api/bazkideak)", ConsoleColor.White);

        Console.Write("Izena: ");
        string? izena = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(izena))
        {
            Console.WriteLine("Izena beharrezkoa da.");
            return;
        }

        Console.Write("Abizena: ");
        string? abizena = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(abizena))
        {
            Console.WriteLine("Abizena beharrezkoa da.");
            return;
        }

        Console.Write("Emaila: ");
        string? email = Console.ReadLine()?.Trim().ToLower();
        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("Emaila beharrezkoa da.");
            return;
        }

        Console.Write("Hasierako bonoa (saioak, adib. 5 edo 0): ");
        int.TryParse(Console.ReadLine(), out int hasierakoBonoa);

        var dto = new BazkideaSortuDTO
        {
            Izena = izena,
            Abizena = abizena,
            Emaila = email,
            HasierakoBonoa = hasierakoBonoa
        };

        Console.WriteLine("\n-> Eskaera API-ra bidaltzen (POST /api/bazkideak)...");
        var res = await api.SortuBazkideaAsync(dto);
        KontsolaLaguntzailea.ErakutsiApiEmaitza(res);
    }

    private static async Task EzabatuBazkideaAsync(KiroldegiaApiClient api, bool hard)
    {
        string mota = hard ? "HARD DELETE (Fisikoki behin betiko)" : "SOFT DELETE (Logikoa)";
        Console.ForegroundColor = hard ? ConsoleColor.Red : ConsoleColor.Yellow;
        Console.WriteLine($"\n--- BAZKIDEA EZABATU: {mota} ---");
        Console.ResetColor();

        Console.Write("Sartu ezabatu nahi duzun bazkidearen ID-a: ");
        if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
        {
            Console.WriteLine("ID baliogabea.");
            return;
        }

        if (hard)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"KONTUZ: [{id}] bazkidea betiko ezabatuko da (erreserbarik ez badu soilik). Ziur? (b/E): ");
            Console.ResetColor();
            string? b = Console.ReadLine()?.Trim().ToLower();
            if (b is not ("b" or "bai" or "s" or "y"))
            {
                Console.WriteLine("Ezeztatua.");
                return;
            }
        }

        Console.WriteLine($"-> API-ra DELETE eskaera (DELETE /api/bazkideak/{id}?hard={hard})...");
        var res = await api.EzabatuBazkideaAsync(id, hard);
        KontsolaLaguntzailea.ErakutsiApiEmaitza(res);
    }

    private static async Task BerreskuratuBazkideaAsync(KiroldegiaApiClient api)
    {
        KontsolaLaguntzailea.ErakutsiIzenburua("DESGAITUTAKO BAZKIDEA BERRESKURATU (PUT /api/bazkideak/{id}/restore)", ConsoleColor.Cyan);

        Console.Write("Sartu berreskuratu nahi duzun bazkidearen ID-a: ");
        if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
        {
            Console.WriteLine("ID baliogabea.");
            return;
        }

        Console.WriteLine($"-> API-ra PUT eskaera (PUT /api/bazkideak/{id}/restore)...");
        var res = await api.BerreskuratuBazkideaAsync(id);
        KontsolaLaguntzailea.ErakutsiApiEmaitza(res);
    }
}

