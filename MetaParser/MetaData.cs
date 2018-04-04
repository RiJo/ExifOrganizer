//
// MetaData.cs: Data structure for parsed meta data.
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

using System.Collections.Generic;

namespace ExifOrganizer.Meta
{
    public enum MetaType
    {
        Directory,
        File,

        Image,
        Video,
        Music
    }

    public enum MetaKey
    {
        OriginalName,
        FileName,
        Size,
        Timestamp,
        DateCreated,
        DateModified,
        Resolution,
        Width,
        Height,
        Camera,
        Tags
    }

    public class MetaData
    {
        public MetaType Type;
        public string Path;
        public Dictionary<MetaKey, object> Data;
#if DEBUG
        public object Origin;
#endif
    }
}