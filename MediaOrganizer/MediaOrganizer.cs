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
using System.Threading;
using System.Threading.Tasks;

namespace ExifOrganizer.Organizer
{
    public enum CopyPrecondition
    {
        None,
        RequireEmpty,
        WipeBefore
    }

    public enum CopyMode
    {
        KeepExisting,
        Delta,
        ForceOverwrite,
    }

    public enum DuplicateMode
    {
        Ignore,
        Unique,
        KeepAll
    }

    // TODO: implement
    public enum ExceptionHandling
    {
        Revert,
        Abort,
        Ignore
    }

    public class OrganizeSummary
    {
        public string[] parsed;
        public string[] valid;
        public string[] duplicates;

        public override string ToString()
        {
            return String.Format("Summary {{ Parsed: {0}, Valid: {1}, Duplicates: {2} }}", parsed != null ? parsed.Length : 0, valid != null ? valid.Length : 0, duplicates != null ? duplicates.Length : 0);
        }
    }

    public class MediaOrganizer
    {
        private enum GroupType
        {
            Year,
            Month,
            Day,
            Name,
            Tags
        }

        public MediaOrganizer()
        {
        }

        public event Action<MediaOrganizer, double> OnProgress = delegate { };

        public string sourcePath;
        public string destinationPath;
        public bool Recursive = true;
        public CultureInfo Localization = Thread.CurrentThread.CurrentCulture;
        public string DestinationPatternImage = @"%y/%m/%t/%n";
        public string DestinationPatternVideo = @"%y/%m/Video/%t/%n";
        public string DestinationPatternAudio = @"%y/%m/Audio/%t/%n";
        public CopyPrecondition CopyPrecondition = CopyPrecondition.None;
        public CopyMode CopyMode = CopyMode.Delta;
        public DuplicateMode DuplicateMode = DuplicateMode.Unique;
        public string[] IgnorePaths = null;

        private readonly Dictionary<string, GroupType> organizeGroups = new Dictionary<string, GroupType>()
        {
            { "%y", GroupType.Year },
            { "%m", GroupType.Month },
            { "%d", GroupType.Day },
            { "%n", GroupType.Name },
            { "%t", GroupType.Tags }
        };

        public CopyItems copyItems;

        public OrganizeSummary Parse()
        {
            if (sourcePath.DirectoryAreSame(destinationPath))
            {
                throw new NotSupportedException("TODO");
                // TODO: how to properly handle
                //if (CopyMode == CopyMode.WipeBefore || CopyMode == CopyMode.RequireEmpty)
                //    throw new MediaOrganizerException("Copy mode {0} does not support same source and destination paths", CopyMode);
            }

            OrganizeSummary summary = new OrganizeSummary();

            copyItems = new CopyItems();
            copyItems.sourcePath = sourcePath;
            copyItems.destinationPath = destinationPath;
            copyItems.items = ParseItems(sourcePath, destinationPath, ref summary);

            OnProgress(this, 0.1);

            FilterDuplicateItems(ref summary);
            summary.valid = Array.ConvertAll<CopyItem, string>(copyItems.items.ToArray(), x => x.sourcePath);

            OnProgress(this, 0.2);

            return summary;
        }

        public void Organize()
        {
            if (copyItems == null)
                throw new InvalidOperationException("Parse() must be executed prior to Organize()");

            PrepareDestinationPath();

            // Copy items to destination path
            int itemCount = copyItems.items.Count;
            for (int i = 0; i < itemCount; i++)
            {
                CopyItem item = copyItems.items[i];

                if (!Directory.Exists(Path.GetDirectoryName(item.destinationPath)))
                {
                    string destinationDirectory = Path.GetDirectoryName(item.destinationPath);
                    try
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }
                    catch (Exception ex)
                    {
                        throw new MediaOrganizerException(String.Format("Failed to create directory: {0}", destinationDirectory), ex.Message);
                    }
                }

                bool overwrite;
                bool skipIdentical;
                switch (CopyMode)
                {
                    case CopyMode.KeepExisting:
                        overwrite = false;
                        skipIdentical = true;
                        break;

                    case CopyMode.Delta:
                        overwrite = true;
                        skipIdentical = true;
                        break;

                    case CopyMode.ForceOverwrite:
                        overwrite = true;
                        skipIdentical = false;
                        break;

                    default:
                        throw new NotImplementedException(String.Format("CopyMode: {0}", CopyMode));
                }

                if (skipIdentical && File.Exists(item.destinationPath))
                {
                    bool filesIdentical = item.sourceInfo.AreFilesIdentical(new FileInfo(item.destinationPath));
                    if (filesIdentical)
                        continue;
                }

                try
                {
                    File.Copy(item.sourcePath, item.destinationPath, overwrite);
                }
                catch (Exception ex)
                {
                    throw new MediaOrganizerException(String.Format("Failed to copy file. Overwrite: {0}. Source: {1}. Destination: {2}", overwrite, item.sourcePath, item.destinationPath), ex.Message);
                }

                if (i % 10 == 0)
                    OnProgress(this, 0.2 + ((double)i / (double)itemCount));
            }

            OnProgress(this, 1.0);
        }

        private void FilterDuplicateItems(ref OrganizeSummary summary)
        {
            HashSet<string> handledFilenames = new HashSet<string>();
            Dictionary<string, HashSet<string>> handledChecksums = new Dictionary<string, HashSet<string>>();
            Dictionary<DateTime, HashSet<string>> handledTimestamps = new Dictionary<DateTime, HashSet<string>>();
            Dictionary<long, HashSet<string>> handledSizes = new Dictionary<long, HashSet<string>>();

            HashSet<string> duplicates = new HashSet<string>();
            foreach (CopyItem item in copyItems.items.ToArray())
            {
                // TODO: better comparison: md5 etc
                bool conflictingDestination = !handledFilenames.Add(item.destinationPath);
                
                object temp;

                bool conflictingTimestamp = false;
                if (item.meta.TryGetValue(MetaKey.Date, out temp))
                {
                    DateTime timestamp = (DateTime)temp;
                    if (!handledTimestamps.ContainsKey(timestamp))
                        handledTimestamps[timestamp] = new HashSet<string>();

                    handledTimestamps[timestamp].Add(item.sourcePath);
                    conflictingTimestamp = handledTimestamps[timestamp].Count > 1;
                }

                bool conflictingSize = false;
                if (item.meta.TryGetValue(MetaKey.Size, out temp))
                {
                    long size= (long)temp;
                    if (!handledSizes.ContainsKey(size))
                        handledSizes[size] = new HashSet<string>();

                    handledSizes[size].Add(item.sourcePath);
                    conflictingSize = handledSizes[size].Count > 1;
                }

                bool conflictingChecksum = false;
                if (conflictingTimestamp || conflictingSize)
                {
                    string checksum = item.GetChecksum();
                    if (!handledChecksums.ContainsKey(checksum))
                        handledChecksums[checksum] = new HashSet<string>();

                    handledChecksums[checksum].Add(item.sourcePath);
                    conflictingChecksum = handledChecksums[checksum].Count > 1;
                }
                
                bool anyConflict = (conflictingDestination || conflictingChecksum || conflictingTimestamp || conflictingSize);

                switch (DuplicateMode)
                {
                    case DuplicateMode.Ignore:
                        // Noop
                        if (anyConflict)
                            duplicates.Add(item.sourcePath);
                        break;
                    case DuplicateMode.Unique:
                        // TODO: implement selection logic (which one to keep)
                        if (anyConflict)
                        {
                            Console.WriteLine("Ignoring not unique (destination: {0}, checksum: {1}, timestamp: {2}) file: {3}", conflictingDestination, conflictingChecksum, conflictingTimestamp, item);
                            copyItems.items.Remove(item);
                            duplicates.Add(item.sourcePath);
                            continue;
                        }
                        break;
                    case DuplicateMode.KeepAll:
                        int i = 2;
                        while (conflictingDestination)
                        {
                            string newDestinationPath = String.Format("{0}\\{1}({2}){3}", Path.GetDirectoryName(item.destinationPath), Path.GetFileNameWithoutExtension(item.destinationPath), i++, Path.GetExtension(item.destinationPath));
                            conflictingDestination = !handledFilenames.Add(newDestinationPath);
                            if (!conflictingDestination)
                                item.destinationPath = newDestinationPath;
                        }
                        break;
                    default:
                        throw new NotImplementedException(String.Format("Unhandled duplicate mode: {0}", DuplicateMode));
                }
            }
            summary.duplicates = duplicates.ToArray();
        }

        private void PrepareDestinationPath()
        {
            // Setup destination path
            if (!Directory.Exists(copyItems.destinationPath))
            {
                Directory.CreateDirectory(copyItems.destinationPath);
                return;
            }

            switch (CopyPrecondition)
            {
                case CopyPrecondition.None:
                    break;

                case CopyPrecondition.RequireEmpty:
                    if (Directory.Exists(copyItems.destinationPath))
                    {
                        if (Directory.GetFiles(copyItems.destinationPath).Length > 0)
                            throw new MediaOrganizerException("Path contains files but is required to be empty: {0}", copyItems.destinationPath);
                        if (Directory.GetDirectories(copyItems.destinationPath).Length > 0)
                            throw new MediaOrganizerException("Path contains directories but is required to be empty: {0}", copyItems.destinationPath);
                    }
                    break;

                case CopyPrecondition.WipeBefore:
                    if (copyItems.sourcePath.DirectoryAreSame(copyItems.destinationPath))
                    {
                        // TODO: handle exceptions
                        // Move source/destination path to temporary place before copying
                        string tempSourcePath = Path.GetTempFileName();
                        File.Delete(tempSourcePath);
                        Directory.Move(copyItems.sourcePath, tempSourcePath);
                        copyItems.sourcePath = tempSourcePath; // TODO: delete this path after organization
                    }
                    else
                    {
                        Directory.Delete(copyItems.destinationPath, true);
                    }
                    Directory.CreateDirectory(copyItems.destinationPath);
                    break;

                default:
                    throw new NotImplementedException(String.Format("CopyPrecondition: {0}", CopyPrecondition));
            }
        }

        // TODO: use hashset to remove duplicates? Or better: define behaviour of which to insert into list
        private List<CopyItem> ParseItems(string sourcePath, string destinationPath, ref OrganizeSummary summary)
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

            HashSet<string> valid = new HashSet<string>();
            List<CopyItem> items = new List<CopyItem>();
            foreach (MetaData meta in data)
            {
                CopyItem item = ParseItem(destinationPath, meta);
                items.Add(item);
                valid.Add(item.sourcePath);
            }
            summary.parsed = valid.ToArray();
            return items;
        }

        private CopyItem ParseItem(string destinationPath, MetaData meta)
        {
            string sourcePath = meta.Path;

            CopyItem item = new CopyItem();
            item.sourceInfo = new FileInfo(sourcePath);
            item.sourcePath = sourcePath;
            item.destinationPath = CalculateDestinationPath(destinationPath, meta);
            item.meta = meta.Data;
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
                GroupType groupType;
                if (organizeGroups.TryGetValue(subpattern, out groupType))
                {
                    switch (groupType)
                    {
                        case GroupType.Year:
                            {
                                object temp;
                                if (!meta.Data.TryGetValue(MetaKey.Date, out temp))
                                    throw new MediaOrganizerException("Failed to retrieve key {0} from meta data to parse %y", MetaKey.Date);

                                DateTime datetime = (DateTime)temp;
                                currentPath = Path.Combine(currentPath, datetime.Year.ToString());
                            }
                            break;
                        case GroupType.Month:
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
                        case GroupType.Day:
                            {
                                object temp;
                                if (!meta.Data.TryGetValue(MetaKey.Date, out temp))
                                    throw new MediaOrganizerException("Failed to retrieve key {0} from meta data to parse %m", MetaKey.Date);

                                DateTime datetime = (DateTime)temp;

                                DateTimeFormatInfo dateinfo = Localization.DateTimeFormat;
                                string dayName = dateinfo.DayNames[datetime.Day - 1].UppercaseFirst();
                                currentPath = Path.Combine(currentPath, dayName);
                            }
                            break;
                        case GroupType.Name:
                            {
                                object temp;
                                if (!meta.Data.TryGetValue(MetaKey.Filename, out temp))
                                    throw new MediaOrganizerException("Failed to retrieve key {0} from meta data to parse %o", MetaKey.Filename);

                                string filename = (string)temp;
                                currentPath = Path.Combine(currentPath, filename);
                            }
                            break;
                        case GroupType.Tags:
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
                        default:
                            break;
                    }
                }
                else
                {
                    if (subpattern.StartsWith("%"))
                        throw new MediaOrganizerException("Unhandled pattern item: {0}", subpattern);

                    currentPath = Path.Combine(currentPath, subpattern);
                }
            }

            return currentPath;
        }
    }
}
