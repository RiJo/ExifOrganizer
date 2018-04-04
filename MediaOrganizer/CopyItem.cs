//
// CopyItem.cs: Data structure describing single item to be copied.
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
using System.IO;

namespace ExifOrganizer.Organizer
{
    public class CopyItem
    {
        public string sourcePath;
        public string destinationPath;
        public FileInfo sourceInfo;
        public Dictionary<MetaKey, object> meta;

        private string checksum;

        public CopyItem()
        {
        }

        public string GetChecksum()
        {
            if (!String.IsNullOrEmpty(checksum))
                return checksum;

            checksum = sourceInfo.GetMD5Sum();
            return checksum;
        }

        public override string ToString()
        {
            return $"[{sourcePath}] ---> [{destinationPath}]";
        }
    }
}