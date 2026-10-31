# Kiroldegia Erreserbak - C# Terminala (REST API Bezero Modularra)

Ordiziako Majori Kiroldegiko erreserbak kudeatzeko C# kontsola-aplikazio modularra (.NET 10.0).

Aplikazio honek ez du datu-basera zuzenean konektatzen; horren ordez, API_kiroldegia_db REST API zerbitzariaren bidez komunikatzen da HTTP (HttpClient) erabiliz.

---

## 1. Arkitektura Orokorra

Aplikazioak geruzen araberako banaketa jarraitzen du:

```text
+-------------------------------------------------------+
|        kiroldegia_erreserbak_c_sharp_terminala        |
|               (C# Kontsola Bezeroa)                   |
|                                                       |
|   +--------------+    +---------------------------+   |
|   |  Program.cs  |--->| Kudeatzaileak (UI/Logika) |   |
|   +--------------+    +---------------------------+   |
|                                     |                 |
|                                     v                 |
|                       +---------------------------+   |
|                       |    Services (API Client)  |   |
|                       +---------------------------+   |
+-------------------------------------|-----------------+
                                      |
                                      | HTTP JSON (GET, POST, PUT, DELETE)
                                      v http://localhost:5089/api/
+-------------------------------------------------------+
|                   API_kiroldegia_db                   |
|              (ASP.NET Core REST Web API)              |
|                                                       |
|  - Transakzioak (BeginTransaction, Commit, Rollback)  |
|  - Blokeo pesimistak (SELECT ... FOR UPDATE)          |
|  - Konkurrentzia eta isolamendua (Read Committed)     |
|  - Salbuespenen kontrola (MySQL 1062, 1213, 1205)     |
+-------------------------------------------------------+
                                      |
                                      | ADO.NET (MySqlConnector)
                                      v
+-------------------------------------------------------+
|                MySQL InnoDB Datu-Basea                |
|                     (kiroldegia)                      |
+-------------------------------------------------------+
```

	---

## 2. Proiektuaren Egitura eta Antolaketa

Kodea garbi, irakurgarri eta erantzukizun bakarrarekin (Single Responsibility Principle) antolatzeko, fitxategiak honela banatu dira:

```text
kiroldegia_erreserbak_c_sharp_terminala/
│
├── Program.cs                  # Menua eta aplikazioaren ziklo nagusia soilik
│
├── Kudeatzaileak/              # Erabiltzailearen elkarrekintza eta kontsolako menuak
│   ├── BazkideaKudeatzailea.cs # Bazkideen azpimenua, zerrendatzea, sortzea, ezabaketak eta berreskuratzea
│   ├── PistaKudeatzailea.cs    # Pisten azpimenua, zerrendatzea, sortzea, ezabaketak eta berreskuratzea
│   ├── ErreserbaKudeatzailea.cs# Erreserben formularioa, zerrendatzea, ezabaketak, berreskuratzea eta garbiketa
│   ├── BonoaKudeatzailea.cs    # Bonoen azpimenua, saldoak ikustea, kargatzea eta sortzea
│   └── KontsolaLaguntzailea.cs # Kontsolako pantaila-garbiketa, izenburuak eta API emaitzak koloretan formatzea
│
├── Services/                   # REST API bezeroa (partial class KiroldegiaApiClient bidez banatua)
│   ├── ApiResult.cs            # ApiResult<T> erantzun generikoaren bilgarria
│   ├── KiroldegiaApiClient.cs  # HttpClient oinarria, BaseUrl, konexio-egiaztapena eta HTTP metodo laguntzaileak
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

## 3. Menu Nagusiko Aukerak

1. **Erreserba berria egin (POST /api/erreserbak):**

   - Bazkidea aukeratu (saldoak ikusgai).
   - Pista aukeratu (desgaituta daudenak adierazita).
   - Data eta ordua zehaztu.
   - Bonoaren bidez ordaintzeko aukera (saio bat deskontatuz transakzioan).
   - Konkurrentzia-blokeoa simulatzeko geldialdia ezartzeko aukera (milisegundotan).
2. **Egun dauden erreserbak ikusi (GET /api/erreserbak):**

   - Erreserba aktiboak berdez eta Soft-Delete bidez ezabatutakoak grisez.
3. **Erreserba ezabatu - Soft Delete (DELETE /api/erreserbak/{id}?hard=false):**

   - Erreserba logikoki desgaitzen du (ezabatua=true), pista libre utziz beste erreserbentzat.
4. **Erreserba ezabatu - Hard Delete (DELETE /api/erreserbak/{id}?hard=true):**

   - Erreserba behin betiko ezabatzen du datu-basetik.
5. **Soft-Delete erreserba berreskuratu (PUT /api/erreserbak/{id}/restore):**

   - Ezeztatutako erreserba berrezartzen du, betiere ordu horretan pista libre badago eta pista bera aktibo badago.
6. **Pisten Kudeaketa (PistaKudeatzailea):**

   - Pistak zerrendatu.
   - Pista berria sortu (POST /api/pistak).
   - Soft Delete: pista eta harekin erlazionatutako erreserbak batera desgaitu (DELETE /api/pistak/{id}?hard=false).
   - Hard Delete: pista eta erreserbak behin betiko ezabatu (DELETE /api/pistak/{id}?hard=true).
   - Restore: desgaitutako pista berreskuratu (PUT /api/pistak/{id}/restore).
7. **Bazkideen Kudeaketa (BazkideaKudeatzailea):**

   - Bazkideak zerrendatu (saldoekin).
   - Bazkide berria sortu (POST /api/bazkideak).
   - Soft Delete (DELETE /api/bazkideak/{id}?hard=false).
   - Hard Delete: erreserbarik ez badu soilik ezabatu (DELETE /api/bazkideak/{id}?hard=true).
   - Restore: desgaitutako bazkidea berreskuratu (PUT /api/bazkideak/{id}/restore).
8. **Bonoen Kudeaketa (BonoaKudeatzailea):**

   - Bonoen zerrenda eta saldoak ikusi (GET /api/bonoak).
   - Bonoari saioak gehitu transakzio seguru batean (PUT /api/bonoak/{email}/kargatu).
   - Bono berria sortu edo erosi (POST /api/bonoak).
9. **Erreserba taula garbitu (DELETE /api/erreserbak/garbitu):**

   - Erreserba guztiak hustu probak hutsetik hasteko.
10. **Irten.**

---

## 4. Abiarazteko Argibideak

### 1. Backend APIa martxan jarri

Terminaleko bezeroak funtziona dezan, backend APIak entzuten egon behar du:

```bash
cd ../API_kiroldegia_db/API_kiroldegia_db
dotnet run
```

APIa lehenetsita http://localhost:5089 helbidean jarriko da martxan.

### 2. Terminaleko aplikazioa abiarazi

Beste kontsola batean:

```bash
cd kiroldegia_erreserbak_c_sharp_terminala
dotnet run
```

Oharra: APIa beste helbide edo ataka batean egonez gero, KIROLDEGIA_API_URL ingurune-aldagaia erabil daiteke (adibidez: set KIROLDEGIA_API_URL=http://localhost:5262).
