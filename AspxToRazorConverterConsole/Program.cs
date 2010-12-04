using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Dlrsoft.AspxToRazorConverter;
using Dlrsoft.AspxToRazorConverter.Dom;

namespace Dlrsoft.AspxToRazorConverterConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PringUsage();
                return;
            }

            string path = args[0];
            FileSystemWalker walker = new FileSystemWalker();
            walker.Walk(path, p =>
                {
                    Converter converter = new Converter();
                    converter.Convert(p);
                });
            Console.WriteLine("Done");
            Console.WriteLine("Press any key to continue...");
            Console.Read();
        }

        static void PringUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("\tAspx2Razor path");
            Console.WriteLine("where:");
            Console.WriteLine("\tpath: path to a file or a directory");
            Console.WriteLine("Press any key to continue...");
            Console.Read();
        }

    }
}
