//
// ExtensionMethods.cs: Static extension methods used within the project.
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
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ExifOrganizer.Meta
{
    public static class ExtensionMethods
    {
        public static MetaData Merge(this MetaData meta, MetaData other)
        {
            if (meta.Path != other.Path)
                throw new MetaParseException("Cannot merge meta data: path differ");
            if (meta.Origin != other.Origin)
                throw new MetaParseException("Cannot merge meta data: origin differ");
            if (meta.Type != other.Type)
                throw new MetaParseException("Cannot merge meta data: type differ");

            foreach (var kvp in other.Data)
            {
                if (meta.Data.ContainsKey(kvp.Key))
                {
                    if (kvp.Value != meta.Data[kvp.Key])
                        Trace.WriteLine($"[MetaData] merge ignore duplicate key: \"{kvp.Key}\"");
                    continue;
                }

                meta.Data[kvp.Key] = kvp.Value;
            }

            return meta;
        }
    }
}