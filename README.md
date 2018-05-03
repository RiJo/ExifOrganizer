ExifOrganizer
=============

Organize image collections based on Exif data with customizable destination paths.
### Status
Currently in development of first beta version.

## Description
The idea is to select a source path to be organized, where media files (mainly images, but also videos and audio) are gathered from. Those files are then copied and structured into a destination path. How the media files are organized is configurable via a graphical user interface (GUI) or by command line interface (CLI) for scripting.

### Supported meta
* Exif - .jpeg .jpg .tiff .tif
* MP4 - .mp4 .m4a .mov .3gp .3g2
* ID3 - .mp3
* PNG - .png

Most other media files are parsed in a generic way and lacks the detailed meta data.

### Screenshot
![ExifOrganizer GUI](screenshot.png)

## Environment
Everything is written in C# 6.0 and .NET 4.5 using Visual Studio 2017. There are no external dependencies.

## License
This project is licensed under GPLv3.

