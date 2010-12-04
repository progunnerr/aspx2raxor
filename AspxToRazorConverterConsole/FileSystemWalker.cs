using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Dlrsoft.AspxToRazorConverterConsole
{
    public class FileSystemWalker
    {
        public void Walk(string path, Action<string> fileHandler)
        {
            FileAttributes fa = File.GetAttributes(path);
            if ((fa & FileAttributes.Directory) == FileAttributes.Directory)
            {
                Walk(new DirectoryInfo(path), fileHandler);
            }
            else
            {
                Walk(new FileInfo(path), fileHandler);
            }
        }

        private void Walk(DirectoryInfo di, Action<string> fileHandler)
        {
            foreach (FileInfo fi in di.GetFiles())
            {
                Walk(fi, fileHandler);
            }

            foreach (DirectoryInfo cdi in di.GetDirectories())
            {
                Walk(cdi, fileHandler);
            }
        }

        private void Walk(FileInfo fi, Action<string> fileHandler)
        {
            fileHandler(fi.FullName);
        }
    }
}
