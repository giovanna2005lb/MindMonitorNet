using System;
using System.Data.SQLite;
using System.IO;

namespace MindMonitor
{
    public static class Database
    {
        private static string DbFile => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.db");
        private static string ConnectionString => $"Data Source={DbFile};Version=3;";

        public static SQLiteConnection GetConnection()
        {
            bool create = !File.Exists(DbFile);
            if (create)
            {
                SQLiteConnection.CreateFile(DbFile);
            }

            var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            if (create)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS RegistroDiario (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Colaborador TEXT NOT NULL,
    Data TEXT NOT NULL,
    Desmotivacao INTEGER NOT NULL,
    Sobrecarga INTEGER NOT NULL,
    Estresse INTEGER NOT NULL
);";
                cmd.ExecuteNonQuery();
            }

            return conn;
        }
    }
}
