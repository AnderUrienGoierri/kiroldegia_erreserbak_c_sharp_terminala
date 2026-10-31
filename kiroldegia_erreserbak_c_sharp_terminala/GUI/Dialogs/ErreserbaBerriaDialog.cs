using kiroldegia_erreserbak_c_sharp_terminala.Models;

namespace kiroldegia_erreserbak_c_sharp_terminala.GUI.Dialogs;

public class ErreserbaBerriaDialog : Form
{
    private readonly ComboBox _cmbPistak = new();
    private readonly ComboBox _cmbBazkideak = new();
    private readonly DateTimePicker _dtpData = new();
    private readonly ComboBox _cmbOrdua = new();
    private readonly CheckBox _chkErabiliBonoa = new();
    private readonly NumericUpDown _numItxaron = new();
    private readonly Button _btnErreserbatu = new();
    private readonly Button _btnUtzi = new();

    private readonly List<PistaDTO> _pistak;
    private readonly List<BazkideaDTO> _bazkideak;

    public ErreserbaSortuDTO? EmaitzaDTO { get; private set; }

    public ErreserbaBerriaDialog(List<PistaDTO> pistak, List<BazkideaDTO> bazkideak)
    {
        _pistak = pistak.Where(p => !p.Ezabatua).ToList();
        _bazkideak = bazkideak.Where(b => !b.Ezabatua).ToList();

        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Erreserba Berria Egin";
        Size = new Size(460, 360);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

        var lblPista = new Label { Text = "Pista:", Location = new Point(25, 25), AutoSize = true };
        _cmbPistak.Location = new Point(145, 22);
        _cmbPistak.Size = new Size(270, 26);
        _cmbPistak.DropDownStyle = ComboBoxStyle.DropDownList;
        foreach (var p in _pistak)
        {
            _cmbPistak.Items.Add($"[{p.Id}] {p.Izena} ({p.Kirola})");
        }
        if (_cmbPistak.Items.Count > 0) _cmbPistak.SelectedIndex = 0;

        var lblBazkidea = new Label { Text = "Bazkidea:", Location = new Point(25, 65), AutoSize = true };
        _cmbBazkideak.Location = new Point(145, 62);
        _cmbBazkideak.Size = new Size(270, 26);
        _cmbBazkideak.DropDownStyle = ComboBoxStyle.DropDownList;
        foreach (var b in _bazkideak)
        {
            _cmbBazkideak.Items.Add($"[{b.Id}] {b.Izena} {b.Abizena} - Bonoa: {b.BonoSaioak} saio");
        }
        if (_cmbBazkideak.Items.Count > 0) _cmbBazkideak.SelectedIndex = 0;

        var lblData = new Label { Text = "Data:", Location = new Point(25, 105), AutoSize = true };
        _dtpData.Location = new Point(145, 102);
        _dtpData.Size = new Size(160, 26);
        _dtpData.Format = DateTimePickerFormat.Short;
        _dtpData.MinDate = DateTime.Today;

        var lblOrdua = new Label { Text = "Hasiera Ordua:", Location = new Point(25, 145), AutoSize = true };
        _cmbOrdua.Location = new Point(145, 142);
        _cmbOrdua.Size = new Size(160, 26);
        _cmbOrdua.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbOrdua.Items.AddRange(["08:00:00", "09:00:00", "10:00:00", "11:00:00", "12:00:00", "13:00:00", "16:00:00", "17:00:00", "18:00:00", "19:00:00", "20:00:00", "21:00:00"]);
        _cmbOrdua.SelectedIndex = 2; // 10:00

        _chkErabiliBonoa.Text = "Erabili bazkidearen bonoko saio bat";
        _chkErabiliBonoa.Location = new Point(145, 185);
        _chkErabiliBonoa.AutoSize = true;
        _chkErabiliBonoa.Checked = true;

        var lblItxaron = new Label { Text = "Itxaron (seg):", Location = new Point(25, 225), AutoSize = true };
        _numItxaron.Location = new Point(145, 222);
        _numItxaron.Size = new Size(80, 26);
        _numItxaron.Minimum = 0;
        _numItxaron.Maximum = 60;
        _numItxaron.Value = 0;

        var lblItxaronInfo = new Label
        {
            Text = "(Transakzio/Konkurrentzia probetarako)",
            Location = new Point(235, 225),
            AutoSize = true,
            ForeColor = Color.Gray
        };

        _btnErreserbatu.Text = "Erreserbatu";
        _btnErreserbatu.Location = new Point(200, 275);
        _btnErreserbatu.Size = new Size(110, 34);
        _btnErreserbatu.BackColor = Color.FromArgb(39, 174, 96);
        _btnErreserbatu.ForeColor = Color.White;
        _btnErreserbatu.FlatStyle = FlatStyle.Flat;
        _btnErreserbatu.Click += BtnErreserbatu_Click;

        _btnUtzi.Text = "Utzi";
        _btnUtzi.Location = new Point(320, 275);
        _btnUtzi.Size = new Size(95, 34);
        _btnUtzi.DialogResult = DialogResult.Cancel;

        AcceptButton = _btnErreserbatu;
        CancelButton = _btnUtzi;

        Controls.AddRange([
            lblPista, _cmbPistak,
            lblBazkidea, _cmbBazkideak,
            lblData, _dtpData,
            lblOrdua, _cmbOrdua,
            _chkErabiliBonoa,
            lblItxaron, _numItxaron, lblItxaronInfo,
            _btnErreserbatu, _btnUtzi
        ]);
    }

    private void BtnErreserbatu_Click(object? sender, EventArgs e)
    {
        if (_cmbPistak.SelectedIndex < 0 || _cmbPistak.SelectedIndex >= _pistak.Count)
        {
            MessageBox.Show("Aukeratu pista bat zerrendatik.", "Balioztatze-errorea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_cmbBazkideak.SelectedIndex < 0 || _cmbBazkideak.SelectedIndex >= _bazkideak.Count)
        {
            MessageBox.Show("Aukeratu bazkide bat zerrendatik.", "Balioztatze-errorea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var pista = _pistak[_cmbPistak.SelectedIndex];
        var bazkidea = _bazkideak[_cmbBazkideak.SelectedIndex];
        string data = _dtpData.Value.ToString("yyyy-MM-dd");
        string ordua = _cmbOrdua.Text;

        EmaitzaDTO = new ErreserbaSortuDTO
        {
            PistaId = pista.Id,
            BazkideaId = bazkidea.Id,
            Emaila = bazkidea.Emaila,
            Data = data,
            Ordua = ordua,
            ErabiliBonoa = _chkErabiliBonoa.Checked,
            Itxaron = (int)_numItxaron.Value
        };

        DialogResult = DialogResult.OK;
        Close();
    }
}

