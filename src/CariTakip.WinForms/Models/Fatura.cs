namespace CariTakip.Models;

public sealed class Fatura
{
    public long Id { get; set; }
    public long CariId { get; set; }
    public string FaturaNo { get; set; } = "";
    public DateTime Tarih { get; set; }
    public decimal Toplam { get; set; }
}
