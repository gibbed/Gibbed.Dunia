/* Copyright (c) 2012 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.Dunia.FileFormats;
using NDesk.Options;

namespace DumpBinaryStrings
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            bool showHelp = false;

            var options = new OptionSet()
            {
                {
                    "h|help",
                    "show this message and exit", 
                    v => showHelp = v != null
                },
            };

            List<string> extras;

            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (showHelp == true || extras.Count != 1)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var targetPath = extras[0];

            var values = new List<string>();
            foreach (var inputPath in Directory.GetFiles(targetPath, "*.fcb", SearchOption.AllDirectories))
            {
                var bf = new BinaryResourceFile();
                using (var input = File.OpenRead(inputPath))
                {
                    Console.Error.WriteLine("Reading " + inputPath);
                    foreach (var value in BinaryResourceFileDumper.Dump(input))
                    {
                        if (values.Contains(value) == false)
                        {
                            values.Add(value);
                        }
                    }
                }
            }

            values.Sort();
            foreach (var value in values)
            {
                Console.WriteLine(value);
            }
        }
    }
}
