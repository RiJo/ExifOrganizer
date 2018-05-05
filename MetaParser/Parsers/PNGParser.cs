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
    internal class PNGParser : FileParser
    {
        private static readonly Encoding iso_8859_1 = Encoding.GetEncoding("iso-8859-1");

        private enum PNGTag
        {
            // IHDR
            ImageWidth,
            ImageHeight,
            BitDepth,
            ColorType,
            CompressionMethod,
            FilterMethod,
            InterlaceMethod,
            // tIME
            LastModified,
            // tEXt/iTXt
            Title,
            Author,
            Description,
            Copyright,
            CreationTime,
            Software,
            Disclaimer,
            Warning,
            Source,
            Comment
        }

        private enum PNGBitDepth : byte
        {
            Greyscale = 0,
            Truecolor = 2,
            IndexedColor = 3,
            GreyscaleWithAlpha = 4,
            TruecolorWithAlpha = 6
        }

        internal override IEnumerable<string> GetSupportedFileExtensions()
        {
            return new string[] { ".png" };
        }

        internal override bool ContainsMeta(Stream stream)
        {
            return true; // TODO: implement
        }

        protected override MetaData ParseFile(Stream stream, MetaData meta)
        {
            meta.Type = MetaType.Image;

            Dictionary<PNGTag, object> png = ParsePNG(meta.Path);
            if (png.ContainsKey(PNGTag.ImageWidth))
                meta.Data[MetaKey.Width] = png[PNGTag.ImageWidth];
            if (png.ContainsKey(PNGTag.ImageHeight))
                meta.Data[MetaKey.Height] = png[PNGTag.ImageHeight];
            if (meta.Data.ContainsKey(MetaKey.Width) && meta.Data.ContainsKey(MetaKey.Height))
                meta.Data[MetaKey.Resolution] = $"{meta.Data[MetaKey.Width]}x{meta.Data[MetaKey.Height]}";
            if (png.ContainsKey(PNGTag.LastModified))
                meta.Data[MetaKey.DateModified] = png[PNGTag.LastModified];
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

                    foreach (var kvp in GetChunkData(stream, chunkLength, chunkType))
                    {
                        if (tags.ContainsKey(kvp.Key))
                        {
                            Trace.WriteLine($"[PNGParser] duplicate meta key ignored: \"{kvp.Key}\"");
                            continue;
                        }
                        tags[kvp.Key] = kvp.Value;
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

        private static Dictionary<PNGTag, object> GetChunkData(Stream stream, int length, string type)
        {
            Dictionary<PNGTag, object> tags = new Dictionary<PNGTag, object>();

            // TODO: parse "eXIf" - Exif meta data
            // TODO: parse "pHYs" - indended pixel size, ratio, etc.
            // TODO: parse "zTXt" - compressed text
            switch (type)
            {
                case "IHDR":
                    {
                        byte[] chunkData = ReadChunkData(stream, length, type);
                        if (chunkData.Length != 13)
                            throw new MetaParseException($"Invalid PNG \"{type}\" chunk length: {length}");

                        tags[PNGTag.ImageWidth] = BitConverter.ToInt32(chunkData.Take(4).Reverse().ToArray(), 0);
                        tags[PNGTag.ImageHeight] = BitConverter.ToInt32(chunkData.Skip(4).Take(4).Reverse().ToArray(), 0);
                        tags[PNGTag.BitDepth] = (int)chunkData[8];
                        tags[PNGTag.ColorType] = (PNGBitDepth)chunkData[9];
                        tags[PNGTag.CompressionMethod] = (int)chunkData[10];
                        tags[PNGTag.FilterMethod] = (int)chunkData[11];
                        tags[PNGTag.InterlaceMethod] = (int)chunkData[12];
                    }
                    break;

                case "tIME":
                    {
                        byte[] chunkData = ReadChunkData(stream, length, type);
                        if (chunkData.Length != 7)
                            throw new MetaParseException($"Invalid PNG \"{type}\" chunk length: {length}");

                        int year = BitConverter.ToInt16(chunkData.Take(2).Reverse().ToArray(), 0);
                        int month = chunkData[2];
                        int day = chunkData[3];
                        int hour = chunkData[4];
                        int minute = chunkData[5];
                        int second = chunkData[6];
                        DateTime lastModification = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
                        tags[PNGTag.LastModified] = lastModification;
                    }
                    break;

                case "tEXt":
                case "iTXt":
                    {
                        byte[] chunkData = ReadChunkData(stream, length, type);

                        byte[] keyData = chunkData.TakeWhile(c => c != '\0').ToArray();
                        if (keyData.Length == length)
                            throw new MetaParseException($"Invalid PNG key-value data in chunk \"{type}\"");
                        byte[] valueData = chunkData.Skip(keyData.Length + 1).ToArray();

                        string key = Encoding.ASCII.GetString(keyData);
                        string value = (type == "iTXt") ? Encoding.UTF8.GetString(valueData) : iso_8859_1.GetString(valueData);
                        switch (key)
                        {
                            case "Title":
                                tags[PNGTag.Title] = value;
                                break;

                            case "Author":
                                tags[PNGTag.Author] = value;
                                break;

                            case "Description":
                                tags[PNGTag.Description] = value;
                                break;

                            case "Copyright":
                                tags[PNGTag.Copyright] = value;
                                break;

                            case "Creation Time":
                                tags[PNGTag.CreationTime] = value;
                                break;

                            case "Software":
                                tags[PNGTag.Software] = value;
                                break;

                            case "Disclaimer":
                                tags[PNGTag.Disclaimer] = value;
                                break;

                            case "Warning":
                                tags[PNGTag.Warning] = value;
                                break;

                            case "Source":
                                tags[PNGTag.Source] = value;
                                break;

                            case "Comment":
                                tags[PNGTag.Comment] = value;
                                break;

                            default:
                                Trace.WriteLine($"[PNGParser] ignoring \"{type}\" chunk key: \"{key}\"");
                                break;
                        }
                    }
                    break;

                default:
                    Trace.WriteLine($"[PNGParser] ignoring chunk type: \"{type}\"");
                    stream.Position += length + 4; // Skip data + CRC32
                    break;
            }

            return tags;
        }

        private static byte[] ReadChunkData(Stream stream, int length, string type)
        {
            byte[] data = new byte[length];
            if (stream.Read(data, 0, data.Length) != data.Length)
                throw new MetaParseException($"Unable to read full PNG chunk \"{type}\" data");

            byte[] crc32 = new byte[4];
            if (stream.Read(crc32, 0, crc32.Length) != crc32.Length)
                throw new MetaParseException($"Unable to read full PNG chunk \"{type}\" CRC32");

            byte[] crcData = type.Select(c => (byte)c).Concat(data).ToArray();
            uint calculated = CRC32.Calculate(crcData, 0, crcData.Length, 0);
            uint given = BitConverter.ToUInt32(crc32.Reverse().ToArray(), 0);
            if (calculated != given)
                throw new MetaParseException($"Checksum verification failure of PNG chunk \"{type}\"");
            return data;
        }

        // Reference: https://stackoverflow.com/questions/24082305/how-is-png-crc-calculated-exactly
        private static class CRC32
        {
            static uint[] crcTable;

            // Stores a running CRC (initialized with the CRC of "IDAT" string). When
            // you write this to the PNG, write as a big-endian value
            static uint idatCrc = Calculate(new byte[] { (byte)'I', (byte)'D', (byte)'A', (byte)'T' }, 0, 4, 0);

            // Call this function with the compressed image bytes, 
            // passing in idatCrc as the last parameter
            public static uint Calculate(byte[] stream, int offset, int length, uint crc)
            {
                uint c;
                if (crcTable == null)
                {
                    crcTable = new uint[256];
                    for (uint n = 0; n <= 255; n++)
                    {
                        c = n;
                        for (var k = 0; k <= 7; k++)
                        {
                            if ((c & 1) == 1)
                                c = 0xEDB88320 ^ ((c >> 1) & 0x7FFFFFFF);
                            else
                                c = ((c >> 1) & 0x7FFFFFFF);
                        }
                        crcTable[n] = c;
                    }
                }
                c = crc ^ 0xffffffff;
                var endOffset = offset + length;
                for (var i = offset; i < endOffset; i++)
                {
                    c = crcTable[(c ^ stream[i]) & 255] ^ ((c >> 8) & 0xFFFFFF);
                }
                return c ^ 0xffffffff;
            }
        }
    }
}