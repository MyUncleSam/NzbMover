using Alphaleonis.Win32.Filesystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NzbMover
{
    public static class FileHelper
    {
        /// <summary>
        /// gets a unique target path to copy files to (with allow duplicates options
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <param name="allowDuplicates"></param>
        /// <exception cref="FileHelperAlreadyExists"></exception>
        /// <returns></returns>
        public static string GetUniqueFilename(DirectoryInfo target, FileInfo source, bool allowDuplicates)
        {
            string folder = target.FullName;
            string fileName = Path.GetFileNameWithoutExtension(source.FullName);
            string ext = Path.GetExtension(source.FullName).TrimStart('.');

            string fullFileName = System.IO.Path.Combine(folder, string.Format("{0}.{1}", fileName, ext));

            if(File.Exists(fullFileName))
            {
                if (!allowDuplicates)
                    throw new FileHelperAlreadyExists(string.Format("Target already contains the file '{0}'.", Path.GetFileName(fullFileName)));

                // now iterate through the files till we have a unique filename
                int counter = 1;

                do
                {
                    fullFileName = System.IO.Path.Combine(folder, string.Format("{1} - {0}.{2}", fileName, counter++, ext));
                } while (File.Exists(fullFileName));
            }

            return fullFileName;
        }

        /// <summary>
        /// get the path of the exeting program
        /// </summary>
        /// <returns></returns>
        public static string GetExePath()
        {
            return new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory.FullName;
        }
    }

    public class FileHelperAlreadyExists : Exception
    {
        public FileHelperAlreadyExists(string message) : base(message) { }
    }
}
