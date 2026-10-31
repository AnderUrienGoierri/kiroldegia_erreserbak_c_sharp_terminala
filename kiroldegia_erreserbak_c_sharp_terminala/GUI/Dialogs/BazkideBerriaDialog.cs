using kiroldegia_erreserbak_c_sharp_terminala.Models;

namespace kiroldegia_erreserbak_c_sharp_terminala.GUI.Dialogs;

public class BazkideBerriaDialog : Form
{
    private readonly TextBox _txtIzena = new();
    private readonly TextBox _txtAbizena = new();
    private readonly TextBox _txtEmaila = new();
    private readonly NumericUpDown _numBonoa = new();
    private readonly Button _btnGorde = new();
    private readonly Button _btnUtzi = new();

    public BazkideaSortuDTO? EmaitzaDTO { get; private set; }

    public BazkideBerriaDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Bazkide Berria Sortu";
        Size = new Size(420, 290);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

        var lblIzena = new Label { Text = "Izena:", Location = new Point(25, 25), AutoSize = true };
        _txtIzena.Location = new Point(140, 22);
        _txtIzena.Size = new Size(230, 26);

        var lblAbizena = new Label { Text = "Abizena:", Location = new Point(25, 65), AutoSize = true };
        _txtAbizena.Location = new Point(140, 62);
        _txtAbizena.Size = new Size(230, 26);

        var lblEmaila = new Label { Text = "Emaila:", Location = new Point(25, 105), AutoSize = true };
        _txtEmaila.Location = new Point(140, 102);
        _txtEmaila.Size = new Size(230, 26);

        var lblBonoa = new Label { Text = "Hasierako saioak:", Location = new Point(25, 145), AutoSize = true };
        _numBonoa.Location = new Point(140, 142);
        _numBonoa.Size = new Size(100, 26);
        _numBonoa.Minimum = 0;
        _numBonoa.Maximum = 100;
        _numBonoa.Value = 0;

        _btnGorde.Text = "Gorde";
        _btnGorde.Location = new Point(180, 195);
        _btnGorde.Size = new Size(95, 32);
        _btnGorde.BackColor = Color.FromArgb(41, 128, 185);
        _btnGorde.ForeColor = Color.White;
        _btnGorde.FlatStyle = FlatStyle.Flat;
        _btnGorde.Click += BtnGorde_Click;

        _btnUtzi.Text = "Utzi";
        _btnUtzi.Location = new Point(285, 195);
        _btnUtzi.Size = new Size(85, 32);
        _btnUtzi.DialogResult = DialogResult.Cancel;

        AcceptButton = _btnGorde;
        CancelButton = _btnUtzi;

        Controls.AddRange([
            lblIzena, _txtIzena,
            lblAbizena, _txtAbizena,
            lblEmaila, _txtEmaila,
            lblBonoa, _numBonoa,
            _btnGorde, _btnUtzi
        ]);
    }

    private void BtnGorde_Click(object? sender, EventArgs e)
    {
        string izena = _txtIzena.Text.Trim();
        string abizena = _txtAbizena.Text.Trim();
        string emaila = _txtEmaila.Text.Trim();

        if (string.IsNullOrWhiteSpace(izena) || string.IsNullOrWhiteSpace(abizena))
        {
            MessageBox.Show("Izena eta Abizena derrigorrezkoak dira.", "Balioztatze-errorea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(emaila) || !emaila.Contains('@'))
        {
            MessageBox.Show("Email helbide baliogabea sartu duzu.", "Balioztatze-errorea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        EmaitzaDTO = new BazkideaSortuDTO
        {
            Izena = izena,
            Abizena = abizena,
            Emaila = emaila,
            HasierakoBonoa = (int)_numBonoa.Value
        };

        DialogResult = DialogResult.OK;
        Close();
    }
}
