using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SqlDependencySorter
{
    public class Sorter
    {

        public static string Run(Setting s)
        {

            List<string> files = new List<string>();
            foreach (var directory in s.Directories)
            {
                files.AddRange(Directory.GetFiles(directory, s.Pattern, s.Option));
            }

            List<DbObject> list = new List<DbObject>();

            foreach (var file in files)
            {
                var lines = File.ReadAllLines(file, Encoding.UTF8);

                list.AddRange(ReadSqlFile(lines, file));
            }

            Console.WriteLine(list.Count);


            foreach (var file in files)
            {
                //EncodingAutoDetection
                 var text = File.ReadAllText(file, Encoding.UTF8).Trim();
                if (!text.EndsWith(";"))
                {
                    text += ";";
                }
                AnalyzeContainsObject(file, text, ref list);
            }


            list = list.OrderBy(x => x.ObjectType).ThenBy(x => x.DependOnObject.Count).ThenBy(x => x.Name).ToList();

            list = Sort(list);


            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                Console.WriteLine(list[i].DeclaredFilePath);
                sb.AppendLine(File.ReadAllText(list[i].DeclaredFilePath));
            }

            string path = System.IO.Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), $"combinedfile{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.sql");


            File.WriteAllText(path, sb.ToString());

            return path;
        }



        private static List<DbObject> ReadSqlFile(string[] lines, string filePath)
        {
            List<DbObject> list = new List<DbObject>();

            foreach (var line in lines)
            {
                var upperLine = line.ToUpper();

                if (upperLine.Contains("DROP ")) { continue; }

                if (upperLine.Contains("CREATE ") || upperLine.Contains("OR REPLACE "))
                {
                    DbObjectTypes t = DbObjectTypes.View;
                    if (upperLine.Contains(" VIEW "))
                    {
                        const string TYPE_TEXT = " VIEW ";
                        int startIdx = upperLine.IndexOf(TYPE_TEXT) + TYPE_TEXT.Length;

                        int endIdx = upperLine.IndexOf(" AS ", startIdx);
                        if (endIdx == -1) { endIdx = upperLine.Length; }
                        string viewName = line.Substring(startIdx, endIdx - startIdx).Trim();
                        list.Add(new DbObject { ObjectType = DbObjectTypes.View, Name = viewName, DeclaredFilePath = filePath });

                    }
                    else if (upperLine.Contains(" PROCEDURE "))
                    {
                        const string TYPE_TEXT = " PROCEDURE ";
                        int startIdx = upperLine.IndexOf(TYPE_TEXT) + TYPE_TEXT.Length;
                        int endIdx = upperLine.IndexOf("(", startIdx);

                        string funcName = line.Substring(startIdx, endIdx - startIdx).Trim();
                        list.Add(new DbObject { ObjectType = DbObjectTypes.Procedure, Name = funcName, DeclaredFilePath = filePath });


                    }
                    else if (upperLine.Contains(" FUNCTION "))
                    {
                        const string TYPE_TEXT = " FUNCTION ";
                        int startIdx = upperLine.IndexOf(TYPE_TEXT) + TYPE_TEXT.Length;
                        int endIdx = upperLine.IndexOf("(", startIdx);

                        string funcName = line.Substring(startIdx, endIdx - startIdx).Trim();
                        list.Add(new DbObject { ObjectType = DbObjectTypes.Procedure, Name = funcName, DeclaredFilePath = filePath });


                    }
                    else if (upperLine.Contains(" TRIGGER "))
                    {
                        const string TYPE_TEXT = " TRIGGER ";
                        int startIdx = upperLine.IndexOf(TYPE_TEXT) + TYPE_TEXT.Length;
                        int endIdx = upperLine.IndexOf(" ", startIdx);

                        string triggerName = line.Substring(startIdx, endIdx - startIdx).Trim();
                        list.Add(new DbObject { ObjectType = DbObjectTypes.Trigger, Name = triggerName, DeclaredFilePath = filePath });

                    }

                }

            }

            return list;
        }

        private static void AnalyzeContainsObject(string filePath, string text, ref List<DbObject> list)
        {
            var obj = list.First(x => x.DeclaredFilePath == filePath);

            foreach (var item in list)
            {
                if (text.ToLower().Contains(item.Name.ToLower()) && item.Name.ToLower() != obj.Name.ToLower())
                {
                    obj.DependOnObject.Add(item);
                }
            }
        }

        private static List<DbObject> Sort(List<DbObject> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var child = list[i].DependOnObject;
                if (child.Count > 0)
                {

                    int newIdx = i;
                    foreach (var item in child)
                    {
                        int idx = list.FindIndex(x => x.Name == item.Name);
                        if (idx > newIdx)
                        {
                            newIdx = idx + 1;
                        }
                    }
                    if (i != newIdx)
                    {
                        var moveItem = list[i];
                        list.Insert(newIdx, moveItem);
                        list.RemoveAt(i);
                        i--;
                    }
                }
            }

            return list;

        }
    }
}
