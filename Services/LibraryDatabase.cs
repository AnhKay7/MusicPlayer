using Microsoft.Data.Sqlite;
using MusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MusicPlayer.Services
{
    public class LibraryDatabase
    {
        private readonly string connectionString;

        public LibraryDatabase()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(localAppData, "MusicPlayer");
            Directory.CreateDirectory(appFolder);
            string dbPath = Path.Combine(appFolder, "library.db");
            connectionString = $"Data Source={dbPath}";
        }

        private SqliteConnection OpenConnection()
        {
            var conn = new SqliteConnection(connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA foreign_keys = ON;";

            cmd.ExecuteNonQuery();
            return conn;
        }
        public void CreateTables()
        {
            using var conn = OpenConnection();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS ScanFolders (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    Path        TEXT NOT NULL UNIQUE,
                    AddedAt     TEXT NOT NULL
                );
                """;

            cmd.CommandText += """
                CREATE TABLE IF NOT EXISTS Songs (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    FilePath        TEXT NOT NULL UNIQUE,
                    Title           TEXT NOT NULL,
                    Artists         TEXT,
                    Album           TEXT,
                    DurationMs      INTEGER NOT NULL,
                    FileSize        INTEGER NOT NULL,
                    LastModified    TEXT NOT NULL,
                    DateAdded       TEXT NOT NULL,
                    FolderId        INTEGER REFERENCES ScanFolders(Id)
                );
                """;
            cmd.ExecuteNonQuery();
        }

        public void InsertSong(Song song, long? folderId = null)
        {

            using var conn = OpenConnection();

            using var cmd = conn.CreateCommand();

            cmd.CommandText = """
                INSERT INTO Songs (FilePath, Title, Artists, Album, DurationMs, FileSize, LastModified, DateAdded, FolderId)
                    VALUES (@FilePath, @Title, @Artists, @Album, @DurationMs, @FileSize, @LastModified, @DateAdded, @FolderId);
                """;
            cmd.Parameters.AddWithValue("@FilePath",        song.FilePath);
            cmd.Parameters.AddWithValue("@Title",           song.Title);
            cmd.Parameters.AddWithValue("@Artists",         song.Artists);
            cmd.Parameters.AddWithValue("@Album",           song.Album);
            cmd.Parameters.AddWithValue("@DurationMs",      (long) song.Duration.TotalMilliseconds);
            cmd.Parameters.AddWithValue("@FileSize",        song.FileSize);   
            cmd.Parameters.AddWithValue("@LastModified",    song.LastModified.ToString("O"));
            cmd.Parameters.AddWithValue("@DateAdded",       song.DateAdded.ToString("O"));
            cmd.Parameters.AddWithValue("@FolderId",        (object?) folderId ?? DBNull.Value);

            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid();";
            object? result = cmd.ExecuteScalar();
            if (result != null)
                song.Id = Convert.ToInt64(result);

            //Debug.Print($"Added song: {song.Title}, Id: {song.Id}");
        }

        public List<Song> GetAllSongs()
        {
            using var conn = OpenConnection();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT Id, FilePath, Title, Artists, Album, DurationMs, FileSize, LastModified, DateAdded
                    FROM Songs;
                """;

            List<Song> songs = new List<Song>();

            using SqliteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Song song = new Song();
                song.Id = reader.GetInt64(reader.GetOrdinal("Id"));
                song.FilePath = reader.GetString(reader.GetOrdinal("FilePath"));
                song.Title = reader.GetString(reader.GetOrdinal("Title"));

                int artists = reader.GetOrdinal("Artists");
                if (!reader.IsDBNull(artists))
                    song.Artists = reader.GetString(artists);

                int album = reader.GetOrdinal("Album");
                if (!reader.IsDBNull(album))
                    song.Album = reader.GetString(album);

                song.Duration = TimeSpan.FromMilliseconds(reader.GetInt64(reader.GetOrdinal("DurationMs")));
                song.FileSize = reader.GetInt64(reader.GetOrdinal("FileSize"));
                song.LastModified = Convert.ToDateTime(reader.GetString(reader.GetOrdinal("LastModified")));
                song.DateAdded = Convert.ToDateTime(reader.GetString(reader.GetOrdinal("DateAdded")));
                songs.Add(song);
                Debug.Print($"Has read song: {song.Title}, Id:{song.Id}");
            }

            return songs;
        }
    }
}
