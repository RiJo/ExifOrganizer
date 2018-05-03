//
// PNGParser.cs: PNG (Portable Network Graphics) meta parser class.
//
// Copyright (C) 2018 Rikard Johansson
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExifOrganizer.Meta.Parsers
{
    internal class PNGParser : Parser
    {
        private static readonly Encoding iso_8859_1 = Encoding.GetEncoding("iso-8859-1");

        private enum PNGTag
        {
            Comment
        }

        public static MetaData Parse(string filename)
        {
            Task<MetaData> task = ParseAsync(filename);
            task.ConfigureAwait(false); // Prevent deadlock of caller
            return task.Result;
        }

        public static Task<MetaData> ParseAsync(string filename)
        {
            return Task.Run(() => ParseThread(filename));
        }

        private static MetaData ParseThread(string filename)
        {
            MetaData meta = GetBaseMetaFileData(filename, MetaType.Image);

            Dictionary<PNGTag, object> png = ParsePNG(filename);
            if (png.ContainsKey(PNGTag.Comment))
                meta.Data[MetaKey.Comment] = png[PNGTag.Comment];

            return meta;
        }

        private static Dictionary<PNGTag, object> ParsePNG(string filename)
        {
            Dictionary<PNGTag, object> tags = new Dictionary<PNGTag, object>();

            using (FileStream stream = File.OpenRead(filename))
            {
                if (!VerifyHeaderSignature(stream))
                    return tags;

                while (stream.Position < stream.Length)
                {
                    byte[] chunkHeader = new byte[8];
                    if (stream.Read(chunkHeader, 0, chunkHeader.Length) != chunkHeader.Length)
                        throw new MetaParseException("Unable to read full PNG chunk header");

                    int chunkLength = BitConverter.ToInt32(chunkHeader.Take(4).Reverse().ToArray(), 0);
                    string chunkType = Encoding.ASCII.GetString(chunkHeader, 4, 4);

                    KeyValuePair<string, string>? kvp = GetChunkData(stream, chunkLength, chunkType);
                    if (!kvp.HasValue)
                        continue;

                    switch (kvp.Value.Key)
                    {
                        case "Comment":
                            tags[PNGTag.Comment] = kvp.Value.Value;
                            break;

                        default:
                            Trace.WriteLine($"[PNGParser] ignoring meta data: \"{kvp.Value.Key}\" = \"{kvp.Value.Value}\"");
                            break;
                    }
                }
            }

            return tags;
        }

        private static bool VerifyHeaderSignature(Stream stream)
        {
            if (stream.Length < 8)
                return false;

            byte[] signature = new byte[8];
            if (stream.Read(signature, 0, signature.Length) != signature.Length)
                throw new MetaParseException("Unable to read full PNG header signature");

            if (signature[0] != 0x89)
                return false;
            if (signature[1] != 'P' || signature[2] != 'N' || signature[3] != 'G')
                return false;
            if (signature[4] != 0x0d || signature[5] != 0x0a || signature[6] != 0x1a || signature[5] != 0x0a)
                return false;
            return true;
        }

        private static KeyValuePair<string, string>? GetChunkData(Stream stream, int length, string type)
        {
            switch (type)
            {
                case "tEXt":
                case "iTXt":
                    byte[] chunkData = new byte[length];
                    if (stream.Read(chunkData, 0, chunkData.Length) != chunkData.Length)
                        throw new MetaParseException($"Unable to read full PNG chunk \"{type}\" data");

                    byte[] key = chunkData.TakeWhile(c => c != '\0').ToArray();
                    if (key.Length == length)
                        throw new MetaParseException($"Invalid PNG key-value data in chunk \"{type}\"");
                    byte[] value = chunkData.Skip(key.Length + 1).ToArray();

                    stream.Position += 4; // Skip CRC32

                    if (type == "iTXt")
                        return new KeyValuePair<string, string>(Encoding.ASCII.GetString(key), Encoding.UTF8.GetString(value));
                    else
                        return new KeyValuePair<string, string>(Encoding.ASCII.GetString(key), iso_8859_1.GetString(value));

                default:
                    Trace.WriteLine($"[PNGParser] ignoring chunk type: \"{type}\"");
                    stream.Position += length; // Skip data
                    stream.Position += 4; // Skip CRC32
                    return null;
            }
        }
    }
}