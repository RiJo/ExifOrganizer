//
// GenericFileParser.cs: Generic meta data parser class.
//
// Copyright (C) 2014 Rikard Johansson
//
// This program is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option) any
// later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// this program. If not, see http://www.gnu.org/licenses/.
//

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ExifOrganizer.Meta.Parsers
{
    internal class GenericFileParser : Parser
    {
        public static MetaData Parse(string filename, MetaType type)
        {
            Task<MetaData> task = ParseAsync(filename, type);
            task.ConfigureAwait(false); // Prevent deadlock of caller
            return task.Result;
        }

        public static Task<MetaData> ParseAsync(string filename, MetaType type)
        {
            return Task.Run(() => ParseThread(filename, type));
        }

        private static MetaData ParseThread(string filename, MetaType type)
        {
            if (!File.Exists(filename))
                throw new MetaParseException("File not found: {0}", filename);

            MetaData meta = new MetaData();
            meta.Type = type;
            meta.Path = filename;
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
    }
}