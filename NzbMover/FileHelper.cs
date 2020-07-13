using Alphaleonis.Win32.Filesystem;
using Nzb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
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
        public static string GetUniqueFilename(DirectoryInfo target, FileInfo source, bool allowDuplicates, string suffixToAdd = null)
        {
            string folder = target.FullName;
            string fileName = Path.GetFileNameWithoutExtension(source.FullName);
            string ext = Path.GetExtension(source.FullName).TrimStart('.');

            if (!string.IsNullOrWhiteSpace(suffixToAdd))
                fileName = string.Format("{0}{1}", fileName, suffixToAdd);

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

        /// <summary>
        /// tries to get the password, first from filename and then from nzb metainformation
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        public static string GetPassword(FileInfo fi, Configuration.ConfigSettings.PasswordExtractionMethod method)
        {
            if (method == Configuration.ConfigSettings.PasswordExtractionMethod.None)
                return null;

            // extract both passwords
            string pwFilename = GetFileNamePassword(fi.Name);
            string pwNzb = GetNzbPassword(fi.FullName);

            // use the method provided
            switch (method)
            {
                case Configuration.ConfigSettings.PasswordExtractionMethod.OnlyName:
                    return pwFilename;
                case Configuration.ConfigSettings.PasswordExtractionMethod.OnlyNzbMetadata:
                    return pwNzb;
                case Configuration.ConfigSettings.PasswordExtractionMethod.FilenameOverNzbMetadata:
                    if (!string.IsNullOrWhiteSpace(pwFilename))
                        return pwFilename;

                    return pwNzb;
                case Configuration.ConfigSettings.PasswordExtractionMethod.NzbMetadataOverFilename:
                    if (!string.IsNullOrWhiteSpace(pwNzb))
                        return pwNzb;

                    return pwFilename;
                default:
                    throw new NotImplementedException("Unknown password extraction method provided.");
            }
        }

        /// <summary>
        /// tries to get the password from filename
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileNamePassword(string fileName)
        {
            string regCode = @"\{\{(?<password>(.*))\}\}";
            string ret = Regex.Match(fileName, regCode).Groups["password"].Value;

            if (!string.IsNullOrWhiteSpace(ret))
                return ret;

            return null;
        }

        /// <summary>
        /// tries to get the password from nzb file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetNzbPassword(string filePath)
        {
            string ret = null;

            try
            {
                using (System.IO.FileStream fsNzb = File.OpenRead(filePath))
                {
                    Task<NzbDocument> tDoc = NzbDocument.Load(fsNzb);
                    tDoc.Wait();
                    NzbDocument nDoc = tDoc.Result;

                    if (nDoc.Metadata.Any(a => a.Key.Equals("password", StringComparison.OrdinalIgnoreCase)))
                        ret = nDoc.Metadata.First(f => f.Key.Equals("password", StringComparison.OrdinalIgnoreCase)).Value;
                }
            }
            catch { }

            if (!string.IsNullOrWhiteSpace(ret))
                return ret;

            return null;
        }
    }

    public class FileHelperAlreadyExists : Exception
    {
        public FileHelperAlreadyExists(string message) : base(message) { }
    }
}
