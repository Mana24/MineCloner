using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace MineCloner
{
    public class Config
    {
        [XmlAttribute]
        public int TableColumnCount;
        [XmlAttribute]
        public int TableRowCount;
        [XmlAttribute]
        public int MineCount;

        public Config(int tableColumnCount, int tableRowCount, int mineCount)
        {
            TableColumnCount = tableColumnCount;
            TableRowCount = tableRowCount;
            MineCount = mineCount;
        }

        public Config() { }
    }
}
