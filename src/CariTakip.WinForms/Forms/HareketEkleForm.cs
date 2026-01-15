using CariTakip.Models;
using System.Globalization;

namespace CariTakip.Forms;

internal sealed class HareketEkleForm : Form
{
    private readonly DateTimePicker _dtTarih = new();
    private readonly ComboBox _cmbIslem = new();
    private readonly TextBox _txtTutar = new();
    private readonly TextBox _txtAciklama = new();
    private readonly Button _btnOk = new();
    private readonly Button _btnCancel = new();

    private decimal _parsedTutar;

    internal DateTime Tarih => _dtTarih.Value.Date;
    internal int Etki => (int)_cmbIslem.SelectedValue!;
    internal decimal Tutar => _parsedTutar;
    internal string? Aciklama => string.IsNullOrWhiteSpace(_txtAciklama.Text) ? null : _txtAciklama.Text;

    internal HareketEkleForm(string cariUnvan, CariTip cariTip)
    {
        Text = "Hareket Ekle";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 250);

        var lblCari = new Label { Text = $"Cari: {cariUnvan}", Left = 12, Top = 12, AutoSize = true };

        var lblTarih = new Label { Text = "Tarih", Left = 12, Top = 48, AutoSize = true };
        _dtTarih.Left = 140;
        _dtTarih.Top = 44;
        _dtTarih.Width = 200;
        _dtTarih.Format = DateTimePickerFormat.Short;

        var lblIslem = new Label { Text = "İşlem", Left = 12, Top = 84, AutoSize = true };
        _cmbIslem.Left = 140;
        _cmbIslem.Top = 80;
        _cmbIslem.Width = 360;
        _cmbIslem.DropDownStyle = ComboBoxStyle.DropDownList;

        var items = new List<KeyValuePair<string, int>>();
        if (cariTip == CariTip.Musteri)
        {
            items.Add(new KeyValuePair<string, int>("Borç Arttır (Satış vb.)", 1));
            items.Add(new KeyValuePair<string, int>("Borç Azalt (Tahsilat)", -1));
        }
        else
        {
            items.Add(new KeyValuePair<string, int>("Borç Arttır (Fatura/Alım)", 1));
            items.Add(new KeyValuePair<string, int>("Borç Azalt (Ödeme)", -1));
        }

        _cmbIslem.DataSource = items;
        _cmbIslem.DisplayMember = "Key";
        _cmbIslem.ValueMember = "Value";

        var lblTutar = new Label { Text = "Tutar", Left = 12, Top = 120, AutoSize = true };
        _txtTutar.Left = 140;
        _txtTutar.Top = 116;
        _txtTutar.Width = 200;

        var lblAciklama = new Label { Text = "Açıklama", Left = 12, Top = 156, AutoSize = true };
        _txtAciklama.Left = 140;
        _txtAciklama.Top = 152;
        _txtAciklama.Width = 360;

        _btnOk.Text = "Kaydet";
        _btnOk.Left = 330;
        _btnOk.Top = 200;
        _btnOk.Width = 85;
        _btnOk.DialogResult = DialogResult.OK;
        _btnOk.Click += (_, _) =>
        {
            if (!decimal.TryParse(_txtTutar.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var tutar) || tutar <= 0)
            {
                MessageBox.Show(this, "Geçerli bir tutar girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            _parsedTutar = tutar;
        };

        _btnCancel.Text = "İptal";
        _btnCancel.Left = 425;
        _btnCancel.Top = 200;
        _btnCancel.Width = 85;
        _btnCancel.DialogResult = DialogResult.Cancel;

        Controls.AddRange([
            lblCari,
            lblTarih, _dtTarih,
            lblIslem, _cmbIslem,
            lblTutar, _txtTutar,
            lblAciklama, _txtAciklama,
            _btnOk, _btnCancel
        ]);

        AcceptButton = _btnOk;
        CancelButton = _btnCancel;

        _dtTarih.Value = DateTime.Today;
        _cmbIslem.SelectedIndex = 0;
    }
}
