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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaParser
{
    public enum MetaType
    {
        Image,
        Video,
        Music
    }

    public enum MetaKey
    {
        Filename,
        Date,
        //Size,
        Tags
    }

    public class MetaData
    {
        public MetaType Type;
        public string Path;
        public Dictionary<MetaKey, object> Data;
        public object Source;
    }
}
