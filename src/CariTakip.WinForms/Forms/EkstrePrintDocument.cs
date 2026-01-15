using System.Drawing;
using System.Drawing.Printing;
using CariTakip.Models;

namespace CariTakip.Forms;

internal sealed class EkstrePrintDocument : PrintDocument
{
    private readonly string _cariBaslik;
    private readonly DateTime? _from;
    private readonly DateTime? _to;
    private readonly List<Hareket> _hareketler;

    private readonly decimal _borcTop;
    private readonly decimal _odemeTop;
    private readonly decimal _bakiye;

    private int _rowIndex;
    private bool _printTotalsPending;

    internal EkstrePrintDocument(string cariBaslik, DateTime? from, DateTime? to, List<Hareket> hareketler)
    {
        _cariBaslik = cariBaslik;
        _from = from;
        _to = to;
        _hareketler = hareketler;

        _borcTop = _hareketler.Where(x => x.Etki > 0).Sum(x => x.Tutar);
        _odemeTop = _hareketler.Where(x => x.Etki < 0).Sum(x => x.Tutar);
        _bakiye = _hareketler.Sum(x => x.Tutar * x.Etki);

        DefaultPageSettings.Landscape = false;
    }

    protected override void OnBeginPrint(PrintEventArgs e)
    {
        base.OnBeginPrint(e);
        _rowIndex = 0;
        _printTotalsPending = false;
    }

    protected override void OnPrintPage(PrintPageEventArgs e)
    {
        base.OnPrintPage(e);

        var g = e.Graphics;
        if (g is null) return;

        using var fontTitle = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
        using var font = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Regular);
        using var pen = new Pen(Color.Black);

        var left = e.MarginBounds.Left;
        var top = e.MarginBounds.Top;
        var width = e.MarginBounds.Width;

        var title = "Cari Ekstre";
        g.DrawString(title, fontTitle, Brushes.Black, left, top);
        top += 26;

        g.DrawString(_cariBaslik, font, Brushes.Black, left, top);
        top += 18;

        var rangeText = $"Tarih Aralığı: {(_from?.ToString("dd.MM.yyyy") ?? "-")} - {(_to?.ToString("dd.MM.yyyy") ?? "-")}";
        g.DrawString(rangeText, font, Brushes.Black, left, top);
        top += 22;

        var colTarih = left;
        var colAciklama = left + 90;
        var colBorc = left + width - 180;
        var colOdeme = left + width - 90;

        g.DrawLine(pen, left, top, left + width, top);
        top += 6;

        g.DrawString("Tarih", font, Brushes.Black, colTarih, top);
        g.DrawString("Açıklama", font, Brushes.Black, colAciklama, top);
        g.DrawString("Borç", font, Brushes.Black, colBorc, top);
        g.DrawString("Ödeme", font, Brushes.Black, colOdeme, top);
        top += 16;

        g.DrawLine(pen, left, top, left + width, top);
        top += 8;

        var rowHeight = 18;

        if (_printTotalsPending)
        {
            DrawTotals(g, e, font, pen, left, ref top, width);
            e.HasMorePages = false;
            _printTotalsPending = false;
            return;
        }

        while (_rowIndex < _hareketler.Count)
        {
            if (top + rowHeight > e.MarginBounds.Bottom)
            {
                e.HasMorePages = true;
                return;
            }

            var h = _hareketler[_rowIndex];

            g.DrawString(h.Tarih.ToString("dd.MM.yyyy"), font, Brushes.Black, colTarih, top);
            g.DrawString(h.Aciklama ?? "", font, Brushes.Black, new RectangleF(colAciklama, top, colBorc - colAciklama - 8, rowHeight));

            var borc = h.Etki > 0 ? h.Tutar.ToString("N2") : "";
            var odeme = h.Etki < 0 ? h.Tutar.ToString("N2") : "";

            var sf = new StringFormat { Alignment = StringAlignment.Far };
            g.DrawString(borc, font, Brushes.Black, new RectangleF(colBorc, top, 80, rowHeight), sf);
            g.DrawString(odeme, font, Brushes.Black, new RectangleF(colOdeme, top, 80, rowHeight), sf);

            top += rowHeight;
            _rowIndex++;
        }

        if (top + 36 > e.MarginBounds.Bottom)
        {
            _printTotalsPending = true;
            e.HasMorePages = true;
            return;
        }

        DrawTotals(g, e, font, pen, left, ref top, width);
        e.HasMorePages = false;
    }

    private void DrawTotals(Graphics g, PrintPageEventArgs e, Font font, Pen pen, int left, ref int top, int width)
    {
        g.DrawLine(pen, left, top + 4, left + width, top + 4);
        top += 12;

        var text = $"Toplam Borç: {_borcTop:N2}   Toplam Ödeme: {_odemeTop:N2}   Bakiye: {_bakiye:N2}";
        g.DrawString(text, font, Brushes.Black, left, top);
        top += 18;
    }
}
