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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExifOrganizer.UI
{
    public class CLI
    {
        public CLI()
        {
            Console.WriteLine("ExifOrganizer CLI");
        }

        public void Run(string[] args)
        {
            System.Diagnostics.Debugger.Launch();
            foreach (Arg arg in ParseArgs.Parse(args, new string[] { "-v" }, new string[] { "-s", "-d" }))
                Console.WriteLine(String.Format(" * {0}", arg));
        } 
    }
}
