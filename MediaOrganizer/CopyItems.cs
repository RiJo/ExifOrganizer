using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaParser;

namespace MediaOrganizer
{
    public class CopyItem
    {
        public string sourcePath;
        public string destinationPath;

        public override string ToString()
        {
            return String.Format("[{0}] ---> [{1}]", sourcePath, destinationPath);
        }
    }

    public class CopyItems
    {
        public string sourcePath;
        public string destinationPath;
        public List<CopyItem> items;

        public override string ToString()
        {
            List<string> itemStrings = new List<string>();
            if (items != null)
            {
                foreach (CopyItem item in items)
itemStrings.Add(item.ToString());
            }
            return String.Format("Copy: [{0}] ---> [{1}]{2}Items:{2}{3}", sourcePath, destinationPath, Environment.NewLine, String.Join(Environment.NewLine, itemStrings.ToArray()));
        }
    }
}
