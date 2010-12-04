using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Dlrsoft.AspxToRazorConverter.Dom;

namespace Dlrsoft.AspxToRazorConverter
{
    public class Converter
    {
        public void Convert(string infile)
        {
            string ext = Path.GetExtension(infile).ToLower();
            if (".master".Equals(ext) || ".aspx".Equals(ext) || ".ascx".Equals(ext))
            {
                Trace.WriteLine("Converting " + infile + "...");
                int x = infile.LastIndexOf(".");
                string outfile = infile.Substring(0, x) + ".cshtml";
                AspxParser parser = new AspxParser();
                AspxDocument doc = parser.Parse(File.OpenText(infile));
                RazorGenerator generator = new RazorGenerator();
                generator.Generate(doc, outfile);
                Trace.WriteLine("Saved to " + outfile);
            }
        }
    }
}
