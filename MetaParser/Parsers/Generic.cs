using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaParser.Parsers
{
    public static class Generic
    {
        public static MetaData Parse(string filename, MetaType type)
        {
            if (!File.Exists(filename))
                throw new MetaParseException("File not found: {0}", filename);

            MetaData meta = new MetaData();
            meta.Type = type;
            meta.Path = filename;
            meta.Source = null;
            meta.Data = new Dictionary<MetaKey, object>();
            meta.Data[MetaKey.Date] = File.GetCreationTime(filename);
            meta.Data[MetaKey.Filename] = Path.GetFileName(filename);
            meta.Data[MetaKey.Tags] = new string[0];
            return meta;
        }
    }
}
