using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SqlDependencySorter
{
    public class Sorter
    {

        public static string Run(Setting s)
        {

            var files = GetFiles(s);

            var list = SortSql(files);

            return WriteFile(files, list);

        }

        public static List<(string fileName, Encoding? encoding)> GetFiles(Setting s)
        {
            EncodingAutoDetection.EncodingDetector encodingDetector = new EncodingAutoDetection.EncodingDetector();

            List<(string fileName, Encoding? encoding)> files = new List<(string, Encoding?)>();
            foreach (var directory in s.Directories)
            {
                files.AddRange(Directory.GetFiles(directory, s.Pattern, s.Option).Select(x => (x, encodingDetector.GetFileEncoding(x))));
            }

            return files;
        }

        public static List<DbObject> SortSql(List<(string fileName, Encoding? encoding)> files)
        {

            List<DbObject> list = new List<DbObject>();



            foreach (var file in files)
            {
                if (file.encoding != null)
                {
                    var lines = File.ReadAllLines(file.fileName, file.encoding);
                    list.AddRange(ReadSqlFile(lines, file.fileName));
                }
            }

            Console.WriteLine(list.Count);


            foreach (var file in files)
            {
                if (file.encoding != null)
                {
                    var text = File.ReadAllText(file.fileName, file.encoding).Trim();

                    AnalyzeContainsObject(file.fileName, text, ref list);
                }
            }


            list = list.OrderBy(x => x.ObjectType).ThenBy(x => x.DependOnObject.Count).ThenBy(x => x.Name).ToList();

            list = Sort(list);
            return list;
        }

        public static string WriteFile(List<(string fileName, Encoding? encoding)> files, List<DbObject> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SET CLIENT_ENCODING TO \"UTF-8\";");
            for (int i = 0; i < list.Count; i++)
            {
                var fileObj = files.First(x => x.fileName == list[i].DeclaredFilePath);

                Console.WriteLine(fileObj.fileName);

                var fileText = File.ReadAllText(fileObj.fileName, fileObj.encoding ?? Encoding.UTF8);
                if (!fileText.EndsWith(";"))
                {
                    //ファイル末尾にセミコロンがない場合つける
                    fileText += ";";
                }
                if (fileText.StartsWith("SET CLIENT_ENCODING TO ") || fileText.StartsWith("set client_encoding to "))
                {
                    //Encoding設定はコメント化する
                    fileText = "--" + fileText;
                }

                sb.AppendLine(fileText);

            }

            string path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty, $"combinedfile{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.sql");


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
