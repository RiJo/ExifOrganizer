//
// MetaParser.cs: Static class to parse meta data from different filetypes.
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

using ExifOrganizer.Common;
using ExifOrganizer.Meta.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExifOrganizer.Meta
{
    public struct MetaParserConfig
    {
        public bool Recursive;
        public IEnumerable<MetaType> FilterTypes;
        public IEnumerable<string> IgnorePaths;
    }

    public static class MetaParser
    {
        public static IEnumerable<MetaData> Parse(string path, MetaParserConfig config)
        {
            Task<IEnumerable<MetaData>> task = ParseAsync(path, config);
            task.ConfigureAwait(false); // Prevent deadlock of caller
            return task.Result;
        }

        public static async Task<IEnumerable<MetaData>> ParseAsync(string path, MetaParserConfig config)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (Directory.Exists(path))
                return await ParseDirectoryAsync(path, config);
            else if (File.Exists(path))
                return new MetaData[] { await ParseFileAsync(path, config) };
            else
                throw new FileNotFoundException($"Could not find file or directory: {path}");
        }

        public static MetaData ParseFile(string path, MetaParserConfig config)
        {
            Task<MetaData> task = ParseFileAsync(path, config);
            task.ConfigureAwait(false); // Prevent deadlock of caller
            return task.Result;
        }

        public static Task<MetaData> ParseFileAsync(string path, MetaParserConfig config)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path))
                throw new MetaParseException("File not found: {0}", path);

            string extension = Path.GetExtension(path).ToLower();
            switch (extension)
            {
                // Images (Exif)
                case ".jpg":
                case ".jpeg":
                case ".tif":
                case ".tiff":
                    if (config.FilterTypes != null && !config.FilterTypes.Contains(MetaType.Image))
                        return Task.FromResult<MetaData>(null);
                    return ExifParser.ParseAsync(path);

                // Images (PNG)
                case ".png":
                    if (config.FilterTypes != null && !config.FilterTypes.Contains(MetaType.Image))
                        return Task.FromResult<MetaData>(null);
                    return PNGParser.ParseAsync(path);

                // Images (generic)
                case ".gif":
                case ".bmp":
                    if (config.FilterTypes != null && !config.FilterTypes.Contains(MetaType.Image))
                        return Task.FromResult<MetaData>(null);
                    return GenericFileParser.ParseAsync(path, MetaType.Image);

                // Music (ID3)
                case ".mp3":
                    if (config.FilterTypes != null && !config.FilterTypes.Contains(MetaType.Music))
                        return Task.FromResult<MetaData>(null);
                    return ID3Parser.ParseAsync(path);

                // Music (MP4)
                case ".m4a":
                    if (config.FilterTypes != null && !config.FilterTypes.Contains(MetaType.Music))
                        return Task.FromResult<MetaData>(null);
                    return MP4Parser.ParseAsync(path);

                // Music (generic)
                case ".wav": // TODO: exif
                case ".flac":
                case ".aac":
                    if (config.FilterTypes != null && !config.FilterTypes.Contains(MetaType.Music))
                        return Task.FromResult<MetaData>(null);
                    return GenericFileParser.ParseAsync(path, MetaType.Music);

                // Movies (MP4)
                case ".mp4":
                case ".mpg4":
                case ".mov":
                case ".3gp":
                case ".3g2":
                    if (config.FilterTypes != null && !config.FilterTypes.Contains(MetaType.Video))
                        return Task.FromResult<MetaData>(null);
                    return MP4Parser.ParseAsync(path);

                // Movies (generic)
                case ".mpg":
                case ".mpeg":
                    if (config.FilterTypes != null && !config.FilterTypes.Contains(MetaType.Video))
                        return Task.FromResult<MetaData>(null);
                    return GenericFileParser.ParseAsync(path, MetaType.Video);

                default:
                    if (config.FilterTypes != null && !config.FilterTypes.Contains(MetaType.File))
                        return Task.FromResult<MetaData>(null);
                    return GenericFileParser.ParseAsync(path, MetaType.File);
            }
        }

        public static IEnumerable<MetaData> ParseDirectory(string path, MetaParserConfig config)
        {
            Task<IEnumerable<MetaData>> task = ParseDirectoryAsync(path, config);
            task.ConfigureAwait(false); // Prevent deadlock of caller
            return task.Result;
        }

        public static async Task<IEnumerable<MetaData>> ParseDirectoryAsync(string path, MetaParserConfig config)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (!Directory.Exists(path))
                throw new MetaParseException("Directory not found: {0}", path);

            List<MetaData> list = new List<MetaData>();
            if (config.IgnorePaths != null)
            {
                foreach (string ignorePath in config.IgnorePaths)
                {
                    if (ignorePath.DirectoryIsSubPath(path, true))
                        return list;
                }
            }

            LinkedList<Task<MetaData>> fileTasks = new LinkedList<Task<MetaData>>();
            fileTasks.AddLast(DirectoryParser.ParseAsync(path));
            foreach (string file in Directory.GetFiles(path))
            {
                fileTasks.AddLast(ParseFileAsync(file, config));
            }
            list.AddRange(await Task.WhenAll(fileTasks));

            if (config.Recursive)
            {
                LinkedList<Task<IEnumerable<MetaData>>> directoryTasks = new LinkedList<Task<IEnumerable<MetaData>>>();
                foreach (string directory in Directory.GetDirectories(path))
                    directoryTasks.AddLast(ParseDirectoryAsync(directory, config));
                foreach (IEnumerable<MetaData> nestedMetaData in await Task.WhenAll(directoryTasks))
                    list.AddRange(nestedMetaData);
            }

            return list.Where(x => x != null);
        }
    }
}