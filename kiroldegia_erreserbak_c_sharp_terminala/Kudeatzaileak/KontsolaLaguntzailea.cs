using kiroldegia_erreserbak_c_sharp_terminala.Services;

namespace kiroldegia_erreserbak_c_sharp_terminala.Kudeatzaileak;

public static class KontsolaLaguntzailea
{
    public static void GarbituPantaila()
    {
        try
        {
            if (!Console.IsOutputRedirected)
                Console.Clear();
        }
        catch { }
    }

    public static void ErakutsiIzenburua(string testua, ConsoleColor kolorea = ConsoleColor.Cyan)
    {
        Console.ForegroundColor = kolorea;
        Console.WriteLine($"\n--- {testua} ---");
        Console.ResetColor();
    }

    public static void ErakutsiApiEmaitza(ApiResult<object> res)
    {
        if (res.Arrakasta)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[ARRAKASTA - HTTP {res.StatusCode}] {res.Mezua}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = res.StatusCode == 409 ? ConsoleColor.Yellow : ConsoleColor.Red;
            Console.WriteLine($"\n[ERROREA / GATAZKA - HTTP {res.StatusCode}] {res.Mezua}");
            if (!string.IsNullOrWhiteSpace(res.Xehetasuna))
            {
                Console.WriteLine($"   Xehetasuna: {res.Xehetasuna}");
            }
            Console.ResetColor();
        }
    }

    public static void ItxaronEnter()
    {
        Console.WriteLine("\nSakatu ENTER menura itzultzeko...");
        Console.ReadLine();
    }
}

