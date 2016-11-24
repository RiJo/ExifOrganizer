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

using ExifOrganizer.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ExifOrganizer.Querier
{
	public enum QueryType
	{
		Duplicates,
		LowResolution
	}

	[Flags]
	public enum DuplicateComparator
	{
		None = 0x00,
		FileName = 0x01,
		FileSize = 0x02,
		ChecksumMD5 = 0x04,
		ChecksumSHA1 = 0x08
	}

	public class QuerySummary
	{
		public string[] parsed;
		public string[] ignored;
		public string[] totalFiles;
		public string[] totalDirectories;

		public override string ToString()
		{
			return String.Format("Summary {{ Parsed: {0} (Ignored: {1}), Total files: {2}, Total directories: {3} }}",
				parsed != null ? parsed.Length : 0,
				ignored != null ? ignored.Length : 0,
				totalFiles != null ? totalFiles.Length : 0,
				totalDirectories != null ? totalDirectories.Length : 0
			);
		}
	}

	public class QueryDuplicateSummary : QuerySummary
	{
		public Dictionary<string, IEnumerable<Tuple<string, DuplicateComparator>>> duplicates;
	}

	public class MediaQuerier
	{
		public event Action<MediaQuerier, double, string> OnProgress = delegate { };

		public MediaQuerier()
		{
		}

		public QueryDuplicateSummary QueryDuplicates(string sourcePath, bool recursive, DuplicateComparator comparator)
		{
			Task<QueryDuplicateSummary> task = QueryDuplicatesAsync(sourcePath, recursive, comparator);
			task.ConfigureAwait(false); // Prevent deadlock of caller
			return task.Result;
		}

		public async Task<QueryDuplicateSummary> QueryDuplicatesAsync(string sourcePath, bool recursive, DuplicateComparator comparator)
		{
			Dictionary<string, IEnumerable<Tuple<string, DuplicateComparator>>> duplicates = new Dictionary<string, IEnumerable<Tuple<string, DuplicateComparator>>>();
			Dictionary<string, string> md5sums = new Dictionary<string, string>();
			Dictionary<string, string> sha1sums = new Dictionary<string, string>();
			IEnumerable<MetaData> data = await GetMetaData(sourcePath, recursive);
			foreach (MetaData item in data.Where(x => x.Type != MetaType.Directory))
			{
				List<Tuple<string, DuplicateComparator>> matches = new List<Tuple<string, DuplicateComparator>>();
				foreach (MetaData other in data.Where(x => x.Type != MetaType.Directory && !x.Equals(item)))
				{
					DuplicateComparator match = DuplicateComparator.None;
					if (comparator.HasFlag(DuplicateComparator.FileName))
					{
						if (Path.GetFileName(item.Path).Equals(Path.GetFileName(other.Path), StringComparison.Ordinal /* TODO: which type? */))
							match |= DuplicateComparator.FileName;
					}

					if (comparator.HasFlag(DuplicateComparator.FileSize))
					{
						FileInfo itemInfo = new FileInfo(item.Path);
						FileInfo otherInfo = new FileInfo(other.Path);

						// Special case: look for files with same name but different sizes
						if (comparator == (DuplicateComparator.FileName | DuplicateComparator.FileSize))
						{
							if (match.HasFlag(DuplicateComparator.FileName) && itemInfo.Length != otherInfo.Length)
								matches.Add(Tuple.Create(other.Path, match | DuplicateComparator.FileSize));
							continue;
						}

						if (itemInfo.Length == otherInfo.Length)
							match |= DuplicateComparator.FileSize;
					}

					if (comparator.HasFlag(DuplicateComparator.ChecksumMD5))
					{
						if (comparator.HasFlag(DuplicateComparator.FileSize) && !match.HasFlag(DuplicateComparator.FileSize))
						{
							// Optimization: different sizes must, most of the time, cause different checksums
							comparator |= DuplicateComparator.ChecksumMD5;
						}
						else
						{
							string md5item;
							if (!md5sums.TryGetValue(item.Path, out md5item))
							{
								md5item = GetMD5(item.Path);
								md5sums[item.Path] = md5item;
							}

							string md5other;
							if (!md5sums.TryGetValue(other.Path, out md5other))
							{
								md5other = GetMD5(other.Path);
								md5sums[other.Path] = md5other;
							}

							if (md5item.Equals(md5other, StringComparison.Ordinal))
								match |= DuplicateComparator.ChecksumMD5;
						}
					}

					if (comparator.HasFlag(DuplicateComparator.ChecksumSHA1))
					{
						if (comparator.HasFlag(DuplicateComparator.FileSize) && !match.HasFlag(DuplicateComparator.FileSize))
						{
							// Optimization: different sizes must, most of the time, cause different checksums
							comparator |= DuplicateComparator.ChecksumSHA1;
						}
						else
						{
							string sha1item;
							if (!sha1sums.TryGetValue(item.Path, out sha1item))
							{
								sha1item = GetSHA1(item.Path);
								sha1sums[item.Path] = sha1item;
							}

							string sha1other;
							if (!sha1sums.TryGetValue(other.Path, out sha1other))
							{
								sha1other = GetSHA1(other.Path);
								sha1sums[other.Path] = sha1other;
							}

							if (sha1item.Equals(sha1other, StringComparison.Ordinal))
								match |= DuplicateComparator.ChecksumSHA1;
						}
					}

					if (match != DuplicateComparator.None)
						matches.Add(Tuple.Create(other.Path, match));
				}

				if (matches.Count > 0)
					duplicates[item.Path] = matches;
			}

			QueryDuplicateSummary result = new QueryDuplicateSummary();
			result.duplicates = duplicates;
			return result;
		}

		private static string GetMD5(string filename)
		{

			using (FileStream stream = File.OpenRead(filename))
			{
				using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
				{
					using (MD5 md5 = MD5.Create())
					{
						byte[] checksum = md5.ComputeHash(bufferedStream);
						return BitConverter.ToString(checksum).Replace("-", String.Empty);
					}
				}
			}
		}

		private static string GetSHA1(string filename)
		{

			using (FileStream stream = File.OpenRead(filename))
			{
				using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
				{
					using (SHA1 sha1 = SHA1.Create())
					{
						byte[] checksum = sha1.ComputeHash(bufferedStream);
						return BitConverter.ToString(checksum).Replace("-", String.Empty);
					}
				}
			}
		}

		public async Task<IEnumerable<MetaData>> GetMetaData(string sourcePath, bool recursive)
		{
			try
			{
				return await MetaParser.ParseAsync(sourcePath, recursive);
			}
			catch (MetaParseException ex)
			{
				throw new MediaQuerierException("Failed to parse meta data", ex);
			}
		}
	}
}
