//
// DirectoryParser.cs: Directory meta data parser class.
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

using System.IO;
using System.Threading.Tasks;

namespace ExifOrganizer.Meta.Parsers
{
    internal class DirectoryParser : Parser
    {
        internal override MetaData Parse(string path)
        {
            Task<MetaData> task = ParseAsync(path);
            task.ConfigureAwait(false); // Prevent deadlock of caller
            return task.Result;
        }

        internal override Task<MetaData> ParseAsync(string path)
        {
            //return Task.Run(() => ParseThread(path));
            return Task.FromResult(ParseThread(path));
        }

        private MetaData ParseThread(string path)
        {
            if (!Directory.Exists(path))
                throw new MetaParseException("Directory not found: {0}", path);

            MetaData meta = new MetaData();
            meta.Type = MetaType.Directory;
            meta.Path = path;
            return meta;
        }
    }
}