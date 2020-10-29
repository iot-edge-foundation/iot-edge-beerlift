﻿using BeerliftDashboard.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace BeerliftDashboard.Data
{
    public class SqliteService : IDisposable
    {
        private const string C_TABLE_SETTING = "setting";
        private const string C_TABLE_BEERLIFT = "beerlift";
        private const string C_COLUMN_ID = "id";
        private const string C_COLUMN_KEY = "key";
        private const string C_COLUMN_VALUE = "value";
        private const string C_COLUMN_NAME = "name";
        private const string C_COLUMN_STATE = "state";
        private const string C_COLUMN_DEVICEID = "deviceid";
        private const string C_COLUMN_MODULENAME = "modulename";

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

        public void IntializeBeerlift(string deviceId, string moduleName)
        {
            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = $"select count(*) from {C_TABLE_BEERLIFT} where {C_COLUMN_DEVICEID} = '{deviceId}' and {C_COLUMN_MODULENAME} = '{moduleName}'";

            var count = Convert.ToInt32(cmd.ExecuteScalar());

            if (count == 0)
            {
                for (int i = 0; i < 16; i++)
                {
                    cmd.CommandText = $"INSERT INTO {C_TABLE_BEERLIFT}({C_COLUMN_DEVICEID}, {C_COLUMN_MODULENAME}, {C_COLUMN_NAME}, {C_COLUMN_STATE}) VALUES('{deviceId}', '{moduleName}', '', '')";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Bottleholder> GetBottleHolders(string deviceId, string moduleName)
        {
            var result = new List<Bottleholder>();

            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = $"select {C_COLUMN_ID}, {C_COLUMN_NAME}, {C_COLUMN_STATE} from {C_TABLE_BEERLIFT} where {C_COLUMN_DEVICEID} = '{deviceId}' and {C_COLUMN_MODULENAME} = '{moduleName}' order by {C_COLUMN_ID}";

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                result.Add(new Bottleholder { id = rdr.GetInt32(0), name = rdr.GetString(1), state = rdr.GetString(2) });
            }

            return result;
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

            cmd.CommandText = $"INSERT INTO {C_TABLE_SETTING}({C_COLUMN_KEY}, {C_COLUMN_VALUE}) VALUES('password', 'bl1234!')";
            cmd.ExecuteNonQuery();

            Console.WriteLine($"Table {C_TABLE_SETTING} in {con.Database} created");

            cmd.CommandText = $"CREATE TABLE {C_TABLE_BEERLIFT}(id INTEGER PRIMARY KEY, {C_COLUMN_NAME} TEXT, {C_COLUMN_STATE} TEXT, {C_COLUMN_DEVICEID} TEXT, {C_COLUMN_MODULENAME} TEXT)";
            cmd.ExecuteNonQuery();
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