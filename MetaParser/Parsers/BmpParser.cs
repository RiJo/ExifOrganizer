//
// BmpParser.cs: Bitmap/DIB (device independent bitmap) meta parser class.
//
// Copyright (C) 2018 Rikard Johansson
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExifOrganizer.Meta.Parsers
{
    internal class BmpParser : FileParser
    {
        internal override IEnumerable<string> GetSupportedFileExtensions()
        {
            return new string[] { ".bmp", ".dib" };
        }

        internal override MetaType? GetMetaTypeByFileExtension(string extension)
        {
            if (GetSupportedFileExtensions().Contains(extension))
                return MetaType.Image;
            return base.GetMetaTypeByFileExtension(extension);
        }

        protected override MetaData ParseFile(Stream stream, MetaData meta)
        {
            byte[] bitmapHeader = new byte[14];
            if (stream.Read(bitmapHeader, 0, bitmapHeader.Length) != bitmapHeader.Length)
                throw new MetaParseException("Unable to read full bitmap header data");

            string signature = Convert.ToString((char)bitmapHeader[0]) + Convert.ToString((char)bitmapHeader[1]);
            meta.Data[MetaKey.MetaType] = $"DIB ({signature})";

            //int fileSize = BitConverter.ToInt32(bitmapHeader, 4);
            //int offsetImageData = BitConverter.ToInt32(bitmapHeader, 10);


            byte[] dibHeaderSizeData = new byte[4];
            if (stream.Read(dibHeaderSizeData, 0, dibHeaderSizeData.Length) != dibHeaderSizeData.Length)
                throw new MetaParseException("Unable to read full DIB header size");

            int dibHeaderSize = BitConverter.ToInt32(dibHeaderSizeData, 0);

            byte[] dibHeader = new byte[dibHeaderSize - 4];
            if (stream.Read(dibHeader, 0, dibHeader.Length) != dibHeader.Length)
                throw new MetaParseException("Unable to read full DIB header data");

            if (dibHeaderSize == 12)
            {
                // OS/2 1.x BITMAPCOREHEADER

                int bitmapWidth = BitConverter.ToUInt16(dibHeader, 0);
                meta.Data[MetaKey.Width] = bitmapWidth;

                int bitmapHeight = BitConverter.ToUInt16(dibHeader, 2);
                meta.Data[MetaKey.Height] = bitmapHeight;

                //int colorPlanes = BitConverter.ToUInt16(dibHeader, 4);
                //int bitsPerPixel = BitConverter.ToUInt16(dibHeader, 6);
            }
            else if (dibHeaderSize == 40)
            {
                // Windows BITMAPINFOHEADER

                int bitmapWidth = BitConverter.ToInt32(dibHeader, 0);
                meta.Data[MetaKey.Width] = bitmapWidth;

                int bitmapHeight = BitConverter.ToInt32(dibHeader, 4);
                meta.Data[MetaKey.Height] = bitmapHeight;

                //int colorPlanes = BitConverter.ToUInt16(dibHeader, 8);
                //int bitsPerPixel = BitConverter.ToUInt16(dibHeader, 10);
                //int compression = BitConverter.ToInt32(dibHeader, 12);
                //int imageSize = BitConverter.ToInt32(dibHeader, 16);
                //int horizontalResolution = BitConverter.ToInt32(dibHeader, 20);
                //int verticalResolution = BitConverter.ToInt32(dibHeader, 24);
                //int colorsInPalette = BitConverter.ToInt32(dibHeader, 28);
                //int importantColors = BitConverter.ToInt32(dibHeader, 32);

            }
            else
            {
                throw new NotImplementedException($"[BmpParser] unhandled DIB header size: {dibHeaderSize}");
            }

            meta.Data[MetaKey.Resolution] = $"{meta.Data[MetaKey.Width]}x{meta.Data[MetaKey.Height]}";


            return meta;
        }
    }
}
