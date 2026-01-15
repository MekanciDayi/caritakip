using CariTakip.Models;

namespace CariTakip.Forms;

internal sealed class CariEditForm : Form
{
    private readonly ComboBox _cmbTip = new();
    private readonly TextBox _txtUnvan = new();
    private readonly TextBox _txtTelefon = new();
    private readonly TextBox _txtNotlar = new();
    private readonly Button _btnOk = new();
    private readonly Button _btnCancel = new();

    internal CariTip SelectedTip => (CariTip)_cmbTip.SelectedItem!;
    internal string Unvan => _txtUnvan.Text;
    internal string Telefon => _txtTelefon.Text;
    internal string Notlar => _txtNotlar.Text;

    internal CariEditForm(string title, Cari? existing = null)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(460, 260);

        var lblTip = new Label { Text = "Tip", Left = 12, Top = 16, AutoSize = true };
        _cmbTip.Left = 120;
        _cmbTip.Top = 12;
        _cmbTip.Width = 300;
        _cmbTip.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbTip.DataSource = Enum.GetValues(typeof(CariTip));

        var lblUnvan = new Label { Text = "Ünvan", Left = 12, Top = 52, AutoSize = true };
        _txtUnvan.Left = 120;
        _txtUnvan.Top = 48;
        _txtUnvan.Width = 300;

        var lblTelefon = new Label { Text = "Telefon", Left = 12, Top = 88, AutoSize = true };
        _txtTelefon.Left = 120;
        _txtTelefon.Top = 84;
        _txtTelefon.Width = 300;

        var lblNotlar = new Label { Text = "Notlar", Left = 12, Top = 124, AutoSize = true };
        _txtNotlar.Left = 120;
        _txtNotlar.Top = 120;
        _txtNotlar.Width = 300;
        _txtNotlar.Height = 80;
        _txtNotlar.Multiline = true;

        _btnOk.Text = "Kaydet";
        _btnOk.Left = 240;
        _btnOk.Top = 214;
        _btnOk.Width = 85;
        _btnOk.DialogResult = DialogResult.OK;
        _btnOk.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_txtUnvan.Text))
            {
                MessageBox.Show(this, "Ünvan boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
        };

        _btnCancel.Text = "İptal";
        _btnCancel.Left = 335;
        _btnCancel.Top = 214;
        _btnCancel.Width = 85;
        _btnCancel.DialogResult = DialogResult.Cancel;

        Controls.AddRange([
            lblTip, _cmbTip,
            lblUnvan, _txtUnvan,
            lblTelefon, _txtTelefon,
            lblNotlar, _txtNotlar,
            _btnOk, _btnCancel
        ]);

        AcceptButton = _btnOk;
        CancelButton = _btnCancel;

        if (existing is not null)
        {
            _cmbTip.SelectedItem = existing.Tip;
            _txtUnvan.Text = existing.Unvan;
            _txtTelefon.Text = existing.Telefon ?? "";
            _txtNotlar.Text = existing.Notlar ?? "";
        }
        else
        {
            _cmbTip.SelectedItem = CariTip.Musteri;
        }
    }
}
