using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using SMOStoredProcedure = Microsoft.SqlServer.Management.Smo.StoredProcedure;
using SMOStoredProcedureParameter = Microsoft.SqlServer.Management.Smo.StoredProcedureParameter;

namespace StoredProcedureWrapper.DatabaseHelper
{
    public sealed class DatabaseHelper
    {
        private readonly SqlConnectionStringBuilder connectionStringBuilder;

        public DatabaseHelper(string connectionString)
        {
            connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
        }

        public IEnumerable<StoredProcedure> GetStoredProcedures()
        {
            var procedures = GetStoredProceduresWithoutParameters();
            var parameters = GetStoredProceduresParameters(procedures);
            var storedProcedures = MergeStoredProceduresWithParameters(procedures, parameters);

            return storedProcedures;
        }

        private IEnumerable<StoredProcedure> GetStoredProceduresWithoutParameters()
        {
            var storedProcedures = new List<StoredProcedure>();
            using (var conn = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                const string procedureIdColumn = "StoredProcedureId";
                const string procedureNameColumn = "ProcedureName";
                const string schemaNameColumn = "SchemaName";

                string sql = String.Format(
@"
SELECT
    [{0}] = P.object_id,
    [{1}] = P.name,
    [{2}] = S.name
FROM sys.procedures AS P
    JOIN sys.schemas AS S ON S.schema_id = P.schema_id
WHERE P.is_ms_shipped = 0", procedureIdColumn, procedureNameColumn, schemaNameColumn);

                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        int procedureIdColumnIndex = reader.GetOrdinal(procedureIdColumn);
                        int procedureNameColumnIndex = reader.GetOrdinal(procedureNameColumn);
                        int schemaNameColumnIndex = reader.GetOrdinal(schemaNameColumn);

                        while (reader.Read())
                        {
                            int storedProcedureId = Convert.ToInt32(reader.GetValue(procedureIdColumnIndex));
                            string procedureName = reader.GetString(procedureNameColumnIndex);
                            string schemaName = reader.GetString(schemaNameColumnIndex);

                            var storedProcedure = new StoredProcedure(storedProcedureId, procedureName, schemaName);
                            storedProcedures.Add(storedProcedure);
                        }
                    }
                }
                conn.Close();
            }
            return storedProcedures;
        }

        private IEnumerable<StoredProcedureParameter> GetStoredProceduresParameters(IEnumerable<StoredProcedure> storedProcedures)
        {
            var smoProcedures = GetSmoStoredProcedures(storedProcedures);
            var smoParameters = GetSmoStoredProceduresParameters(smoProcedures);
            var parameters = ConvertSmoParameters(smoParameters);

            return parameters;
        }

        private IEnumerable<SMOStoredProcedure> GetSmoStoredProcedures(IEnumerable<StoredProcedure> storedProcedures)
        {
            var server = CreateServer();
            var database = ChooseSourceDatabase(server);

            return storedProcedures.Select(storedProcedure => database.StoredProcedures.ItemById(storedProcedure.Id)).ToList();
        }

        private Server CreateServer()
        {
            Server server;
            if (connectionStringBuilder.IntegratedSecurity)
            {
                server = new Server(connectionStringBuilder.DataSource);
            }
            else
            {
                var serverConnection = new ServerConnection(connectionStringBuilder.DataSource, connectionStringBuilder.UserID, connectionStringBuilder.Password);
                server = new Server(serverConnection);
            }
            return server;
        }

        private Database ChooseSourceDatabase(Server server)
        {
            return server.Databases[connectionStringBuilder.InitialCatalog];
        }

        private static IEnumerable<SMOStoredProcedureParameter> GetSmoStoredProceduresParameters(IEnumerable<SMOStoredProcedure> smoProcedures)
        {
            var smoParameters = new List<SMOStoredProcedureParameter>();
            foreach (var smoProcedure in smoProcedures)
            {
                for (int i = 0; i < smoProcedure.Parameters.Count; i++)
                {
                    smoParameters.Add(smoProcedure.Parameters[i]);
                }
            }
            return smoParameters;
        }

        private static IEnumerable<StoredProcedureParameter> ConvertSmoParameters(IEnumerable<SMOStoredProcedureParameter> smoParameters)
        {
            var parameters = new List<StoredProcedureParameter>();
            foreach (var smoParameter in smoParameters)
            {
                string sqlTypeName = smoParameter.DataType.Name;
                if (String.IsNullOrWhiteSpace(sqlTypeName))
                    sqlTypeName = smoParameter.DataType.SqlDataType.ToString().ToLowerInvariant();

                var storedProcedureParameter = new StoredProcedureParameter(
                    smoParameter.ID,
                    smoParameter.Parent.ID,
                    smoParameter.Name,
                    sqlTypeName,
                    smoParameter.DataType.MaximumLength,
                    smoParameter.IsOutputParameter,
                    smoParameter.DefaultValue
                );
                parameters.Add(storedProcedureParameter);
            }
            return parameters;
        }

        private static IEnumerable<StoredProcedure> MergeStoredProceduresWithParameters(IEnumerable<StoredProcedure> storedProceduresWithoutParameters, IEnumerable<StoredProcedureParameter> parameters)
        {
            return storedProceduresWithoutParameters.Select(sp => new StoredProcedure(sp.Id, sp.Name, sp.SchemaName, parameters.Where(p => p.StoredProcedureId == sp.Id))).ToList();
        }
    }
}