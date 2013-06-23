namespace StoredProcedureWrapper.DatabaseHelper
{
    public sealed class StoredProcedureParameter
    {
        public int Id { get; private set; }
        public int StoredProcedureId { get; private set; }
        public string Name { get; private set; }
        public string SqlTypeName { get; private set; }
        public int MaxLength { get; private set; }
        public bool IsOutput { get; private set; }
        public string DefaultValue { get; private set; }

        public StoredProcedureParameter(int parameterId, int storedProcedureId, string parameterName, string sqlTypeName, int maxLength, bool isOutput, string defaultValue = null)
        {
            Id = parameterId;
            StoredProcedureId = storedProcedureId;
            Name = parameterName;
            SqlTypeName = sqlTypeName;
            MaxLength = maxLength;
            IsOutput = isOutput;
            DefaultValue = defaultValue;
        }
    }
}