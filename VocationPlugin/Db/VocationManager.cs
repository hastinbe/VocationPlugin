#region Copyright & License Information
/*
 * Copyright 2015 Beau D. Hastings
 * This file is part of VocationPlugin, a plugin for TShock, which is free
 * software. It is made available to you under the terms of the GNU 
 * General Public License as published by the Free Software Foundation.
 * For more information, see COPYING.
 */
#endregion

using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using MySql.Data.MySqlClient;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace VocationPlugin.Db
{
    public class VocationManager
    {
        private string _tablePrefix = "plugin";
        private string _tableName = "Vocation";
        private readonly IDbConnection _conn;

        internal string TableName
        {
            get 
            {
                return String.Concat(_tablePrefix, _tableName);
            } 
        }
        private IQueryBuilder DbProvider
        {
            get
            {
                return _conn.GetSqlType() == SqlType.Mysql 
                    ? (IQueryBuilder)new MysqlQueryCreator()
                    : (IQueryBuilder)new SqliteQueryCreator();
            }
        }

        public VocationManager(IDbConnection conn)
        {
            _conn = conn;

			var table = new SqlTable(TableName,
				new SqlColumn("id", MySqlDbType.Int32) { Primary = true },
				new SqlColumn("level", MySqlDbType.Int32),
				new SqlColumn("experience", MySqlDbType.Int64),
				new SqlColumn("vocation", MySqlDbType.VarChar) { Primary = true, Length = 16 });

            var creator = new SqlTableCreator(conn, DbProvider);
            creator.EnsureTableStructure(table);
        }
    }
}
