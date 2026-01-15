namespace CariTakip.Models;

public enum HareketKaynak
{
    Manuel = 0,
    ToptanciFatura = 1
}

public sealed class Hareket
{
    public long Id { get; set; }
    public long CariId { get; set; }
    public DateTime Tarih { get; set; }
    public int Etki { get; set; }
    public decimal Tutar { get; set; }
    public string? Aciklama { get; set; }
    public HareketKaynak Kaynak { get; set; }
    public long? KaynakId { get; set; }
}
