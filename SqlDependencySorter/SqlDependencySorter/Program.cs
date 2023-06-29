using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDependencySorter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "--gen_setting")
                {
                    new Setting().Save();
                    Console.WriteLine("sample setting file is generated.");
                    return;
                }
                else if (args[0] == "--help")
                {
                    Console.WriteLine(
                        "---------------------------------------------------------\n" +
                        "                    SqlDependencySorter\n" +
                        "---------------------------------------------------------\n" +
                        "options: \n" +
                        "          --gen_setting: generate sample setting file\n" +
                        "" +
                        "" +
                        "");
                    return;
                }

            }

            if (File.Exists(Setting.GetSavePath()))
            {
                var file = Sorter.Run(Setting.Load());
                Console.WriteLine(file);
                Console.WriteLine($"sql file is generated.");
            }
            else
            {
                Console.WriteLine("setting file is not found.\n" +
                    "please run with --gen_setting option.");
                Console.ReadKey();
            }
        }
    }
}
