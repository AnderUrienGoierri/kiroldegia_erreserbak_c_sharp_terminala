using System.Text;
using kiroldegia_erreserbak_c_sharp_terminala.GUI;
using kiroldegia_erreserbak_c_sharp_terminala.Kudeatzaileak;
using kiroldegia_erreserbak_c_sharp_terminala.Services;

namespace kiroldegia_erreserbak_c_sharp_terminala;

public class Program
{
    public static readonly KiroldegiaApiClient Api = new();

    [STAThread]
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // 1. Komando-lerroko argumentuak aztertu (--gui edo --cli)
        if (args.Length > 0)
        {
            string arg = args[0].Trim().ToLowerInvariant();
            if (arg is "--gui" or "-g" or "/gui" or "gui")
            {
                GuiLauncher.AbiaraziGui(Api);
                return;
            }

            if (arg is "--cli" or "-c" or "--terminal" or "-t" or "cli" or "terminal")
            {
                await ExekutatuTerminalModuaAsync();
                return;
            }
        }

        // 2. Argumenturik ez badago, hasierako aukeraketa menua erakutsi
        while (true)
        {
            KontsolaLaguntzailea.GarbituPantaila();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("====================================================================");
            Console.WriteLine("       ORDIZIAKO KIROLDEGIKO KUDEAKETA SISTEMA (C#)                 ");
            Console.WriteLine("====================================================================");
            Console.ResetColor();
            Console.WriteLine("Aukeratu aplikazioa nola exekutatu nahi duzun:\n");
            Console.WriteLine("  1. Terminal modua (Kontsola / CLI)");
            Console.WriteLine("  2. Interfaze Grafikoa (GUI - Windows Forms)");
            Console.WriteLine("  0. Irten\n");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Aholkua: Zuzenean abiarazteko: `dotnet run -- --gui` edo `--cli`");
            Console.ResetColor();
            Console.WriteLine("--------------------------------------------------------------------");
            Console.Write("Aukeratu [1/2/0] (Lehenetsia: 1): ");

            string? aukera = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(aukera) || aukera == "1" || aukera.Equals("cli", StringComparison.OrdinalIgnoreCase))
            {
                await ExekutatuTerminalModuaAsync();
                break;
            }
            else if (aukera == "2" || aukera.Equals("gui", StringComparison.OrdinalIgnoreCase))
            {
                GuiLauncher.AbiaraziGui(Api);
                break;
            }
            else if (aukera == "0" || aukera.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Agur!");
                return;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Aukera baliogabea. Sakatu Enter berriro saiatzeko...");
                Console.ResetColor();
                Console.ReadLine();
            }
        }
    }

    /// <summary>
    /// Jatorrizko Terminal/Kontsola menua exekutatzen du, bere aukera guztiekin.
    /// </summary>
    public static async Task ExekutatuTerminalModuaAsync()
    {
        // Hasierako konexio egiaztapena
        var (konektatuta, _) = await Api.EgiaztatuKonexioaAsync();

        while (true)
        {
            KontsolaLaguntzailea.GarbituPantaila();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("====================================================================");
            Console.WriteLine("  ORDIZIAKO KIROLDEGIKO ERRESERBAK - REST API TERMINAL BEZEROA      ");
            Console.WriteLine("====================================================================");

            if (konektatuta)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  [KONTEXUA: REST API BIDEZKOA -> {Api.BaseUrl}]");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  [KONTEXUA: REST API ({Api.BaseUrl}) - DESKONEKTATUTA/SAIATZEN]");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("  * Abisua: Ziurtatu 'API_kiroldegia_db' martxan dagoela.");
            }

            Console.ResetColor();
            Console.WriteLine("  1. Erreserba berria egin (Formularioa + Bonoa/Pista)");
            Console.WriteLine("  2. Egun dauden erreserbak ikusi");
            Console.WriteLine("  3. Erreserba ezabatu: SOFT DELETE (Logikoa, pista askatu)");
            Console.WriteLine("  4. Erreserba ezabatu: HARD DELETE (Fisikoa, behin betiko)");
            Console.WriteLine("  5. Soft-Delete egindako erreserba berreskuratu (RESTORE)");
            Console.WriteLine("  ------------------------------------------------------------------");
            Console.WriteLine("  6. PISTEN KUDEAKETA (Zerrendatu, Sortu, Soft/Hard Delete & Restore)");
            Console.WriteLine("  7. BAZKIDEEN KUDEAKETA (Zerrendatu, Sortu, Soft/Hard Delete & Restore)");
            Console.WriteLine("  8. BONOEN KUDEAKETA (Ikusi saldoak, Saioak kargatu transakzioan)");
            Console.WriteLine("  9. Erreserba taula garbitu (DELETE /api/erreserbak/garbitu)");
            Console.WriteLine("  ------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("  G. Interfaze Grafikoa ireki (GUI - Windows Forms)");
            Console.ResetColor();
            Console.WriteLine("  0. Irten");
            Console.WriteLine("--------------------------------------------------------------------");
            Console.Write("Aukeratu zenbaki bat: ");

            string? aukera = Console.ReadLine()?.Trim();
            Console.WriteLine();

            if (aukera?.Equals("g", StringComparison.OrdinalIgnoreCase) == true || aukera == "10")
            {
                Console.WriteLine("Interfaze Grafikoa (GUI) irekitzen...");
                GuiLauncher.AbiaraziGui(Api);
                // GUI itxi denean, konexio egoera eguneratu eta kontsolarekin jarraitu
                var egiaztapena = await Api.EgiaztatuKonexioaAsync();
                konektatuta = egiaztapena.Konektatuta;
                continue;
            }

            switch (aukera)
            {
                case "1":
                    await ErreserbaKudeatzailea.EginErreserbaFormularioaAsync(Api);
                    break;
                case "2":
                    await ErreserbaKudeatzailea.ErakutsiErreserbakAsync(Api);
                    break;
                case "3":
                    await ErreserbaKudeatzailea.EzabatuErreserbaAsync(Api, hard: false);
                    break;
                case "4":
                    await ErreserbaKudeatzailea.EzabatuErreserbaAsync(Api, hard: true);
                    break;
                case "5":
                    await ErreserbaKudeatzailea.BerreskuratuErreserbaAsync(Api);
                    break;
                case "6":
                    await PistaKudeatzailea.KudeatuMenuAsync(Api);
                    break;
                case "7":
                    await BazkideaKudeatzailea.KudeatuMenuAsync(Api);
                    break;
                case "8":
                    await BonoaKudeatzailea.KudeatuMenuAsync(Api);
                    break;
                case "9":
                    await ErreserbaKudeatzailea.GarbituTaulaAsync(Api);
                    break;
                case "0":
                    Console.WriteLine("Agur!");
                    return;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Aukera baliogabea. Saiatu berriro.");
                    Console.ResetColor();
                    break;
            }

            // Hurrengo biran konexio egoera eguneratu
            var egiaztapenBerria = await Api.EgiaztatuKonexioaAsync();
            konektatuta = egiaztapenBerria.Konektatuta;

            KontsolaLaguntzailea.ItxaronEnter();
        }
    }
}
