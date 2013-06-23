using System;
using System.Data;
using Dapper;

namespace StoredProcedureWrapper.Sample
{
    public partial class Database
    {
        private class StoredProcedureContext
        {
            public string FullName { get; private set; }
            public DynamicParameters Parameters { get; private set; }
            public CommandType CommandType
            {
                get { return CommandType.StoredProcedure; }
            }
            
            public StoredProcedureContext(string schemaName, string procedureName, DynamicParameters parameters)
            {
                FullName = String.Format("[{0}].[{1}]", schemaName, procedureName);
                Parameters = parameters;
            }
        }

        private static class dbo
        {
            // Procedure dbo.uspUser_Get
            // Parameter:
            // Name, Type, MaxLength, IsOutput
            // - @userId, int, 4, False
            public static StoredProcedureContext uspUser_Get(Int32? userId)
            {
                var p = new DynamicParameters();
                p.Add("@userId", userId, DbType.Int32);

                return new StoredProcedureContext("dbo", "uspUser_Get", p);
            }
            // Procedure dbo.uspUser_Add
            // Parameters:
            // Name, Type, MaxLength, IsOutput
            // - @isLocked, bit, 1, False
            // - @firstName, nvarchar, 50, False
            // - @middleName, nvarchar, 50, False
            // - @lastName, nvarchar, 100, False
            // - @birthday, date, 3, False
            // - @userId, int, 4, True
            public static StoredProcedureContext uspUser_Add(Boolean? isLocked, String firstName, String middleName, String lastName, DateTime? birthday, Int32? userId)
            {
                var p = new DynamicParameters();
                p.Add("@isLocked", isLocked, DbType.Boolean);
                p.Add("@firstName", firstName, DbType.String, size: 50);
                p.Add("@middleName", middleName, DbType.String, size: 50);
                p.Add("@lastName", lastName, DbType.String, size: 100);
                p.Add("@birthday", birthday, DbType.Date);
                p.Add("@userId", userId, DbType.Int32, direction: ParameterDirection.InputOutput);

                return new StoredProcedureContext("dbo", "uspUser_Add", p);
            }
        }
    }
}