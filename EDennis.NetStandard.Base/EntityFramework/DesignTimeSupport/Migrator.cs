using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System;
using Microsoft.EntityFrameworkCore.Migrations;
using EDennis.MigrationsExtensions;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Performs code-first migrations on a context.
    /// Ensure that TContext has a constructor that takes DbContextOptions<TContext> as an argument.
    /// Also, if using SQL, rather than HasData for seeding the database, ensure that you
    /// provide the pathToSqlFolder argument (e.g., "..\\EDennis.Samples.ColorApp\\Shared\\MigrationsInserts".
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public static class Migrator<TContext>
        where TContext : DbContext {


        public static void Migrate(IHost host, Serilog.ILogger logger, string pathToSqlFolder = null) {
            Migrate(host, new SerilogLoggerProvider(logger).CreateLogger(nameof(Migrator<TContext>)), pathToSqlFolder);
        }

        public static void Migrate(IHost host, ILogger logger = null, string pathToSqlFolder = null) {

            using var scope = host.Services.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<TContext>();

            //replace the IMigrationsSqlGenerator with custom generator that can generate system versioned tables 
            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            optionsBuilder.UseSqlServer(context.Database.GetDbConnection().ConnectionString)
                .ReplaceService<IMigrationsSqlGenerator, MigrationsExtensionsSqlGenerator>();
            context = Activator.CreateInstance(typeof(TContext), new object[] { optionsBuilder.Options }) as TContext;



            Log("Dropping Database...", logger);
            context.Database.EnsureDeleted();

            Log("Migrating Database...", logger);
            context.Database.Migrate();

            var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            var contentRoot = env.ContentRootPath;


            if (pathToSqlFolder == null)
                return;

            pathToSqlFolder = ResolveDirectory(contentRoot, pathToSqlFolder, logger);

            if (Directory.Exists(pathToSqlFolder)) {
                using var cxn = context.Database.GetDbConnection() as SqlConnection;
                if (cxn.State != System.Data.ConnectionState.Open)
                    cxn.Open();
                var sqlFiles = Directory.EnumerateFiles(pathToSqlFolder);
                foreach (var file in sqlFiles) {
                    var sql = File.ReadAllText(file);
                    Log($"Executing {file}...",logger);
                    var cmd = new SqlCommand(sql, cxn);
                    cmd.ExecuteScalar();
                }
            }

        }


        private static string ResolveDirectory(string referenceDirectory, string relativePath, ILogger logger = null) {
            var folders = relativePath.Split('/', '\\');
            var di = new DirectoryInfo(referenceDirectory);
            foreach (var folder in folders) {
                if (folder == "..")
                    di = di.Parent;
                else {
                    try {
                        di = di.GetDirectories().Single(s=>s.Name.Equals(folder));
                    } catch (Exception ex) {
                        Log($"relativePath {relativePath} is not valid from {referenceDirectory}.", logger);
                        throw new ArgumentException(ex.Message);
                    }
                }
            }
            return di.FullName;
        }


        private static void Log(string message, ILogger logger = null) {
            if (logger != null)
                logger.LogInformation(message);
            else
                Debug.WriteLine(message);
        }

    }
}
