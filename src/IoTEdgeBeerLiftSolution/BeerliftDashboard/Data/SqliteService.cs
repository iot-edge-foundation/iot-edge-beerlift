using System;
using System.Data.SQLite;

namespace BeerliftDashboard.Data
{
    public class SqliteService : IDisposable
    {
        private const string C_TABLE_SETTING = "setting";
        private const string C_COLUMN_KEY = "key";
        private const string C_COLUMN_VALUE = "value";

        private SQLiteConnection con = null;

        public SqliteService(string connectionString)
        {
            con = new SQLiteConnection(connectionString);
            con.Open();

            if (!StructureExists())
            {
                CreateStructure();
            }
        }

        public string GetVersion()
        {
            string stm = "SELECT SQLITE_VERSION()";

            using var cmd = new SQLiteCommand(stm, con);
            string version = cmd.ExecuteScalar().ToString();

            Console.WriteLine($"SQLite version: {version}");

            return version;
        }

        private bool StructureExists()
        {
            try
            {
                string stm = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{C_TABLE_SETTING}'";
                using var cmdCheckInitialDB = new SQLiteCommand(stm, con);

                object tbl = cmdCheckInitialDB.ExecuteScalar();

                if (tbl != null)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");

                return false;
            }
        }

        public string ReadSetting(string key)
        {
            var result = string.Empty;

            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = $"select {C_COLUMN_VALUE} from {C_TABLE_SETTING} where {C_COLUMN_KEY} = '{key}'";

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                result = rdr.GetString(0);
            }

            return result;
        }

        public void WriteSetting(string key, string value)
        {
            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = $"update {C_TABLE_SETTING} set {C_COLUMN_VALUE} = '{value}' where {C_COLUMN_KEY} = '{key}'";

            cmd.ExecuteNonQuery();
        }

        private void CreateStructure()
        {
            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = $"CREATE TABLE {C_TABLE_SETTING}(id INTEGER PRIMARY KEY, {C_COLUMN_KEY} TEXT, {C_COLUMN_VALUE} TEXT)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = $"INSERT INTO {C_TABLE_SETTING}({C_COLUMN_KEY}, {C_COLUMN_VALUE}) VALUES('deviceId', '')";
            cmd.ExecuteNonQuery();

            cmd.CommandText = $"INSERT INTO {C_TABLE_SETTING}({C_COLUMN_KEY}, {C_COLUMN_VALUE}) VALUES('moduleName', '')";
            cmd.ExecuteNonQuery();

            Console.WriteLine($"Table {C_TABLE_SETTING} in {con.Database} created");
        }

        public void Dispose()
        {
            if (con != null)
            {
                if (con.State != System.Data.ConnectionState.Closed)
                {
                    con.Close();
                }

                con = null;
            }
        }
    }
}