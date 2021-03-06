﻿using System;
using System.Collections.Generic;
using System.Linq;
using Beursspel.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace Beursspel
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(false);
            var doMigrate = commandLineApplication.Option(
                "--ef-migrate",
                "Apply entity framework migrations and exit",
                CommandOptionType.NoValue);
            var verifyMigrate = commandLineApplication.Option(
                "--ef-migrate-check",
                "Check the status of entity framework migrations",
                CommandOptionType.NoValue);
            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.OnExecute(() =>
            {
                RunApp(args, doMigrate, verifyMigrate);
                return 0;
            });
            commandLineApplication.Execute(args);
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://0.0.0.0:5000")
                .Build();

        private static void RunApp(string[] args, CommandOption doMigrate, CommandOption verifyMigrate)
        {
            var webhost = BuildWebHost(args);

            if (verifyMigrate.HasValue() && doMigrate.HasValue())
            {
                Console.WriteLine("ef-migrate and ef-migrate-check are mutually exclusive, select one, and try again");
                Environment.Exit(2);
            }

            if (verifyMigrate.HasValue())
            {
                Console.WriteLine("Validating status of Entity Framework migrations");
                using (var context = webhost.Services.GetService<ApplicationDbContext>())
                {
                    var pendingMigrations = context.Database.GetPendingMigrations();
                    var migrations = pendingMigrations as IList<string> ?? pendingMigrations.ToList();
                    if (!migrations.Any())
                    {
                        Console.WriteLine("No pending migratons");
                        Environment.Exit(0);
                    }

                    Console.WriteLine("Pending migratons {0}", migrations.Count());
                    foreach (var migration in migrations)
                    {
                        Console.WriteLine($"\t{migration}");
                    }

                    Environment.Exit(3);
                }
            }

            if (doMigrate.HasValue())
            {
                Console.WriteLine("Applyting Entity Framework migrations");
                using (var context = webhost.Services.GetService<ApplicationDbContext>())
                {
                    context.Database.Migrate();
                    Console.WriteLine("All done, closing app");
                    Environment.Exit(0);
                }
            }

            var createSettings = Settings.LoadSettings();
            createSettings.Wait();

            webhost.Run();


        }
    }
}
