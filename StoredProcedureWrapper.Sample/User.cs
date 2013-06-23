using System;

namespace StoredProcedureWrapper.Sample
{
    public class User
    {
        public int Id { get; set; }
        public bool IsLocked { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthday { get; set; }
    }
}