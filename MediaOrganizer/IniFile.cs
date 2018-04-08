//
// IniFile.cs: Data structure used to save/load an ini file.
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

namespace ExifOrganizer.Organizer
{
    public class IniFile : Dictionary<string, string>
    {
        private const char CommentSymbol = ';';
        private const char KeyValueSeparator = ':';

        public IniFile()
            : base()
        {
        }

        public IniFile(string filename)
            : base()
        {
            TryLoad(filename);
        }

        public bool TryLoad(string filename)
        {
            try
            {
                Load(filename);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Load(string filename)
        {
            if (String.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename);

            Clear();

            foreach (string line in File.ReadAllLines(filename))
            {
                if (line.Trim().Length == 0)
                    continue; // Empty line
                if (line.Trim().StartsWith(CommentSymbol.ToString()))
                    continue; // Comment

                string[] keyValue = line.Split(new char[] { KeyValueSeparator }, 2);
                if (keyValue.Length != 2)
                    continue; // Invalid row

                string key = keyValue[0].Trim();
                string value = keyValue[1].Trim();
                Add(key, value);
            }
        }

        public bool TrySave(string filename)
        {
            try
            {
                Save(filename);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Save(string filename)
        {
            if (String.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");

            List<string> lines = new List<string>();
            foreach (KeyValuePair<string, string> kvp in this)
            {
                string line = $"{kvp.Key}{KeyValueSeparator} {kvp.Value}";
                lines.Add(line);
            }

            File.WriteAllLines(filename, lines);
        }
    }
}