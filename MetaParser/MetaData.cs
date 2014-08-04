using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaParser
{
    public enum MetaType
    {
        Image,
        Video,
        Music
    }

    public enum MetaKey
    {
        Filename,
        Date,
        //Size,
        Tags
    }

    public class MetaData
    {
        public MetaType Type;
        public string Path;
        public Dictionary<MetaKey, object> Data;
        public object Source;
    }
}
