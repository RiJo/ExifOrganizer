//
// Organizer.cs: Static class to organize media files.
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

using MetaParser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaOrganizer
{
    public enum CopyMode
    {
        RequireEmpty,
        Delta,
        ForceOverwrite
    }

    public class Organizer
    {
        public Organizer()
        {
        }

        public bool Recursive = true;
        public string Localization = "SV-se";
        public string DestinationPatternImage = @"%y/%m/%t/%o";
        public string DestinationPatternVideo = @"%y/%m/Video/%t/%o";
        public string DestinationPatternMusic = @"%y/%m/Music/%t/%o";
        public CopyMode CopyMode = CopyMode.RequireEmpty; // TODO: implement
        /*
            %y = year
            %m = month
            %o = original name

        */

        public CopyItems Parse(string sourcePath, string destinationPath)
        {
            if (sourcePath == destinationPath)
                throw new NotImplementedException("Update path not yet implemented");

            CopyItems reference = new CopyItems();
            reference.sourcePath = sourcePath;
            reference.destinationPath = destinationPath;
            reference.items = ParseItems(sourcePath, destinationPath);

            return reference;
        }

        public void Organize(CopyItems reference)
        {
            if (Directory.Exists(reference.destinationPath))
            {
                if (Directory.GetFiles(reference.destinationPath).Length > 0)
                    throw new MediaOrganizerException("Destination path contains files: {0}", reference.destinationPath);
                if (Directory.GetDirectories(reference.destinationPath).Length > 0)
                    throw new MediaOrganizerException("Destination path contains directories: {0}", reference.destinationPath);
            }
            else
            {
                Directory.CreateDirectory(reference.destinationPath);
            }


            foreach (CopyItem item in reference.items)
            {
                if (!Directory.Exists(Path.GetDirectoryName(item.destinationPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(item.destinationPath));
                File.Copy(item.sourcePath, item.destinationPath);
            }
        }

        // TODO: use hashset to remove duplicates? Or better: define behaviour of which to insert into list
        private List<CopyItem> ParseItems(string sourcePath, string destinationPath)
        {
            IEnumerable<MetaData> data;
            try
            {
                data = Parser.Parse(sourcePath, Recursive);
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
                    destinationPattern = DestinationPatternMusic;
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

                            CultureInfo locale = new CultureInfo(Localization);
                            DateTimeFormatInfo dateinfo = locale.DateTimeFormat;
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
                    case "%o": // Original name
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
