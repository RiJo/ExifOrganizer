//
// MediaOrganizer.cs: Static class to organize media files.
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

using ExifOrganizer.Meta;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExifOrganizer.Organizer
{
    public enum CopyMode
    {
        RequireEmpty, // Incompatbile with sourcePath == destinationPath
        Delta,
        ForceOverwrite,
        WipeBefore
    }

    public enum DuplicateMode
    {
        Unique,
        KeepAll
    }

    public class MediaOrganizer
    {
        public MediaOrganizer()
        {
        }

        public string sourcePath;
        public string destinationPath;
        public bool Recursive = true;
        public CultureInfo Localization = new CultureInfo("SV-se");
        public string DestinationPatternImage = @"%y/%m/%t/%n";
        public string DestinationPatternVideo = @"%y/%m/Video/%t/%n";
        public string DestinationPatternAudio = @"%y/%m/Audio/%t/%n";
        public CopyMode CopyMode = CopyMode.WipeBefore;
        public DuplicateMode DuplicateMode = DuplicateMode.Unique;
        public string[] IgnorePaths = null;

        public CopyItems items;

        public void Parse()
        {
            if (sourcePath.DirectoryAreSame(destinationPath))
            {
                throw new NotSupportedException("TODO");
                // TODO: how to properly handle
                //if (CopyMode == CopyMode.WipeBefore || CopyMode == CopyMode.RequireEmpty)
                //    throw new MediaOrganizerException("Copy mode {0} does not support same source and destination paths", CopyMode);
            }

            CopyItems reference = new CopyItems();
            reference.sourcePath = sourcePath;
            reference.destinationPath = destinationPath;
            reference.items = ParseItems(sourcePath, destinationPath);
            items = reference;
        }

        public void Organize()
        {
            if (items == null)
                throw new InvalidOperationException("Nothing has been parsed yet");

            FilterDuplicateItems(items);
            PrepareDestinationPath(items);

            // Copy items to destination path
            foreach (CopyItem item in items.items)
            {
                if (!Directory.Exists(Path.GetDirectoryName(item.destinationPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(item.destinationPath));

                bool overwrite = false;
                if (File.Exists(item.destinationPath))
                {
                    switch (CopyMode)
                    {
                        case CopyMode.RequireEmpty:
                        case CopyMode.WipeBefore:
                            throw new MediaOrganizerException("Destination file already exists: {0}", item.destinationPath);
                        case CopyMode.Delta:
                            // TODO: check if files are identical?
                            overwrite = false;
                            continue; // Skip existing files
                        case CopyMode.ForceOverwrite:
                            // TODO: check if files are identical?
                            overwrite = true;
                            break; // 
                    }
                }

                File.Copy(item.sourcePath, item.destinationPath, overwrite);
            }
        }

        private void FilterDuplicateItems(CopyItems reference)
        {
            HashSet<string> handledItems = new HashSet<string>();

            foreach (CopyItem item in reference.items.ToArray())
            {
                // TODO: better comparison: md5 etc
                bool destinationConflict = !handledItems.Add(item.destinationPath);

                if (DuplicateMode == DuplicateMode.Unique)
                {
                    // TODO: implement selection logic (which one to keep)
                    if (destinationConflict)
                    {
                        Console.WriteLine("Ignoring file because destination path is not unique: {0}", item);
                        reference.items.Remove(item);
                        continue;
                    }
                }
                else if (DuplicateMode == DuplicateMode.KeepAll)
                {
                    int i = 2;
                    while (destinationConflict)
                    {
                        string newDestinationPath = String.Format("{0}\\{1}({2}){3}", Path.GetDirectoryName(item.destinationPath), Path.GetFileNameWithoutExtension(item.destinationPath), i++, Path.GetExtension(item.destinationPath));
                        destinationConflict = !handledItems.Add(newDestinationPath);
                        if (!destinationConflict)
                            item.destinationPath = newDestinationPath;
                    }
                }
            }
        }

        private void PrepareDestinationPath(CopyItems reference)
        {
            // Setup destination path
            if (!Directory.Exists(reference.destinationPath))
            {
                Directory.CreateDirectory(reference.destinationPath);
                return;
            }

            switch (CopyMode)
            {
                case CopyMode.RequireEmpty:
                    if (Directory.Exists(reference.destinationPath))
                    {
                        if (Directory.GetFiles(reference.destinationPath).Length > 0)
                            throw new MediaOrganizerException("Path contains files but is required to be empty: {0}", reference.destinationPath);
                        if (Directory.GetDirectories(reference.destinationPath).Length > 0)
                            throw new MediaOrganizerException("Path contains directories but is required to be empty: {0}", reference.destinationPath);
                    }
                    break;

                case CopyMode.WipeBefore:
                    if (reference.sourcePath.DirectoryAreSame(reference.destinationPath))
                    {
                        // Move source/destination path to temporary place before copying
                        string tempSourcePath = Path.GetTempFileName();
                        File.Delete(tempSourcePath);
                        Directory.Move(reference.sourcePath, tempSourcePath);
                        reference.sourcePath = tempSourcePath; // TODO: delete this path after organization
                    }
                    else
                    {
                        Directory.Delete(reference.destinationPath, true);
                    }
                    Directory.CreateDirectory(reference.destinationPath);
                    break;
            }
        }

        // TODO: use hashset to remove duplicates? Or better: define behaviour of which to insert into list
        private List<CopyItem> ParseItems(string sourcePath, string destinationPath)
        {
            List<string> ignore = new List<string>();
            if (IgnorePaths != null)
                ignore.AddRange(IgnorePaths);
            if (!sourcePath.DirectoryAreSame(destinationPath))
                ignore.Add(destinationPath);

            IEnumerable<MetaData> data;
            try
            {
                data = MetaParser.Parse(sourcePath, Recursive, ignore);
            }
            catch (MetaParseException ex)
            {
                throw new MediaOrganizerException("Failed to parse meta data", ex);
            }

            List<CopyItem> items = new List<CopyItem>();
            foreach (MetaData meta in data)
            {
                CopyItem item = ParseItem(destinationPath, meta);
                if (item == null)
                    continue;
                items.Add(item);

            }
            return items;
        }

        private CopyItem ParseItem(string destinationPath, MetaData meta)
        {
            CopyItem item = new CopyItem();
            item.sourcePath = meta.Path;
            item.destinationPath = CalculateDestinationPath(destinationPath, meta);
            return item;
        }

        private string CalculateDestinationPath(string destinationPath, MetaData meta)
        {
            string destinationPattern = null;
            switch (meta.Type)
            {
                case MetaType.Image:
                    destinationPattern = DestinationPatternImage;
                    break;
                case MetaType.Video:
                    destinationPattern = DestinationPatternVideo;
                    break;
                case MetaType.Music:
                    destinationPattern = DestinationPatternAudio;
                    break;
                default:
                    throw new NotSupportedException(String.Format("Meta media type not supported: {0}", meta.Type));
            }
            string[] pattern = destinationPattern.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            string currentPath = destinationPath;
            foreach (string subpattern in pattern)
            {
                switch (subpattern)
                {
                    case "%y": // Year
                        {
                            object temp;
                            if (!meta.Data.TryGetValue(MetaKey.Date, out temp))
                                throw new MediaOrganizerException("Failed to retrieve key {0} from meta data to parse %y", MetaKey.Date);

                            DateTime datetime = (DateTime)temp;
                            currentPath = Path.Combine(currentPath, datetime.Year.ToString());
                        }
                        break;
                    case "%m": // Month
                        {
                            object temp;
                            if (!meta.Data.TryGetValue(MetaKey.Date, out temp))
                                throw new MediaOrganizerException("Failed to retrieve key {0} from meta data to parse %m", MetaKey.Date);

                            DateTime datetime = (DateTime)temp;

                            DateTimeFormatInfo dateinfo = Localization.DateTimeFormat;
                            string monthName = dateinfo.MonthNames[datetime.Month - 1].UppercaseFirst();
                            currentPath = Path.Combine(currentPath, monthName);
                        }
                        break;
                    case "%t":
                        {
                            object temp;
                            if (!meta.Data.TryGetValue(MetaKey.Tags, out temp))
                                throw new MediaOrganizerException("Failed to retrieve key {0} from meta data to parse %t", MetaKey.Tags);

                            string[] tags = temp as string[];
                            if (tags == null || tags.Length == 0)
                                continue;

                            string tag = tags[0]; // TODO: how to solve multiple tags?
                            currentPath = Path.Combine(currentPath, tag);
                        }
                        break;
                    case "%n": // Original name
                        {
                            object temp;
                            if (!meta.Data.TryGetValue(MetaKey.Filename, out temp))
                                throw new MediaOrganizerException("Failed to retrieve key {0} from meta data to parse %o", MetaKey.Filename);

                            string filename = (string)temp;
                            currentPath = Path.Combine(currentPath, filename);
                        }
                        break;

                    default:
                        if (subpattern.StartsWith("%"))
                            throw new MediaOrganizerException("Invalid pattern item: {0}", subpattern);

                        currentPath = Path.Combine(currentPath, subpattern);
                        break;
                }
            }

            return currentPath;
        }
    }
}
