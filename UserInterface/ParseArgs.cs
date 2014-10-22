//
// ParseArgs.cs: Argument parser use by CLI (command line interface).
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
    public enum ArgType
    {
        Flag,
        Variable,
        Undefined
    }

    public class Arg
    {
        public ArgType Type;
        public string Key;
        public string Value;

        public override string ToString()
        {
            switch (Type)
            {
                case ArgType.Flag:
                    return String.Format("Arg {{ Flag: \"{0}\" }}", Key);
                case ArgType.Variable:
                    return String.Format("Arg {{ Variable: \"{0}\" = \"{1}\" }}", Key, Value);
                case ArgType.Undefined:
                    return String.Format("Arg {{ Undefined: \"{0}\" }}", Key);
                default:
                    return null;
            }

        }
    }

    public class ParseArgs
    {
        public static string KeyValueSeparator = "=";

        public static IEnumerable<Arg> Parse(string[] args, IEnumerable<string> flagKeys, IEnumerable<string> variableKeys)
        {
            List<Arg> result = new List<Arg>();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                // Flag
                if (flagKeys.Contains(arg))
                {
                    Arg flagArg = new Arg();
                    flagArg.Type = ArgType.Flag;
                    flagArg.Key = arg;
                    flagArg.Value = arg;
                    result.Add(flagArg);
                    continue;
                }

                // Variable
                if (variableKeys.Contains(arg))
                {
                    Arg variableArg = new Arg();
                    variableArg.Type = ArgType.Variable;
                    variableArg.Key = arg;
                    if (i < args.Length - 1)
                    {
                        if (!variableKeys.Contains(args[i + 1]))
                        {
                            // Next argument is interpreted as value
                            i++;
                            arg = args[i];
                            variableArg.Value = arg;
                        }
                        else
                        {
                            // Next argument is a flag: no value defined for this variable
                            variableArg.Value = null;
                        }
                    }
                    else
                    {
                        // No value defined after variable's key
                        variableArg.Value = null;
                    }
                    result.Add(variableArg);
                    continue;
                }
                bool found = false;
                foreach (string variableKey in variableKeys)
                {
                    if (arg.StartsWith(variableKey + KeyValueSeparator))
                    {
                        // Key-value defined in same argument with separator
                        Arg variableArg = new Arg();
                        variableArg.Type = ArgType.Variable;
                        variableArg.Key = variableKey;
                        variableArg.Value = arg.Substring(variableKey.Length + KeyValueSeparator.Length);
                        result.Add(variableArg);
                        found = true;
                    }
                }
                if (found)
                    continue;

                // Undefined
                Arg undefinedArg = new Arg();
                undefinedArg.Type = ArgType.Undefined;
                undefinedArg.Key = arg;
                undefinedArg.Value = arg;
                result.Add(undefinedArg);
            }

            return result;
        }
    }
}
