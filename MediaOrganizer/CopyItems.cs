//
// CopyItems.cs: Data structure describing multiple items to be copied.
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

namespace ExifOrganizer.Organizer
{
    public class CopyItems : List<CopyItem>
    {
        public string sourcePath;
        public string destinationPath;

        public override string ToString()
        {
            return $"Copy: [{sourcePath}] ---> [{destinationPath}]{Environment.NewLine}Items:{Environment.NewLine}{String.Join(Environment.NewLine, this)}";
        }
    }
}