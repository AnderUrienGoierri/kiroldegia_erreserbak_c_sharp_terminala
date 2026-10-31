using kiroldegia_erreserbak_c_sharp_terminala.Models;

namespace kiroldegia_erreserbak_c_sharp_terminala.GUI.Dialogs;

public class PistaBerriaDialog : Form
{
    private readonly TextBox _txtIzena = new();
    private readonly ComboBox _cmbKirola = new();
    private readonly Button _btnGorde = new();
    private readonly Button _btnUtzi = new();

    public PistaSortuDTO? EmaitzaDTO { get; private set; }

    public PistaBerriaDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Pista Berria Sortu";
        Size = new Size(390, 230);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

        var lblIzena = new Label { Text = "Pistaren Izena:", Location = new Point(25, 30), AutoSize = true };
        _txtIzena.Location = new Point(135, 27);
        _txtIzena.Size = new Size(210, 26);

        var lblKirola = new Label { Text = "Kirola / Mota:", Location = new Point(25, 75), AutoSize = true };
        _cmbKirola.Location = new Point(135, 72);
        _cmbKirola.Size = new Size(210, 26);
        _cmbKirola.Items.AddRange(["Tenisa", "Padela", "Saskibaloia", "Futbola", "Igeriketa", "Eskubaloia"]);
        _cmbKirola.SelectedIndex = 0;

        _btnGorde.Text = "Gorde";
        _btnGorde.Location = new Point(155, 135);
        _btnGorde.Size = new Size(95, 32);
        _btnGorde.BackColor = Color.FromArgb(41, 128, 185);
        _btnGorde.ForeColor = Color.White;
        _btnGorde.FlatStyle = FlatStyle.Flat;
        _btnGorde.Click += BtnGorde_Click;

        _btnUtzi.Text = "Utzi";
        _btnUtzi.Location = new Point(260, 135);
        _btnUtzi.Size = new Size(85, 32);
        _btnUtzi.DialogResult = DialogResult.Cancel;

        AcceptButton = _btnGorde;
        CancelButton = _btnUtzi;

        Controls.AddRange([
            lblIzena, _txtIzena,
            lblKirola, _cmbKirola,
            _btnGorde, _btnUtzi
        ]);
    }

    private void BtnGorde_Click(object? sender, EventArgs e)
    {
        string izena = _txtIzena.Text.Trim();
        string kirola = _cmbKirola.Text.Trim();

        if (string.IsNullOrWhiteSpace(izena))
        {
            MessageBox.Show("Pistaren izena beharrezkoa da.", "Balioztatze-errorea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(kirola))
        {
            MessageBox.Show("Kirol mota zehaztu behar da.", "Balioztatze-errorea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        EmaitzaDTO = new PistaSortuDTO
        {
            Izena = izena,
            Kirola = kirola
        };

        DialogResult = DialogResult.OK;
        Close();
    }
}
