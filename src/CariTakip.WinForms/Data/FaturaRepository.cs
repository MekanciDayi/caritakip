using CariTakip.Models;

namespace CariTakip.Data;

internal sealed class FaturaRepository
{
    internal long InsertForToptanciAndCreateHareket(Fatura fatura)
    {
        using var connection = Db.OpenConnection();
        using var tx = connection.BeginTransaction();

        long faturaId;

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = @"
INSERT INTO Faturalar (CariId, FaturaNo, Tarih, ToplamKurus)
VALUES ($cariId, $no, $tarih, $toplamKurus);
SELECT last_insert_rowid();
";
            cmd.Parameters.AddWithValue("$cariId", fatura.CariId);
            cmd.Parameters.AddWithValue("$no", fatura.FaturaNo.Trim());
            cmd.Parameters.AddWithValue("$tarih", fatura.Tarih.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$toplamKurus", Money.ToKurus(fatura.Toplam));
            faturaId = (long)(cmd.ExecuteScalar() ?? 0L);
        }

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = @"
INSERT INTO Hareketler (CariId, Tarih, Etki, TutarKurus, Aciklama, Kaynak, KaynakId)
VALUES ($cariId, $tarih, 1, $tutarKurus, $aciklama, $kaynak, $kaynakId);
";
            cmd.Parameters.AddWithValue("$cariId", fatura.CariId);
            cmd.Parameters.AddWithValue("$tarih", fatura.Tarih.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$tutarKurus", Money.ToKurus(fatura.Toplam));
            cmd.Parameters.AddWithValue("$aciklama", $"Fatura: {fatura.FaturaNo}");
            cmd.Parameters.AddWithValue("$kaynak", (int)HareketKaynak.ToptanciFatura);
            cmd.Parameters.AddWithValue("$kaynakId", faturaId);
            cmd.ExecuteNonQuery();
        }

        tx.Commit();
        return faturaId;
    }
}
