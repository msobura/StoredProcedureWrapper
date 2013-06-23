using System;
using System.Data;
using Xunit;
using Xunit.Extensions;

namespace StoredProcedureWrapper.DatabaseHelper.Test
{
    namespace ParameterConverterClass
    {
        public class ToMethodParameterStringMethod
        {
            [Theory]
            [InlineData("bigint", "Int64?")]
            [InlineData("binary", "Byte[]")]
            [InlineData("bit", "Boolean?")]
            [InlineData("char", "String")]
            [InlineData("date", "DateTime?")]
            [InlineData("datetime", "DateTime?")]
            [InlineData("datetime2", "DateTime?")]
            [InlineData("datetimeoffset", "DateTimeOffset?")]
            [InlineData("decimal", "Decimal?")]
            [InlineData("float", "Double?")]
            [InlineData("image", "Byte[]")]
            [InlineData("int", "Int32?")]
            [InlineData("money", "Decimal?")]
            [InlineData("nchar", "String")]
            [InlineData("ntext", "String")]
            [InlineData("numeric", "Decimal?")]
            [InlineData("nvarchar", "String")]
            [InlineData("real", "Single?")]
            [InlineData("rowversion", "Byte[]")]
            [InlineData("smalldatetime", "DateTime?")]
            [InlineData("smallint", "Int16?")]
            [InlineData("smallmoney", "Decimal?")]
            [InlineData("sql_variant", "Object")]
            [InlineData("sysname", "String")]
            [InlineData("text", "String")]
            [InlineData("time", "TimeSpan?")]
            [InlineData("timestamp", "Byte[]")]
            [InlineData("tinyint", "Byte?")]
            [InlineData("uniqueidentifier", "Guid?")]
            [InlineData("varbinary", "Byte[]")]
            [InlineData("varchar", "String")]
            [InlineData("xml", "String")]
            public void should_return_dotnet_type_with_original_name_for_given_tsql_type(string sqlType, string dotNetType)
            {
                var expectedParameterName = sqlType + "Parameter";
                var expectedString = String.Format("{0} {1}", dotNetType, expectedParameterName);

                var sqlParameterName = "@" + expectedParameterName;
                var parameter = new StoredProcedureParameter(0, 0, sqlParameterName, sqlType, 0, false);
                var actualString = ParameterConverter.ToMethodParameterString(parameter);

                Assert.Equal(expectedString, actualString);
            }
        }

        public class ToDynamicParameterStringMethod
        {
            [Theory]
            [InlineData("bigint", DbType.Int64)]
            [InlineData("bit", DbType.Boolean)]
            [InlineData("date", DbType.Date)]
            [InlineData("datetime", DbType.DateTime)]
            [InlineData("datetime2", DbType.DateTime2)]
            [InlineData("datetimeoffset", DbType.DateTimeOffset)]
            [InlineData("decimal", DbType.Decimal)]
            [InlineData("float", DbType.Double)]
            [InlineData("image", DbType.Binary)]
            [InlineData("int", DbType.Int32)]
            [InlineData("money", DbType.Decimal)]
            [InlineData("ntext", DbType.String)]
            [InlineData("numeric", DbType.Decimal)]
            [InlineData("real", DbType.Single)]
            [InlineData("rowversion", DbType.Binary)]
            [InlineData("smalldatetime", DbType.DateTime)]
            [InlineData("smallint", DbType.Int16)]
            [InlineData("smallmoney", DbType.Decimal)]
            [InlineData("sql_variant", DbType.Object)]
            [InlineData("sysname", DbType.String)]
            [InlineData("text", DbType.String)]
            [InlineData("time", DbType.Time)]
            [InlineData("timestamp", DbType.Binary)]
            [InlineData("tinyint", DbType.Byte)]
            [InlineData("uniqueidentifier", DbType.Guid)]
            [InlineData("xml", DbType.Xml)]
            public void should_return_string_containing_sql_parameter_name_simple_name_and_dbtype(string sqlType, DbType dbType)
            {
                var expectedParameterName = sqlType + "Parameter";
                var sqlParameterName = "@" + expectedParameterName;
                var expectedString = String.Format("\"{0}\", {1}, DbType.{2}", sqlParameterName, expectedParameterName, dbType);

                var parameter = new StoredProcedureParameter(0, 0, sqlParameterName, sqlType, 0, false);
                var actualString = ParameterConverter.ToDynamicParameterString(parameter);

                Assert.Equal(expectedString, actualString);
            }

            [Theory]
            [InlineData("bigint", DbType.Int64)]
            [InlineData("bit", DbType.Boolean)]
            [InlineData("date", DbType.Date)]
            [InlineData("datetime", DbType.DateTime)]
            [InlineData("datetime2", DbType.DateTime2)]
            [InlineData("datetimeoffset", DbType.DateTimeOffset)]
            [InlineData("decimal", DbType.Decimal)]
            [InlineData("float", DbType.Double)]
            [InlineData("image", DbType.Binary)]
            [InlineData("int", DbType.Int32)]
            [InlineData("money", DbType.Decimal)]
            [InlineData("ntext", DbType.String)]
            [InlineData("numeric", DbType.Decimal)]
            [InlineData("real", DbType.Single)]
            [InlineData("rowversion", DbType.Binary)]
            [InlineData("smalldatetime", DbType.DateTime)]
            [InlineData("smallint", DbType.Int16)]
            [InlineData("smallmoney", DbType.Decimal)]
            [InlineData("sql_variant", DbType.Object)]
            [InlineData("sysname", DbType.String)]
            [InlineData("text", DbType.String)]
            [InlineData("time", DbType.Time)]
            [InlineData("timestamp", DbType.Binary)]
            [InlineData("tinyint", DbType.Byte)]
            [InlineData("uniqueidentifier", DbType.Guid)]
            [InlineData("xml", DbType.Xml)]
            public void should_return_string_containing_sql_parameter_name_simple_name_dbtype_and_direction(string sqlType, DbType dbType)
            {
                var expectedParameterName = sqlType + "Parameter";
                var sqlParameterName = "@" + expectedParameterName;
                var expectedString = String.Format("\"{0}\", {1}, DbType.{2}, direction: ParameterDirection.InputOutput", sqlParameterName, expectedParameterName, dbType);

                var parameter = new StoredProcedureParameter(0, 0, sqlParameterName, sqlType, 0, true);
                var actualString = ParameterConverter.ToDynamicParameterString(parameter);

                Assert.Equal(expectedString, actualString);
            }

            [Theory]
            [InlineData("binary", DbType.Binary, 1)]
            [InlineData("binary", DbType.Binary, 10)]
            [InlineData("char", DbType.StringFixedLength, 1)]
            [InlineData("char", DbType.StringFixedLength, 10)]
            [InlineData("nchar", DbType.StringFixedLength, 1)]
            [InlineData("nchar", DbType.StringFixedLength, 10)]
            [InlineData("nvarchar", DbType.String, 1)]
            [InlineData("nvarchar", DbType.String, 10)]
            [InlineData("nvarchar", DbType.String, -1)]
            [InlineData("varbinary", DbType.Binary, 1)]
            [InlineData("varbinary", DbType.Binary, 10)]
            [InlineData("varbinary", DbType.Binary, -1)]
            [InlineData("varchar", DbType.String, 1)]
            [InlineData("varchar", DbType.String, 10)]
            [InlineData("varchar", DbType.String, -1)]
            public void should_return_string_containing_sql_parameter_name_simple_name_dbtype_and_size(string sqlType, DbType dbType, int size)
            {
                var expectedParameterName = sqlType + "Parameter";
                var sqlParameterName = "@" + expectedParameterName;
                var expectedString = String.Format("\"{0}\", {1}, DbType.{2}, size: {3}", sqlParameterName, expectedParameterName, dbType, size);

                var parameter = new StoredProcedureParameter(0, 0, sqlParameterName, sqlType, size, false);
                var actualString = ParameterConverter.ToDynamicParameterString(parameter);

                Assert.Equal(expectedString, actualString);
            }

            [Theory]
            [InlineData("binary", DbType.Binary, 1)]
            [InlineData("binary", DbType.Binary, 10)]
            [InlineData("char", DbType.StringFixedLength, 1)]
            [InlineData("char", DbType.StringFixedLength, 10)]
            [InlineData("nchar", DbType.StringFixedLength, 1)]
            [InlineData("nchar", DbType.StringFixedLength, 10)]
            [InlineData("nvarchar", DbType.String, 1)]
            [InlineData("nvarchar", DbType.String, 10)]
            [InlineData("nvarchar", DbType.String, -1)]
            [InlineData("varbinary", DbType.Binary, 1)]
            [InlineData("varbinary", DbType.Binary, 10)]
            [InlineData("varbinary", DbType.Binary, -1)]
            [InlineData("varchar", DbType.String, 1)]
            [InlineData("varchar", DbType.String, 10)]
            [InlineData("varchar", DbType.String, -1)]
            public void should_return_string_containing_sql_parameter_name_simple_name_dbtype_diretion_and_size(string sqlType, DbType dbType, int size)
            {
                var expectedParameterName = sqlType + "Parameter";
                var sqlParameterName = "@" + expectedParameterName;
                var expectedString = String.Format("\"{0}\", {1}, DbType.{2}, direction: ParameterDirection.InputOutput, size: {3}", sqlParameterName, expectedParameterName, dbType, size);

                var parameter = new StoredProcedureParameter(0, 0, sqlParameterName, sqlType, size, true);
                var actualString = ParameterConverter.ToDynamicParameterString(parameter);

                Assert.Equal(expectedString, actualString);
            }
        }
    }
}