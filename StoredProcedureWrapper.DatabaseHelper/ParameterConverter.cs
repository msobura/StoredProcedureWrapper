using System;
using System.Collections.Generic;
using System.Data;

namespace StoredProcedureWrapper.DatabaseHelper
{
    public static class ParameterConverter
    {
        private class TypeMapping
        {
            public Type DotNetType { get; private set; }
            public string DotNetTypeName { get; private set; }
            public DbType DbType { get; private set; }

            public TypeMapping(Type dotNetType, DbType dbType)
            {
                DotNetType = dotNetType;
                DotNetTypeName = dotNetType.Name + (dotNetType.IsValueType ? "?" : String.Empty);
                DbType = dbType;
            }
        }

        private static readonly Dictionary<string, TypeMapping> SqlTypeMapping;

        static ParameterConverter()
        {
            SqlTypeMapping = new Dictionary<string, TypeMapping>
            {
                { "bigint", new TypeMapping( typeof(long), DbType.Int64) },
                { "binary", new TypeMapping( typeof(byte[]), DbType.Binary) },
                { "bit", new TypeMapping( typeof(bool), DbType.Boolean) },
                { "char", new TypeMapping( typeof(string), DbType.StringFixedLength) },
                { "date", new TypeMapping( typeof(DateTime), DbType.Date) },
                { "datetime", new TypeMapping( typeof(DateTime), DbType.DateTime) },
                { "datetime2", new TypeMapping( typeof(DateTime), DbType.DateTime2) },
                { "datetimeoffset", new TypeMapping( typeof(DateTimeOffset), DbType.DateTimeOffset) },
                { "decimal", new TypeMapping( typeof(decimal), DbType.Decimal) },
                { "float", new TypeMapping( typeof(double), DbType.Double) },
                { "image", new TypeMapping( typeof(byte[]), DbType.Binary) },
                { "int", new TypeMapping( typeof(int), DbType.Int32) },
                { "money", new TypeMapping( typeof(decimal), DbType.Decimal) },
                { "nchar", new TypeMapping( typeof(string), DbType.StringFixedLength) },
                { "ntext", new TypeMapping( typeof(string), DbType.String) },
                { "numeric", new TypeMapping( typeof(decimal), DbType.Decimal) },
                { "nvarchar", new TypeMapping( typeof(string), DbType.String) },
                { "real", new TypeMapping( typeof(float), DbType.Single) },
                { "rowversion", new TypeMapping( typeof(byte[]), DbType.Binary) },
                { "smalldatetime", new TypeMapping( typeof(DateTime), DbType.DateTime) },
                { "smallint", new TypeMapping( typeof(short), DbType.Int16) },
                { "smallmoney", new TypeMapping( typeof(decimal), DbType.Decimal) },
                { "sql_variant", new TypeMapping( typeof(object), DbType.Object) },
                { "sysname", new TypeMapping( typeof(string), DbType.String) },
                { "text", new TypeMapping( typeof(string), DbType.String) },
                { "time", new TypeMapping( typeof(TimeSpan), DbType.Time) },
                { "timestamp", new TypeMapping( typeof(byte[]), DbType.Binary) },
                { "tinyint", new TypeMapping( typeof(byte), DbType.Byte) },
                { "uniqueidentifier", new TypeMapping( typeof(Guid), DbType.Guid) },
                { "varbinary", new TypeMapping( typeof(byte[]), DbType.Binary) },
                { "varchar", new TypeMapping( typeof(string), DbType.String) },
                { "xml", new TypeMapping( typeof(string), DbType.Xml) }
            };
        }

        public static string ToMethodParameterString(StoredProcedureParameter parameter)
        {
            string parameterName = parameter.Name.Replace("@", "");
            string dotNetTypeName = String.Empty;

            TypeMapping typeMapping;
            if (SqlTypeMapping.TryGetValue(parameter.SqlTypeName, out typeMapping))
                dotNetTypeName = typeMapping.DotNetTypeName;

            return String.Format("{0} {1}", dotNetTypeName, parameterName);
        }

        public static string ToDynamicParameterString(StoredProcedureParameter parameter)
        {
            string simpleParameterName = parameter.Name.Replace("@", "");
            string sqlTypeName = String.Empty;

            TypeMapping typeMapping;
            if (SqlTypeMapping.TryGetValue(parameter.SqlTypeName, out typeMapping))
                sqlTypeName = typeMapping.DbType.ToString();

            string direction = String.Empty;
            if (parameter.IsOutput)
                direction = String.Format(", direction: ParameterDirection.InputOutput");

            string size = String.Empty;
            if (IncludeMaxLength(parameter.SqlTypeName))
                size = String.Format(", size: {0}", parameter.MaxLength);

            return String.Format("\"{0}\", {1}, DbType.{2}{3}{4}", parameter.Name, simpleParameterName, sqlTypeName, direction, size);
        }

        private static bool IncludeMaxLength(string sqlTypeName)
        {
            return sqlTypeName.Contains("char")
                   || sqlTypeName.Contains("binary");
        }
    }
}