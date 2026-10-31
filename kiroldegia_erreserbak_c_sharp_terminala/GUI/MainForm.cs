using kiroldegia_erreserbak_c_sharp_terminala.GUI.Dialogs;
using kiroldegia_erreserbak_c_sharp_terminala.Models;
using kiroldegia_erreserbak_c_sharp_terminala.Services;

namespace kiroldegia_erreserbak_c_sharp_terminala.GUI;

public class MainForm : Form
{
    private readonly KiroldegiaApiClient _api;

    // Egoera kudeaketa eta tenporizadorea
    private bool _konektatuta = false;
    private bool _egiaztatzenAriDa = false;
    private readonly System.Windows.Forms.Timer _konexioTenporizadorea = new();

    // Goiko panela
    private readonly Label _lblIzenburua = new();
    private readonly Label _lblApiEgoera = new();
    private readonly Button _btnEgiaztatuKonexioa = new();
    private readonly Button _btnItzuliTerminalera = new();

    // TabControl
    private readonly TabControl _tabs = new();
    private readonly TabPage _tabBazkideak = new();
    private readonly TabPage _tabPistak = new();
    private readonly TabPage _tabErreserbak = new();
    private readonly TabPage _tabBonoak = new();

    // DataGridViews
    private readonly DataGridView _gridBazkideak = new();
    private readonly DataGridView _gridPistak = new();
    private readonly DataGridView _gridErreserbak = new();
    private readonly DataGridView _gridBonoak = new();

    // Checkboxes
    private readonly CheckBox _chkBazkideakEzabatuak = new();
    private readonly CheckBox _chkPistakEzabatuak = new();
    private readonly CheckBox _chkErreserbakEzabatuak = new();

    // Beheko egoera barra
    private readonly StatusStrip _statusStrip = new();
    private readonly ToolStripStatusLabel _statusLabel = new();

    // Memorian gordetako zerrendak (elkarrizketa-koadroetarako)
    private List<BazkideaDTO> _azkenBazkideak = [];
    private List<PistaDTO> _azkenPistak = [];
    private List<BonoDTO> _azkenBonoak = [];

    public MainForm(KiroldegiaApiClient api)
    {
        _api = api;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Ordiziako Kiroldegia - Kudeaketa Sistema (REST API GUI)";
        Size = new Size(1100, 750);
        MinimumSize = new Size(950, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

        // --- Goiko Panela ---
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 72,
            BackColor = Color.FromArgb(44, 62, 80),
            Padding = new Padding(15, 8, 15, 8)
        };

        var pnlButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 10, 0, 0),
            BackColor = Color.Transparent
        };

        var pnlTitles = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 4, 0, 0)
        };

        _lblIzenburua.Text = "ORDIZIAKO KIROLDEGIKO KUDEAKETA SISTEMA";
        _lblIzenburua.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        _lblIzenburua.ForeColor = Color.White;
        _lblIzenburua.Location = new Point(0, 2);
        _lblIzenburua.AutoSize = true;

        _lblApiEgoera.Text = $"API Konexioa: Egiaztatzen ({_api.BaseUrl})...";
        _lblApiEgoera.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _lblApiEgoera.ForeColor = Color.FromArgb(241, 196, 15);
        _lblApiEgoera.Location = new Point(0, 32);
        _lblApiEgoera.AutoSize = true;

        pnlTitles.Controls.Add(_lblIzenburua);
        pnlTitles.Controls.Add(_lblApiEgoera);

        _btnEgiaztatuKonexioa.Text = "Egiaztatu Konexioa";
        _btnEgiaztatuKonexioa.Size = new Size(150, 32);
        _btnEgiaztatuKonexioa.BackColor = Color.FromArgb(52, 152, 219);
        _btnEgiaztatuKonexioa.ForeColor = Color.White;
        _btnEgiaztatuKonexioa.FlatStyle = FlatStyle.Flat;
        _btnEgiaztatuKonexioa.Cursor = Cursors.Hand;
        _btnEgiaztatuKonexioa.Click += async (s, e) => await EgiaztatuKonexioaAsync(eskuzkoa: true);

        _btnItzuliTerminalera.Text = "Irten / Itzuli";
        _btnItzuliTerminalera.Size = new Size(110, 32);
        _btnItzuliTerminalera.BackColor = Color.FromArgb(127, 140, 141);
        _btnItzuliTerminalera.ForeColor = Color.White;
        _btnItzuliTerminalera.FlatStyle = FlatStyle.Flat;
        _btnItzuliTerminalera.Cursor = Cursors.Hand;
        _btnItzuliTerminalera.Click += (s, e) => Close();

        pnlButtons.Controls.Add(_btnEgiaztatuKonexioa);
        pnlButtons.Controls.Add(_btnItzuliTerminalera);

        // WinForms-en eskuineko panela lehenik gehitu behar da dock zuzen funtzionatzeko
        pnlHeader.Controls.Add(pnlButtons);
        pnlHeader.Controls.Add(pnlTitles);

        // --- Tabs ---
        _tabs.Dock = DockStyle.Fill;
        _tabs.Padding = new Point(12, 6);

        _tabBazkideak.Text = " Bazkideak ";
        _tabPistak.Text = " Pistak ";
        _tabErreserbak.Text = " Erreserbak ";
        _tabBonoak.Text = " Bonoak ";

        EraikiBazkideakTab();
        EraikiPistakTab();
        EraikiErreserbakTab();
        EraikiBonoakTab();

        _tabs.TabPages.AddRange([_tabBazkideak, _tabPistak, _tabErreserbak, _tabBonoak]);
        _tabs.SelectedIndexChanged += async (s, e) => await FitxaAldaketaKudeatuAsync();

        // --- Status Strip ---
        _statusStrip.Items.Add(_statusLabel);
        _statusLabel.Text = "Prest";

        Controls.Add(_tabs);
        Controls.Add(pnlHeader);
        Controls.Add(_statusStrip);

        // --- Konexio Tenporizadorea (3 segunduro atzealdean automatikoki egiaztatzeko) ---
        _konexioTenporizadorea.Interval = 3000;
        _konexioTenporizadorea.Tick += async (s, e) => await EgiaztatuKonexioaAutomatikoaAsync();

        Load += async (s, e) =>
        {
            await EgiaztatuKonexioaAsync(eskuzkoa: false);
            if (_konektatuta)
            {
                await KargatuDatuGuztiakAsync();
            }
            _konexioTenporizadorea.Start();
        };

        FormClosed += (s, e) =>
        {
            _konexioTenporizadorea.Stop();
            _konexioTenporizadorea.Dispose();
        };
    }

    // =========================================================================
    // KONEXIO EGOERAREN KUDEAKETA AUTOMATIKOA
    // =========================================================================
    private void MarkatuKonektatuta()
    {
        _konektatuta = true;
        _lblApiEgoera.Text = $"API Konexioa: KONEKTATUTA ({_api.BaseUrl})";
        _lblApiEgoera.ForeColor = Color.FromArgb(46, 204, 113);
    }

    private void MarkatuDeskonektatuta(string? mezua = null)
    {
        _konektatuta = false;
        string arrazoia = string.IsNullOrWhiteSpace(mezua) ? "Zerbitzaria itxita edo erantzun gabe" : mezua;
        if (arrazoia.Length > 70) arrazoia = arrazoia[..67] + "...";
        _lblApiEgoera.Text = $"API Konexioa: DESKONEKTATUTA ({arrazoia})";
        _lblApiEgoera.ForeColor = Color.FromArgb(231, 76, 60);
    }

    private async Task EgiaztatuKonexioaAutomatikoaAsync()
    {
        if (_egiaztatzenAriDa) return;
        _egiaztatzenAriDa = true;
        try
        {
            var (egoera, mezua) = await _api.EgiaztatuKonexioaAsync();
            if (egoera)
            {
                bool lehenDeskonektatuta = !_konektatuta;
                MarkatuKonektatuta();
                if (lehenDeskonektatuta)
                {
                    SetStatus("API zerbitzaria martxan jarri da. Datuak automatikoki kargatzen...");
                    await KargatuDatuGuztiakAsync();
                }
            }
            else
            {
                MarkatuDeskonektatuta(mezua);
            }
        }
        finally
        {
            _egiaztatzenAriDa = false;
        }
    }

    private async Task EgiaztatuKonexioaAsync(bool eskuzkoa = false)
    {
        if (eskuzkoa) SetStatus("API konexioa egiaztatzen...");
        var (konektatuta, mezua) = await _api.EgiaztatuKonexioaAsync();
        if (konektatuta)
        {
            bool lehenDeskonektatuta = !_konektatuta;
            MarkatuKonektatuta();
            SetStatus("APIa konektatuta eta erantzuteko prest.");
            if (eskuzkoa || lehenDeskonektatuta)
            {
                await KargatuDatuGuztiakAsync();
            }
        }
        else
        {
            MarkatuDeskonektatuta(mezua);
            SetStatus($"Kontuz: ezin izan da APIarekin konektatu ({_api.BaseUrl}). Ziurtatu 'API_kiroldegia_db' martxan dagoela.");
        }
    }

    private async Task FitxaAldaketaKudeatuAsync()
    {
        if (!_konektatuta) return;

        if (_tabs.SelectedTab == _tabBazkideak && _azkenBazkideak.Count == 0)
        {
            await KargatuBazkideakAsync();
        }
        else if (_tabs.SelectedTab == _tabPistak && _azkenPistak.Count == 0)
        {
            await KargatuPistakAsync();
        }
        else if (_tabs.SelectedTab == _tabErreserbak && _gridErreserbak.Rows.Count == 0)
        {
            await KargatuErreserbakAsync();
        }
        else if (_tabs.SelectedTab == _tabBonoak && _azkenBonoak.Count == 0)
        {
            await KargatuBonoakAsync();
        }
    }

    // =========================================================================
    // TAB 1: BAZKIDEAK
    // =========================================================================
    private void EraikiBazkideakTab()
    {
        var pnlBarra = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 45,
            Padding = new Padding(8, 6, 8, 6),
            BackColor = Color.FromArgb(236, 240, 241)
        };

        var btnKargatu = SortuBotoia("Birkargatu", Color.FromArgb(52, 152, 219));
        btnKargatu.Click += async (s, e) => await KargatuBazkideakAsync();

        var btnBerria = SortuBotoia("Bazkide Berria", Color.FromArgb(39, 174, 96));
        btnBerria.Click += async (s, e) => await SortuBazkideBerriaAsync();

        var btnSoftDelete = SortuBotoia("Soft Delete (Ezgaitu)", Color.FromArgb(230, 126, 34));
        btnSoftDelete.Click += async (s, e) => await EzabatuBazkideaHautatuaAsync(hard: false);

        var btnBerreskuratu = SortuBotoia("Berreskuratu", Color.FromArgb(46, 204, 113));
        btnBerreskuratu.Click += async (s, e) => await BerreskuratuBazkideaHautatuaAsync();

        var btnHardDelete = SortuBotoia("Hard Delete (Behin betiko)", Color.FromArgb(192, 57, 43));
        btnHardDelete.Click += async (s, e) => await EzabatuBazkideaHautatuaAsync(hard: true);

        _chkBazkideakEzabatuak.Text = "Erakutsi ezabatuak";
        _chkBazkideakEzabatuak.Checked = true;
        _chkBazkideakEzabatuak.AutoSize = true;
        _chkBazkideakEzabatuak.Margin = new Padding(12, 8, 6, 6);
        _chkBazkideakEzabatuak.CheckedChanged += async (s, e) => await KargatuBazkideakAsync();

        pnlBarra.Controls.AddRange([btnKargatu, btnBerria, btnSoftDelete, btnBerreskuratu, btnHardDelete, _chkBazkideakEzabatuak]);

        KonfiguratuGrid(_gridBazkideak);
        _gridBazkideak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", Width = 60 });
        _gridBazkideak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Izena", DataPropertyName = "Izena", Width = 150 });
        _gridBazkideak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Abizena", DataPropertyName = "Abizena", Width = 160 });
        _gridBazkideak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Emaila", DataPropertyName = "Emaila", Width = 220 });
        _gridBazkideak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Bono Saioak", DataPropertyName = "BonoSaioak", Width = 110 });
        _gridBazkideak.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Ezabatua", DataPropertyName = "Ezabatua", Width = 80 });
        _gridBazkideak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ezabaketa Data", DataPropertyName = "EzabaketaData", Width = 160 });

        _tabBazkideak.Controls.Add(_gridBazkideak);
        _tabBazkideak.Controls.Add(pnlBarra);
    }

    // =========================================================================
    // TAB 2: PISTAK
    // =========================================================================
    private void EraikiPistakTab()
    {
        var pnlBarra = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 45,
            Padding = new Padding(8, 6, 8, 6),
            BackColor = Color.FromArgb(236, 240, 241)
        };

        var btnKargatu = SortuBotoia("Birkargatu", Color.FromArgb(52, 152, 219));
        btnKargatu.Click += async (s, e) => await KargatuPistakAsync();

        var btnBerria = SortuBotoia("Pista Berria", Color.FromArgb(39, 174, 96));
        btnBerria.Click += async (s, e) => await SortuPistaBerriaAsync();

        var btnSoftDelete = SortuBotoia("Soft Delete (Ezgaitu)", Color.FromArgb(230, 126, 34));
        btnSoftDelete.Click += async (s, e) => await EzabatuPistaHautatuaAsync(hard: false);

        var btnBerreskuratu = SortuBotoia("Berreskuratu", Color.FromArgb(46, 204, 113));
        btnBerreskuratu.Click += async (s, e) => await BerreskuratuPistaHautatuaAsync();

        var btnHardDelete = SortuBotoia("Hard Delete (Behin betiko)", Color.FromArgb(192, 57, 43));
        btnHardDelete.Click += async (s, e) => await EzabatuPistaHautatuaAsync(hard: true);

        _chkPistakEzabatuak.Text = "Erakutsi ezabatuak";
        _chkPistakEzabatuak.Checked = true;
        _chkPistakEzabatuak.AutoSize = true;
        _chkPistakEzabatuak.Margin = new Padding(12, 8, 6, 6);
        _chkPistakEzabatuak.CheckedChanged += async (s, e) => await KargatuPistakAsync();

        pnlBarra.Controls.AddRange([btnKargatu, btnBerria, btnSoftDelete, btnBerreskuratu, btnHardDelete, _chkPistakEzabatuak]);

        KonfiguratuGrid(_gridPistak);
        _gridPistak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", Width = 60 });
        _gridPistak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Pistaren Izena", DataPropertyName = "Izena", Width = 200 });
        _gridPistak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kirola / Mota", DataPropertyName = "Kirola", Width = 180 });
        _gridPistak.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Ezabatua", DataPropertyName = "Ezabatua", Width = 90 });
        _gridPistak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ezabaketa Data", DataPropertyName = "EzabaketaData", Width = 170 });

        _tabPistak.Controls.Add(_gridPistak);
        _tabPistak.Controls.Add(pnlBarra);
    }

    // =========================================================================
    // TAB 3: ERRESERBAK
    // =========================================================================
    private void EraikiErreserbakTab()
    {
        var pnlBarra = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 45,
            Padding = new Padding(8, 6, 8, 6),
            BackColor = Color.FromArgb(236, 240, 241)
        };

        var btnKargatu = SortuBotoia("Birkargatu", Color.FromArgb(52, 152, 219));
        btnKargatu.Click += async (s, e) => await KargatuErreserbakAsync();

        var btnBerria = SortuBotoia("Erreserba Berria", Color.FromArgb(39, 174, 96));
        btnBerria.Click += async (s, e) => await SortuErreserbaBerriaAsync();

        var btnSoftDelete = SortuBotoia("Soft Delete (Ezeztatu)", Color.FromArgb(230, 126, 34));
        btnSoftDelete.Click += async (s, e) => await EzabatuErreserbaHautatuaAsync(hard: false);

        var btnBerreskuratu = SortuBotoia("Berreskuratu", Color.FromArgb(46, 204, 113));
        btnBerreskuratu.Click += async (s, e) => await BerreskuratuErreserbaHautatuaAsync();

        var btnHardDelete = SortuBotoia("Hard Delete", Color.FromArgb(192, 57, 43));
        btnHardDelete.Click += async (s, e) => await EzabatuErreserbaHautatuaAsync(hard: true);

        var btnGarbitu = SortuBotoia("Garbitu Guztiak", Color.FromArgb(142, 68, 173));
        btnGarbitu.Click += async (s, e) => await GarbituErreserbakAsync();

        _chkErreserbakEzabatuak.Text = "Erakutsi ezabatuak";
        _chkErreserbakEzabatuak.Checked = true;
        _chkErreserbakEzabatuak.AutoSize = true;
        _chkErreserbakEzabatuak.Margin = new Padding(12, 8, 6, 6);
        _chkErreserbakEzabatuak.CheckedChanged += async (s, e) => await KargatuErreserbakAsync();

        pnlBarra.Controls.AddRange([btnKargatu, btnBerria, btnSoftDelete, btnBerreskuratu, btnHardDelete, btnGarbitu, _chkErreserbakEzabatuak]);

        KonfiguratuGrid(_gridErreserbak);
        _gridErreserbak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", Width = 60 });
        _gridErreserbak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Pista", DataPropertyName = "PistaIzena", Width = 140 });
        _gridErreserbak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kirola", DataPropertyName = "Kirola", Width = 110 });
        _gridErreserbak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Bazkidea", DataPropertyName = "BazkideaIzena", Width = 160 });
        _gridErreserbak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Emaila", DataPropertyName = "BazkideaEmaila", Width = 180 });
        _gridErreserbak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Data", DataPropertyName = "Data", Width = 100 });
        _gridErreserbak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ordua", DataPropertyName = "Ordua", Width = 90 });
        _gridErreserbak.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Ezabatua", DataPropertyName = "Ezabatua", Width = 80 });
        _gridErreserbak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ezabaketa Data", DataPropertyName = "EzabaketaData", Width = 150 });

        _tabErreserbak.Controls.Add(_gridErreserbak);
        _tabErreserbak.Controls.Add(pnlBarra);
    }

    // =========================================================================
    // TAB 4: BONOAK
    // =========================================================================
    private void EraikiBonoakTab()
    {
        var pnlBarra = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 45,
            Padding = new Padding(8, 6, 8, 6),
            BackColor = Color.FromArgb(236, 240, 241)
        };

        var btnKargatu = SortuBotoia("Birkargatu", Color.FromArgb(52, 152, 219));
        btnKargatu.Click += async (s, e) => await KargatuBonoakAsync();

        var btnKargatuSaioak = SortuBotoia("Saioak Kargatu / Bonoa Sortu", Color.FromArgb(39, 174, 96));
        btnKargatuSaioak.Click += async (s, e) => await KargatuBonoSaioakAsync();

        pnlBarra.Controls.AddRange([btnKargatu, btnKargatuSaioak]);

        KonfiguratuGrid(_gridBonoak);
        _gridBonoak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Bazkidearen Emaila", DataPropertyName = "Emaila", Width = 260 });
        _gridBonoak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Saldo Saioak", DataPropertyName = "Saioak", Width = 130 });
        _gridBonoak.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Ezabatua", DataPropertyName = "Ezabatua", Width = 90 });
        _gridBonoak.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ezabaketa Data", DataPropertyName = "EzabaketaData", Width = 180 });

        _tabBonoak.Controls.Add(_gridBonoak);
        _tabBonoak.Controls.Add(pnlBarra);
    }

    // =========================================================================
    // GRID ETA BOTOI LAGUNTZAILEAK
    // =========================================================================
    private static void KonfiguratuGrid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        grid.AutoGenerateColumns = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.RowHeadersVisible = false;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        grid.EnableHeadersVisualStyles = false;
        grid.RowTemplate.Height = 28;
    }

    private static Button SortuBotoia(string text, Color backColor)
    {
        return new Button
        {
            Text = text,
            AutoSize = true,
            Height = 32,
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(3, 4, 3, 4),
            Cursor = Cursors.Hand
        };
    }

    private static string FormatMezua<T>(ApiResult<T> res) =>
        string.IsNullOrEmpty(res.Xehetasuna) ? res.Mezua : $"{res.Mezua} ({res.Xehetasuna})";

    // =========================================================================
    // API ERAGIKETAK ETA DATU KARGAK
    // =========================================================================
    private async Task KargatuDatuGuztiakAsync()
    {
        await KargatuBazkideakAsync();
        await KargatuPistakAsync();
        await KargatuErreserbakAsync();
        await KargatuBonoakAsync();
    }

    // --- Bazkideak ---
    private async Task KargatuBazkideakAsync()
    {
        SetStatus("Bazkideak kargatzen...");
        bool barne = _chkBazkideakEzabatuak.Checked;
        var res = await _api.LortuBazkideakAsync(barne);
        if (res.Arrakasta && res.DatuZerrenda != null)
        {
            MarkatuKonektatuta();
            _azkenBazkideak = res.DatuZerrenda;
            _gridBazkideak.DataSource = null;
            _gridBazkideak.DataSource = _azkenBazkideak;
            MargotuEzabatuak(_gridBazkideak);
            SetStatus($"Bazkideak kargatuta: {_azkenBazkideak.Count} erregistro.");
        }
        else
        {
            if (res.StatusCode == 503 || res.Mezua.Contains("Sare-errorea") || res.Mezua.Contains("konektatu"))
            {
                MarkatuDeskonektatuta(res.Mezua);
            }
            SetStatus($"Errorea bazkideak kargatzean: {FormatMezua(res)}");
        }
    }

    private async Task SortuBazkideBerriaAsync()
    {
        using var dlg = new BazkideBerriaDialog();
        if (dlg.ShowDialog(this) == DialogResult.OK && dlg.EmaitzaDTO != null)
        {
            SetStatus("Bazkidea sortzen...");
            var res = await _api.SortuBazkideaAsync(dlg.EmaitzaDTO);
            if (res.Arrakasta)
            {
                MarkatuKonektatuta();
                MessageBox.Show("Bazkidea arrakastaz sortu da!", "Arrakasta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await KargatuBazkideakAsync();
                await KargatuBonoakAsync();
            }
            else
            {
                MessageBox.Show($"Ezin izan da bazkidea sortu:\n{FormatMezua(res)}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus($"Errorea: {FormatMezua(res)}");
            }
        }
    }

    private async Task EzabatuBazkideaHautatuaAsync(bool hard)
    {
        if (_gridBazkideak.CurrentRow?.DataBoundItem is not BazkideaDTO b)
        {
            MessageBox.Show("Aukeratu bazkide bat zerrendatik lehenik.", "Abisua", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string mota = hard ? "BEHIN BETIKO (Hard Delete)" : "LOGIKOKI (Soft Delete)";
        var confirm = MessageBox.Show(
            $"Ziur zaude #{b.Id} {b.Izena} {b.Abizena} bazkidea {mota} ezabatu nahi duzula?",
            "Ezabatu berretsi",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm == DialogResult.Yes)
        {
            SetStatus($"Bazkidea ezabatzen ({mota})...");
            var res = await _api.EzabatuBazkideaAsync(b.Id, hard);
            if (res.Arrakasta)
            {
                MarkatuKonektatuta();
                MessageBox.Show("Bazkidea arrakastaz ezabatu da.", "Arrakasta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await KargatuBazkideakAsync();
            }
            else
            {
                MessageBox.Show($"Errorea bazkidea ezabatzean:\n{FormatMezua(res)}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus($"Errorea: {FormatMezua(res)}");
            }
        }
    }

    private async Task BerreskuratuBazkideaHautatuaAsync()
    {
        if (_gridBazkideak.CurrentRow?.DataBoundItem is not BazkideaDTO b)
        {
            MessageBox.Show("Aukeratu bazkide bat zerrendatik lehenik.", "Abisua", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetStatus("Bazkidea berreskuratzen...");
        var res = await _api.BerreskuratuBazkideaAsync(b.Id);
        if (res.Arrakasta)
        {
            MarkatuKonektatuta();
            MessageBox.Show("Bazkidea arrakastaz berreskuratu da!", "Arrakasta", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await KargatuBazkideakAsync();
        }
        else
        {
            MessageBox.Show($"Errorea bazkidea berreskuratzean:\n{FormatMezua(res)}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus($"Errorea: {FormatMezua(res)}");
        }
    }

    // --- Pistak ---
    private async Task KargatuPistakAsync()
    {
        SetStatus("Pistak kargatzen...");
        bool barne = _chkPistakEzabatuak.Checked;
        var res = await _api.LortuPistakAsync(barne);
        if (res.Arrakasta && res.DatuZerrenda != null)
        {
            MarkatuKonektatuta();
            _azkenPistak = res.DatuZerrenda;
            _gridPistak.DataSource = null;
            _gridPistak.DataSource = _azkenPistak;
            MargotuEzabatuak(_gridPistak);
            SetStatus($"Pistak kargatuta: {_azkenPistak.Count} pista.");
        }
        else
        {
            if (res.StatusCode == 503 || res.Mezua.Contains("Sare-errorea") || res.Mezua.Contains("konektatu"))
            {
                MarkatuDeskonektatuta(res.Mezua);
            }
            SetStatus($"Errorea pistak kargatzean: {FormatMezua(res)}");
        }
    }

    private async Task SortuPistaBerriaAsync()
    {
        using var dlg = new PistaBerriaDialog();
        if (dlg.ShowDialog(this) == DialogResult.OK && dlg.EmaitzaDTO != null)
        {
            SetStatus("Pista sortzen...");
            var res = await _api.SortuPistaAsync(dlg.EmaitzaDTO);
            if (res.Arrakasta)
            {
                MarkatuKonektatuta();
                MessageBox.Show("Pista arrakastaz sortu da!", "Arrakasta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await KargatuPistakAsync();
            }
            else
            {
                MessageBox.Show($"Ezin izan da pista sortu:\n{FormatMezua(res)}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus($"Errorea: {FormatMezua(res)}");
            }
        }
    }

    private async Task EzabatuPistaHautatuaAsync(bool hard)
    {
        if (_gridPistak.CurrentRow?.DataBoundItem is not PistaDTO p)
        {
            MessageBox.Show("Aukeratu pista bat zerrendatik lehenik.", "Abisua", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string mota = hard ? "BEHIN BETIKO (Hard Delete)" : "LOGIKOKI (Soft Delete)";
        var confirm = MessageBox.Show(
            $"Ziur zaude #{p.Id} {p.Izena} ({p.Kirola}) pista {mota} ezabatu nahi duzula?",
            "Ezabatu berretsi",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm == DialogResult.Yes)
        {
            SetStatus($"Pista ezabatzen ({mota})...");
            var res = await _api.EzabatuPistaAsync(p.Id, hard);
            if (res.Arrakasta)
            {
                MarkatuKonektatuta();
                MessageBox.Show("Pista arrakastaz ezabatu da.", "Arrakasta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await KargatuPistakAsync();
            }
            else
            {
                MessageBox.Show($"Errorea pista ezabatzean:\n{FormatMezua(res)}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus($"Errorea: {FormatMezua(res)}");
            }
        }
    }

    private async Task BerreskuratuPistaHautatuaAsync()
    {
        if (_gridPistak.CurrentRow?.DataBoundItem is not PistaDTO p)
        {
            MessageBox.Show("Aukeratu pista bat zerrendatik lehenik.", "Abisua", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetStatus("Pista berreskuratzen...");
        var res = await _api.BerreskuratuPistaAsync(p.Id);
        if (res.Arrakasta)
        {
            MarkatuKonektatuta();
            MessageBox.Show("Pista arrakastaz berreskuratu da!", "Arrakasta", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await KargatuPistakAsync();
        }
        else
        {
            MessageBox.Show($"Errorea pista berreskuratzean:\n{FormatMezua(res)}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus($"Errorea: {FormatMezua(res)}");
        }
    }

    // --- Erreserbak ---
    private async Task KargatuErreserbakAsync()
    {
        SetStatus("Erreserbak kargatzen...");
        bool barne = _chkErreserbakEzabatuak.Checked;
        var res = await _api.LortuErreserbakAsync(barne);
        if (res.Arrakasta && res.DatuZerrenda != null)
        {
            MarkatuKonektatuta();
            _gridErreserbak.DataSource = null;
            _gridErreserbak.DataSource = res.DatuZerrenda;
            MargotuEzabatuak(_gridErreserbak);
            SetStatus($"Erreserbak kargatuta: {res.DatuZerrenda.Count} erreserba.");
        }
        else
        {
            if (res.StatusCode == 503 || res.Mezua.Contains("Sare-errorea") || res.Mezua.Contains("konektatu"))
            {
                MarkatuDeskonektatuta(res.Mezua);
            }
            SetStatus($"Errorea erreserbak kargatzean: {FormatMezua(res)}");
        }
    }

    private async Task SortuErreserbaBerriaAsync()
    {
        if (_azkenPistak.Count == 0 || _azkenBazkideak.Count == 0)
        {
            await KargatuPistakAsync();
            await KargatuBazkideakAsync();
        }

        using var dlg = new ErreserbaBerriaDialog(_azkenPistak, _azkenBazkideak);
        if (dlg.ShowDialog(this) == DialogResult.OK && dlg.EmaitzaDTO != null)
        {
            SetStatus("Erreserba egiten APIaren bidez...");
            var res = await _api.SortuErreserbaAsync(dlg.EmaitzaDTO, dlg.EmaitzaDTO.Itxaron);
            if (res.Arrakasta)
            {
                MarkatuKonektatuta();
                MessageBox.Show("Erreserba arrakastaz gorde da!", "Arrakasta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await KargatuErreserbakAsync();
                await KargatuBazkideakAsync();
                await KargatuBonoakAsync();
            }
            else
            {
                MessageBox.Show($"Ezin izan da erreserba egin:\n{FormatMezua(res)}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus($"Errorea: {FormatMezua(res)}");
            }
        }
    }

    private async Task EzabatuErreserbaHautatuaAsync(bool hard)
    {
        if (_gridErreserbak.CurrentRow?.DataBoundItem is not ErreserbaDTO e)
        {
            MessageBox.Show("Aukeratu erreserba bat zerrendatik lehenik.", "Abisua", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string mota = hard ? "BEHIN BETIKO (Hard Delete)" : "LOGIKOKI (Soft Delete)";
        var confirm = MessageBox.Show(
            $"Ziur zaude #{e.Id} erreserba ({e.PistaIzena} - {e.Data} {e.Ordua}) {mota} ezabatu nahi duzula?",
            "Ezabatu berretsi",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm == DialogResult.Yes)
        {
            SetStatus($"Erreserba ezabatzen ({mota})...");
            var res = await _api.EzabatuErreserbaAsync(e.Id, hard);
            if (res.Arrakasta)
            {
                MarkatuKonektatuta();
                MessageBox.Show("Erreserba arrakastaz ezabatu da.", "Arrakasta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await KargatuErreserbakAsync();
            }
            else
            {
                MessageBox.Show($"Errorea erreserba ezabatzean:\n{FormatMezua(res)}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus($"Errorea: {FormatMezua(res)}");
            }
        }
    }

    private async Task BerreskuratuErreserbaHautatuaAsync()
    {
        if (_gridErreserbak.CurrentRow?.DataBoundItem is not ErreserbaDTO e)
        {
            MessageBox.Show("Aukeratu erreserba bat zerrendatik lehenik.", "Abisua", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetStatus("Erreserba berreskuratzen...");
        var res = await _api.BerreskuratuErreserbaAsync(e.Id);
        if (res.Arrakasta)
        {
            MarkatuKonektatuta();
            MessageBox.Show("Erreserba arrakastaz berreskuratu da!", "Arrakasta", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await KargatuErreserbakAsync();
        }
        else
        {
            MessageBox.Show($"Errorea erreserba berreskuratzean:\n{FormatMezua(res)}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus($"Errorea: {FormatMezua(res)}");
        }
    }

    private async Task GarbituErreserbakAsync()
    {
        var confirm = MessageBox.Show(
            "KONTUZ: Ekintza honek erreserba guztiak garbituko ditu datu-basetik.\nZiur zaude jarraitu nahi duzula?",
            "Garbitu Erreserba Guztiak",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm == DialogResult.Yes)
        {
            SetStatus("Erreserba taula garbitzen...");
            var res = await _api.GarbituErreserbakAsync();
            if (res.Arrakasta)
            {
                MarkatuKonektatuta();
                MessageBox.Show("Erreserba guztiak arrakastaz garbitu dira.", "Arrakasta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await KargatuErreserbakAsync();
            }
            else
            {
                MessageBox.Show($"Errorea erreserbak garbitzean:\n{FormatMezua(res)}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus($"Errorea: {FormatMezua(res)}");
            }
        }
    }

    // --- Bonoak ---
    private async Task KargatuBonoakAsync()
    {
        SetStatus("Bonoak kargatzen...");
        var res = await _api.LortuBonoakAsync();
        if (res.Arrakasta && res.DatuZerrenda != null)
        {
            MarkatuKonektatuta();
            _azkenBonoak = res.DatuZerrenda;
            _gridBonoak.DataSource = null;
            _gridBonoak.DataSource = _azkenBonoak;
            MargotuEzabatuak(_gridBonoak);
            SetStatus($"Bonoak kargatuta: {_azkenBonoak.Count} bono.");
        }
        else
        {
            if (res.StatusCode == 503 || res.Mezua.Contains("Sare-errorea") || res.Mezua.Contains("konektatu"))
            {
                MarkatuDeskonektatuta(res.Mezua);
            }
            SetStatus($"Errorea bonoak kargatzean: {FormatMezua(res)}");
        }
    }

    private async Task KargatuBonoSaioakAsync()
    {
        var emailak = _azkenBazkideak.Select(b => b.Emaila).Where(e => !string.IsNullOrEmpty(e)).Select(e => e!).ToList();
        if (emailak.Count == 0)
        {
            await KargatuBazkideakAsync();
            emailak = _azkenBazkideak.Select(b => b.Emaila).Where(e => !string.IsNullOrEmpty(e)).Select(e => e!).ToList();
        }

        using var dlg = new BonoKargatuDialog(emailak);
        if (dlg.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(dlg.AukeratutakoEmaila))
        {
            SetStatus("Bonoari saioak kargatzen...");
            var res = await _api.KargatuSaioakAsync(dlg.AukeratutakoEmaila, dlg.AukeratutakoSaioak);
            if (res.Arrakasta)
            {
                MarkatuKonektatuta();
                MessageBox.Show("Bonoaren saioak arrakastaz eguneratu dira!", "Arrakasta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await KargatuBonoakAsync();
                await KargatuBazkideakAsync();
            }
            else
            {
                MessageBox.Show($"Errorea bonoa kargatzean:\n{FormatMezua(res)}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus($"Errorea: {FormatMezua(res)}");
            }
        }
    }

    private static void MargotuEzabatuak(DataGridView grid)
    {
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.DataBoundItem != null)
            {
                bool ezabatua = false;
                if (row.DataBoundItem is BazkideaDTO b) ezabatua = b.Ezabatua;
                else if (row.DataBoundItem is PistaDTO p) ezabatua = p.Ezabatua;
                else if (row.DataBoundItem is ErreserbaDTO e) ezabatua = e.Ezabatua;
                else if (row.DataBoundItem is BonoDTO bo) ezabatua = bo.Ezabatua;

                if (ezabatua)
                {
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(149, 165, 166);
                    row.DefaultCellStyle.BackColor = Color.FromArgb(250, 235, 235);
                }
            }
        }
    }

    private void SetStatus(string message)
    {
        _statusLabel.Text = message;
    }
}
