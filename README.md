# Kiroldegia Erreserbak - C# Bezeroa (Terminala eta Interfaze Grafikoa - GUI)

Ordiziako Majori Kiroldegiko erreserbak kudeatzeko C# aplikazio modularra (.NET 10.0-windows).

Aplikazio honek **bi exekuzio modu** ditu bateragarritasun osoz:
1. **Terminal modua (Kontsola / CLI):** Jatorrizko testu-menua, arina eta azkarra.
2. **Interfaze Grafikoa (GUI - Windows Forms):** Leiho bisuala taula dinamikoekin (DataGridView), fitxekin (Bazkideak, Pistak, Erreserbak, Bonoak), botoiekin eta formulario laguntzaileekin.

Aplikazioak ez du datu-basera zuzenean konektatzen; horren ordez, `API_kiroldegia_db` REST API zerbitzariaren bidez komunikatzen da HTTP (`HttpClient`) bidez, JSON datuak trukatuz.

---

## 1. Arkitektura Orokorra

Aplikazioak geruzen araberako banaketa garbia jarraitzen du, bezeroaren bi moduek zerbitzu- eta eredu-geruza bera partekatuz:

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

Kodea garbi, irakurgarri eta erantzukizun bakarrarekin (Single Responsibility Principle) antolatzeko, fitxategiak honela banatuta daude:

```text
kiroldegia_erreserbak_c_sharp_terminala/
│
├── Program.cs                  # Hasierako abiarazlea: CLI/GUI hautatzailea eta argumentu-analisia
│
├── GUI/                        # Interfaze Grafikoa (Windows Forms)
│   ├── GuiLauncher.cs          # GUI hari seguruan (STA) abiarazteko arduraduna
│   ├── MainForm.cs             # Leiho nagusia fitxekin: Bazkideak, Pistak, Erreserbak, Bonoak
│   └── Dialogs/                # Datuak sartzeko elkarrizketa-koadroak
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
└── Models/                     # DTOak eta JSON datu-ereduak
    ├── ApiResponse.cs          # ApiResponse<T> eta ApiError eredua
    ├── Bazkidea.cs             # BazkideaDTO eta BazkideaSortuDTO
    ├── Pista.cs                # PistaDTO eta PistaSortuDTO
    ├── Erreserba.cs            # ErreserbaDTO eta ErreserbaSortuDTO
    └── Bonoa.cs                # BonoDTO eta BonoKargatuDTO
```

---

## 3. Aplikazioa Bi Modutara Abiaraztea

Aplikazioa nahi den moduan abiaraz daiteke:

### A) Interaktiboki (Menu bidez aukeratuz)
Argumenturik gabe abiarazten bada, hasieran aukeraketa eskaintzen du:
```bash
dotnet run
```
```text
Aukeratu aplikazioa nola exekutatu nahi duzun:
  1. Terminal modua (Kontsola / CLI)
  2. Interfaze Grafikoa (GUI - Windows Forms)
  0. Irten
```

### B) Zuzenean Interfaze Grafikoan (GUI)
Komando-lerrotik `--gui` edo `-g` pasatuta:
```bash
dotnet run -- --gui
```

### C) Zuzenean Terminal Moduan (CLI)
Komando-lerrotik `--cli` edo `-c` pasatuta:
```bash
dotnet run -- --cli
```

### D) Terminaletik Interfaze Grafikoara salto egitea
Terminaleko menuan zaudela, `G` sakatuta Windows Forms leihoa irekitzen da. Leihoa ixtean, terminalera itzultzen da automatikoki.

---

## 4. Funtzionalitate Nagusiak

Bi moduek (CLI eta GUI) kudeaketa osoa eskaintzen dute:

1. **Erreserben Kudeaketa:**
   - Erreserba berria egin (pista, bazkidea, data, ordua, bonoa erabili ala ez).
   - Zerrenda osoa ikusi (aktiboak zein ezabatuak).
   - **Soft Delete:** Erreserba logikoki ezeztatu (pista libre uzten du, baina erregistroa gordetzen da).
   - **Hard Delete:** Erreserba behin betiko ezabatu.
   - **Restore:** Ezeztatutako erreserba berreskuratu (pista libre badago).
   - Taula garbitu (probetarako).
2. **Pisten Kudeaketa:**
   - Pistak zerrendatu eta pista berriak gehitu.
   - Soft Delete, Restore eta Hard Delete.
3. **Bazkideen Kudeaketa:**
   - Bazkideak zerrendatu (bonoko saldoekin).
   - Bazkide berriak sortu (hasierako bono aukerarekin).
   - Soft Delete, Restore eta Hard Delete.
4. **Bonoen Kudeaketa:**
   - Bonoen saldoak kontsultatu.
   - Bonoari saioak kargatu transakzio seguruan.
