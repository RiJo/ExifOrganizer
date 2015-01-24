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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExifOrganizer.Meta.Parsers;
using System.Threading.Tasks;

namespace ExifOrganizer.Meta
{
    public static class MetaParser
    {
        public static IEnumerable<MetaData> Parse(string path, bool recursive, IEnumerable<string> ignorePaths = null)
        {
            Task<IEnumerable<MetaData>> task = ParseAsync(path, recursive, ignorePaths);
            task.ConfigureAwait(false); // Prevent deadlock of caller
            return task.Result;
        }

        public static async Task<IEnumerable<MetaData>> ParseAsync(string path, bool recursive, IEnumerable<string> ignorePaths = null)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (Directory.Exists(path))
                return await ParseDirectoryAsync(path, recursive, ignorePaths);
            else if (File.Exists(path))
                return new MetaData[] { await ParseFileAsync(path) };
            else
                throw new FileNotFoundException(String.Format("Could not find file or directory: {0}", path));
        }

        public static MetaData ParseFile(string path)
        {
            Task<MetaData> task = ParseFileAsync(path);
            task.ConfigureAwait(false); // Prevent deadlock of caller
            return task.Result;
        }

        public static async Task<MetaData> ParseFileAsync(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
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
                    return await ExifParser.ParseAsync(path);

                // Images (generic)
                case ".png":
                case ".gif":
                case ".bmp":
                    return await GenericFileParser.ParseAsync(path, MetaType.Image);

                // Music (generic)
                case ".mp3": // TODO: id3
                case ".wav": // TODO: exif
                case ".flac":
                case ".aac":
                    return await GenericFileParser.ParseAsync(path, MetaType.Music);

                // Movies (generic)
                case ".mpg":
                case ".mpeg":
                case ".mov":
                case ".mp4":
                    return await GenericFileParser.ParseAsync(path, MetaType.Video);

                default:
                    return await GenericFileParser.ParseAsync(path, MetaType.File);
            }
        }

        public static IEnumerable<MetaData> ParseDirectory(string path, bool recursive, IEnumerable<string> ignorePaths = null)
        {
            Task<IEnumerable<MetaData>> task = ParseDirectoryAsync(path, recursive, ignorePaths);
            task.ConfigureAwait(false); // Prevent deadlock of caller
            return task.Result;
        }

        public static async Task<IEnumerable<MetaData>> ParseDirectoryAsync(string path, bool recursive, IEnumerable<string> ignorePaths = null)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            if (!Directory.Exists(path))
                throw new MetaParseException("Directory not found: {0}", path);

            List<MetaData> list = new List<MetaData>();
            if (ignorePaths != null)
            {
                foreach (string ignorePath in ignorePaths)
                {
                    if (ignorePath.DirectoryIsSubPath(path, true))
                        return list;
                }
            }

            // TODO: run sub directories in paralell with files
            List<Task<MetaData>> fileTasks = new List<Task<MetaData>>();
            fileTasks.Add(DirectoryParser.ParseAsync(path));
            foreach (string file in Directory.GetFiles(path))
            {
                fileTasks.Add(ParseFileAsync(file));
            }
            list.AddRange(await Task.WhenAll(fileTasks));

            if (recursive)
            {
                List<Task<IEnumerable<MetaData>>> directoryTasks = new List<Task<IEnumerable<MetaData>>>();
                foreach (string directory in Directory.GetDirectories(path))
                    directoryTasks.Add(ParseDirectoryAsync(directory, recursive, ignorePaths));
                foreach (IEnumerable<MetaData> nestedMetaData in await Task.WhenAll(directoryTasks))
                    list.AddRange(nestedMetaData);
            }

            return list;
        }
    }
}
