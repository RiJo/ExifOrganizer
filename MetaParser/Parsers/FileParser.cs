using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExifOrganizer.Common;

namespace ExifOrganizer.Meta.Parsers
{
    internal abstract class FileParser : Parser
    {
        internal abstract IEnumerable<string> GetSupportedFileExtensions();
        internal virtual MetaType? GetMetaTypeByFileExtension(string extension) { return null; }

        internal override MetaData Parse(string filename)
        {
            Task<MetaData> task = ParseAsync(filename);
            task.ConfigureAwait(false); // Prevent deadlock of caller
            return task.Result;
        }

        internal override Task<MetaData> ParseAsync(string filename)
        {
            MetaData meta = GetBaseMetaFileData(filename);
            return Task.Run(() => { using (FileStream stream = File.OpenRead(filename)) return ParseFile(stream, meta); });
        }

        protected abstract MetaData ParseFile(Stream stream, MetaData meta);

        private MetaData GetBaseMetaFileData(string filename)
        {
            if (!File.Exists(filename))
                throw new MetaParseException("File not found: {0}", filename);

            string extension = Path.GetExtension(filename).ToLower();
            MetaType? type = GetMetaTypeByFileExtension(extension);
            if (!type.HasValue)
                throw new MetaParseException($"Target file extension not handled by parser: {extension}");

            MetaData meta = new MetaData();
            meta.Path = filename;
            meta.Type = type.Value;
            meta.Data = new Dictionary<MetaKey, object>();
            meta.Data[MetaKey.FileName] = Path.GetFileName(filename);
            meta.Data[MetaKey.OriginalName] = meta.Data[MetaKey.FileName];
            meta.Data[MetaKey.Size] = GetFileSize(filename);
            meta.Data[MetaKey.DateCreated] = File.GetCreationTime(filename);
            meta.Data[MetaKey.DateModified] = File.GetLastWriteTime(filename);
            meta.Data[MetaKey.Timestamp] = meta.Data[MetaKey.DateModified];
            meta.Data[MetaKey.Tags] = new string[0];

            return meta;
        }

        private static long GetFileSize(string filename)
        {
            return new FileInfo(filename).Length;
        }
    }
}
