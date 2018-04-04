//
// Program.cs: Program main function entry point.
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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ExifOrganizer.UI
{
    internal static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        private const int ATTACH_PARENT_PROCESS = -1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                return GUI(args);
            }
            else
            {
                return CLI(args);
            }
        }

        private static int GUI(string[] args)
        {
            // Graphical interface
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
            return 0;
        }

        private static int CLI(string[] args)
        {
            // Make Console.Write() work
            AttachConsole(ATTACH_PARENT_PROCESS);

            // Command line interface
            CLI cli = new CLI();
            return cli.Run(args);
        }
    }
}