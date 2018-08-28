﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace SQLCover.Gateway
{
    public class DatabaseGateway
    {
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly int _commandTimeout;
        private readonly SqlConnectionStringBuilder _connectionStringBuilder;

        public string DataSource { get { return _connectionStringBuilder.DataSource; } }

        public DatabaseGateway()
        {
            //for mocking.
        }
        public DatabaseGateway(string connectionString, string databaseName, int commandTimeout = 30)
        {
            _connectionString = connectionString;
            _databaseName = databaseName;
            _commandTimeout = commandTimeout;
            _connectionStringBuilder =
                new SqlConnectionStringBuilder(connectionString) {InitialCatalog = _databaseName};
        }

        public virtual string GetString(string query)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                {
                    return cmd.ExecuteScalar().ToString();
                }
            }
        }

        public virtual DataTable GetRecords(string query)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        var ds = new DataTable();
                        ds.Load(reader);
                        return ds;
                    }
                }
            }
        }

        public virtual DataTable GetTraceRecords(string query)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        var ds = new DataTable();
                        ds.Columns.Add(new DataColumn("xml"));
                        while (reader.Read())
                        {
                            XmlDocument xml = new XmlDocument();
                            xml.LoadXml(reader[0].ToString());
                             
                            var root = xml.SelectNodes("/event").Item(0);

                            var objectId = xml.SelectNodes("/event/data[@name='object_id']").Item(0);
                            var offset = xml.SelectNodes("/event/data[@name='offset']").Item(0);
                            var offsetEnd = xml.SelectNodes("/event/data[@name='offset_end']").Item(0);

                            root.RemoveAll();
                                 
                            root.AppendChild(objectId);
                            root.AppendChild(offset);
                            root.AppendChild(offsetEnd);

                            var row = ds.NewRow();
                            row["xml"] = root.OuterXml;
                            ds.Rows.Add(row);

                        }

                        return ds;
                    }
                }
            }
        }

        public void Execute(string command, int timeOut)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(command, conn))
                {
                    cmd.CommandTimeout = timeOut;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
