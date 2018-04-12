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

using ExifOrganizer.Common;
using System;
using System.IO;
using System.Security.Cryptography;

namespace ExifOrganizer.Organizer
{
    public static class ExtensionMethods
    {
        public static bool FileExistsInDirectory(this FileInfo fileInfo, DirectoryInfo directory, FileComparator comparator)
        {
            foreach (FileInfo tempFile in directory.GetFiles())
            {
                if (tempFile.AreFilesIdentical(fileInfo, comparator))
                    return true;
            }
            return false;
        }

        public static bool AreFilesIdentical(this FileInfo fileInfo, FileInfo otherFile, FileComparator comparator)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));
            if (otherFile == null)
                throw new ArgumentNullException(nameof(otherFile));
            if (!fileInfo.Exists)
                return false;
            if (!otherFile.Exists)
                return false;

            bool identical = true;
            if (identical && comparator.HasFlag(FileComparator.FileSize))
                identical &= (fileInfo.Length == otherFile.Length);
            if (identical && comparator.HasFlag(FileComparator.ChecksumMD5))
                identical &= fileInfo.GetMD5Sum() == otherFile.GetMD5Sum();
            if (identical && comparator.HasFlag(FileComparator.ChecksumSHA1))
                identical &= fileInfo.GetSHA1Sum() == otherFile.GetSHA1Sum();
            if (identical && comparator.HasFlag(FileComparator.ChecksumSHA256))
                identical &= fileInfo.GetSHA256Sum() == otherFile.GetSHA256Sum();
            if (identical && comparator.HasFlag(FileComparator.Created))
                identical &= (fileInfo.CreationTimeUtc == otherFile.CreationTimeUtc);
            if (identical && comparator.HasFlag(FileComparator.Modified))
                identical &= (fileInfo.LastWriteTimeUtc == otherFile.LastWriteTimeUtc);
            return identical;
        }

        public static string GetGroupTypeText(this GroupType groupType)
        {
            switch (groupType)
            {
                case GroupType.MonthName: return "Name of month";
                case GroupType.MonthNumber: return "Month";
                case GroupType.DayName: return "Name of day";
                case GroupType.DayNumber: return "Day";
                case GroupType.OriginalName: return "Original file name";
                case GroupType.FileName: return "File name";
                case GroupType.FileExtension: return "File extension";
                default: return groupType.ToString();
            }
        }
    }
}