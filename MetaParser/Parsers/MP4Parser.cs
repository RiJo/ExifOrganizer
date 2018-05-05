//
// MP4Parser.cs: MPEG-4 Part 14 (MP4) container meta parser class.
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
    internal class MP4Parser : FileParser
    {
        private static readonly Encoding iso_8859_1 = Encoding.GetEncoding("iso-8859-1");

        private enum MP4Tag
        {
            // ftyp
            FileFormat,

            // moov.udta.meta.ilst
            Album,
            Artist,
            Comment,
            Year,
            Title,
            Genre,
            TrackNumber,
            DiscNumber,
            Composer,
            Encoder,
            BPM,
            Copyright,
            Compilation,
            Grouping,
            Category,
            Keyword,
            Description
        }

        private enum MP4DataType : byte
        {
            UInt8_a = 0,
            Text = 1,
            JPEG = 13,
            PNG = 14,
            UInt8_b = 21,
        }

        internal override IEnumerable<string> GetSupportedFileExtensions()
        {
            return new string[] { ".mp4", ".m4a", ".mov", ".3gp", ".3g2" };
        }

        internal override bool ContainsMeta(Stream stream)
        {
            return true; // TODO: implement
        }

        protected override MetaData ParseFile(Stream stream, MetaData meta)
        {
            meta.Type = GetMetaType(meta.Path);

            Dictionary<MP4Tag, object> mp4 = ParseMP4(meta.Path);
            if (mp4.ContainsKey(MP4Tag.FileFormat))
                meta.Data[MetaKey.MetaType] = $"MPEG-4 Part 14 ({mp4[MP4Tag.FileFormat]})";
            if (mp4.ContainsKey(MP4Tag.Album))
                meta.Data[MetaKey.Album] = mp4[MP4Tag.Album];
            if (mp4.ContainsKey(MP4Tag.Artist))
                meta.Data[MetaKey.Artist] = mp4[MP4Tag.Artist];
            if (mp4.ContainsKey(MP4Tag.Comment))
                meta.Data[MetaKey.Comment] = mp4[MP4Tag.Comment];
            if (mp4.ContainsKey(MP4Tag.Year))
                meta.Data[MetaKey.Year] = mp4[MP4Tag.Year];
            if (mp4.ContainsKey(MP4Tag.Title))
                meta.Data[MetaKey.Title] = mp4[MP4Tag.Title];
            if (mp4.ContainsKey(MP4Tag.Genre))
                meta.Data[MetaKey.Genre] = mp4[MP4Tag.Genre];
            if (mp4.ContainsKey(MP4Tag.TrackNumber))
                meta.Data[MetaKey.Track] = mp4[MP4Tag.TrackNumber];

            return meta;
        }

        private static MetaType GetMetaType(string filename)
        {
            return Path.GetExtension(filename) == ".m4a" ? MetaType.Music : MetaType.Video; // TODO: properly define
        }

        private static Dictionary<MP4Tag, object> ParseMP4(string filename)
        {
            Dictionary<MP4Tag, object> tags = new Dictionary<MP4Tag, object>();

            using (FileStream stream = File.OpenRead(filename))
                tags = ParseBox(stream, stream.Length);

            return tags;
        }

        private static Dictionary<MP4Tag, object> ParseBox(Stream stream, long endPosition, string parent = "", int depth = 0)
        {
            Dictionary<MP4Tag, object> tags = new Dictionary<MP4Tag, object>();

            while (stream.Position < endPosition)
            {
                byte[] header = new byte[8];
                if (stream.Read(header, 0, header.Length) != header.Length)
                    throw new MetaParseException("Unable to read full MP4 box header");

                int size = BitConverter.ToInt32(header.Take(4).Reverse().ToArray(), 0);
                if (size < 8)
                    continue;
                size -= 8;
                string type = iso_8859_1.GetString(header, 4, 4);
                int padding = GetBoxPadding(type);
                if (padding > 0)
                {
                    size -= padding;
                    stream.Position += padding;
                }

                //Trace.WriteLine($"[MP4Parser] {new string('-', depth * 3)}( type: \"{type}\", depth: {depth},  size: {size}, padding: {padding} )");
                switch (type)
                {
                    case "ftyp":
                        tags[MP4Tag.FileFormat] = GetBoxAsString(stream, size);
                        break;

                    // Containers
                    case "moov":
                    //case "trak":
                    //case "mdia":
                    //case "minf":
                    //case "dinf":
                    //case "stbl":
                    case "udta":
                    case "meta":
                    case "ilst":
                        foreach (var kvp in ParseBox(stream, stream.Position + size, type, depth + 1))
                            tags[kvp.Key] = kvp.Value;
                        break;

                    default:
                        if (parent == "ilst")
                        {
                            foreach (var kvp in ParseIlstChild(stream, type))
                                tags[kvp.Key] = kvp.Value;
                            break;
                        }

                        Trace.WriteLine($"[MP4Parser] ignoring box type: \"{type}\"");
                        stream.Position += size;
                        break;
                }
            }

            return tags;
        }

        private static int GetBoxPadding(string type)
        {
            switch (type)
            {
                case "stsd": return 8;
                case "mp4a": return 28;
                case "drms": return 28;
                case "meta": return 4;
                default: return 0;
            }
        }

        private static Dictionary<MP4Tag, object> ParseIlstChild(Stream stream, string type)
        {
            Dictionary<MP4Tag, object> tags = new Dictionary<MP4Tag, object>();

            byte[] childHeader = new byte[8];
            if (stream.Read(childHeader, 0, childHeader.Length) != childHeader.Length)
                throw new MetaParseException($"Unable to read full MP4 \"ilst.{type}\" header");

            int childSize = BitConverter.ToInt32(childHeader.Take(4).Reverse().ToArray(), 0);
            if (childSize < 8)
                return tags;
            childSize -= 8;
            string childType = iso_8859_1.GetString(childHeader, 4, 4);
            if (childType != "data")
                throw new MetaParseException($"MP4 \"ilst.{type}\" must have a child of type \"data\", actual: \"{childType}\"");

            byte[] childFlags = new byte[4];
            if (stream.Read(childFlags, 0, childFlags.Length) != childFlags.Length)
                throw new MetaParseException($"Unable to read full MP4 \"ilst.{type}.{childType}\" flags");

            stream.Position += 4; // Ignore null bytes
            childSize -= 8;

            MP4DataType dataType = (MP4DataType)childFlags[3];

            switch (type)
            {
                case "©alb":
                    tags[MP4Tag.Album] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "©art":
                    tags[MP4Tag.Artist] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "©cmt":
                    tags[MP4Tag.Comment] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "©day":
                    tags[MP4Tag.Year] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "©nam":
                    tags[MP4Tag.Title] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "©gen":
                case "gnre":
                    tags[MP4Tag.Genre] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "trkn":
                    tags[MP4Tag.TrackNumber] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "disk":
                    tags[MP4Tag.DiscNumber] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "©wrt":
                    tags[MP4Tag.Composer] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "©too":
                    tags[MP4Tag.Encoder] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "tmpo":
                    tags[MP4Tag.BPM] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "cprt":
                    tags[MP4Tag.Copyright] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "cpil":
                    tags[MP4Tag.Composer] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "©grp":
                    tags[MP4Tag.Grouping] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "catg":
                    tags[MP4Tag.Category] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "keyw":
                    tags[MP4Tag.Keyword] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                case "desc":
                    tags[MP4Tag.Description] = GetBoxAsObject(stream, childSize, dataType);
                    break;

                default:
                    Trace.WriteLine($"[MP4Parser] ignoring ilst.{type}.{childType} data");
                    stream.Position += childSize;
                    break;
            }

            return tags;
        }

        private static string GetBoxAsString(Stream stream, int size)
        {
            return (string)GetBoxAsObject(stream, size, MP4DataType.Text);
        }

        private static object GetBoxAsObject(Stream stream, int size, MP4DataType type)
        {
            byte[] buffer = new byte[size];
            if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
                throw new MetaParseException("Unable to read full MP4 box content");

            switch (type)
            {
                case MP4DataType.UInt8_a:
                case MP4DataType.UInt8_b:
                    if (buffer.Length != 1)
                        throw new MetaParseException($"Box content type {type} require data to be a single byte. number of bytes: {buffer.Length}.");
                    return (int)buffer[0];

                case MP4DataType.Text:
                    if (buffer.Length == 0)
                        return String.Empty;
                    byte[] text = buffer.TakeWhile(c => c != '\0').ToArray();
                    return iso_8859_1.GetString(text);

                default:
                    throw new NotImplementedException($"Unsupported box content type: {type}");
            }
        }
    }
}