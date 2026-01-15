using CariTakip.Models;
using Microsoft.Data.Sqlite;
using System.Globalization;

namespace CariTakip.Data;

internal sealed class CariRepository
{
    internal List<Cari> List(CariTip? tipFilter, string? search)
    {
        using var connection = Db.OpenConnection();
        using var cmd = connection.CreateCommand();

        var where = new List<string>();

        if (tipFilter is not null)
        {
            where.Add("Tip = $tip");
            cmd.Parameters.AddWithValue("$tip", (int)tipFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            where.Add("Unvan LIKE $q");
            cmd.Parameters.AddWithValue("$q", $"%{search.Trim()}%");
        }

        var whereSql = where.Count == 0 ? "" : ("WHERE " + string.Join(" AND ", where));

        cmd.CommandText = $@"
SELECT Id, Tip, Unvan, Telefon, Notlar, CreatedAt
FROM Cariler
{whereSql}
ORDER BY Unvan COLLATE NOCASE;
";

        using var reader = cmd.ExecuteReader();
        var list = new List<Cari>();

        while (reader.Read())
        {
            list.Add(new Cari
            {
                Id = reader.GetInt64(0),
                Tip = (CariTip)reader.GetInt32(1),
                Unvan = reader.GetString(2),
                Telefon = reader.IsDBNull(3) ? null : reader.GetString(3),
                Notlar = reader.IsDBNull(4) ? null : reader.GetString(4),
                CreatedAt = DateTime.ParseExact(reader.GetString(5), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
            });
        }

        return list;
    }

    internal long Insert(Cari cari)
    {
        using var connection = Db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Cariler (Tip, Unvan, Telefon, Notlar, CreatedAt)
VALUES ($tip, $unvan, $telefon, $notlar, $createdAt);
SELECT last_insert_rowid();
";
        cmd.Parameters.AddWithValue("$tip", (int)cari.Tip);
        cmd.Parameters.AddWithValue("$unvan", cari.Unvan.Trim());
        cmd.Parameters.AddWithValue("$telefon", (object?)cari.Telefon?.Trim() ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$notlar", (object?)cari.Notlar?.Trim() ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        return (long)(cmd.ExecuteScalar() ?? 0L);
    }

    internal void Update(Cari cari)
    {
        using var connection = Db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
UPDATE Cariler
SET Tip = $tip,
    Unvan = $unvan,
    Telefon = $telefon,
    Notlar = $notlar
WHERE Id = $id;
";
        cmd.Parameters.AddWithValue("$id", cari.Id);
        cmd.Parameters.AddWithValue("$tip", (int)cari.Tip);
        cmd.Parameters.AddWithValue("$unvan", cari.Unvan.Trim());
        cmd.Parameters.AddWithValue("$telefon", (object?)cari.Telefon?.Trim() ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$notlar", (object?)cari.Notlar?.Trim() ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    internal bool Delete(long cariId)
    {
        using var connection = Db.OpenConnection();
        using var tx = connection.BeginTransaction();

        using (var check = connection.CreateCommand())
        {
            check.Transaction = tx;
            check.CommandText = "SELECT COUNT(1) FROM Hareketler WHERE CariId = $id";
            check.Parameters.AddWithValue("$id", cariId);
            var hareketCount = (long)(check.ExecuteScalar() ?? 0L);
            if (hareketCount > 0)
            {
                tx.Rollback();
                return false;
            }
        }

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = "DELETE FROM Faturalar WHERE CariId = $id";
            cmd.Parameters.AddWithValue("$id", cariId);
            cmd.ExecuteNonQuery();
        }

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = "DELETE FROM Cariler WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", cariId);
            cmd.ExecuteNonQuery();
        }

        tx.Commit();
        return true;
    }

    internal decimal GetBakiye(long cariId)
    {
        using var connection = Db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
SELECT COALESCE(SUM(TutarKurus * Etki), 0)
FROM Hareketler
WHERE CariId = $id;
";
        cmd.Parameters.AddWithValue("$id", cariId);

        var result = cmd.ExecuteScalar();
        if (result is null || result is DBNull) return 0m;
        return Money.FromKurus(Convert.ToInt64(result));
    }

    internal List<(long Id, string Unvan, CariTip Tip)> ListForLookup(CariTip? tipFilter)
    {
        using var connection = Db.OpenConnection();
        using var cmd = connection.CreateCommand();

        var where = "";
        if (tipFilter is not null)
        {
            where = "WHERE Tip = $tip";
            cmd.Parameters.AddWithValue("$tip", (int)tipFilter.Value);
        }

        cmd.CommandText = $@"
SELECT Id, Unvan, Tip
FROM Cariler
{where}
ORDER BY Unvan COLLATE NOCASE;
";

        using var reader = cmd.ExecuteReader();
        var list = new List<(long, string, CariTip)>();
        while (reader.Read())
        {
            list.Add((reader.GetInt64(0), reader.GetString(1), (CariTip)reader.GetInt32(2)));
        }
        return list;
    }
}
