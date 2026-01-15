namespace CariTakip.Models;

public enum CariTip
{
    Musteri = 0,
    Toptanci = 1
}

public sealed class Cari
{
    public long Id { get; set; }
    public CariTip Tip { get; set; }
    public string Unvan { get; set; } = "";
    public string? Telefon { get; set; }
    public string? Notlar { get; set; }
    public DateTime CreatedAt { get; set; }
}
