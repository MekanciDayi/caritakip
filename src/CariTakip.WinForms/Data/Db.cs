using Microsoft.Data.Sqlite;

namespace CariTakip.Data;

internal static class Db
{
    internal static string GetDbFilePath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CariTakip");

        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "cari.db");
    }

    internal static SqliteConnection OpenConnection()
    {
        var path = GetDbFilePath();
        var connection = new SqliteConnection($"Data Source={path}");
        connection.Open();
        return connection;
    }

    internal static void Initialize()
    {
        using var connection = OpenConnection();

        MigrateMoneyColumnsIfNeeded(connection);

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Cariler (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Tip INTEGER NOT NULL,
  Unvan TEXT NOT NULL,
  Telefon TEXT NULL,
  Notlar TEXT NULL,
  CreatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Hareketler (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  CariId INTEGER NOT NULL,
  Tarih TEXT NOT NULL,
  Etki INTEGER NOT NULL,
  TutarKurus INTEGER NOT NULL,
  Aciklama TEXT NULL,
  Kaynak INTEGER NOT NULL,
  KaynakId INTEGER NULL,
  FOREIGN KEY(CariId) REFERENCES Cariler(Id)
);

CREATE TABLE IF NOT EXISTS Faturalar (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  CariId INTEGER NOT NULL,
  FaturaNo TEXT NOT NULL,
  Tarih TEXT NOT NULL,
  ToplamKurus INTEGER NOT NULL,
  FOREIGN KEY(CariId) REFERENCES Cariler(Id)
);

CREATE INDEX IF NOT EXISTS IX_Hareketler_CariId_Tarih ON Hareketler (CariId, Tarih);
CREATE INDEX IF NOT EXISTS IX_Faturalar_CariId_Tarih ON Faturalar (CariId, Tarih);
";
        cmd.ExecuteNonQuery();
    }

    private static void MigrateMoneyColumnsIfNeeded(SqliteConnection connection)
    {
        using var tx = connection.BeginTransaction();

        var hareketHasTutar = HasColumn(connection, tx, "Hareketler", "Tutar");
        var hareketHasTutarKurus = HasColumn(connection, tx, "Hareketler", "TutarKurus");

        if (hareketHasTutar && !hareketHasTutarKurus)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Hareketler_new (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  CariId INTEGER NOT NULL,
  Tarih TEXT NOT NULL,
  Etki INTEGER NOT NULL,
  TutarKurus INTEGER NOT NULL,
  Aciklama TEXT NULL,
  Kaynak INTEGER NOT NULL,
  KaynakId INTEGER NULL,
  FOREIGN KEY(CariId) REFERENCES Cariler(Id)
);

INSERT INTO Hareketler_new (Id, CariId, Tarih, Etki, TutarKurus, Aciklama, Kaynak, KaynakId)
SELECT Id, CariId, Tarih, Etki, CAST(ROUND(Tutar * 100.0, 0) AS INTEGER), Aciklama, Kaynak, KaynakId
FROM Hareketler;

DROP TABLE Hareketler;
ALTER TABLE Hareketler_new RENAME TO Hareketler;
";
                cmd.ExecuteNonQuery();
            }
        }

        var faturaHasToplam = HasColumn(connection, tx, "Faturalar", "Toplam");
        var faturaHasToplamKurus = HasColumn(connection, tx, "Faturalar", "ToplamKurus");

        if (faturaHasToplam && !faturaHasToplamKurus)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Faturalar_new (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  CariId INTEGER NOT NULL,
  FaturaNo TEXT NOT NULL,
  Tarih TEXT NOT NULL,
  ToplamKurus INTEGER NOT NULL,
  FOREIGN KEY(CariId) REFERENCES Cariler(Id)
);

INSERT INTO Faturalar_new (Id, CariId, FaturaNo, Tarih, ToplamKurus)
SELECT Id, CariId, FaturaNo, Tarih, CAST(ROUND(Toplam * 100.0, 0) AS INTEGER)
FROM Faturalar;

DROP TABLE Faturalar;
ALTER TABLE Faturalar_new RENAME TO Faturalar;
";
                cmd.ExecuteNonQuery();
            }
        }

        tx.Commit();
    }

    private static bool HasColumn(SqliteConnection connection, SqliteTransaction tx, string tableName, string columnName)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = $"PRAGMA table_info({tableName});";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var name = reader.GetString(1);
            if (string.Equals(name, columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
