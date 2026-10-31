# Transakzioen Egoerak eta DELETE Motaren Justifikazioa

**Aplikazioa:** Ordiziako Kiroldegiko Erreserba Sistema (C# Terminala & REST API)
**Irakasgaia:** Datu-Atzipena (2MG3-2026)

---

## 1. Aurreikusitako Transakzio Egoeren Taula Laburpena

Ondorengo taulan sistema konkorrente batean gerta daitezkeen arriskuak, horien ondorioak eta gure proiektuan ezarritako soluzioak ladira:

| # | Transakzio Egoera                                                  | Gerta daitekeen Arazoa (Arriskua)                                                                                                                  | Irtenbide Teknikoa                                                                                                                              | Erabilitako SQL / C# Mekanismoa                                                                                           |
| - | ------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| 1 | **Erreserba Bikoitza** *(Double Booking / Race Condition)* | Bi erabiltzaile aldi berean pista eta ordu berean erreserba egiten saiatzea. Transakziorik gabe, biek libre ikusten dute eta biek txertatzen dute. | Pistan eta orduan blokeo pesimista ezarri transakzio barruan, eta hutsunea libre dagoela egiaztatu. DB mailan UNIQUE murrizketa ezarri.         | `SELECT COUNT(*) ... FOR UPDATE;` eta `UNIQUE KEY (pista_id, data, ordua)`. Salbuespena: MySQL 1062 kudeaketa.        |
| 2 | **Bonoen Deskontua eta Saldo Negatiboa** *(Overdraft)*     | Bonoan saio bakarra geratzen denean, bi gailutatik batera erreserbatzean saldoa negatiboan (-1) geratzea.                                          | Bonoaren irakurketa eta deskontua erreserbaren transakzio berean blokeatzea (`FOR UPDATE`), eta saio nahikorik ez badago `ROLLBACK` egitea. | `SELECT saioak FROM bonoak WHERE emaila = @email FOR UPDATE;` eta `UPDATE bonoak SET saioak = saioak - 1;` atomikoki. |
| 3 | **Deadlock-ak** *(Elkar-blokeoak)*                         | Bi transakziok baliabide berak blokeatu nahi izatea alderantzizko ordenean (adib. Tx1: Pista -> Bonoa; Tx2: Bonoa -> Pista).                       | Baliabideak beti ordena finko eta berean blokeatzea: `Pista -> Bonoa -> Erreserba`. Deadlock gertatuz gero harrapatu eta desegitea.           | Ordena uniformea kode osoan. Salbuespena: MySQL 1213 (Deadlock) harrapatu eta `RollbackAsync()`.                        |
| 4 | **Pista Desgaituan Erreserbatzea**                           | Mantentze lanetan edo Soft-Delete bidez desgaitutako pista batean erreserba berria sartzea.                                                        | Erreserba egin aurretik pistaren `ezabatua` zutabea egiaztatu eta blokeatzea. Desgaituta badago, transakzioa bertan behera utzi.              | `SELECT ezabatua FROM pista WHERE id = @p FOR UPDATE;` eta `if (ezabatua) await tx.RollbackAsync();`                  |
| 5 | **Erreserba Berreskuratze Gatazka** *(Restore Conflict)*   | Soft-Delete egindako erreserba bat berrezartzerakoan, tartean beste norbaitek ordu hori erreserbatu izana edo pista bera desgaituta egotea.        | Berreskuratu aurretik transakzioan egiaztatzea: 1) pista aktibo dagoela, eta 2) hutsunea libre dagoela. Libre badago soilik berritu.            | `SELECT COUNT(*) ... FOR UPDATE` eta pista egiaztapena. Gatazka badago `ROLLBACK`.                                    |

---

## 2. TRANSAKZIO EGOERAK

### Egoera 1: Erreserba Bikoitza (Double Booking)

- **Arazoa:** Erabiltzaile batek baino gehiagok une berean pista, data eta ordu bera hautatzen dutenean, bi eskaerek aldi berean egiaztatzen dute pista libre dagoela (`COUNT(*) == 0`). Biek jarraitzen dute aurrera eta bi erreserba sortzen dira ordu berean.
- **Soluzioa:**
  1. `SELECT ... FOR UPDATE` bidez errenkadak/hutsunea blokeatzen dira transakzio barruan (Read Committed isolamenduarekin). Bigarren eskaerak itxaron egin behar du lehenengoak transakzioa amaitu arte.
  2. Datu-basean UNIQUE indizea definitu da: `UNIQUE KEY uq_pista_data_ordua (pista_id, data, ordua)`.
  3. C# kodean MySQL 1062 errorea harrapatu eta transakzioa desegiten da (`ROLLBACK`).

**Erabilitako kode zatia (`ErreserbakKontroladorea.cs`):**

```csharp
await using var tx = await kon.BeginTransactionAsync(IsolationLevel.ReadCommitted);
try
{
    // Hutsunea egiaztatu eta blokeatu
    var cmdCheck = new MySqlCommand(@"
        SELECT COUNT(*) FROM erreserba 
        WHERE pista_id = @pista AND data = @data AND ordua = @ordua AND ezabatua = FALSE 
        FOR UPDATE;", kon, tx);

    long kopurua = Convert.ToInt64(await cmdCheck.ExecuteScalarAsync());
    if (kopurua > 0)
    {
        await tx.RollbackAsync();
        return ConflictError("Pista eta ordu horretan jada erreserba aktibo bat dago!");
    }

    // Libre badago, txertatu
    var cmdInsert = new MySqlCommand(@"
        INSERT INTO erreserba (pista_id, bazkidea_id, data, ordua, ezabatua) 
        VALUES (@pista, @bazkidea, @data, @ordua, FALSE);", kon, tx);
    await cmdInsert.ExecuteNonQueryAsync();

    await tx.CommitAsync();
}
catch (MySqlException ex) when (ex.Number == 1062) // UNIQUE constraint
{
    await tx.RollbackAsync();
    return ConflictError("Konkurrentzia-gatazka (MySQL 1062): Beste erabiltzaile batek aldi berean erreserbatu du.");
}
```

---

### Egoera 2: Bonoen Deskontua eta Saldoa Negatiboan Geratzea (Overdraft)

- **Arazoa:** Bazkide baten bonoan saio bakarra geratzen denean (`saioak = 1`), bi pertsonak batera kontsumitzen badute bonoa, biek irakurriko dute saio 1 libre dagoela eta biek deskontatuko dute, saldoa `-1` balioan utziz.
- **Soluzioa:** Bonoaren saldoa irakurri eta deskontatzea erreserbaren txertaketarekin batera transakzio beraren barruan egiten da `FOR UPDATE` klausularekin. Horrela, saldoa blokeatu egiten da eragiketa amaitu arte; saldo nahikorik ez badago (`saioak <= 0`), eragiketa osoa atzera botatzen da (`ROLLBACK`).

**Erabilitako kode zatia (`ErreserbakKontroladorea.cs`):**

```csharp
// Bonoaren saldoa blokeatu eta egiaztatu
var cmdBono = new MySqlCommand("SELECT saioak FROM bonoak WHERE emaila = @email FOR UPDATE;", kon, tx);
cmdBono.Parameters.AddWithValue("@email", bazkideEmaila);
var saioakObj = await cmdBono.ExecuteScalarAsync();

int saioak = Convert.ToInt32(saioakObj);
if (saioak <= 0)
{
    await tx.RollbackAsync();
    return ConflictError($"Ez daukazu saio nahikorik bonoan! Egungo saioak: {saioak}.");
}

// Saio bat deskontatu
int geratzenDira = saioak - 1;
var cmdUpdateBono = new MySqlCommand("UPDATE bonoak SET saioak = @saioak WHERE emaila = @email;", kon, tx);
cmdUpdateBono.Parameters.AddWithValue("@saioak", geratzenDira);
cmdUpdateBono.Parameters.AddWithValue("@email", bazkideEmaila);
await cmdUpdateBono.ExecuteNonQueryAsync();
```

---

### Egoera 3: Deadlock-ak (Elkar-blokeo Hilgarriak - MySQL 1213)

- **Arazoa:** Bi transakziok baliabide berak behar dituztenean baina ordena ezberdinean eskatzen dituztenean (adibidez: 1. transakzioak Pista blokeatzen du eta Bonoa behar du; 2. transakzioak Bonoa blokeatzen du eta Pista behar du). Biek elkarri itxaroten diote betiko.
- **Soluzioa:**
  1. Sistema osoan baliabideak blokeatzeko ordena finko bat ezarri da: beti lehenengo **Pista**, ondoren **Bonoa**, eta azkenik **Erreserba**.
  2. Kodean MySQL 1213 errorea kontrolatu da, gertatuz gero transakzioa modu seguruan desegiteko.

**Erabilitako kode zatia:**

```csharp
catch (MySqlException ex) when (ex.Number == 1213) // Deadlock
{
    await tx.RollbackAsync();
    return ConflictError("Deadlock (MySQL 1213): Elkar-blokeoa gertatu da aldi bereko transakzioen artean. Saiatu berriro.");
}
catch (MySqlException ex) when (ex.Number == 1205) // Lock wait timeout
{
    await tx.RollbackAsync();
    return ConflictError("Blokeo muga gainditu da (MySQL 1205): Beste transakzio batek pista blokeatuta dauka.");
}
```

---

## 3. Pista eta Erreserba Arteko Erlazioan Aukeratutako DELETE Mota

Gure sisteman **SOFT DELETE (Ezabaketa Logikoa)** aukeratu dugu negozio eta aplikazio mailan, eta datu-base mailan **ON DELETE RESTRICT**.

### A. Zergatik SOFT DELETE aplikazio mailan?

1. **Datuen auditoretza eta historia:** Kiroldegiko erabilera-estatistikak, fakturazioa eta egindako erreserben historikoa ez dira inoiz galtzen.
2. **Kaskadako desgaitzea (Cascade Disabling):** Pista bat mantentze-lanetan jartzean (`ezabatua = TRUE`), transakzio atomiko batean harekin lotutako erreserba aktibo guztiak batera desgaitzen dira, datuak fisikoki ezabatu gabe:

```csharp
// 1) Pista logikoki ezabatu
var cmdPista = new MySqlCommand("UPDATE pista SET ezabatua = TRUE, ezabaketa_data = NOW() WHERE id = @id;", kon, tx);
await cmdPista.ExecuteNonQueryAsync();

// 2) Erlazionatutako erreserbak batera desgaitu
var cmdErreserbak = new MySqlCommand("UPDATE erreserba SET ezabatua = TRUE, ezabaketa_data = NOW() WHERE pista_id = @id AND ezabatua = FALSE;", kon, tx);
await cmdErreserbak.ExecuteNonQueryAsync();

await tx.CommitAsync();
```

3. **Akatsen aurreko segurtasuna (Restore):** Okerrez ezeztatutako pista edo erreserba bat berehala berreskuratu daiteke (`PUT .../restore`), datu-galerarik gabe.

---

### B. Zergatik ON DELETE RESTRICT datu-base mailan (eta ez CASCADE)?

Datu-baseko atzerriko gakoan (`FOREIGN KEY`) `ON DELETE RESTRICT` ezarri dugu esplizituki:

```sql
CONSTRAINT fk_erreserba_pista FOREIGN KEY (pista_id) REFERENCES pista (id) ON DELETE RESTRICT
```

- **Zergatik RESTRICT?** Segurtasun-sare gisa jokatzen du. Erreserbak dituen pista bat inork ezin du datu-basetik fisikoki ezabatu (`DELETE FROM pista`), MySQL-k blokeatu egiten baitu (`Error 1451`).
- **Zergatik EZ CASCADE?** `ON DELETE CASCADE`-k ezabatze fisikoa (Hard Delete) egingo luke kaskadan, eta pista ezabatzean erreserba eta kobrantza historiko guztiak betiko desagertuko lirateke.

**Ondorioa:** Datu-baseak datu-galera fisikoak eragozten ditu (`ON DELETE RESTRICT`), eta aplikazioak transakzio atomikoen bidez kudeatzen du kaskadako ezabaketa logikoa (`SOFT DELETE`).
