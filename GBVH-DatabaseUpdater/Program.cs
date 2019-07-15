using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace GBVH_DatabaseUpdater
{
    internal class Program
    {
        private const string DefaultScriptsPath = "..\\Assets\\DatabaseSQLScripts";
        private const string DefaultDbPath = "..\\Assets\\StreamingAssets\\world.bytes";
        private const string DefaultSavePath = "..\\Assets\\StreamingAssets\\progress.bytes"; 
        private static string Error = "";
        
        private const int WORLD = 0;
        private const int PROGRESS = 1;
        
        private static readonly Dictionary<int, (string, string)> _scripts = new Dictionary<int, (string, string)>
        {
            {WORLD, ("world", "00000000-0000_world_create.sql")},
            {PROGRESS, ("progress", "00000000-0000_progress_create.sql")}
        };


        
        public static void Main(string[] args)
        {
            try
            {
                ConsoleKeyInfo key;
                var scr = DefaultScriptsPath;
                var dbp = DefaultDbPath;
                var dbs = DefaultSavePath;
                if (args.Length > 0)
                {
                    if (args[0] =="-s")
                        StartUpdate(WORLD, scr, dbp, false);
                        StartUpdate(PROGRESS, scr, dbs);
                    return;
                }
                do
                {
                    Console.Clear();
                    Console.WriteLine("GeekBrains | Project ┌Van Helsing┘ | Database updater | v1.1 | by Nelfias");
                    Console.WriteLine($"Scripts Path: {scr}");
                    Console.WriteLine($"World Database Path: {dbp}");
                    Console.WriteLine($"Progress Template Database Path: {dbs}");
                    if (Error != "")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine();
                        Console.WriteLine($"Error: {Error}");
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }

                    Console.WriteLine("Start update? [y] - yes, [e] - exit, [o] - change paths");
                    key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.Y:
                            StartUpdate(WORLD, scr, dbp, false);
                            StartUpdate(PROGRESS, scr, dbs);
                            break;
                        case ConsoleKey.O:
                            (scr, dbp, dbs) = ChangeSettings();
                            break;
                    }
                } while (key.Key != ConsoleKey.E);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }

        private static (string, string, string) ChangeSettings()
        {
            Console.Write("Enter Path to scripts:");
            var scr = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Enter Path to World db:");
            var dbp = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Enter Path to Progress db:");
            var dbs = Console.ReadLine();
            return (scr, dbp, dbs);
        }

        private static void StartUpdate(int dbm, string scr, string dbp, bool waitInput = true)
        {
            var db = new DatabaseWrapper(dbp);
            if (db.ExecuteQueryWithAnswer("SELECT name FROM sqlite_master WHERE type='table' AND name='ddl_info';") !=
                "ddl_info")
            {
                var createScript = Path.Combine(scr, _scripts[dbm].Item2);
                if (!File.Exists(createScript))
                {
                    Error = $"{createScript} not found!";
                    return;
                }

                var sql = File.ReadAllText(createScript);
                db.ExecuteQueryWithoutAnswer(sql);
            }

            var historyDataTable = db.GetTable("select * from ddl_info;");
            var history = new List<string>();
            foreach (DataRow row in historyDataTable.Rows)
            {
                history.Add(row.GetString("Patch"));
            }

            var files = Directory.GetFiles(scr, $"*_{_scripts[dbm].Item1}_*.sql");
            Array.Sort(files);
            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                Console.Write($"> {fi.Name} --- ");
                if (history.Contains(fi.Name))
                {
                    Console.WriteLine("OK");
                    continue;
                }
                Console.Write("NO --- Executing... ");
                var str = File.ReadAllText(file);
                db.ExecuteQueryWithoutAnswer(str);
                db.ExecuteQueryWithoutAnswer($"insert into ddl_info (Patch) values (\"{fi.Name}\");");
                Console.WriteLine("OK");
            }

            if (!waitInput) return;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Update Complete! Press any key...");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.ReadKey(true);
        }
    }
}