using ExifOrganizer.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExifOrganizer.Organizer
{
    public class CopyItem
    {
        public string sourcePath;
        public string destinationPath;
        public FileInfo sourceInfo;
        public Dictionary<MetaKey, object> meta;

        private string checksum;

        public CopyItem()
        {
        }

        public string GetChecksum()
        {
            if (!String.IsNullOrEmpty(checksum))
                return checksum;

            checksum = sourceInfo.GetMD5Sum();
            return checksum;
        }

        public override string ToString()
        {
            return String.Format("[{0}] ---> [{1}]", sourcePath, destinationPath);
        }
    }
}
