using CariTakip.Models;
using System.Globalization;

namespace CariTakip.Data;

internal sealed class HareketRepository
{
    internal long InsertManual(Hareket hareket)
    {
        using var connection = Db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Hareketler (CariId, Tarih, Etki, TutarKurus, Aciklama, Kaynak, KaynakId)
VALUES ($cariId, $tarih, $etki, $tutarKurus, $aciklama, $kaynak, $kaynakId);
SELECT last_insert_rowid();
";
        cmd.Parameters.AddWithValue("$cariId", hareket.CariId);
        cmd.Parameters.AddWithValue("$tarih", hareket.Tarih.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$etki", hareket.Etki);
        cmd.Parameters.AddWithValue("$tutarKurus", Money.ToKurus(hareket.Tutar));
        cmd.Parameters.AddWithValue("$aciklama", (object?)hareket.Aciklama?.Trim() ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$kaynak", (int)hareket.Kaynak);
        cmd.Parameters.AddWithValue("$kaynakId", (object?)hareket.KaynakId ?? DBNull.Value);

        return (long)(cmd.ExecuteScalar() ?? 0L);
    }

    internal List<Hareket> ListByCari(long cariId, DateTime? from, DateTime? to)
    {
        using var connection = Db.OpenConnection();
        using var cmd = connection.CreateCommand();

        var where = new List<string> { "CariId = $id" };
        cmd.Parameters.AddWithValue("$id", cariId);

        if (from is not null)
        {
            where.Add("Tarih >= $from");
            cmd.Parameters.AddWithValue("$from", from.Value.ToString("yyyy-MM-dd"));
        }

        if (to is not null)
        {
            where.Add("Tarih <= $to");
            cmd.Parameters.AddWithValue("$to", to.Value.ToString("yyyy-MM-dd"));
        }

        cmd.CommandText = $@"
SELECT Id, CariId, Tarih, Etki, TutarKurus, Aciklama, Kaynak, KaynakId
FROM Hareketler
WHERE {string.Join(" AND ", where)}
ORDER BY Tarih, Id;
";

        using var reader = cmd.ExecuteReader();
        var list = new List<Hareket>();
        while (reader.Read())
        {
            list.Add(new Hareket
            {
                Id = reader.GetInt64(0),
                CariId = reader.GetInt64(1),
                Tarih = DateTime.ParseExact(reader.GetString(2), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                Etki = reader.GetInt32(3),
                Tutar = Money.FromKurus(reader.GetInt64(4)),
                Aciklama = reader.IsDBNull(5) ? null : reader.GetString(5),
                Kaynak = (HareketKaynak)reader.GetInt32(6),
                KaynakId = reader.IsDBNull(7) ? null : reader.GetInt64(7)
            });
        }

        return list;
    }

    internal bool DeleteIfManual(long hareketId)
    {
        using var connection = Db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
DELETE FROM Hareketler
WHERE Id = $id AND Kaynak = $kaynak;
";
        cmd.Parameters.AddWithValue("$id", hareketId);
        cmd.Parameters.AddWithValue("$kaynak", (int)HareketKaynak.Manuel);
        return cmd.ExecuteNonQuery() > 0;
    }
}
