using System;
using System.IO;

namespace Bibles.DataResources
{
    internal class DbConstraints
    {
        public const string DatabaseFilename = "BiblesVSN.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath
        {
            get
            {
                string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                return Path.Combine(basePath, DbConstraints.DatabaseFilename);
            }
        }
    }
}
