//
// IniFile.cs: Data structure used to save/load ini files.
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
using System.Text;
using System.Threading.Tasks;

namespace ExifOrganizer.Organizer
{
    public class IniFile
    {
        private Dictionary<string, string> values = new Dictionary<string, string>();

        public IniFile()
        {
        }

        public string this[string key]
        {
            get { return values[key]; }
            set { values[key] = value; }
        }

        public bool Contains(string key)
        {
            return values.ContainsKey(key);
        }

        public void Load(string filename)
        {
            if (String.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename);

            values.Clear();

            foreach (string line in File.ReadAllLines(filename))
            {
                if (line.Trim().Length == 0)
                    continue;
                if (line.Trim().StartsWith(";"))
                    continue;

                string[] keyValue = line.Split(new char[] { ':' }, 2);
                if (keyValue.Length != 2)
                    continue; // Invalid row

                string key = keyValue[0].Trim();
                string value = keyValue[1].Trim();
                values[key] = value;
            }
        }

        public void Save(string filename)
        {
            if (String.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");

            List<string> lines = new List<string>();
            foreach (string key in values.Keys)
            {
                string line = String.Format("{0}: {1}", key, values[key]);
                lines.Add(line);
            }

            File.WriteAllLines(filename, lines.ToArray());
        }
    }
}
