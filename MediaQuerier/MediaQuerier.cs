//
// MediaQuerier.cs: Static class to query media files based on properties.
//
// Copyright (C) 2016 Rikard Johansson
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

using ExifOrganizer.Common;
using ExifOrganizer.Meta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ExifOrganizer.Querier
{
    [Flags]
    public enum QueryType
    {
        None = 0x00,
        EqualFileName = 0x01,
        EqualFileNameDifferentFileSize = 0x02,
        EqualChecksumMD5 = 0x04,
        EqualFileNameDifferentChecksumMD5 = 0x08,
        EqualChecksumSHA1 = 0x10,
        EqualFileNameDifferentChecksumSHA1 = 0x20,
        EqualChecksumSHA256 = 0x40,
        EqualFileNameDifferentChecksumSHA256 = 0x80,
        LowResolution = 0x100,
        EqualFileNameDifferentResolution = 0x200,
        All = 0xFFFF
    }

    public class QuerySummary
    {
        public Dictionary<string, IEnumerable<Tuple<string, QueryType>>> matches;
    }

    public class MediaQuerier
    {
        public event Action<MediaQuerier, double, string> OnProgress = delegate { };

        public MediaQuerier()
        {
        }

        public QuerySummary Query(string sourcePath, bool recursive, QueryType queries, params MetaType[] types)
        {
            Task<QuerySummary> task = QueryAsync(sourcePath, recursive, queries, types);
            task.ConfigureAwait(false); // Prevent deadlock of caller
            return task.Result;
        }

        public async Task<QuerySummary> QueryAsync(string sourcePath, bool recursive, QueryType queries, params MetaType[] types)
        {
            if (String.IsNullOrEmpty(sourcePath))
                throw new ArgumentNullException(nameof(sourcePath));
            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException(sourcePath);
            if (queries == QueryType.None)
                throw new ArgumentException("No queries given.");

            Dictionary<string, IEnumerable<Tuple<string, QueryType>>> allMatches = new Dictionary<string, IEnumerable<Tuple<string, QueryType>>>();
            Dictionary<string, string> md5sums = new Dictionary<string, string>();
            Dictionary<string, string> sha1sums = new Dictionary<string, string>();
            Dictionary<string, string> sha256sums = new Dictionary<string, string>();
            IEnumerable<MetaData> data = await GetMetaData(sourcePath, recursive, types);
            foreach (MetaData item in data.Where(x => x.Type != MetaType.Directory && x.Type != MetaType.File))
            {
                List<Tuple<string, QueryType>> matches = new List<Tuple<string, QueryType>>();
                foreach (MetaData other in data.Where(x => x.Type != MetaType.Directory && x.Type != MetaType.File && !x.Equals(item)))
                {
                    QueryType match = QueryType.None;

                    bool equalFilename = Path.GetFileName(item.Path).Equals(Path.GetFileName(other.Path), StringComparison.Ordinal);
                    if (queries.HasFlag(QueryType.EqualFileName) && equalFilename)
                        match |= QueryType.EqualFileName;

                    if (queries.HasFlag(QueryType.EqualFileNameDifferentFileSize) && equalFilename)
                    {
                        if (RequireMetaKey(MetaKey.Size, item, other))
                        {
                            if (item.Data[MetaKey.Size] != other.Data[MetaKey.Size])
                                match |= QueryType.EqualFileNameDifferentFileSize;
                        }
                    }

                    if (queries.HasFlag(QueryType.EqualChecksumMD5) || queries.HasFlag(QueryType.EqualFileNameDifferentChecksumMD5))
                    {
                        string md5item;
                        if (!md5sums.TryGetValue(item.Path, out md5item))
                        {
                            using (FileStream fileStream = File.OpenRead(item.Path))
                                md5item = fileStream.GetMD5Sum();
                            md5sums[item.Path] = md5item;
                        }

                        string md5other;
                        if (!md5sums.TryGetValue(other.Path, out md5other))
                        {
                            using (FileStream fileStream = File.OpenRead(other.Path))
                                md5other = fileStream.GetMD5Sum();
                            md5sums[other.Path] = md5other;
                        }

                        bool md5match = md5item.Equals(md5other, StringComparison.Ordinal);
                        if (queries.HasFlag(QueryType.EqualChecksumMD5) && md5match)
                            match |= QueryType.EqualChecksumMD5;
                        if (queries.HasFlag(QueryType.EqualFileNameDifferentChecksumMD5) && equalFilename && !md5match)
                            match |= QueryType.EqualFileNameDifferentChecksumMD5;
                    }

                    if (queries.HasFlag(QueryType.EqualChecksumSHA1) || queries.HasFlag(QueryType.EqualFileNameDifferentChecksumSHA1))
                    {
                        string sha1item;
                        if (!sha1sums.TryGetValue(item.Path, out sha1item))
                        {
                            using (FileStream fileStream = File.OpenRead(item.Path))
                                sha1item = fileStream.GetSHA1Sum();
                            sha1sums[item.Path] = sha1item;
                        }

                        string sha1other;
                        if (!sha1sums.TryGetValue(other.Path, out sha1other))
                        {
                            using (FileStream fileStream = File.OpenRead(other.Path))
                                sha1other = fileStream.GetSHA1Sum();
                            sha1sums[other.Path] = sha1other;
                        }

                        bool sha1match = sha1item.Equals(sha1other, StringComparison.Ordinal);
                        if (queries.HasFlag(QueryType.EqualChecksumSHA1) && sha1match)
                            match |= QueryType.EqualChecksumSHA1;
                        if (queries.HasFlag(QueryType.EqualFileNameDifferentChecksumSHA1) && equalFilename && !sha1match)
                            match |= QueryType.EqualFileNameDifferentChecksumSHA1;
                    }

                    if (queries.HasFlag(QueryType.EqualChecksumSHA256) || queries.HasFlag(QueryType.EqualFileNameDifferentChecksumSHA256))
                    {
                        string sha256item;
                        if (!sha256sums.TryGetValue(item.Path, out sha256item))
                        {
                            using (FileStream fileStream = File.OpenRead(item.Path))
                                sha256item = fileStream.GetSHA256Sum();
                            sha256sums[item.Path] = sha256item;
                        }

                        string sha256other;
                        if (!sha256sums.TryGetValue(other.Path, out sha256other))
                        {
                            using (FileStream fileStream = File.OpenRead(other.Path))
                                sha256other = fileStream.GetSHA256Sum();
                            sha256sums[other.Path] = sha256other;
                        }

                        bool sha256match = sha256item.Equals(sha256other, StringComparison.Ordinal);
                        if (queries.HasFlag(QueryType.EqualChecksumSHA256) && sha256match)
                            match |= QueryType.EqualChecksumSHA256;
                        if (queries.HasFlag(QueryType.EqualFileNameDifferentChecksumSHA256) && equalFilename && !sha256match)
                            match |= QueryType.EqualFileNameDifferentChecksumSHA256;
                    }

                    if (queries.HasFlag(QueryType.LowResolution) || queries.HasFlag(QueryType.EqualFileNameDifferentResolution))
                    {
                        if (RequireMetaKey(MetaKey.Width, item) && RequireMetaKey(MetaKey.Height, item))
                        {
                            //const int limit = 960 * 1280;
                            const int resolutionLimit = 1024 * 768;

                            int resolutionItem = (int)item.Data[MetaKey.Width] * (int)item.Data[MetaKey.Height];
                            if (queries.HasFlag(QueryType.LowResolution) && resolutionItem <= resolutionLimit)
                                match |= QueryType.LowResolution;

                            if (queries.HasFlag(QueryType.EqualFileNameDifferentResolution) && equalFilename)
                            {
                                if (RequireMetaKey(MetaKey.Width, other) && RequireMetaKey(MetaKey.Height, other))
                                {
                                    int resolutionOther = (int)other.Data[MetaKey.Width] * (int)other.Data[MetaKey.Height];
                                    if (resolutionItem != resolutionOther)
                                        match |= QueryType.EqualFileNameDifferentResolution;
                                }
                            }
                        }
                    }

                    if (match != QueryType.None)
                        matches.Add(Tuple.Create(other.Path, match));
                }

                if (matches.Count > 0)
                    allMatches[item.Path] = matches;
            }

            QuerySummary result = new QuerySummary();
            result.matches = allMatches;
            return result;
        }

        private bool RequireMetaKey(MetaKey key, params MetaData[] items)
        {
            bool result = true;
            foreach (MetaData item in items)
            {
                if (!item.Data.ContainsKey(key))
                {
                    Trace.WriteLine($"[MediaQuerier] required meta key \"{key}\" not found in file: {item.Path}");
                    result = false;
                }
            }

            return result;
        }

        public async Task<IEnumerable<MetaData>> GetMetaData(string sourcePath, bool recursive, params MetaType[] types)
        {
            try
            {
                MetaParserConfig config = new MetaParserConfig() { Recursive = recursive, FilterTypes = types };
                return await MetaParser.ParseAsync(sourcePath, config);
            }
            catch (MetaParseException ex)
            {
                throw new MediaQuerierException("Failed to parse meta data", ex);
            }
        }
    }
}