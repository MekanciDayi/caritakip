using System.Drawing.Printing;
using System.Drawing;
using CariTakip.Data;
using CariTakip.Forms;
using CariTakip.Models;

namespace CariTakip;

internal sealed class MainForm : Form
{
    private readonly CariRepository _cariRepo = new();
    private readonly HareketRepository _hareketRepo = new();
    private readonly FaturaRepository _faturaRepo = new();

    private readonly TabControl _tabs = new();

    private readonly ComboBox _cmbCariTipFilter = new();
    private readonly TextBox _txtCariSearch = new();
    private readonly Button _btnCariRefresh = new();
    private readonly Button _btnCariAdd = new();
    private readonly Button _btnCariEdit = new();
    private readonly Button _btnCariDelete = new();
    private readonly DataGridView _gridCariler = new();

    private readonly Button _btnHareketEkle = new();

    private readonly Button _btnFaturaEkle = new();

    private readonly ComboBox _cmbEkstreCari = new();
    private readonly ComboBox _cmbEkstreTipFilter = new();
    private readonly DateTimePicker _dtEkstreFrom = new();
    private readonly DateTimePicker _dtEkstreTo = new();
    private readonly CheckBox _chkFrom = new();
    private readonly CheckBox _chkTo = new();
    private readonly Button _btnEkstreGetir = new();
    private readonly Button _btnEkstreYazdir = new();
    private readonly DataGridView _gridEkstre = new();
    private readonly Label _lblEkstreToplam = new();

    private List<Cari> _cariler = [];
    private List<Hareket> _ekstre = [];

    internal MainForm()
    {
        Text = "Cari Takip";
        Icon = SystemIcons.Application;
        Width = 1100;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;

        Load += (_, _) =>
        {
            Db.Initialize();
            SetupUi();
            RefreshCariler();
            RefreshEkstreCariLookup();
        };
    }

    private void SetupUi()
    {
        _tabs.Dock = DockStyle.Fill;
        Controls.Add(_tabs);

        var tabCariler = new TabPage("Cariler");
        var tabIslemler = new TabPage("İşlemler");
        var tabFatura = new TabPage("Toptancı Fatura");
        var tabEkstre = new TabPage("Ekstre");

        _tabs.TabPages.Add(tabCariler);
        _tabs.TabPages.Add(tabIslemler);
        _tabs.TabPages.Add(tabFatura);
        _tabs.TabPages.Add(tabEkstre);

        SetupCarilerTab(tabCariler);
        SetupIslemlerTab(tabIslemler);
        SetupFaturaTab(tabFatura);
        SetupEkstreTab(tabEkstre);
    }

    private void SetupCarilerTab(TabPage tab)
    {
        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 44 };

        _cmbCariTipFilter.Left = 12;
        _cmbCariTipFilter.Top = 10;
        _cmbCariTipFilter.Width = 140;
        _cmbCariTipFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbCariTipFilter.Items.AddRange(["Tümü", "Müşteri", "Toptancı"]);
        _cmbCariTipFilter.SelectedIndex = 0;

        _txtCariSearch.Left = 160;
        _txtCariSearch.Top = 10;
        _txtCariSearch.Width = 220;

        _btnCariRefresh.Text = "Yenile";
        _btnCariRefresh.Left = 390;
        _btnCariRefresh.Top = 8;
        _btnCariRefresh.Width = 80;
        _btnCariRefresh.Click += (_, _) => RefreshCariler();

        _btnCariAdd.Text = "Yeni";
        _btnCariAdd.Left = 480;
        _btnCariAdd.Top = 8;
        _btnCariAdd.Width = 80;
        _btnCariAdd.Click += (_, _) => AddCari();

        _btnCariEdit.Text = "Düzenle";
        _btnCariEdit.Left = 570;
        _btnCariEdit.Top = 8;
        _btnCariEdit.Width = 80;
        _btnCariEdit.Click += (_, _) => EditCari();

        _btnCariDelete.Text = "Sil";
        _btnCariDelete.Left = 660;
        _btnCariDelete.Top = 8;
        _btnCariDelete.Width = 80;
        _btnCariDelete.Click += (_, _) => DeleteCari();

        pnlTop.Controls.AddRange([
            _cmbCariTipFilter, _txtCariSearch, _btnCariRefresh, _btnCariAdd, _btnCariEdit, _btnCariDelete
        ]);

        _gridCariler.Dock = DockStyle.Fill;
        _gridCariler.ReadOnly = true;
        _gridCariler.AllowUserToAddRows = false;
        _gridCariler.AllowUserToDeleteRows = false;
        _gridCariler.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _gridCariler.MultiSelect = false;
        _gridCariler.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _gridCariler.CellDoubleClick += (_, _) => EditCari();

        tab.Controls.Add(_gridCariler);
        tab.Controls.Add(pnlTop);

        _txtCariSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                RefreshCariler();
                e.Handled = true;
            }
        };
    }

    private void SetupIslemlerTab(TabPage tab)
    {
        var lbl = new Label
        {
            Text = "Cari listeden seçim yapıp 'Hareket Ekle' ile ödeme/tahsilat veya borç girişi yapabilirsiniz.",
            Dock = DockStyle.Top,
            Height = 40
        };

        _btnHareketEkle.Text = "Seçili Cariye Hareket Ekle";
        _btnHareketEkle.Dock = DockStyle.Top;
        _btnHareketEkle.Height = 40;
        _btnHareketEkle.Click += (_, _) => AddHareketForSelectedCari();

        tab.Controls.Add(_btnHareketEkle);
        tab.Controls.Add(lbl);
    }

    private void SetupFaturaTab(TabPage tab)
    {
        var lbl = new Label
        {
            Text = "Toptancı için fatura girildiğinde otomatik borç hareketi oluşur.",
            Dock = DockStyle.Top,
            Height = 40
        };

        _btnFaturaEkle.Text = "Seçili Toptancıya Fatura Gir";
        _btnFaturaEkle.Dock = DockStyle.Top;
        _btnFaturaEkle.Height = 40;
        _btnFaturaEkle.Click += (_, _) => AddFaturaForSelectedToptanci();

        tab.Controls.Add(_btnFaturaEkle);
        tab.Controls.Add(lbl);
    }

    private void SetupEkstreTab(TabPage tab)
    {
        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 78 };

        _cmbEkstreTipFilter.Left = 12;
        _cmbEkstreTipFilter.Top = 10;
        _cmbEkstreTipFilter.Width = 120;
        _cmbEkstreTipFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbEkstreTipFilter.Items.AddRange(["Tümü", "Müşteri", "Toptancı"]);
        _cmbEkstreTipFilter.SelectedIndex = 0;
        _cmbEkstreTipFilter.SelectedIndexChanged += (_, _) => RefreshEkstreCariLookup();

        var lblCari = new Label { Text = "Cari", Left = 12, Top = 14, AutoSize = true };
        lblCari.Left = 145;
        _cmbEkstreCari.Left = 190;
        _cmbEkstreCari.Top = 10;
        _cmbEkstreCari.Width = 300;
        _cmbEkstreCari.DropDownStyle = ComboBoxStyle.DropDownList;

        _chkFrom.Text = "Başlangıç";
        _chkFrom.Left = 510;
        _chkFrom.Top = 12;
        _chkFrom.Width = 90;

        _dtEkstreFrom.Left = 600;
        _dtEkstreFrom.Top = 10;
        _dtEkstreFrom.Width = 120;
        _dtEkstreFrom.Format = DateTimePickerFormat.Short;
        _dtEkstreFrom.Value = DateTime.Today.AddMonths(-1);

        _chkTo.Text = "Bitiş";
        _chkTo.Left = 735;
        _chkTo.Top = 12;
        _chkTo.Width = 60;

        _dtEkstreTo.Left = 800;
        _dtEkstreTo.Top = 10;
        _dtEkstreTo.Width = 120;
        _dtEkstreTo.Format = DateTimePickerFormat.Short;
        _dtEkstreTo.Value = DateTime.Today;

        _btnEkstreGetir.Text = "Getir";
        _btnEkstreGetir.Left = 935;
        _btnEkstreGetir.Top = 8;
        _btnEkstreGetir.Width = 80;
        _btnEkstreGetir.Click += (_, _) => RefreshEkstre();

        _btnEkstreYazdir.Text = "Yazdır";
        _btnEkstreYazdir.Left = 1020;
        _btnEkstreYazdir.Top = 8;
        _btnEkstreYazdir.Width = 80;
        _btnEkstreYazdir.Click += (_, _) => PrintEkstre();

        _lblEkstreToplam.Left = 12;
        _lblEkstreToplam.Top = 46;
        _lblEkstreToplam.Width = 950;

        pnlTop.Controls.AddRange([
            _cmbEkstreTipFilter,
            lblCari, _cmbEkstreCari,
            _chkFrom, _dtEkstreFrom,
            _chkTo, _dtEkstreTo,
            _btnEkstreGetir, _btnEkstreYazdir,
            _lblEkstreToplam
        ]);

        _gridEkstre.Dock = DockStyle.Fill;
        _gridEkstre.ReadOnly = true;
        _gridEkstre.AllowUserToAddRows = false;
        _gridEkstre.AllowUserToDeleteRows = false;
        _gridEkstre.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _gridEkstre.MultiSelect = false;
        _gridEkstre.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        tab.Controls.Add(_gridEkstre);
        tab.Controls.Add(pnlTop);

        _chkFrom.Checked = true;
        _chkTo.Checked = true;
    }

    private void RefreshCariler()
    {
        CariTip? tip = _cmbCariTipFilter.SelectedIndex switch
        {
            1 => CariTip.Musteri,
            2 => CariTip.Toptanci,
            _ => null
        };

        _cariler = _cariRepo.List(tip, _txtCariSearch.Text);

        var rows = _cariler.Select(c => new
        {
            c.Id,
            Tip = c.Tip.ToString(),
            c.Unvan,
            c.Telefon,
            Bakiye = _cariRepo.GetBakiye(c.Id)
        }).ToList();

        _gridCariler.DataSource = rows;
        RefreshEkstreCariLookup();
    }

    private Cari? GetSelectedCari()
    {
        if (_gridCariler.CurrentRow?.DataBoundItem is null) return null;
        var idProp = _gridCariler.CurrentRow.DataBoundItem.GetType().GetProperty("Id");
        if (idProp is null) return null;
        var id = Convert.ToInt64(idProp.GetValue(_gridCariler.CurrentRow.DataBoundItem));
        return _cariler.FirstOrDefault(c => c.Id == id);
    }

    private void AddCari()
    {
        using var frm = new CariEditForm("Yeni Cari");
        if (frm.ShowDialog(this) != DialogResult.OK) return;

        var cari = new Cari
        {
            Tip = frm.SelectedTip,
            Unvan = frm.Unvan,
            Telefon = string.IsNullOrWhiteSpace(frm.Telefon) ? null : frm.Telefon,
            Notlar = string.IsNullOrWhiteSpace(frm.Notlar) ? null : frm.Notlar
        };

        _cariRepo.Insert(cari);
        RefreshCariler();
    }

    private void EditCari()
    {
        var cari = GetSelectedCari();
        if (cari is null)
        {
            MessageBox.Show(this, "Lütfen bir cari seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var frm = new CariEditForm("Cari Düzenle", cari);
        if (frm.ShowDialog(this) != DialogResult.OK) return;

        cari.Tip = frm.SelectedTip;
        cari.Unvan = frm.Unvan;
        cari.Telefon = string.IsNullOrWhiteSpace(frm.Telefon) ? null : frm.Telefon;
        cari.Notlar = string.IsNullOrWhiteSpace(frm.Notlar) ? null : frm.Notlar;

        _cariRepo.Update(cari);
        RefreshCariler();
    }

    private void DeleteCari()
    {
        var cari = GetSelectedCari();
        if (cari is null)
        {
            MessageBox.Show(this, "Lütfen bir cari seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(this, $"Silinsin mi?\n\n{cari.Unvan}", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        var ok = _cariRepo.Delete(cari.Id);
        if (!ok)
        {
            MessageBox.Show(this, "Bu cariye ait hareket olduğu için silinemez.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        RefreshCariler();
    }

    private void AddHareketForSelectedCari()
    {
        var cari = GetSelectedCari();
        if (cari is null)
        {
            MessageBox.Show(this, "Hareket eklemek için önce Cariler sekmesinden bir cari seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var frm = new HareketEkleForm(cari.Unvan, cari.Tip);
        if (frm.ShowDialog(this) != DialogResult.OK) return;

        var hareket = new Hareket
        {
            CariId = cari.Id,
            Tarih = frm.Tarih,
            Etki = frm.Etki,
            Tutar = frm.Tutar,
            Aciklama = frm.Aciklama,
            Kaynak = HareketKaynak.Manuel,
            KaynakId = null
        };

        _hareketRepo.InsertManual(hareket);
        RefreshCariler();
    }

    private void AddFaturaForSelectedToptanci()
    {
        var cari = GetSelectedCari();
        if (cari is null)
        {
            MessageBox.Show(this, "Fatura girmek için önce Cariler sekmesinden bir toptancı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (cari.Tip != CariTip.Toptanci)
        {
            MessageBox.Show(this, "Fatura sadece Toptancı için girilebilir.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var frm = new FaturaEkleForm(cari.Unvan);
        if (frm.ShowDialog(this) != DialogResult.OK) return;

        var fatura = new Fatura
        {
            CariId = cari.Id,
            FaturaNo = frm.FaturaNo,
            Tarih = frm.Tarih,
            Toplam = frm.Toplam
        };

        _faturaRepo.InsertForToptanciAndCreateHareket(fatura);
        RefreshCariler();
    }

    private void RefreshEkstreCariLookup()
    {
        CariTip? tip = _cmbEkstreTipFilter.SelectedIndex switch
        {
            1 => CariTip.Musteri,
            2 => CariTip.Toptanci,
            _ => null
        };

        var list = _cariRepo.ListForLookup(tip);
        _cmbEkstreCari.DataSource = list;
        _cmbEkstreCari.DisplayMember = "Item2";
        _cmbEkstreCari.ValueMember = "Item1";

        if (list.Count > 0 && _cmbEkstreCari.SelectedIndex < 0)
            _cmbEkstreCari.SelectedIndex = 0;
    }

    private void RefreshEkstre()
    {
        if (_cmbEkstreCari.SelectedItem is not ValueTuple<long, string, CariTip> item)
        {
            _ekstre = [];
            _gridEkstre.DataSource = null;
            return;
        }

        var from = _chkFrom.Checked ? _dtEkstreFrom.Value.Date : (DateTime?)null;
        var to = _chkTo.Checked ? _dtEkstreTo.Value.Date : (DateTime?)null;

        _ekstre = _hareketRepo.ListByCari(item.Item1, from, to);

        var rows = _ekstre.Select(h => new
        {
            h.Id,
            Tarih = h.Tarih.ToString("dd.MM.yyyy"),
            Borc = h.Etki > 0 ? h.Tutar : 0m,
            Odeme = h.Etki < 0 ? h.Tutar : 0m,
            h.Aciklama
        }).ToList();

        _gridEkstre.DataSource = rows;

        var borcTop = _ekstre.Where(x => x.Etki > 0).Sum(x => x.Tutar);
        var odemeTop = _ekstre.Where(x => x.Etki < 0).Sum(x => x.Tutar);
        var bakiye = _ekstre.Sum(x => x.Tutar * x.Etki);

        _lblEkstreToplam.Text = $"Borç: {borcTop:N2}   Ödeme: {odemeTop:N2}   Bakiye: {bakiye:N2}";
    }

    private void PrintEkstre()
    {
        if (_cmbEkstreCari.SelectedItem is not ValueTuple<long, string, CariTip> item)
        {
            MessageBox.Show(this, "Cari seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_ekstre.Count == 0)
        {
            MessageBox.Show(this, "Yazdırmak için önce ekstre getirin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var from = _chkFrom.Checked ? _dtEkstreFrom.Value.Date : (DateTime?)null;
        var to = _chkTo.Checked ? _dtEkstreTo.Value.Date : (DateTime?)null;

        var cariBaslik = $"{item.Item2} ({item.Item3})";

        using var doc = new EkstrePrintDocument(cariBaslik, from, to, _ekstre);
        using var preview = new PrintPreviewDialog
        {
            Document = doc,
            Width = 1000,
            Height = 700
        };

        preview.ShowDialog(this);
    }
}
