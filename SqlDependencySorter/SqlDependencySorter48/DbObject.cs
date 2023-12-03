using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDependencySorter
{


    internal class DbObject
    {
 
        public string DeclaredFilePath { get; set; }

        public string Name { get; set; }

        public DbObjectTypes ObjectType { get; set; }

        public List<DbObject> DependOnObject { get;set; } = new List<DbObject>();

        


    }
}
