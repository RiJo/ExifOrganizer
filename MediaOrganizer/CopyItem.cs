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

        private Dictionary<string, string> checksums = new Dictionary<string, string>();

        public CopyItem()
        {
        }

        public string GetChecksumMD5()
        {
            if (!checksums.ContainsKey("md5"))
                checksums["md5"] = sourceInfo.GetMD5Sum();
            return checksums["md5"];
        }

        public string GetChecksumSHA1()
        {
            if (!checksums.ContainsKey("sha1"))
                checksums["sha1"] = sourceInfo.GetSHA1Sum();
            return checksums["sha1"];
        }

        public string GetChecksumSHA256()
        {
            if (!checksums.ContainsKey("sha256"))
                checksums["sha256"] = sourceInfo.GetSHA256Sum();
            return checksums["sha256"];
        }

        public override string ToString()
        {
            return $"[{sourcePath}] ---> [{destinationPath}]";
        }
    }
}