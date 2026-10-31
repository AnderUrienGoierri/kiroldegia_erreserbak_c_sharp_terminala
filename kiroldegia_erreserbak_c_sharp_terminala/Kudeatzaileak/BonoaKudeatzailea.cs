using kiroldegia_erreserbak_c_sharp_terminala.Services;

namespace kiroldegia_erreserbak_c_sharp_terminala.Kudeatzaileak;

public static class BonoaKudeatzailea
{
    public static async Task KudeatuMenuAsync(KiroldegiaApiClient api)
    {
        while (true)
        {
            KontsolaLaguntzailea.GarbituPantaila();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("====================================================================");
            Console.WriteLine("        BONOEN KUDEAKETA: SAIOAK, SORTU, EZABATU & RESTORE (REST API)");
            Console.WriteLine("====================================================================");
            Console.ResetColor();

            await ErakutsiBonoakAsync(api, barneEzabatuak: true);

            Console.WriteLine("\nEkintza:");
            Console.WriteLine("  1. Bonoari saioak kargatu (PUT /api/bonoak/{email}/kargatu)");
            Console.WriteLine("  2. Bono berria sortu (POST /api/bonoak)");
            Console.WriteLine("  3. Bonoa SOFT DELETE egin (Logikoki ezgaitu)");
            Console.WriteLine("  4. Bonoa HARD DELETE egin (Fisikoki ezabatu)");
            Console.WriteLine("  5. Bonoa BERRESKURATU (Restore)");
            Console.WriteLine("  0. Itzuli menu nagusira");
            Console.Write("Aukeratu (0-5): ");

            string? auk = Console.ReadLine()?.Trim();
            switch (auk)
            {
                case "1":
                    await KargatuSaioakFormularioaAsync(api);
                    break;
                case "2":
                    await SortuBonoaFormularioaAsync(api);
                    break;
                case "3":
                    await EzabatuBonoaAsync(api, hard: false);
                    break;
                case "4":
                    await EzabatuBonoaAsync(api, hard: true);
                    break;
                case "5":
                    await BerreskuratuBonoaAsync(api);
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

    public static async Task ErakutsiBonoakAsync(KiroldegiaApiClient api, bool barneEzabatuak = true)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("EGUNGO BONOAK:");
        Console.ResetColor();

        var resB = await api.LortuBonoakAsync(barneEzabatuak);
        if (!resB.Arrakasta || resB.DatuZerrenda == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROREA] Ezin izan dira bonoak kargatu: {resB.Mezua}");
            Console.ResetColor();
            return;
        }

        if (resB.DatuZerrenda.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  (Ez dago bonorik erregistratuta une honetan)");
            Console.ResetColor();
            return;
        }

        Console.WriteLine(string.Format("{0,-35} | {1,-12} | {2,-10} | {3}",
            "Emaila", "Saldo Saioak", "Egoera", "Ezabaketa Data"));
        Console.WriteLine(new string('-', 80));

        foreach (var b in resB.DatuZerrenda)
        {
            if (b.Ezabatua)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(string.Format("{0,-35} | {1,-12} | {2,-10} | {3:yyyy-MM-dd HH:mm}",
                    b.Emaila, b.Saioak, "EZABATUA", b.EzabaketaData));
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(string.Format("{0,-35} | {1,-12} | {2,-10} | {3}",
                    b.Emaila, b.Saioak, "AKTIBOA", "-"));
                Console.ResetColor();
            }
        }
    }

    private static async Task KargatuSaioakFormularioaAsync(KiroldegiaApiClient api)
    {
        KontsolaLaguntzailea.ErakutsiIzenburua("BONOARI SAIOAK KARGATU (TRANSAKZIOA)", ConsoleColor.White);

        Console.Write("Sartu bazkidearen emaila: ");
        string? email = Console.ReadLine()?.Trim().ToLower();
        if (string.IsNullOrWhiteSpace(email)) return;

        Console.Write("Zenbat saio gehitu nahi dituzu? (adib. 5): ");
        if (!int.TryParse(Console.ReadLine(), out int gehitukoDira) || gehitukoDira <= 0)
        {
            Console.WriteLine("Saio kopuru baliogabea.");
            return;
        }

        Console.WriteLine($"\n-> API-ra eskaera bidaltzen (PUT /api/bonoak/{email}/kargatu)...");
        var res = await api.KargatuSaioakAsync(email, gehitukoDira);
        KontsolaLaguntzailea.ErakutsiApiEmaitza(res);
    }

    private static async Task SortuBonoaFormularioaAsync(KiroldegiaApiClient api)
    {
        KontsolaLaguntzailea.ErakutsiIzenburua("BONO BERRIA SORTU / EROSI", ConsoleColor.White);

        Console.Write("Sartu email berria: ");
        string? email = Console.ReadLine()?.Trim().ToLower();
        if (string.IsNullOrWhiteSpace(email)) return;

        Console.Write("Hasierako saio kopurua: ");
        if (!int.TryParse(Console.ReadLine(), out int hasierakoa) || hasierakoa < 0)
        {
            Console.WriteLine("Kopuru baliogabea.");
            return;
        }

        Console.WriteLine($"\n-> API-ra eskaera bidaltzen (POST /api/bonoak)...");
        var res = await api.SortuBonoaAsync(email, hasierakoa);
        KontsolaLaguntzailea.ErakutsiApiEmaitza(res);
    }

    public static async Task EzabatuBonoaAsync(KiroldegiaApiClient api, bool hard)
    {
        string mota = hard ? "HARD DELETE (Fisikoa)" : "SOFT DELETE (Logikoa)";
        KontsolaLaguntzailea.ErakutsiIzenburua($"BONOA EZABATU - {mota}", hard ? ConsoleColor.Red : ConsoleColor.Yellow);

        Console.Write("Sartu ezabatu nahi den bonoaren emaila: ");
        string? email = Console.ReadLine()?.Trim().ToLower();
        if (string.IsNullOrWhiteSpace(email)) return;

        if (hard)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"Ziur zaude '{email}' bonoa BEHIN BETIKO ezabatu nahi duzula? (b/e): ");
            Console.ResetColor();
            if (Console.ReadLine()?.Trim().ToLower() != "b")
            {
                Console.WriteLine("Eragiketa bertan behera utzi da.");
                return;
            }
        }

        Console.WriteLine($"\n-> API-ra eskaera bidaltzen (DELETE /api/bonoak/{email}?hard={hard.ToString().ToLower()})...");
        var res = await api.EzabatuBonoaAsync(email, hard);
        KontsolaLaguntzailea.ErakutsiApiEmaitza(res);
    }

    public static async Task BerreskuratuBonoaAsync(KiroldegiaApiClient api)
    {
        KontsolaLaguntzailea.ErakutsiIzenburua("EZABATUTAKO BONOA BERRESKURATU (RESTORE)", ConsoleColor.Cyan);

        Console.Write("Sartu berreskuratu nahi den bonoaren emaila: ");
        string? email = Console.ReadLine()?.Trim().ToLower();
        if (string.IsNullOrWhiteSpace(email)) return;

        Console.WriteLine($"\n-> API-ra eskaera bidaltzen (PUT /api/bonoak/{email}/restore)...");
        var res = await api.BerreskuratuBonoaAsync(email);
        KontsolaLaguntzailea.ErakutsiApiEmaitza(res);
    }
}
