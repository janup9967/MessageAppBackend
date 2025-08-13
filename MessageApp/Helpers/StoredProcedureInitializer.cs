
using System.Data;
using MessageApp.Data;
using Microsoft.EntityFrameworkCore;


namespace MessageApp.Helpers
{
    public static class StoredProcedureInitializer
    {
        public static void EnsureStoredProcedures(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var connection = context.Database.GetDbConnection();
            connection.Open();

            var requiredProcedures = new[] { "SendMessage", "CreateConversation", "CheckConversationExists", "GetConversationsForUser", "MarkMessageAsRead" , "GetMessagesBetweenUsers"};
            var missingProcedures = new List<string>();

            foreach (var proc in requiredProcedures)
            {
                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = $"SELECT COUNT(*) FROM sys.objects WHERE type = 'P' AND name = '{proc}'";
                var count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count == 0)
                    missingProcedures.Add(proc);
            }

            if (missingProcedures.Any())
            {
                // Read the SQL file and execute it
                var sqlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stored_procedures.sql");
                if (!File.Exists(sqlFilePath))
                {
                    // Try to find in MessageApp subfolder if not in root
                    sqlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MessageApp", "stored_procedures.sql");
                }
                if (File.Exists(sqlFilePath))
                {
                    var sql = File.ReadAllText(sqlFilePath);
                    using var createCmd = connection.CreateCommand();
                    createCmd.CommandText = sql;
                    createCmd.CommandType = CommandType.Text;
                    createCmd.ExecuteNonQuery();
                }
                else
                {
                    throw new FileNotFoundException($"Could not find stored_procedures.sql at {sqlFilePath}");
                }
            }
        }
    }
} 
