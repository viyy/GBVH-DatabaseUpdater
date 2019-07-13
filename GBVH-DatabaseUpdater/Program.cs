using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace GBVH_DatabaseUpdater
{
    internal class Program
    {
        private const string DefaultScriptsPath = "..\\Assets\\DatabaseSQLScripts";
        private const string DefaultDbPath = "..\\Assets\\StreamingAssets\\world.bytes";
        private static string Error = "";
        public static void Main(string[] args)
        {
            try
            {
                ConsoleKeyInfo key;
                var scr = DefaultScriptsPath;
                var dbp = DefaultDbPath;
                do
                {
                    Console.Clear();
                    Console.WriteLine($"Scripts Path: {scr}");
                    Console.WriteLine($"Database Path: {dbp}");
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
                            StartUpdate(scr, dbp);
                            break;
                        case ConsoleKey.O:
                            ChangeSettings(ref scr, ref dbp);
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

        private static void ChangeSettings(ref string scr, ref string dbp)
        {
            Console.Write("Enter Path to scripts:");
            scr = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Enter Path to db:");
            dbp = Console.ReadLine();
        }

        private static void StartUpdate(string scr, string dbp)
        {
            var db = new DatabaseWrapper(dbp);
            if (db.ExecuteQueryWithAnswer("SELECT name FROM sqlite_master WHERE type='table' AND name='ddl_info';") !=
                "ddl_info")
            {
                var createScript = Path.Combine(scr, "00000000-0000_world_create.sql");
                if (!File.Exists(createScript))
                {
                    Error = "00000000-0000_world_create.sql not found!";
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
            foreach (var file in Directory.GetFiles(scr))
            {
                var fi = new FileInfo(file);
                if (!fi.Name.EndsWith(".sql")) continue;
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

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Update Complete! Press any key...");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.ReadKey(true);
        }
    }
}