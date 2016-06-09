using Xunit;
using Xunit.Abstractions;
using System.Xml;
using System.Xml.Schema;

namespace System.Xml.Tests
{
    //[TestCase(Name = "TC_SchemaSet_XmlNameTable", Desc = "")]
    public class TC_SchemaSet_XmlNameTable
    {
        private ITestOutputHelper _output;

        public TC_SchemaSet_XmlNameTable(ITestOutputHelper output)
        {
            _output = output;
        }


        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v1 - Get nametable", Priority = 0)]
        [InlineData()]
        [Theory]
        public void v1()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            XmlNameTable NT = sc.NameTable;
            CError.Compare(NT != null, true, "NameTable");
            return;
        }
    }
}