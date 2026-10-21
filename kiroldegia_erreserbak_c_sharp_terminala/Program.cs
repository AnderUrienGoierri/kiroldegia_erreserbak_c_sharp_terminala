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

    // =========================================================================
    // 1. ERRESERBA BERRIA EGIN (TERMINALEKO FORMULARIOA + BONOA & TRANSAKZIOA)
    // =========================================================================
    private static async Task EginErreserbaFormularioaAsync()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("--- ERRESERBA BERRIAREN FORMULARIOA ---");
        Console.ResetColor();

        await using var kon = await SortuKonexioaAsync();

        // 1. BAZKIDEA AUKERATU
        Console.WriteLine("\n[1] Aukeratu bazkidea:");
        using (var cmdB = new MySqlCommand(@"
            SELECT b.id, b.izena, b.abizena, b.emaila, COALESCE(bo.saioak, 0) AS saioak 
            FROM bazkidea b 
            LEFT JOIN bonoak bo ON b.emaila = bo.emaila 
            ORDER BY b.id;", kon))
        {
            using var r = await cmdB.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                string email = r.IsDBNull(r.GetOrdinal("emaila")) ? "ez du emailik" : r.GetString("emaila");
                int saioak = Convert.ToInt32(r["saioak"]);
                Console.WriteLine($"  [{r["id"]}] {r["izena"]} {r["abizena"]} ({email}) -> Bono saioak: {saioak}");
            }
        }
        Console.Write("Sartu bazkidearen ID-a: ");
        if (!int.TryParse(Console.ReadLine(), out int bazkideaId) || bazkideaId <= 0)
        {
            Console.WriteLine("Bazkide baliogabea. Bertan behera utzi da.");
            return;
        }

        // 2. PISTA AUKERATU (Ezabatuta ez daudenak bakarrik)
        Console.WriteLine("\n[2] Aukeratu pista:");
        using (var cmdP = new MySqlCommand("SELECT id, izena, kirola, ezabatua FROM pista ORDER BY id;", kon))
        {
            using var r = await cmdP.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                bool ezab = r.GetBoolean("ezabatua");
                string egoera = ezab ? " [DESGAITUTA/SOFT-DELETE]" : "";
                Console.WriteLine($"  [{r["id"]}] {r["izena"]} ({r["kirola"]}){egoera}");
            }
        }
        Console.Write("Sartu pistaren ID-a: ");
        if (!int.TryParse(Console.ReadLine(), out int pistaId) || pistaId <= 0)
        {
            Console.WriteLine("Pista baliogabea. Bertan behera utzi da.");
            return;
        }

        // 3. DATA AUKERATU
        Console.Write("\n[3] Sartu data (YYYY-MM-DD, adib. 2026-10-25): ");
        string? data = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(data)) data = "2026-10-25";

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
            _ => orduInput.Length == 5 && orduInput.Contains(':') ? orduInput + ":00" : orduInput
        };

        // 5. BONOA ERABILI NAHI DU?
        Console.Write("\n[5] Erabili bazkidearen bonoa (saio 1 deskontatu)? (b/E): ");
        string? bonoResp = Console.ReadLine()?.Trim().ToLower();
        bool erabiliBonoa = bonoResp is "b" or "bai" or "s" or "y";

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n[DATU HAUTATUAK] Pista: {pistaId} | Bazkidea: {bazkideaId} | Data: {data} | Ordua: {ordua} | Bonoa: {(erabiliBonoa ? "BAI" : "EZ")}");
        Console.ResetColor();

        // 6. TRANSAKZIOEN TEORIA APLIKATU:
        // BeginTransaction ->
        //   1) Pista aktibo dagoen begiratu FOR UPDATE bidez
        //   2) Bonoa erabili behar bada: SELECT saioak FROM bonoak FOR UPDATE -> egiaztatu > 0 -> UPDATE bonoak saioak = saioak - 1
        //   3) SELECT COUNT(*) FROM erreserba WHERE pista_id = @p AND data = @d AND ordua = @o AND ezabatua = FALSE FOR UPDATE
        //   4) INSERT INTO erreserba
        // -> Commit
        await using var tx = await kon.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        try
        {
            Console.WriteLine("\n-> Transakzioa ireki da.");

            // 1. Pista egiaztatu
            var cmdPista = new MySqlCommand("SELECT izena, ezabatua FROM pista WHERE id = @p FOR UPDATE;", kon, tx);
            cmdPista.Parameters.AddWithValue("@p", pistaId);
            using (var readerP = await cmdPista.ExecuteReaderAsync())
            {
                if (!await readerP.ReadAsync())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROREA] Pista ez da existitzen. Transakzioa desegin da.");
                    Console.ResetColor();
                    await tx.RollbackAsync();
                    return;
                }
                bool ezabatua = readerP.GetBoolean("ezabatua");
                if (ezabatua)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[ABISUA] Pista hau [{pistaId}] desgaituta (Soft-Delete) dago! Ezin da erreserbarik egin bertan.");
                    Console.ResetColor();
                    await tx.RollbackAsync();
                    return;
                }
            }

            // 2. Bonoa egiaztatu eta deskontatu
            if (erabiliBonoa)
            {
                var cmdBazk = new MySqlCommand("SELECT emaila FROM bazkidea WHERE id = @b FOR UPDATE;", kon, tx);
                cmdBazk.Parameters.AddWithValue("@b", bazkideaId);
                var emailObj = await cmdBazk.ExecuteScalarAsync();
                if (emailObj == null || emailObj == DBNull.Value || string.IsNullOrWhiteSpace(emailObj.ToString()))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROREA] Bazkide honek ez dauka emailarik bonoa erabiltzeko.");
                    Console.ResetColor();
                    await tx.RollbackAsync();
                    return;
                }

                string email = emailObj.ToString()!;
                var cmdBono = new MySqlCommand("SELECT saioak FROM bonoak WHERE emaila = @email FOR UPDATE;", kon, tx);
                cmdBono.Parameters.AddWithValue("@email", email);
                var saioakObj = await cmdBono.ExecuteScalarAsync();
                if (saioakObj == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROREA] '{email}' emailarekin ez dago bonorik erregistratuta.");
                    Console.ResetColor();
                    await tx.RollbackAsync();
                    return;
                }

                int saioak = Convert.ToInt32(saioakObj);
                if (saioak <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[ABISUA] Bonoan ez da saiorik geratzen ({saioak} saio). Transakzioa desegin da.");
                    Console.ResetColor();
                    await tx.RollbackAsync();
                    return;
                }

                int geratzenDira = saioak - 1;
                var cmdUpdateBono = new MySqlCommand("UPDATE bonoak SET saioak = @saioak WHERE emaila = @email;", kon, tx);
                cmdUpdateBono.Parameters.AddWithValue("@saioak", geratzenDira);
                cmdUpdateBono.Parameters.AddWithValue("@email", email);
                await cmdUpdateBono.ExecuteNonQueryAsync();

                Console.WriteLine($"-> Bonoaren saio 1 deskontatu da ({geratzenDira} geratzen dira).");
            }

            // 3. FOR UPDATE: Pista eta ordu berean hutsunea libre dagoen begiratu
            Console.WriteLine("-> 'SELECT COUNT(*) ... FOR UPDATE' exekutatzen (hutsunea egiaztatu eta blokeatu)...");
            var cmdCheck = new MySqlCommand(@"
                SELECT COUNT(*) FROM erreserba 
                WHERE pista_id = @p AND data = @d AND ordua = @o AND ezabatua = FALSE 
                FOR UPDATE;", kon, tx);
            cmdCheck.Parameters.AddWithValue("@p", pistaId);
            cmdCheck.Parameters.AddWithValue("@d", data);
            cmdCheck.Parameters.AddWithValue("@o", ordua);

            long count = Convert.ToInt64(await cmdCheck.ExecuteScalarAsync());
            if (count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[ABISUA] Pista [{pistaId}] eta ordu horretan ({data} {ordua}) jada erreserba aktibo bat eginda dago!");
                Console.WriteLine("-> Ezin da erreserbatu. Transakzioa desegin (ROLLBACK) da.");
                Console.ResetColor();
                await tx.RollbackAsync();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-> Pista libre dagoela egiaztatu da. Erreserba txertatzen...");
            Console.ResetColor();

            var cmdInsert = new MySqlCommand(@"
                INSERT INTO erreserba (pista_id, bazkidea_id, data, ordua, ezabatua) 
                VALUES (@p, @b, @d, @o, FALSE);", kon, tx);
            cmdInsert.Parameters.AddWithValue("@p", pistaId);
            cmdInsert.Parameters.AddWithValue("@b", bazkideaId);
            cmdInsert.Parameters.AddWithValue("@d", data);
            cmdInsert.Parameters.AddWithValue("@o", ordua);

            await cmdInsert.ExecuteNonQueryAsync();
            long idBerria = cmdInsert.LastInsertedId;

            await tx.CommitAsync();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[ARRAKASTA] Erreserba ondo txertatu da! ID: {idBerria} | Pista: {pistaId} | Data: {data} {ordua}");
            Console.WriteLine("-> COMMIT burutu da eta blokeoak askatu dira.");
            Console.ResetColor();
        }
        catch (MySqlException ex) when (ex.Number == 1062) // UNIQUE constraint
        {
            await tx.RollbackAsync();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[SALBUESPENA 1062 KONTROLATUA] UNIQUE constraint apurtu da!");
            Console.WriteLine("-> Beste erabiltzaile batek pista eta ordu bera txertatu berri du tartean.");
            Console.WriteLine("-> Transakzioa desegin da (ROLLBACK).");
            Console.ResetColor();
        }
        catch (MySqlException ex) when (ex.Number == 1213) // Deadlock
        {
            await tx.RollbackAsync();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"\n[SALBUESPENA 1213 KONTROLATUA] MySQL Deadlock (elkar-blokeoa) gertatu da!");
            Console.WriteLine("-> Transakzioa desegin da (ROLLBACK).");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[ERROREA] Erreserba bertan behera: {ex.Message}");
            Console.ResetColor();
        }
    }
}
