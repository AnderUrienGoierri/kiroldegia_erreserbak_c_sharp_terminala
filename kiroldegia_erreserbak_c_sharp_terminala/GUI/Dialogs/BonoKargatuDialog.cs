using kiroldegia_erreserbak_c_sharp_terminala.Models;

namespace kiroldegia_erreserbak_c_sharp_terminala.GUI.Dialogs;

public class BonoKargatuDialog : Form
{
    private readonly ComboBox _cmbEmaila = new();
    private readonly NumericUpDown _numSaioak = new();
    private readonly Button _btnKargatu = new();
    private readonly Button _btnUtzi = new();

    private readonly List<string> _emailak;

    public string? AukeratutakoEmaila { get; private set; }
    public int AukeratutakoSaioak { get; private set; }

    public BonoKargatuDialog(List<string> emailak)
    {
        _emailak = emailak.Distinct().ToList();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Bonoa Kargatu / Sortu";
        Size = new Size(420, 230);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

        var lblEmaila = new Label { Text = "Bazkidearen Emaila:", Location = new Point(25, 25), AutoSize = true };
        _cmbEmaila.Location = new Point(165, 22);
        _cmbEmaila.Size = new Size(215, 26);
        foreach (var email in _emailak)
        {
            _cmbEmaila.Items.Add(email);
        }
        if (_cmbEmaila.Items.Count > 0) _cmbEmaila.SelectedIndex = 0;

        var lblSaioak = new Label { Text = "Kargatu beharreko saioak:", Location = new Point(25, 75), AutoSize = true };
        _numSaioak.Location = new Point(205, 72);
        _numSaioak.Size = new Size(80, 26);
        _numSaioak.Minimum = 1;
        _numSaioak.Maximum = 100;
        _numSaioak.Value = 10;

        _btnKargatu.Text = "Kargatu";
        _btnKargatu.Location = new Point(195, 135);
        _btnKargatu.Size = new Size(95, 32);
        _btnKargatu.BackColor = Color.FromArgb(41, 128, 185);
        _btnKargatu.ForeColor = Color.White;
        _btnKargatu.FlatStyle = FlatStyle.Flat;
        _btnKargatu.Click += BtnKargatu_Click;

        _btnUtzi.Text = "Utzi";
        _btnUtzi.Location = new Point(300, 135);
        _btnUtzi.Size = new Size(80, 32);
        _btnUtzi.DialogResult = DialogResult.Cancel;

        AcceptButton = _btnKargatu;
        CancelButton = _btnUtzi;

        Controls.AddRange([
            lblEmaila, _cmbEmaila,
            lblSaioak, _numSaioak,
            _btnKargatu, _btnUtzi
        ]);
    }

    private void BtnKargatu_Click(object? sender, EventArgs e)
    {
        string email = _cmbEmaila.Text.Trim();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            MessageBox.Show("Sartu baliozko email helbide bat.", "Balioztatze-errorea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        AukeratutakoEmaila = email;
        AukeratutakoSaioak = (int)_numSaioak.Value;

        DialogResult = DialogResult.OK;
        Close();
    }
}
