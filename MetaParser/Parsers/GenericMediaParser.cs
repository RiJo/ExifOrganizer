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
using System.Linq;

namespace ExifOrganizer.Meta.Parsers
{
    internal class GenericMediaParser : FileParser
    {
        internal override IEnumerable<string> GetSupportedFileExtensions()
        {
            return new string[] { ".gif", "wav", ".flac", ".aac", ".mpg", ".mpeg" };
        }

        internal override MetaType? GetMetaTypeByFileExtension(string extension)
        {
            switch (extension)
            {
                case ".gif":
                case ".bmp":
                    return MetaType.Image;
                case ".wav":
                case ".flac":
                case ".aac":
                    return MetaType.Audio;
                case ".mpg":
                case ".mpeg":
                    return MetaType.Video;
                default:
                    return base.GetMetaTypeByFileExtension(extension);
            }
        }

        protected override MetaData ParseFile(Stream stream, MetaData meta)
        {
            return meta;
        }
    }
}