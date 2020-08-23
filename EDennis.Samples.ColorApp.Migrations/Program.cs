using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace EDennis.Samples.ColorApp.Migrations {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("This app is a dummy app used to run database migrations.");
            Console.WriteLine("Ensure that this project has the MigrationsInserts folder and appsettings.Development.json");
            Console.WriteLine("Ensure that ColorApp.Shared has the DbContext class and the Migrations folder");
            Console.WriteLine("From the Package Manager, run ...");
            Console.WriteLine("Update-Database -Context ColorContext -Project EDennis.Samples.ColorApp.Shared -StartupProject EDennis.Samples.ColorApp.Migrations");
        }
    }
}
