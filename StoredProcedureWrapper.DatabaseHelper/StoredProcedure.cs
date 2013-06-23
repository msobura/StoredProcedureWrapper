using System.Collections.Generic;
using System.Linq;

namespace StoredProcedureWrapper.DatabaseHelper
{
    public sealed class StoredProcedure
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string SchemaName { get; private set; }
        public IEnumerable<StoredProcedureParameter> Parameters { get; private set; }

        public StoredProcedure(int procedureId, string procedureName, string schemaName)
            : this(procedureId, procedureName, schemaName, Enumerable.Empty<StoredProcedureParameter>())
        {

        }

        public StoredProcedure(int procedureId, string procedureName, string schemaName, IEnumerable<StoredProcedureParameter> parameters)
        {
            Id = procedureId;
            Name = procedureName;
            SchemaName = schemaName;
            Parameters = new List<StoredProcedureParameter>(parameters);
        }
    }
}