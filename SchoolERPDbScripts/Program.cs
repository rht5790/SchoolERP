using DbUp;
using DbUp.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SchoolERPDbScripts
{
    internal class Program
    {
        internal static int Main()
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var connectionString = configuration.GetConnectionString("SchoolERPDbConnection");
            Assembly assembly = Assembly.GetExecutingAssembly();
            var scriptsToBeRun = assembly.GetManifestResourceNames();
            if (!scriptsToBeRun.Any()) 
            {
                Console.WriteLine("There are no scripts to run...");
                    return 0;
            }

          
            Console.WriteLine("Start executing migration scripts.....");
            var upgragder = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(assembly, (string s) =>
                s.Contains(".ActiveScripts."))
                .WithTransaction()
                .LogToConsole()
                .Build();

            var scriptResult = upgragder.PerformUpgrade();
            if (!scriptResult.Successful)
            {
                return ReturnError(scriptResult.Error.ToString());
            }

            ShowSuccess();


            Console.WriteLine("Start executing Functions.....");
            var functionExecutor = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(assembly, (string s) =>
                s.Contains(".Functions."))
                .WithTransaction()
                .LogToConsole()
                .Build();

            var functionExecutorResult = functionExecutor.PerformUpgrade();
            if (!functionExecutorResult.Successful)
            {
                return ReturnError(functionExecutorResult.Error.ToString());
            }

            ShowSuccess();

            Console.WriteLine("Start executing Views.....");
            var viewsExecutor = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(assembly, (string s) =>
                s.Contains(".Functions."))
                .WithTransaction()
                .LogToConsole()
                .Build();

            var viewsExecutorResult = viewsExecutor.PerformUpgrade();
            if (!viewsExecutorResult.Successful)
            {
                return ReturnError(viewsExecutorResult.Error.ToString());
            }

            ShowSuccess();


            Console.WriteLine("Start executing stored procedures...");
            var storedProceduresExecutor = 
                DeployChanges.To 
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(assembly,(string s) =>
                s.Contains(".Procedures."))
                .WithTransaction()
                .LogToConsole()
                .JournalTo(new NullJournal())
                .Build();

            var storedProcedureUpgradeResult = storedProceduresExecutor.PerformUpgrade();
            if (!storedProcedureUpgradeResult.Successful)
            {
                return ReturnError(storedProcedureUpgradeResult.Error.ToString());

            }

            ShowSuccess();

            Console.WriteLine("Start executing triggers...");
            var triggerExecutor =
                DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(assembly, (string s) =>
                s.Contains(".Triggers."))
                .WithTransaction()
                .LogToConsole()
                .JournalTo(new NullJournal())
                .Build();
            var triggerUpgradeResult = triggerExecutor.PerformUpgrade();
            if (!triggerUpgradeResult.Successful)
            {
                return ReturnError(triggerUpgradeResult.Error.ToString());

            }

            ShowSuccess();
            return 0;
        }

        private static void ShowSuccess()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success");
            Console.ResetColor();

        }

        private static int ReturnError(string error)
        {
           Console.ForegroundColor= ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
            return -1;
        }
    }
}
