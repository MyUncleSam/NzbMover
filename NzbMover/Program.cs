using System.IO;
using IniParser;
using IniParser.Model;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace NzbMover
{
    class Program
    {
        static void Main(string[] args)
        {
            Output output = new Output();
            Configuration conf = null;
            Version version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;

            output.WriteLine(Output.OutputType.Info, "Program version: {0}", version.ToString());

            try
            {
                // first of all read the config file
                output.WriteStart(Output.OutputType.Info, "Reading config file ... ");

                FileIniDataParser iniParser = new FileIniDataParser();
                iniParser.Parser.Configuration.CommentString = "#";
                iniParser.Parser.Configuration.ThrowExceptionsOnError = true;
                IniData iniConf = iniParser.ReadFile(System.IO.Path.Combine(FileHelper.GetExePath(), "nzbmover.cfg"), Encoding.UTF8);
                conf = new Configuration(iniConf);

                output.WriteEnd(Output.OutputType.Success, "DONE.");
            }
            catch (Exception ex)
            {
                output.WriteEnd(Output.OutputType.Error, "ERROR.");
                output.WriteException("Error reading configuration file.", ex, true);
                output.WaitForKeyPressExit();
                return;
            }

            try 
            {
                FileInfo fi = null;

                try
                {
                    // get file to move/copy
                    output.WriteStart(Output.OutputType.Info, "Getting file to work with ... ");

                    if (args.Length != 1)
                    {
                        output.WriteEnd(Output.OutputType.Error, "ERROR.");
                        throw new System.IO.FileNotFoundException("No file specified.");
                    }

                    fi = new FileInfo(args[0]);
                    output.WriteEnd(Output.OutputType.Success, "DONE ({0}).", fi.Name);
                }
                catch (Exception fileReadEx)
                {
                    output.WriteException(fileReadEx, conf.General.debug);
                    output.WaitForKeyPressExit();
                    return;
                }

                // password section
                string suffixToAdd = null;
                output.WriteStart(Output.OutputType.Info, "Extracting password ... ");
                string password = FileHelper.GetPassword(fi, conf.Settings.password_extraction_method);

                if (string.IsNullOrWhiteSpace(password))
                    output.WriteEnd(Output.OutputType.Warn, "no password found.");
                else
                    output.WriteEnd(Output.OutputType.Success, "DONE ({0})", password);

                if (conf.Settings.ask_for_password && string.IsNullOrWhiteSpace(password))
                {
                    output.WriteStart(Output.OutputType.Warn, "Set password (or press enter for none): ");
                    Console.ResetColor(); // reset color to have white input for the user
                    string newPassword = Console.ReadLine().Trim();

                    if (!string.IsNullOrWhiteSpace(newPassword))
                    {
                        password = newPassword;
                        suffixToAdd = string.Format("{{{{{0}}}}}", newPassword); // create the password suffix {{pw}}
                    }
                    else
                        output.WriteLine(Output.OutputType.Warn, "No password set.");
                }

                // begin the action
                try {
                    // target filename
                    string targetFileName = FileHelper.GetFilenameLimitedSize(fi, conf.Settings.file_name_max_length, password);

                    // get target path
                    string targetFilePath = FileHelper.GetUniqueFilename(conf.Settings.target, targetFileName, conf.Settings.allow_duplicates, suffixToAdd);

                    // time for some action
                    if(conf.Settings.action == Configuration.ConfigSettings.FileAction.MOVE)
                    {
                        try
                        {
                            output.WriteStart(Output.OutputType.Info, "Moving file ... ");
                            File.Move(fi.FullName, targetFilePath);
                            output.WriteEnd(Output.OutputType.Success, "DONE.");
                        }
                        catch(Exception ex)
                        {
                            output.WriteEnd(Output.OutputType.Error, "ERROR (enable debug for details).");
                            conf.Settings.action = Configuration.ConfigSettings.FileAction.COPY;

                            if (conf.General.debug)
                                output.WriteException(ex, conf.General.debug);
                        }
                    }

                    if(conf.Settings.action == Configuration.ConfigSettings.FileAction.COPY)
                    {
                        // copy file (try to even copy it if file is locked by another application)
                        using (System.IO.FileStream fsSource = File.Open(fi.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                        {
                            using (System.IO.FileStream fsTarget = new System.IO.FileStream(targetFilePath, System.IO.FileMode.Create))
                            {
                                byte[] buffer = new byte[0x10000];
                                int bytes;

                                while ((bytes = fsSource.Read(buffer, 0, buffer.Length)) > 0)
                                    fsTarget.Write(buffer, 0, bytes);
                            }
                        }
                    }

                    // successfully finished
                    output.WriteLine(Output.OutputType.Success, "All done.");

                    if (conf.Settings.close_success <= 0)
                        return;

                    output.WriteEmptyLine();
                    output.WriteLine(Output.OutputType.None, "Closing in {0} seconds", conf.Settings.close_success);
                    System.Threading.Thread.Sleep(conf.Settings.close_success * 1000);
                    return;
                }
                catch(Exception operationEx)
                {
                    output.WriteException("Error performing action.", operationEx, conf.General.debug);
                    output.WaitForKeyPressExit();
                    return;
                }
                
            }
            catch(Exception ex)
            {
                output.WriteException("UNKNOWN ERROR", ex, true);
                output.WaitForKeyPressExit();
                return;
            }
        }
    }
}
