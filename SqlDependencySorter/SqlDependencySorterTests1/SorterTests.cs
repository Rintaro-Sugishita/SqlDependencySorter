using NUnit.Framework;
using SqlDependencySorter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDependencySorter.Tests
{
    [TestFixture()]
    public class SorterTests
    {
        [Test()]
        public void RunTest()
        {
            Setting s = new Setting();
            s.Pattern = "*.sql";
            s.RecursiveSearch = true;

            s.Directories = new List<string>()
            {
                ""
            };





            //Assert.Fail();
        }
    }
}