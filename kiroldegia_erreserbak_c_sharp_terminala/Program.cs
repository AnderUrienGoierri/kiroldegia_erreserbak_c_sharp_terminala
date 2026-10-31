using System.Text;
using kiroldegia_erreserbak_c_sharp_terminala.Kudeatzaileak;
using kiroldegia_erreserbak_c_sharp_terminala.Services;

namespace kiroldegia_erreserbak_c_sharp_terminala;

public class Program
{
    private static readonly KiroldegiaApiClient Api = new();

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

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
                Console.WriteLine($"  * Abisua: Ziurtatu 'API_kiroldegia_db' martxan dagoela.");
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
            Console.WriteLine("  0. Irten");
            Console.WriteLine("--------------------------------------------------------------------");
            Console.Write("Aukeratu zenbaki bat: ");

            string? aukera = Console.ReadLine()?.Trim();
            Console.WriteLine();

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
