using System.Data;
using System.Data.SqlClient;
using UFSoft.UBF.Sys.Database;

namespace U9Custom.UI.OutsourceShortageStatistics.Independent.Common
{
    internal static class SqlHelp
    {
        public static DataTable RunSqlDataTable(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(DatabaseManager.GetCurrentConnection().ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 120;

                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                DataTable table = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(table);
                }

                return table;
            }
        }
    }
}

