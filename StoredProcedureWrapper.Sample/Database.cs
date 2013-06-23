using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace StoredProcedureWrapper.Sample
{
    public partial class Database
    {
        private readonly string connectionString;

        public Database(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public int AddUser(User user)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var sp = dbo.uspUser_Add(
                    isLocked: user.IsLocked,
                    firstName: user.FirstName,
                    middleName: user.MiddleName,
                    lastName: user.LastName,
                    birthday: user.Birthday,
                    userId: user.Id
                );

                conn.Execute(sql: sp.FullName, param: sp.Parameters, commandType: sp.CommandType);

                return sp.Parameters.Get<int>("userId");
            }
        }

        public User GetUser(int userId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var sp = dbo.uspUser_Get(
                    userId: userId
                );

                return conn.Query<User>(sql: sp.FullName, param: sp.Parameters, commandType: sp.CommandType).Single();
            }
        }
    }
}
