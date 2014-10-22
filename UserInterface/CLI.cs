//
// CLI.cs: Command line interface (CLI) main class.
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

using ExifOrganizer.Organizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExifOrganizer.UI
{
    public class CLI
    {
        public CLI()
        {
            Console.WriteLine("ExifOrganizer CLI");
        }

        public int Run(string[] args)
        {
            IEnumerable<Arg> parsedArgs = ParseArgs.Parse(args, new string[] { "-r" }, new string[] { "-s", "-d" });
#if DEBUG
            Console.WriteLine("Arguments parsed:");
            foreach (Arg arg in parsedArgs)
                Console.WriteLine(String.Format(" * {0}", arg));
#endif



            string source = null;
            string destination = null;
            bool recursive = false;
            foreach (Arg arg in parsedArgs)
            {
                switch (arg.Key)
                {
                    case "-s":
                        source = arg.Value;
                        break;
                    case "-d":
                        destination = arg.Value;
                        break;
                    case "-r":
                        recursive = true;
                        break;
                }
            }

            MediaOrganizer organizer = new MediaOrganizer();
            organizer.sourcePath = source;
            organizer.destinationPath = destination;
            organizer.Recursive = recursive;
            //organizer.DuplicateMode = DuplicateMode.KeepAll;
            organizer.CopyMode = CopyMode.KeepUnique;
            //organizer.DestinationPatternImage = patternImage.Text;
            //organizer.DestinationPatternVideo = patternVideo.Text;
            //organizer.DestinationPatternAudio = patternAudio.Text;
            organizer.Localization = Thread.CurrentThread.CurrentUICulture;

            if (String.IsNullOrEmpty(source))
                throw new ArgumentException("No source path given (-s)");
            if (String.IsNullOrEmpty(destination))
                throw new ArgumentException("No destination path given (-d)");

            try
            {
                organizer.Parse();
                organizer.Organize();
                Console.WriteLine("Media organization completed successfully");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}
