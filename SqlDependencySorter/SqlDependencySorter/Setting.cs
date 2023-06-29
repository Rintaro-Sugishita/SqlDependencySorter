using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SqlDependencySorter
{

    public class Setting
    {
        public List<string> Directories { get; set; } = new List<string>();

        public string Pattern { get; set; } = "*.sql";

        public bool RecursiveSearch { get; set; } = true;

        [JsonIgnore]
        public System.IO.SearchOption Option
        {
            get
            {
                if (RecursiveSearch)
                {
                    return SearchOption.AllDirectories;
                }
                else
                {
                    return SearchOption.TopDirectoryOnly;
                }
            }
        }


        public static Setting Load()
        {
            var jsonText = File.ReadAllText(GetSavePath());
            return System.Text.Json.JsonSerializer.Deserialize<Setting>(jsonText);


        }

        public void Save()
        {
            var jsonText = System.Text.Json.JsonSerializer.Serialize<Setting>(this);
            File.WriteAllText(GetSavePath(), jsonText);
        }

        public static string GetSavePath()
        {
            return System.IO.Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "setting.json");
        }
    }
}
