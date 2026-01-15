using System.Globalization;

namespace CariTakip.Forms;

internal sealed class FaturaEkleForm : Form
{
    private readonly TextBox _txtFaturaNo = new();
    private readonly DateTimePicker _dtTarih = new();
    private readonly TextBox _txtToplam = new();
    private readonly Button _btnOk = new();
    private readonly Button _btnCancel = new();

    private decimal _parsedToplam;

    internal string FaturaNo => _txtFaturaNo.Text;
    internal DateTime Tarih => _dtTarih.Value.Date;
    internal decimal Toplam => _parsedToplam;

    internal FaturaEkleForm(string toptanciUnvan)
    {
        Text = "Toptancı Faturası";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 190);

        var lblCari = new Label { Text = $"Toptancı: {toptanciUnvan}", Left = 12, Top = 12, AutoSize = true };

        var lblNo = new Label { Text = "Fatura No", Left = 12, Top = 48, AutoSize = true };
        _txtFaturaNo.Left = 140;
        _txtFaturaNo.Top = 44;
        _txtFaturaNo.Width = 360;

        var lblTarih = new Label { Text = "Tarih", Left = 12, Top = 84, AutoSize = true };
        _dtTarih.Left = 140;
        _dtTarih.Top = 80;
        _dtTarih.Width = 200;
        _dtTarih.Format = DateTimePickerFormat.Short;

        var lblToplam = new Label { Text = "Toplam", Left = 12, Top = 120, AutoSize = true };
        _txtToplam.Left = 140;
        _txtToplam.Top = 116;
        _txtToplam.Width = 200;

        _btnOk.Text = "Kaydet";
        _btnOk.Left = 330;
        _btnOk.Top = 150;
        _btnOk.Width = 85;
        _btnOk.DialogResult = DialogResult.OK;
        _btnOk.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_txtFaturaNo.Text))
            {
                MessageBox.Show(this, "Fatura no boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            if (!decimal.TryParse(_txtToplam.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var toplam) || toplam <= 0)
            {
                MessageBox.Show(this, "Geçerli bir toplam girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            _parsedToplam = toplam;
        };

        _btnCancel.Text = "İptal";
        _btnCancel.Left = 425;
        _btnCancel.Top = 150;
        _btnCancel.Width = 85;
        _btnCancel.DialogResult = DialogResult.Cancel;

        Controls.AddRange([
            lblCari,
            lblNo, _txtFaturaNo,
            lblTarih, _dtTarih,
            lblToplam, _txtToplam,
            _btnOk, _btnCancel
        ]);

        AcceptButton = _btnOk;
        CancelButton = _btnCancel;

        _dtTarih.Value = DateTime.Today;
    }
}
