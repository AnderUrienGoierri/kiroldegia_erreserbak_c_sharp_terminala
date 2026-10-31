# Kiroldegia Erreserbak - C# Bezeroa (Terminala eta Interfaze Grafikoa - GUI)

Ordiziako Majori Kiroldegiko erreserbak kudeatzeko C# aplikazio modularra (.NET 10.0-windows).

Aplikazio honek **bi exekuzio modu** eskaintzen ditu bateragarritasun osoz:

1. **Terminal modua (Kontsola / CLI):** Jatorrizko testu-menua, arina eta azkarra.
2. **Interfaze Grafikoa (GUI - Windows Forms):** Leiho bisuala taula dinamikoekin (DataGridView), fitxekin (Bazkideak, Pistak, Erreserbak, Bonoak), kontrol-botoiekin eta formulario laguntzaileekin.

Aplikazioak ez du datu-basera zuzenean konektatzen; horren ordez, `API_kiroldegia_db` REST API zerbitzariaren bidez komunikatzen da HTTP (`HttpClient`) bidez, JSON datuak trukatuz.

---

## 1. Arkitektura Orokorra

Aplikazioak geruzen araberako banaketa garbia jarraitzen du. Bezeroaren bi moduek (CLI eta GUI) zerbitzu- eta eredu-geruza bera partekatzen dute kodea berrerabiliz:

```text
+-------------------------------------------------------------------------+
|                kiroldegia_erreserbak_c_sharp_terminala                  |
|                                                                         |
|   +-----------------------+              +--------------------------+   |
|   |  Terminal Modua (CLI) |              |  Interfaze Grafikoa (GUI)|   |
|   |    (Kudeatzaileak)    |              |     (Windows Forms)      |   |
|   +-----------------------+              +--------------------------+   |
|               \                                      /                  |
|                +------------------+-----------------+                   |
|                                   v                                     |
|                       +---------------------------+                     |
|                       |    Services (API Client)  |                     |
|                       +---------------------------+                     |
|                                   |                                     |
|                                   v                                     |
|                       +---------------------------+                     |
|                       |     Models (DTO / JSON)   |                     |
|                       +---------------------------+                     |
+-----------------------------------|-------------------------------------+
                                    |
                                    | HTTP JSON (GET, POST, PUT, DELETE)
                                    v http://localhost:5089/api/
+-------------------------------------------------------------------------+
|                           API_kiroldegia_db                             |
|                      (ASP.NET Core REST Web API)                        |
|                                                                         |
|  - Transakzioak (BeginTransaction, Commit, Rollback)                    |
|  - Blokeo pesimistak (SELECT ... FOR UPDATE)                            |
|  - Konkurrentzia eta isolamendua (Read Committed)                       |
|  - Salbuespenen kontrola (MySQL 1062, 1213, 1205)                       |
+-------------------------------------------------------------------------+
                                    |
                                    | ADO.NET (MySqlConnector)
                                    v
+-------------------------------------------------------------------------+
|                        MySQL InnoDB Datu-Basea                          |
|                             (kiroldegia)                                |
+-------------------------------------------------------------------------+
```

---

## 2. Proiektuaren Egitura eta Antolaketa

Fitxategiak erantzukizun bakarrarekin (Single Responsibility Principle) antolatu dira:

```text
kiroldegia_erreserbak_c_sharp_terminala/
│
├── Program.cs                  # Hasierako abiarazlea: CLI/GUI hautatzailea eta argumentu-analisia
│
├── GUI/                        # Interfaze Grafikoa (Windows Forms)
│   ├── GuiLauncher.cs          # GUI hari seguruan (STA) abiarazteko arduraduna
│   ├── MainForm.cs             # Leiho nagusia fitxekin: Bazkideak, Pistak, Erreserbak, Bonoak
│   └── Dialogs/                # Datuak sartzeko formulario laguntzaileak
│       ├── BazkideBerriaDialog.cs
│       ├── PistaBerriaDialog.cs
│       ├── ErreserbaBerriaDialog.cs
│       └── BonoKargatuDialog.cs
│
├── Kudeatzaileak/              # Terminaleko menuak eta logika (CLI)
│   ├── BazkideaKudeatzailea.cs # Bazkideen azpimenua, zerrendatzea, sortzea, ezabaketak eta berreskuratzea
│   ├── PistaKudeatzailea.cs    # Pisten azpimenua, zerrendatzea, sortzea, ezabaketak eta berreskuratzea
│   ├── ErreserbaKudeatzailea.cs# Erreserben formularioa, zerrendatzea, ezabaketak, berreskuratzea eta garbiketa
│   ├── BonoaKudeatzailea.cs    # Bonoen azpimenua, saldoak ikustea, kargatzea eta sortzea
│   └── KontsolaLaguntzailea.cs # Kontsolako pantaila-garbiketa, izenburuak eta formatzea
│
├── Services/                   # REST API bezeroa (partial class KiroldegiaApiClient bidez banatua)
│   ├── ApiResult.cs            # ApiResult<T> erantzun generikoaren bilgarria
│   ├── KiroldegiaApiClient.cs  # HttpClient oinarria, BaseUrl, konexio-egiaztapena eta laguntzaileak
│   ├── BazkideaApiClient.cs    # Bazkideen HTTP eskaerak (/api/bazkideak)
│   ├── PistaApiClient.cs       # Pisten HTTP eskaerak (/api/pistak)
│   ├── ErreserbaApiClient.cs   # Erreserben HTTP eskaerak (/api/erreserbak)
│   └── BonoaApiClient.cs       # Bonoen HTTP eskaerak (/api/bonoak)
│
├── Models/                     # DTOak eta JSON datu-ereduak
│   ├── ApiResponse.cs          # ApiResponse<T> eta ApiError eredua
│   ├── Bazkidea.cs             # BazkideaDTO eta BazkideaSortuDTO
│   ├── Pista.cs                # PistaDTO eta PistaSortuDTO
│   ├── Erreserba.cs            # ErreserbaDTO eta ErreserbaSortuDTO
│   └── Bonoa.cs                # BonoDTO eta BonoKargatuDTO
│
├── database/
│   └── kiroldegia_db.sql       # MySQL eskema
│
└── doc/
    └── TRANSAKZIOAK_ETA_DELETE_JUSTIFIKAZIOA.md # Transakzioak eta ezabatze-moten justifikazio teknikoa
```

---

## 3. Aplikazioa Bi Modutara Abiaraztea

### A) Modu Interaktiboa (Menu bidez aukeratuz)

Argumenturik gabe abiarazten denean, aplikazioak hasieran galdetzen du zein modu erabili:

```bash
dotnet run
```

```text
====================================================================
       ORDIZIAKO KIROLDEGIKO KUDEAKETA SISTEMA (C#)
====================================================================
Aukeratu aplikazioa nola exekutatu nahi duzun:

  1. Terminal modua (Kontsola / CLI)
  2. Interfaze Grafikoa (GUI - Windows Forms)
  0. Irten
--------------------------------------------------------------------
Aukeratu [1/2/0] (Lehenetsia: 1):
```

### B) Zuzenean Interfaze Grafikoan (GUI)

Komando-lerroko `--gui` edo `-g` parametroarekin:

```bash
dotnet run -- --gui
```

### C) Zuzenean Terminal Moduan (CLI)

Komando-lerroko `--cli` edo `-c` parametroarekin:

```bash
dotnet run -- --cli
```

### D) Terminaletik Interfaze Grafikoara salto egitea

Terminaleko menuan zaudela, `G` (edo `10`) sakatuta Windows Forms leihoa irekiko da uneoro. Leihoa ixtean, terminaleko menura itzultzen da automatikoki.

---

## 4. Funtzionalitate Nagusiak

Bi moduek (CLI zein GUI) eragiketa multzo berdina eskaintzen dute:

1. **Erreserben Kudeaketa:**
   - Erreserba berria egin (pista, bazkidea, data, ordua, bonoa erabili ala ez).
   - Zerrenda osoa ikusi (aktiboak zein ezabatuak).
   - **Soft Delete:** Erreserba logikoki ezeztatu (`ezabatua = true`, pista libre uzten du beste erreserbentzat baina historikoa mantentzen du).
   - **Hard Delete:** Erreserba behin betiko ezabatu datu-basetik.
   - **Restore:** Ezeztatutako erreserba berreskuratu (pista libre badago ordu horretan).
   - Taula garbitu (probetarako).
2. **Pisten Kudeaketa:**
   - Pistak zerrendatu eta pista berriak gehitu.
   - Soft Delete: Pista eta berari lotutako erreserbak batera desgaitu transakzio bidez.
   - Restore: Desgaitutako pista berrezarri.
   - Hard Delete: Pista behin betiko ezabatu (erreserbarik ez badu, edo kaskadan).
3. **Bazkideen Kudeaketa:**
   - Bazkideak zerrendatu (bonoko saldoekin batera).
   - Bazkide berriak sortu (hasierako bonoko saio kopuruarekin).
   - Soft Delete: Bazkidea desgaitu.
   - Restore: Desgaitutako bazkidea berreskuratu.
   - Hard Delete: Behin betiko ezabatu (erreserbarik ez badu soilik).
4. **Bonoen Kudeaketa:**
   - Bonoen saldoak kontsultatu.
   - Bonoari saioak kargatu transakzio seguru batean.

---

## 5. Datu-Basearen Politika: Soft Delete eta ON DELETE RESTRICT

Proiektuak **Soft Delete** eredua lehenesten du eta datu-base mailan **ON DELETE RESTRICT** aplikatzen du kanpo-gakoetan:

- **Zergatik Soft Delete?**
  - **Historikoa babesteko:** Erreserba bat ezeztatzean, auditoretza, estatistika eta fakturazio datuak ez dira galtzen.
  - **Ustekabeko ezabaketen aurkako babesa:** Akatsez egindako ezeztapenak `restore` bidez leheneratu daitezke.
- **Zergatik ON DELETE RESTRICT?**
  - Pista edo bazkide bat behin betiko ezabatzen saiatuz gero (`DELETE FROM pista`), datu-baseak eragotzi egiten du (`Error 1451: Cannot delete or update a parent row`), erreserba historikoak umezurtz geratzea saihestuz.
  - Eskema honela definituta dago:
    ```sql
    CONSTRAINT fk_erreserbak_pistak FOREIGN KEY (pista_id)
        REFERENCES pistak(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_erreserbak_bazkideak FOREIGN KEY (bazkidea_id)
        REFERENCES bazkidea(id) ON DELETE RESTRICT ON UPDATE CASCADE
    ```

Dokumentazio osoa hemen kontsulta daiteke:

- Markdown formatuan: `doc/TRANSAKZIOAK_ETA_DELETE_JUSTIFIKAZIOA.md`
- Word formatuan: `../1T_ARIKETA_2_API_KIROLDEGIA_WEB_C_SHARP/TRANSAKZIOAK_ETA_DELETE_JUSTIFIKAZIOA.docx`

---

## 6. Abiarazteko Argibideak

### 1. Backend REST APIa abiarazi

Bezeroak datuak eskuratu ahal izateko, backend zerbitzaria martxan egon behar da:

```bash
cd ../API_kiroldegia_db/API_kiroldegia_db
dotnet run
```

APIa `http://localhost:5089` helbidean jarriko da entzuten.

### 2. C# Bezeroa abiarazi

Beste terminal batean:

```bash
cd kiroldegia_erreserbak_c_sharp_terminala
dotnet run
```

_Oharra: APIa beste helbide batean balego, `KIROLDEGIA_API_URL` ingurune-aldagaia erabil daiteke (adibidez: `set KIROLDEGIA_API_URL=http://localhost:5262`)._
