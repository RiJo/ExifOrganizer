using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetaParser.Parsers;

namespace MetaParser
{
    public static class Parser
    {
        public static IEnumerable<MetaData> Parse(string path, bool recursive = true)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (Directory.Exists(path))
                return ParseDirectory(path, recursive);
            else
                return new MetaData[] { ParseFile(path) };
        }

        public static MetaData ParseFile(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (!File.Exists(path))
                throw new MetaParseException("File not found: {0}", path);

            string extension = Path.GetExtension(path).ToLower();
            switch (extension)
            {
                // Images (EXIF)
                case ".jpg":
                case ".jpeg":
                case ".tif":
                case ".tiff":
                    return Exif.Parse(path);

                // Music (generic)
                case ".mp3": // TODO: id3
                case ".wav":
                    return Generic.Parse(path, MetaType.Music);

                // Movies (generic)
                case ".mpg":
                case ".mpeg":
                case ".mov":
                case ".mp4":
                    return Generic.Parse(path, MetaType.Video);

                default:
                    throw new NotSupportedException(String.Format("File extension not recognized: {0}", extension));
            }
        }

        public static IEnumerable<MetaData> ParseDirectory(string path, bool recursive = true)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (!Directory.Exists(path))
                throw new MetaParseException("Directory not found: {0}", path);

            List<MetaData> list = new List<MetaData>();
            foreach (string file in Directory.GetFiles(path))
            {
                MetaData meta;
                try
                {
                    meta = ParseFile(file);
                }
                catch (Exception)
                {
                    continue;
                }

                list.Add(meta);
            }

            if (recursive)
            {
                foreach (string directory in Directory.GetDirectories(path))
                    list.AddRange(ParseDirectory(directory, recursive));
            }

            return list;
        }
    }
}
