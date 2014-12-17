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
using System.Text.RegularExpressions;
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
        KeepUnique,
        KeepExisting,
        OverwriteExisting,
    }

    [Flags]
    public enum FileComparator
    {
        None = 0x00,
        FileSize = 0x01,
        Checksum = 0x02,
        Created = 0x04,
        Modified = 0x08,
        All = 0xFF
    }

    public enum ExceptionHandling
    {
        Throw,
        Ignore,
    }

    public class OrganizeSummary
    {
        public string[] parsed;
        public string[] ignored;
        public string[] totalFiles;
        public string[] totalDirectories;

        public override string ToString()
        {
            return String.Format("Summary {{ Parsed: {0} (Ignored: {1}), Total files: {2}, Total directories: {3} }}",
                parsed != null ? parsed.Length : 0,
                ignored != null ? ignored.Length : 0,
                totalFiles != null ? totalFiles.Length : 0,
                totalDirectories != null ? totalDirectories.Length : 0
            );
        }
    }

    public class MediaOrganizer
    {
        private const double PARSE_PROGRESS_FACTOR = 0.1;

        private enum GroupType
        {
            Index,
            Year,
            MonthName,
            MonthNumber,
            DayName,
            DayNumber,
            OriginalName,
            FileName,
            FileExtension,
            Tags,
            Camera
        }

        public MediaOrganizer()
        {
        }

        public event Action<MediaOrganizer, double, string> OnProgress = delegate { };

        public string sourcePath;
        public string destinationPath;
        public bool Recursive = true;
        public CultureInfo Localization = Thread.CurrentThread.CurrentCulture;
        public string DestinationPatternImage = @"%y/%m %M/%t/%n.%e";
        public string DestinationPatternVideo = @"%y/%m %M/Video/%t/%n.%e";
        public string DestinationPatternAudio = @"%y/%m %M/Audio/%t/%n.%e";
        public CopyPrecondition CopyPrecondition = CopyPrecondition.None;
        public FileComparator FileComparator = FileComparator.FileSize | FileComparator.Checksum;
        public CopyMode CopyMode = CopyMode.KeepUnique;
        public ExceptionHandling ExceptionHandling = ExceptionHandling.Throw;
        public string[] IgnorePaths = null;

        private readonly Dictionary<string, GroupType> organizeGroups = new Dictionary<string, GroupType>()
        {
            { "%i", GroupType.Index },
            { "%y", GroupType.Year },
            { "%m", GroupType.MonthNumber },
            { "%M", GroupType.MonthName },
            { "%d", GroupType.DayNumber },
            { "%D", GroupType.DayName },
            { "%n", GroupType.FileName },
            { "%e", GroupType.FileExtension },
            { "%N", GroupType.OriginalName },
            { "%t", GroupType.Tags },
            { "%c", GroupType.Camera }
        };

        public CopyItems copyItems;


        public OrganizeSummary Parse()
        {
            if (sourcePath.DirectoryAreSame(destinationPath))
            {
                throw new NotSupportedException("TODO");
            }

            OnProgress(this, 0.0, "Parsing source");

            OrganizeSummary summary = new OrganizeSummary();

            copyItems = new CopyItems();
            copyItems.sourcePath = sourcePath;
            copyItems.destinationPath = destinationPath;
            copyItems.items = ParseItems(sourcePath, destinationPath, ref summary);

            OnProgress(this, PARSE_PROGRESS_FACTOR, "Parsing complete");

            return summary;
        }

        public void Organize()
        {
            if (copyItems == null)
                throw new InvalidOperationException("Parse() must be executed prior to Organize()");

            OnProgress(this, PARSE_PROGRESS_FACTOR + 0.1, "Prepare destination");

            PrepareDestinationPath();


            // Copy items to destination path
            int itemCount = copyItems.items.Count;
            for (int i = 0; i < itemCount; i++)
            {
                float progress = (float)(i) / (float)itemCount;
                if ((int)(progress * 10) % 2 == 0)
                    OnProgress(this, PARSE_PROGRESS_FACTOR + 0.1 + (progress * (1.0 - PARSE_PROGRESS_FACTOR - 0.1)), String.Format("Organizing {0} of {1}", i + 1, itemCount));

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
                        if (ExceptionHandling == ExceptionHandling.Throw)
                            throw new MediaOrganizerException(String.Format("Failed to create directory: {0}", destinationDirectory), ex.Message);
                        else
                            continue;
                    }
                }

                bool overwrite = false;
                string destinationPath = item.destinationPath;
                bool fileExists = File.Exists(destinationPath);

                switch (CopyMode)
                {
                    case CopyMode.OverwriteExisting:
                        overwrite = true;
                        break;

                    case CopyMode.KeepExisting:
                        if (fileExists)
                            continue;
                        break;

                    case CopyMode.KeepUnique:
                        FileInfo sourceInfo = item.sourceInfo;
                        FileInfo destinationInfo = new FileInfo(destinationPath);
                        if (fileExists)
                        {
                            // Potentially slow, therefore previous optimizations
                            if (sourceInfo.AreFilesIdentical(destinationInfo, FileComparator))
                                continue;
                        }

                        if (sourceInfo.FileExistsInDirectory(destinationInfo.Directory, FileComparator))
                            continue; // Source file already exists in target directory

                        // Find next unused filename
                        int index = 1;
                        while (fileExists)
                        {
                            destinationPath = destinationInfo.SuffixFileName(index++);
                            fileExists = File.Exists(destinationPath);
                        }
                        break;

                    default:
                        throw new NotImplementedException(String.Format("CopyMode: {0}", CopyMode));
                }

                try
                {
                    File.Copy(item.sourcePath, destinationPath, overwrite);
                }
                catch (Exception ex)
                {
                    if (ExceptionHandling == ExceptionHandling.Throw)
                        throw new MediaOrganizerException(String.Format("Failed to copy file. Mode: {0}. Overwrite: {1}. Source: {2}. Destination: {3}", CopyMode, overwrite, item.sourcePath, item.destinationPath), ex.Message);
                    else
                        continue;
                }
            }

            OnProgress(this, 1.0, "Organization complete");
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
                        try
                        {
                            File.Delete(tempSourcePath);
                            Directory.Move(copyItems.sourcePath, tempSourcePath);
                        }
                        catch (Exception ex)
                        {
                            throw new MediaOrganizerException(String.Format("Failed to generate temporary directory: {0}", tempSourcePath), ex);
                        }
                        copyItems.sourcePath = tempSourcePath; // TODO: delete this path after organization
                    }
                    else
                    {
                        try
                        {
                            Directory.Delete(copyItems.destinationPath, true);
                        }
                        catch (Exception ex)
                        {
                            throw new MediaOrganizerException(String.Format("Failed to wipe destination path: {0}", copyItems.destinationPath), ex);
                        }
                    }

                    if (!Directory.Exists(copyItems.destinationPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(copyItems.destinationPath);
                        }
                        catch (Exception ex)
                        {
                            throw new MediaOrganizerException(String.Format("Failed to create destination path: {0}", copyItems.destinationPath), ex);
                        }
                    }
                    break;

                default:
                    throw new NotImplementedException(String.Format("CopyPrecondition: {0}", CopyPrecondition));
            }
        }

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

            HashSet<string> files = new HashSet<string>();
            HashSet<string> directories = new HashSet<string>();
            HashSet<string> valid = new HashSet<string>();
            HashSet<string> ignored = new HashSet<string>();
            List<CopyItem> items = new List<CopyItem>();
            int index = 0;
            foreach (MetaData meta in data)
            {
                string path = meta.Path;

                switch (meta.Type)
                {
                    case MetaType.Directory:
                        directories.Add(path);
                        continue;
                    case MetaType.File:
                        files.Add(path);
                        ignored.Add(path);
                        continue;
                    default:
                        files.Add(path);
                        break;
                }

                CopyItem item = ParseItem(destinationPath, meta, index++);
                items.Add(item);
                valid.Add(path);
            }

            summary.totalDirectories = directories.ToArray();
            summary.totalFiles = files.ToArray();
            summary.parsed = valid.ToArray();
            summary.ignored = ignored.ToArray();
            return items;
        }

        private CopyItem ParseItem(string destinationPath, MetaData meta, int index)
        {
            string sourcePath = meta.Path;

            CopyItem item = new CopyItem();
            item.sourceInfo = new FileInfo(sourcePath);
            item.sourcePath = sourcePath;
            item.destinationPath = CalculateDestinationPath(destinationPath, meta, index);
            item.meta = meta.Data;
            return item;
        }

        private string CalculateDestinationPath(string destinationPath, MetaData meta, int index)
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

            string subPath = String.Empty;
            foreach (string subPattern in pattern)
            {
                Dictionary<string, string> replacements = new Dictionary<string, string>();

                // Gather replacements
                string regex = @"(?<!%)%[a-zA-Z]";
                foreach (Match match in Regex.Matches(subPattern, regex))
                {
                    if (replacements.ContainsKey(match.Value))
                        continue;

                    string replacement = GetPatternReplacement(meta, index, match.Value);
                    if (String.IsNullOrEmpty(replacement))
                        continue;

                    replacements[match.Value] = replacement;
                }

                // Perform replacement
                string temp = subPattern;
                foreach (KeyValuePair<string, string> kvp in replacements)
                    temp = Regex.Replace(temp, @"(?<!%)" + kvp.Key, kvp.Value);
                temp = temp.Replace("%%", "%"); // Remove escape character

                if (Regex.IsMatch(temp, "$" + regex + "^"))
                    continue; // Pattern matched but not replaced: skip in path

                temp = temp.ReplaceInvalidPathChars();
                subPath = Path.Combine(subPath, temp);
            }

            destinationPath = destinationPath.ReplaceInvalidPathChars();
            return Path.Combine(destinationPath, subPath);
        }

        /*private void FilterDuplicateItems(ref OrganizeSummary summary)
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
                    long size = (long)temp;
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
        }*/

        private string GetPatternReplacement(MetaData meta, int index, string subpattern)
        {
            GroupType groupType;
            if (!organizeGroups.TryGetValue(subpattern, out groupType))
                throw new MediaOrganizerException("Unhandled pattern item: {0}", subpattern);


            switch (groupType)
            {
                case GroupType.Index:
                    {
                        return index.ToString();
                    }

                case GroupType.Year:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.Date, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.Date, GroupType.Year);

                        DateTime datetime = (DateTime)temp;
                        return datetime.Year.ToString();
                    }

                case GroupType.MonthNumber:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.Date, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.Date, GroupType.MonthNumber);

                        DateTime datetime = (DateTime)temp;

                        DateTimeFormatInfo dateinfo = Localization.DateTimeFormat;
                        return datetime.Month.ToString("D2");
                    }

                case GroupType.MonthName:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.Date, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.Date, GroupType.MonthName);

                        DateTime datetime = (DateTime)temp;

                        DateTimeFormatInfo dateinfo = Localization.DateTimeFormat;
                        return dateinfo.MonthNames[datetime.Month - 1].UppercaseFirst();
                    }

                case GroupType.DayNumber:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.Date, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.Date, GroupType.DayNumber);

                        DateTime datetime = (DateTime)temp;

                        DateTimeFormatInfo dateinfo = Localization.DateTimeFormat;
                        return datetime.Day.ToString();
                    }

                case GroupType.DayName:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.Date, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.Date, GroupType.DayName);

                        DateTime datetime = (DateTime)temp;

                        DateTimeFormatInfo dateinfo = Localization.DateTimeFormat;
                        return dateinfo.DayNames[datetime.Day - 1].UppercaseFirst();
                    }

                case GroupType.FileName:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.FileName, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.FileName, GroupType.FileName);

                        return Path.GetFileNameWithoutExtension((string)temp);
                    }

                case GroupType.OriginalName:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.OriginalName, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.OriginalName, GroupType.OriginalName);

                        return Path.GetFileName((string)temp);
                    }

                case GroupType.FileExtension:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.FileName, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.FileName, GroupType.FileExtension);

                        return Path.GetExtension((string)temp).Substring(1);
                    }

                case GroupType.Tags:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.Tags, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.Tags, GroupType.Tags);

                        string[] tags = temp as string[];
                        if (tags == null || tags.Length == 0)
                            return null;

                        string tag = tags[0]; // TODO: how to solve multiple tags?
                        return tag;
                    }

                case GroupType.Camera:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.Camera, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.Camera, GroupType.Camera);

                        return (string)temp;
                    }

                default:
                    throw new NotImplementedException(String.Format("GroupType: {0}", groupType));
            }
        }
    }
}
