//
// PatternPathParser.cs: Helper class used to make real paths out of predefined patterns.
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
using ExifOrganizer.Meta;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExifOrganizer.Organizer
{
    public enum GroupType
    {
        Index,
        Year,
        MonthName,
        MonthNumber,
        DayName,
        DayNumber,
        OriginalName,
        FileName, // TODO: add filename w/o extension
        FileExtension,
        Tags,
        Camera
    }

    public class PatternPathParser
    {
        private const string TagSeparator = ", ";

        private static readonly Dictionary<string, GroupType> organizeGroups = new Dictionary<string, GroupType>()
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

        private CultureInfo locale = null;
        private Dictionary<MetaData, int> indexes = new Dictionary<MetaData, int>();
        private Dictionary<int, string> tags = new Dictionary<int, string>();
        private Dictionary<string, HashSet<DateTime>> tagTimestamps = new Dictionary<string, HashSet<DateTime>>();

        public PatternPathParser(CultureInfo culture)
        {
            locale = culture;
        }

        public void Preload(IEnumerable<MetaData> items)
        {
            // Remove old entries
            indexes.Clear();
            tags.Clear();
            tagTimestamps.Clear();

            // Add new entries
            int index = 0;
            foreach (MetaData meta in items)
            {
                if (meta.Type == MetaType.Directory)
                    continue;
                if (meta.Data == null)
                    continue;

                indexes[meta] = index;

                if (meta.Data.ContainsKey(MetaKey.Tags))
                {
                    string[] temp = meta.Data[MetaKey.Tags] as string[];
                    if (temp != null && temp.Length > 0)
                    {
                        // Store tag name
                        string tagString = String.Join(TagSeparator, temp.Select(x => x.Trim()));
                        tags[index] = tagString;

                        // Store tag timestamp
                        if (!tagTimestamps.ContainsKey(tagString))
                            tagTimestamps[tagString] = new HashSet<DateTime>();
                        tagTimestamps[tagString].Add((DateTime)meta.Data[MetaKey.Timestamp]);
                    }
                }

                index++;
            }
        }

        public string Parse(string destinationPattern, string destinationPath, MetaData meta)
        {
            int index;
            if (!indexes.TryGetValue(meta, out index))
                throw new InvalidOperationException("Meta data hasn't been preloaded");

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

                if (Regex.IsMatch(temp, "^" + regex + "$"))
                    continue; // Pattern matched but not replaced: skip in path

                temp = temp.ReplaceInvalidPathChars();
                subPath = Path.Combine(subPath, temp);
            }

            destinationPath = destinationPath.ReplaceInvalidPathChars();
            return Path.Combine(destinationPath, subPath);
        }

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
                        if (!meta.Data.TryGetValue(MetaKey.Timestamp, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.Timestamp, groupType);

                        DateTime datetime = (DateTime)temp;
                        return datetime.Year.ToString();
                    }

                case GroupType.MonthNumber:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.Timestamp, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.Timestamp, groupType);

                        DateTime datetime = (DateTime)temp;

                        DateTimeFormatInfo dateinfo = locale.DateTimeFormat;
                        return datetime.Month.ToString("D2");
                    }

                case GroupType.MonthName:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.Timestamp, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.Timestamp, groupType);

                        DateTime datetime = (DateTime)temp;

                        DateTimeFormatInfo dateinfo = locale.DateTimeFormat;
                        return dateinfo.MonthNames[datetime.Month - 1].UppercaseFirst();
                    }

                case GroupType.DayNumber:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.Timestamp, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.Timestamp, groupType);

                        DateTime datetime = (DateTime)temp;

                        DateTimeFormatInfo dateinfo = locale.DateTimeFormat;
                        return datetime.Day.ToString();
                    }

                case GroupType.DayName:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.Timestamp, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.Timestamp, groupType);

                        DateTime datetime = (DateTime)temp;

                        DateTimeFormatInfo dateinfo = locale.DateTimeFormat;
                        return dateinfo.DayNames[datetime.Day - 1].UppercaseFirst();
                    }

                case GroupType.FileName:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.FileName, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.FileName, groupType);

                        return Path.GetFileNameWithoutExtension((string)temp);
                    }

                case GroupType.OriginalName:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.OriginalName, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.OriginalName, groupType);

                        return Path.GetFileName((string)temp);
                    }

                case GroupType.FileExtension:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.FileName, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.FileName, groupType);

                        return Path.GetExtension((string)temp).Substring(1);
                    }

                case GroupType.Tags:
                    {
                        //object temp;
                        //if (!meta.Data.TryGetValue(MetaKey.Tags, out temp))
                        //    throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.Tags, groupType);

                        //string[] tags = temp as string[];
                        //if (tags == null || tags.Length == 0)
                        //    return null;

                        //string tag = String.Join(", ", tags);
                        string tag;
                        if (!tags.TryGetValue(index, out tag))
                            return null;

                        return tag;
                    }

                case GroupType.Camera:
                    {
                        object temp;
                        if (!meta.Data.TryGetValue(MetaKey.Camera, out temp))
                            throw new MediaOrganizerException("Failed to retrieve key '{0}' from meta data to parse group type '{1}'", MetaKey.Camera, groupType);

                        return (string)temp;
                    }

                default:
                    throw new NotImplementedException($"GroupType: {groupType}");
            }
        }

        public static string GetUsageText()
        {
            return String.Join(Environment.NewLine, organizeGroups.Select(x => $"{x.Key} : {x.Value.GetGroupTypeText()}").OrderBy(x => x));
        }
    }
}