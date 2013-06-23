using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace StoredProcedureWrapper.DatabaseHelper.Test
{
    namespace DatabaseHelperClass
    {
        public class GetStoredProceduresMethod : IDisposable
        {
            private class StoredProcedure
            {
                public string SchemaName { get; private set; }
                public string Name { get; private set; }
                public IEnumerable<StoredProcedureParameter> Parameters { get; private set; }

                public StoredProcedure(string schemaName, string name, IEnumerable<StoredProcedureParameter> parameters)
                {
                    SchemaName = schemaName;
                    Name = name;
                    Parameters = new List<StoredProcedureParameter>(parameters);
                }

                public bool Equals(StoredProcedureWrapper.DatabaseHelper.StoredProcedure procedure)
                {
                    if (procedure == null)
                        return false;

                    if (!(Name.Equals(procedure.Name)
                        && SchemaName.Equals(procedure.SchemaName)
                        && Parameters.Count() == procedure.Parameters.Count()))
                        return false;

                    return Parameters.All(p => procedure.Parameters.Any(p.Equals));
                }
            }

            private class StoredProcedureParameter
            {
                public string Name { get; private set; }
                public string SqlType { get; private set; }
                public int? MaxLength { get; private set; }
                public bool IsOutput { get; private set; }
                public string DefaultValue { get; private set; }

                public StoredProcedureParameter(string name, string sqlType, int? maxLength = null, bool isOutput = false)
                    : this(name, sqlType, String.Empty, maxLength, isOutput)
                {

                }

                public StoredProcedureParameter(string name, string sqlType, string defaultValue, int? maxLength = null, bool isOutput = false)
                {
                    Name = String.Format("@{0}", name);
                    SqlType = sqlType;
                    MaxLength = maxLength;
                    DefaultValue = defaultValue;
                    IsOutput = isOutput;
                }

                public override string ToString()
                {
                    string maxLengthPhrase = String.Empty;
                    if (MaxLength.HasValue)
                        maxLengthPhrase = String.Format("({0})", MaxLength);

                    string defaultValuePhrase = String.Empty;
                    if (!String.IsNullOrWhiteSpace(DefaultValue))
                        defaultValuePhrase = String.Format(" = {0}", DefaultValue);

                    string isOutputPhrase = String.Empty;
                    if (IsOutput)
                        isOutputPhrase = " OUTPUT";

                    return String.Format("{0} {1}{2}{3}{4}", Name, SqlType, maxLengthPhrase, defaultValuePhrase, isOutputPhrase);
                }

                public bool Equals(StoredProcedureWrapper.DatabaseHelper.StoredProcedureParameter parameter)
                {
                    if (parameter == null)
                        return false;

                    var skipMaxLengthCheck = SkipMaxLengthCheck();

                    return Name.Equals(parameter.Name)
                           && SqlType.Equals(parameter.SqlTypeName)
                           && (skipMaxLengthCheck || MaxLength == parameter.MaxLength)
                           && DefaultValue.Equals(parameter.DefaultValue)
                           && IsOutput == parameter.IsOutput;
                }

                private bool SkipMaxLengthCheck()
                {
                    return !CheckMaxLength();
                }

                private bool CheckMaxLength()
                {
                    // Only the length of the parameter of type:
                    // - char, varchar and nvarchar;
                    // - binary and varbinary
                    // are important when comparing
                    return SqlType.EndsWith("char") || SqlType.EndsWith("binary");
                }
            }

            private readonly string masterDbConnectionString;
            private readonly string testDbName;
            private readonly string testDbConnectionString;

            public GetStoredProceduresMethod()
            {
                masterDbConnectionString = ConfigurationManager.ConnectionStrings["Master"].ConnectionString;
                using (var conn = new SqlConnection(masterDbConnectionString))
                {
                    testDbName = String.Format("TestDb_{0}", Guid.NewGuid().ToString().Replace("-", "_"));
                    string cmdText = String.Format("CREATE DATABASE [{0}]", testDbName);

                    conn.Open();
                    using (var cmd = new SqlCommand(cmdText, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();

                    var connectionStringBuilder = new SqlConnectionStringBuilder(masterDbConnectionString) { InitialCatalog = testDbName };
                    testDbConnectionString = connectionStringBuilder.ConnectionString;
                }
            }

            public void Dispose()
            {
                using (var conn = new SqlConnection(masterDbConnectionString))
                {
                    string cmdText = String.Format(@"
ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [{0}]", testDbName);

                    conn.Open();
                    using (var cmd = new SqlCommand(cmdText, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }

            private void CreateSchema(string schemaName)
            {
                using (var conn = new SqlConnection(testDbConnectionString))
                {
                    string cmdText = String.Format("CREATE SCHEMA [{0}]", schemaName);

                    conn.Open();
                    using (var cmd = new SqlCommand(cmdText, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }

            private StoredProcedure CreateProcedure(string schemaName, string procedureName)
            {
                return CreateProcedure(schemaName, procedureName, Enumerable.Empty<StoredProcedureParameter>());
            }

            private StoredProcedure CreateProcedure(string schemaName, string procedureName, StoredProcedureParameter parameter)
            {
                var parameters = new List<StoredProcedureParameter> { parameter };
                return CreateProcedure(schemaName, procedureName, parameters);
            }

            private StoredProcedure CreateProcedure(string schemaName, string procedureName, IEnumerable<StoredProcedureParameter> paramList)
            {
                var procedure = new StoredProcedure(schemaName, procedureName, paramList);
                ExecuteCreateProcedureQuery(procedure);
                return procedure;
            }

            private void ExecuteCreateProcedureQuery(StoredProcedure storedProcedure)
            {
                string parameters = String.Join(",\n", storedProcedure.Parameters.Select(p => p.ToString()));

                using (var conn = new SqlConnection(testDbConnectionString))
                {
                    string cmdText = String.Format(@"
CREATE PROCEDURE [{0}].[{1}]
{2}
AS
BEGIN
    SET NOCOUNT ON
END
", storedProcedure.SchemaName, storedProcedure.Name, parameters);

                    conn.Open();
                    using (var cmd = new SqlCommand(cmdText, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }

            [Fact]
            public void should_return_one_procedure_without_any_parameters()
            {
                const string schemaName = "dbo";
                const string procedureName = "Test";

                var procedure = CreateProcedure(schemaName, procedureName);

                var dbHelper = new DatabaseHelper(testDbConnectionString);
                var storedProcedures = dbHelper.GetStoredProcedures();

                var storedProcedure = storedProcedures.Single();
                Assert.True(procedure.Equals(storedProcedure));
            }

            [Fact]
            public void should_return_one_procedure_with_one_int_parameter()
            {
                const string schemaName = "dbo";
                const string procedureName = "Test";

                var parameter = new StoredProcedureParameter("testParam", "int");

                var procedure = CreateProcedure(schemaName, procedureName, parameter);

                var dbHelper = new DatabaseHelper(testDbConnectionString);
                var storedProcedures = dbHelper.GetStoredProcedures();

                var storedProcedure = storedProcedures.Single();
                Assert.True(procedure.Equals(storedProcedure));
            }

            [Fact]
            public void should_return_one_procedure_with_one_varchar_parameter_of_max_length_equals_five()
            {
                const string schemaName = "dbo";
                const string procedureName = "Test";

                const int maxLength = 5;
                var parameter = new StoredProcedureParameter("testParam", "varchar", maxLength);

                var procedure = CreateProcedure(schemaName, procedureName, parameter);

                var dbHelper = new DatabaseHelper(testDbConnectionString);
                var storedProcedures = dbHelper.GetStoredProcedures();

                var storedProcedure = storedProcedures.Single();
                Assert.True(procedure.Equals(storedProcedure));
            }

            [Fact]
            public void should_return_one_procedure_with_one_nvarchar_parameter_of_max_length_equals_five()
            {
                const string schemaName = "dbo";
                const string procedureName = "Test";

                const int maxLength = 5;
                var parameter = new StoredProcedureParameter("testParam", "nvarchar", maxLength);

                var procedure = CreateProcedure(schemaName, procedureName, parameter);

                var dbHelper = new DatabaseHelper(testDbConnectionString);
                var storedProcedures = dbHelper.GetStoredProcedures();

                var storedProcedure = storedProcedures.Single();
                Assert.True(procedure.Equals(storedProcedure));
            }

            [Fact]
            public void should_return_one_procedure_with_one_binary_parameter_of_max_length_equals_seven()
            {
                const string schemaName = "dbo";
                const string procedureName = "Test";

                const int maxLength = 7;
                var parameter = new StoredProcedureParameter("testParam", "binary", maxLength);

                var procedure = CreateProcedure(schemaName, procedureName, parameter);

                var dbHelper = new DatabaseHelper(testDbConnectionString);
                var storedProcedures = dbHelper.GetStoredProcedures();

                var storedProcedure = storedProcedures.Single();
                Assert.True(procedure.Equals(storedProcedure));
            }

            [Fact]
            public void should_return_one_procedure_with_one_varbinary_parameter_of_max_length_equals_eight()
            {
                const string schemaName = "dbo";
                const string procedureName = "Test";

                const int maxLength = 8;
                var parameter = new StoredProcedureParameter("testParam", "varbinary", maxLength);

                var procedure = CreateProcedure(schemaName, procedureName, parameter);

                var dbHelper = new DatabaseHelper(testDbConnectionString);
                var storedProcedures = dbHelper.GetStoredProcedures();

                var storedProcedure = storedProcedures.Single();
                Assert.True(procedure.Equals(storedProcedure));
            }

            [Fact]
            public void should_return_one_procedure_with_one_parameter_of_each_base_sql_type()
            {
                const string schemaName = "dbo";
                const string procedureName = "Test";

                const int maxLength = 1;
                var paramList = new List<StoredProcedureParameter>
                {
                    new StoredProcedureParameter("binaryParam", "binary", maxLength),
                    new StoredProcedureParameter("bitParam", "bit"),
                    new StoredProcedureParameter("charParam", "char", maxLength),
                    new StoredProcedureParameter("dateParam", "date"),
                    new StoredProcedureParameter("datetimeParam", "datetime"),
                    new StoredProcedureParameter("datetime2Param", "datetime2"),
                    new StoredProcedureParameter("datetimeoffsetParam", "datetimeoffset"),
                    new StoredProcedureParameter("decimalParam", "decimal"),
                    new StoredProcedureParameter("floatParam", "float"),
                    new StoredProcedureParameter("geographyParam", "geography"),
                    new StoredProcedureParameter("geometryParam", "geometry"),
                    new StoredProcedureParameter("hierarchyidParam", "hierarchyid"),
                    new StoredProcedureParameter("imageParam", "image"),
                    new StoredProcedureParameter("intParam", "int"),
                    new StoredProcedureParameter("moneyParam", "money"),
                    new StoredProcedureParameter("ncharParam", "nchar", maxLength),
                    new StoredProcedureParameter("ntextParam", "ntext"),
                    new StoredProcedureParameter("numericParam", "numeric"),
                    new StoredProcedureParameter("nvarcharParam", "nvarchar", maxLength),
                    new StoredProcedureParameter("realParam", "real"),
                    new StoredProcedureParameter("smalldatetimeParam", "smalldatetime"),
                    new StoredProcedureParameter("smallintParam", "smallint"),
                    new StoredProcedureParameter("smallmoneyParam", "smallmoney"),
                    new StoredProcedureParameter("sql_variantParam", "sql_variant"),
                    new StoredProcedureParameter("sysnameParam", "sysname"),
                    new StoredProcedureParameter("textParam", "text"),
                    new StoredProcedureParameter("timeParam", "time"),
                    new StoredProcedureParameter("timestampParam", "timestamp"),
                    new StoredProcedureParameter("tinyintParam", "tinyint"),
                    new StoredProcedureParameter("uniqueidentifierParam", "uniqueidentifier"),
                    new StoredProcedureParameter("varbinaryParam", "varbinary", maxLength),
                    new StoredProcedureParameter("varcharParam", "varchar", maxLength),
                    new StoredProcedureParameter("xmlParam", "xml")
                };

                var procedure = CreateProcedure(schemaName, procedureName, paramList);

                var dbHelper = new DatabaseHelper(testDbConnectionString);
                var storedProcedures = dbHelper.GetStoredProcedures();

                var storedProcedure = storedProcedures.Single();
                Assert.True(procedure.Equals(storedProcedure));
            }

            [Fact]
            public void should_return_one_procedure_with_one_timestamp_parameter_when_given_single_rowversion_type_name_in_procedure_initialization()
            {
                const string schemaName = "dbo";
                const string procedureName = "Test";

                const string sqlTypeName = "rowversion";
                // The reason that expected type should be timestamp is that rowversion is actually alias of timestamp.
                const string expectedSqlTypeName = "timestamp";
                var parameter = new StoredProcedureParameter("testParam", sqlTypeName);

                CreateProcedure(schemaName, procedureName, parameter);

                var dbHelper = new DatabaseHelper(testDbConnectionString);
                var storedProcedures = dbHelper.GetStoredProcedures();

                var storedProcedure = storedProcedures.Single();
                Assert.Equal(schemaName, storedProcedure.SchemaName);
                Assert.Equal(procedureName, storedProcedure.Name);
                Assert.Equal(1, storedProcedure.Parameters.Count());

                var procedureParameter = storedProcedure.Parameters.Single();
                Assert.Equal(parameter.Name, procedureParameter.Name);
                Assert.Equal(parameter.DefaultValue, procedureParameter.DefaultValue);
                Assert.Equal(parameter.IsOutput, procedureParameter.IsOutput);

                Assert.NotEqual(parameter.SqlType, procedureParameter.SqlTypeName);
                Assert.Equal(expectedSqlTypeName, procedureParameter.SqlTypeName);
            }

            [Fact]
            public void should_return_one_procedure_with_one_output_int_parameter()
            {
                const string schemaName = "dbo";
                const string procedureName = "Test";

                var parameter = new StoredProcedureParameter("testParam", "int", isOutput: true);

                var procedure = CreateProcedure(schemaName, procedureName, parameter);

                var dbHelper = new DatabaseHelper(testDbConnectionString);
                var storedProcedures = dbHelper.GetStoredProcedures();

                var storedProcedure = storedProcedures.Single();
                Assert.True(procedure.Equals(storedProcedure));
            }

            [Fact]
            public void should_return_one_procedure_with_one_int_parameter_with_default_value_of_one()
            {
                const string schemaName = "dbo";
                const string procedureName = "Test";

                var parameter = new StoredProcedureParameter("testParam", "int", "1");

                var procedure = CreateProcedure(schemaName, procedureName, parameter);

                var dbHelper = new DatabaseHelper(testDbConnectionString);
                var storedProcedures = dbHelper.GetStoredProcedures();

                var storedProcedure = storedProcedures.Single();
                Assert.True(procedure.Equals(storedProcedure));
            }

            [Fact]
            public void should_return_one_procedure_with_one_decimal_parameter_with_default_value_of_two_and_a_half()
            {
                const string schemaName = "dbo";
                const string procedureName = "Test";

                var parameter = new StoredProcedureParameter("testParam", "decimal", "2.5");

                var testProcedure = CreateProcedure(schemaName, procedureName, parameter);

                var dbHelper = new DatabaseHelper(testDbConnectionString);
                var storedProcedures = dbHelper.GetStoredProcedures();

                var storedProcedure = storedProcedures.Single();
                Assert.True(testProcedure.Equals(storedProcedure));
            }

            [Fact]
            public void should_return_one_procedure_with_one_varchar_parameter_with_max_length_of_ten_and_default_value_of_a()
            {
                const string schemaName = "dbo";
                const string procedureName = "Test";

                var parameter = new StoredProcedureParameter("testParam", "varchar", "a", 5);

                var testProcedure = CreateProcedure(schemaName, procedureName, parameter);

                var dbHelper = new DatabaseHelper(testDbConnectionString);
                var storedProcedures = dbHelper.GetStoredProcedures();

                var storedProcedure = storedProcedures.Single();
                Assert.True(testProcedure.Equals(storedProcedure));
            }

            [Fact]
            public void should_return_two_procedures_without_any_parameters()
            {
                const string schemaName = "dbo";
                const string firstProcedureName = "Test1";
                const string secondProcedureName = "Test2";

                var firstTestProcedure = CreateProcedure(schemaName, firstProcedureName);
                var secondTestProcedure = CreateProcedure(schemaName, secondProcedureName);

                var dbHelper = new DatabaseHelper(testDbConnectionString);
                var storedProcedures = dbHelper.GetStoredProcedures();


                Assert.Equal(2, storedProcedures.Count());

                var firstStoreProcedure = storedProcedures.Single(sp => sp.Name.Equals(firstProcedureName));
                Assert.True(firstTestProcedure.Equals(firstStoreProcedure));

                var secondStoredProcedure = storedProcedures.Single(sp => sp.Name.Equals(secondProcedureName));
                Assert.True(secondTestProcedure.Equals(secondStoredProcedure));
            }

            [Fact]
            public void should_return_two_procedures_with_the_same_name_but_different_schema_name()
            {
                const string dboSchemaName = "dbo";
                const string testSchemaName = "TestSchema";
                const string testProcedureName = "TestProc";

                CreateSchema(testSchemaName);
                var firstTestProcedure = CreateProcedure(dboSchemaName, testProcedureName);
                var secondTestProcedure = CreateProcedure(testSchemaName, testProcedureName);

                var dbHelper = new DatabaseHelper(testDbConnectionString);
                var storedProcedures = dbHelper.GetStoredProcedures();

                Assert.Equal(2, storedProcedures.Count());

                storedProcedures = storedProcedures.Where(sp => sp.Name.Equals(testProcedureName));
                Assert.Equal(2, storedProcedures.Count());

                var firstStoredProcedure = storedProcedures.Single(sp => sp.SchemaName.Equals(dboSchemaName));
                Assert.True(firstTestProcedure.Equals(firstStoredProcedure));

                var secondStoredProcedure = storedProcedures.Single(sp => sp.SchemaName.Equals(testSchemaName));
                Assert.True(secondTestProcedure.Equals(secondStoredProcedure));
            }
        }
    }
}
