using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Performs code-first migrations on a context.
    /// Ensure that TContext has a constructor that takes DbContextOptions<TContext> as an argument.
    /// Also, if using SQL, rather than HasData for seeding the database, ensure that you
    /// create a link in the project that is executing this Migrator.
    /// <code>
    ///   <ItemGroup>
    ///     <Content Include = "..\EDennis.Samples.ColorApp\Shared\MigrationsInserts\**\*.*" >
    ///       <Link> MigrationsSql\%(RecursiveDir)%(FileName)%(Extension)</Link>
    ///       <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    ///     </Content>
    ///   </ItemGroup>
    /// </code>
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public static class Migrator<TContext>
        where TContext : DbContext {


        public static void Migrate(IHost host, Serilog.ILogger logger, string sqlFolder = null) {
            Migrate(host, new SerilogLoggerProvider(logger).CreateLogger(nameof(Migrator<TContext>)), sqlFolder );
        }

        public static void Migrate(IHost host, ILogger logger = null, string sqlFolder = null ) {

                using var scope = host.Services.CreateScope();

                var context = scope.ServiceProvider
                    .GetRequiredService<TContext>();

                Log("Dropping Database...", logger);
                context.Database.EnsureDeleted();

                Log("Migrating Database...", logger);
                context.Database.Migrate();

                if (Directory.Exists(sqlFolder)) {
                    var cxn = context.Database.GetDbConnection() as SqlConnection;
                    var sqlFiles = Directory.EnumerateFiles(sqlFolder);
                    foreach (var file in sqlFiles) {
                        var sql = File.ReadAllText(file);
                        Log($"Executing {file}...");
                        var cmd = new SqlCommand(sql, cxn);
                        cmd.ExecuteScalar();
                    }
                }

        }

        private static void Log(string message, ILogger logger = null) {
            if (logger != null)
                logger.LogInformation(message);
            else
                Debug.WriteLine(message);
        }

    }
}
