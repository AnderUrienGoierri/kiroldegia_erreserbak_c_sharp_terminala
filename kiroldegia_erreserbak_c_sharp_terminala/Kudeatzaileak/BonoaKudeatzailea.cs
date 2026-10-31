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
            Console.WriteLine("           BONOEN KUDEAKETA: SAIOAK IKUSI ETA KARGATU (REST API)    ");
            Console.WriteLine("====================================================================");
            Console.ResetColor();

            await ErakutsiBonoakAsync(api);

            Console.WriteLine("\nEkintza:");
            Console.WriteLine("  1. Bonoari saioak kargatu (PUT /api/bonoak/{email}/kargatu)");
            Console.WriteLine("  2. Bono berria sortu (POST /api/bonoak)");
            Console.WriteLine("  0. Itzuli menu nagusira");
            Console.Write("Aukeratu (0-2): ");

            string? auk = Console.ReadLine()?.Trim();
            switch (auk)
            {
                case "1":
                    await KargatuSaioakFormularioaAsync(api);
                    break;
                case "2":
                    await SortuBonoaFormularioaAsync(api);
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

    public static async Task ErakutsiBonoakAsync(KiroldegiaApiClient api)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("EGUNGO BONOAK:");
        Console.ResetColor();

        var resB = await api.LortuBonoakAsync();
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

        foreach (var b in resB.DatuZerrenda)
        {
            Console.WriteLine($"  * {b.Emaila,-30} -> {b.Saioak} saio erabilgarri");
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
}

