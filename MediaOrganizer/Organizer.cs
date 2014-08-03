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
    public class Organizer
    {
        public Organizer()
        {
        }

        public bool Recursive = true;
        public string Localization = "SV-se";
        public string[] AdditionalExtensions = new string[] { "mpg", "mpeg", "mov", "mp4" };
        public string DestinationPattern = @"%y/%m/%o";
        /*
            %y = year
            %m = month
            %o = original name

        */

        public CopyItems Parse(string sourcePath, string destinationPath)
        {
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
                data = Parser.Parse(sourcePath, Recursive, AdditionalExtensions);
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
            string[] pattern = DestinationPattern.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

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
                        throw new MediaOrganizerException("Invalid pattern item: {0}", subpattern);
                }
            }

            return currentPath;
        }
    }
}
