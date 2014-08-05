//
// CopyItem.cs: Data structure describing items to copy.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExifOrganizer.Organizer
{
    public class CopyItem
    {
        public string sourcePath;
        public string destinationPath;

        public override string ToString()
        {
            return String.Format("[{0}] ---> [{1}]", sourcePath, destinationPath);
        }
    }

    public class CopyItems
    {
        public string sourcePath;
        public string destinationPath;
        public List<CopyItem> items;

        public override string ToString()
        {
            List<string> itemStrings = new List<string>();
            if (items != null)
            {
                foreach (CopyItem item in items)
itemStrings.Add(item.ToString());
            }
            return String.Format("Copy: [{0}] ---> [{1}]{2}Items:{2}{3}", sourcePath, destinationPath, Environment.NewLine, String.Join(Environment.NewLine, itemStrings.ToArray()));
        }
    }
}
