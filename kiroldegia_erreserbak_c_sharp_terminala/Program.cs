using System.Data;
using System.Text;
using MySqlConnector;
using kiroldegia_erreserbak_c_sharp_terminala.Models;

namespace kiroldegia_erreserbak_c_sharp_terminala;

public class Program
{
    private const string KonexioKateaIkaslea = "Server=localhost;Database=kiroldegia;User=ikaslea;Password=ikaslea;AllowUserVariables=True;";
    private const string KonexioKateaRoot =    "Server=localhost;Database=kiroldegia;User=root;Password=1MG32025;AllowUserVariables=True;";

    private static async Task<MySqlConnection> SortuKonexioaAsync()
    {
        try
        {
            var kon = new MySqlConnection(KonexioKateaIkaslea);
            await kon.OpenAsync();
            return kon;
        }
        catch
        {
            var kon = new MySqlConnection(KonexioKateaRoot);
            await kon.OpenAsync();
            return kon;
        }
    }

    private static void GarbituPantaila()
    {
        try
        {
            if (!Console.IsOutputRedirected)
                Console.Clear();
        }
        catch { }
    }

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        while (true)
        {
            GarbituPantaila();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("====================================================================");
            Console.WriteLine("  ORDIZIAKO KIROLDEGIKO ERRESERBAK - TRANSAKZIOAK, PISTAK & BONOAK  ");
            Console.WriteLine("====================================================================");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  [KONTEXUA: ZUZENEKO DB (MySQL - ADO.NET Transakzioak)]");
            Console.ResetColor();
            Console.WriteLine("  1. Erreserba berria egin (Terminaleko formularioa + Bonoa/Pista)");
            Console.WriteLine("  2. Egun dauden erreserbak ikusi");
            Console.WriteLine("  3. Erreserba ezabatu: SOFT DELETE (Logikoa, pista askatu)");
            Console.WriteLine("  4. Erreserba ezabatu: HARD DELETE (Fisikoa, behin betiko)");
            Console.WriteLine("  5. Soft-Delete egindako erreserba berreskuratu (RESTORE)");
            Console.WriteLine("  ------------------------------------------------------------------");
            Console.WriteLine("  6. PISTEN KUDEAKETA (Soft Delete, Hard Delete & Restore)");
            Console.WriteLine("  7. BONOEN KUDEAKETA (Ikusi, Saioak kargatu transakzioan)");
            Console.WriteLine("  8. Erreserba taula garbitu (DELETE FROM erreserba)");
            Console.WriteLine("  0. Irten");
            Console.WriteLine("--------------------------------------------------------------------");
            Console.Write("Aukeratu zenbaki bat: ");

            string? aukera = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (aukera)
            {
                case "1":
                    await EginErreserbaFormularioaAsync();
                    break;
                case "2":
                    await ErakutsiErreserbakAsync();
                    break;
                case "3":
                    await EzabatuErreserbaAsync(hard: false);
                    break;
                case "4":
                    await EzabatuErreserbaAsync(hard: true);
                    break;
                case "5":
                    await BerreskuratuErreserbaAsync();
                    break;
                case "6":
                    await KudeatuPistakMenuAsync();
                    break;
                case "7":
                    await KudeatuBonoakMenuAsync();
                    break;
                case "8":
                    await GarbituTaulaAsync();
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

            Console.WriteLine("\nSakatu ENTER menura itzultzeko...");
            Console.ReadLine();
        }
    }
}
