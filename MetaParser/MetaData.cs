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
        Audio
    }

    public enum MetaKey
    {
        // Common
        MetaType,
        OriginalName,
        FileName,
        Size,
        Timestamp,
        DateCreated,
        DateModified,
        Comment,

        // Image
        Resolution,
        Width,
        Height,
        Camera,
        Tags,

        // Music
        Title,
        Artist,
        Album,
        Year,
        Track,
        Genre
    }

    public enum MetaMediaType
    {
        Image,
        Audio,
        Video
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

    public static class MetaDataExtensions
    {
        public static IEnumerable<MetaKey> GetByMedia(this MetaMediaType media)
        {
            HashSet<MetaKey> keys = new HashSet<MetaKey>();
            keys.Add(MetaKey.MetaType);
            keys.Add(MetaKey.OriginalName);
            keys.Add(MetaKey.FileName);
            keys.Add(MetaKey.Size);
            keys.Add(MetaKey.Timestamp);
            keys.Add(MetaKey.DateCreated);
            keys.Add(MetaKey.DateModified);
            keys.Add(MetaKey.Comment);

            switch (media)
            {
                case MetaMediaType.Image:
                    keys.Add(MetaKey.Resolution);
                    keys.Add(MetaKey.Width);
                    keys.Add(MetaKey.Height);
                    keys.Add(MetaKey.Camera);
                    keys.Add(MetaKey.Tags);
                    break;

                case MetaMediaType.Audio:
                    keys.Add(MetaKey.Title);
                    keys.Add(MetaKey.Artist);
                    keys.Add(MetaKey.Album);
                    keys.Add(MetaKey.Year);
                    keys.Add(MetaKey.Track);
                    keys.Add(MetaKey.Genre);
                    break;

                case MetaMediaType.Video:
                    break;

                default:
                    break;
            }
            return keys;
        }
    }
}