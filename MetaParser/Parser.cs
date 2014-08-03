using System;
using System.Collections.Generic;
using System.IO;
using MetaParser.Parsers;

namespace MetaParser
{
    public static class Parser
    {
        public static IEnumerable<MetaData> Parse(string path, bool recursive = true, IEnumerable<string> additionalExtensions = null)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (Directory.Exists(path))
                return ParseDirectory(path, recursive);
            else
                return new MetaData[] { ParseFile(path) };
        }

        public static MetaData ParseFile(string path, IEnumerable<string> additionalExtensions = null)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (!File.Exists(path))
                throw new MetaParseException("File not found: {0}", path);

            string extension = Path.GetExtension(path);
            switch (extension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return Exif.Parse(path);

                default:
                    throw new NotSupportedException(String.Format("File extension not recognized: {0}", extension));
            }
        }

        public static IEnumerable<MetaData> ParseDirectory(string path, bool recursive = true, IEnumerable<string> additionalExtensions = null)
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
                    list.AddRange(ParseDirectory(directory));
            }

            return list;
        }
    }
}
