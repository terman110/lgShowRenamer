using System;
using System.Collections.Generic;
using System.IO;

using lgShowRenamer;

namespace lgShowRenamer
{
    class MainClass
    {
        private static bool AskQuestion(string sMessage, string sCaption, bool bDefault = false)
        {
            List<string> lContent = new List<string>(sCaption.Split(new string[] { "\n", "\r", "\r\n", "\n\r" }, StringSplitOptions.None));
            lContent.Add("===");
            lContent.Add("");
            lContent.AddRange(new List<string>(sMessage.Split(new string[] { "\n", "\r", "\r\n", "\n\r" }, StringSplitOptions.None)));
            lContent.Add("");
            lContent.Add("Answer with [y]es or [n]o. Default is " + (bDefault ? "yes" : "no") + ".");
            Console.WriteLine("+----");
            foreach (string line in lContent)
                Console.WriteLine("| " + line);
            Console.WriteLine("+----");

            ConsoleKeyInfo key = Console.ReadKey();
            if (key.ToString().Length < 1)
                return bDefault;
            switch(key.KeyChar)
            {
                case 'y':
                case 'Y':
                case 'j':
                case 'J':
                    return true;
                case 'n':
                case 'N':
                    return false;
                default:
                    return bDefault;
            }
        }
        
        public static void Main(string[] args)
        {
            string sCfgFile = Path.Combine(Environment.CurrentDirectory, "settings.json");
            if (File.Exists(sCfgFile))
            {
                try { Settings.Settings.LoadInstance(sCfgFile); }
                catch { Console.WriteLine("ERROR: Unable to load settings file \"" + sCfgFile + "\"."); }
            }

            Console.WriteLine("Retrieving TV Shows from \"" + Settings.Settings.Instance.SourceDir + "\" ...");
            List<TVShows.Episode> lShowFiles = TVShows.Parser.GetFiles();


            bool bSerious = AskQuestion("Start moving files?", "Moving Files");
            bool? bDeleteSource = null;

            string sDest, sDestPath;
            foreach(TVShows.Episode ep in lShowFiles)
            {
                Console.WriteLine("+++");
                Console.WriteLine(ep.ShowName + " S" + ep.SeasonNo.ToString("00") + "E" + ep.EpisodeNo.ToString("00"));
                sDest = ep.GetDestPath();
                sDestPath = Path.Combine(Settings.Settings.Instance.Shows.DestDir, sDest);

                // Replace?
                if(File.Exists(sDestPath))
                {
                    if (!AskQuestion("File \"" + sDest + "\" already exists.\nReplace the file?", "Replace File"))
                        continue;
                    Console.WriteLine("  Deleting existing file at destination ...");
                    try { if (bSerious) File.Delete(sDestPath); }
                    catch (Exception ex)
                    {
                        Console.WriteLine("  ERROR: Unable to delete \"" + sDest + "\" (" + ex.Message + ")");
                        continue;
                    }
                }

                // Create directory?
                if (!Directory.Exists(Path.GetDirectoryName(sDestPath)))
                {
                    if (!AskQuestion("Directory \"" + Path.GetDirectoryName(sDestPath) + "\" does not exist.\nCreate it?", "Create Directory"))
                        continue;
                    Console.WriteLine("  Creating destination directories \"" + Path.GetDirectoryName(sDestPath) + "\" ...");
                    try { if (bSerious) Directory.CreateDirectory(Path.GetDirectoryName(sDestPath)); }
                    catch (Exception ex)
                    {
                        Console.WriteLine("  ERROR: Unable to create directory \"" + Path.GetDirectoryName(sDestPath) + "\" (" + ex.Message + ")");
                        continue;
                    }
                }

                if (bDeleteSource == null)
                    bDeleteSource = AskQuestion("Delete source files?", "Delete Source");

                Console.WriteLine("  Copying from \"" + ep.SourcePath + "\" to \"" + sDest + "\" ...");
                try { if (bSerious) File.Copy(Path.Combine(Settings.Settings.Instance.SourceDir, ep.SourcePath), sDestPath); }
                catch (Exception ex)
                {
                    Console.WriteLine("  ERROR: Unable to copy source file to destination (" + ex.Message + ")");
                    continue;
                }

                if(bDeleteSource.Value)
                {
                    Console.WriteLine("  Deleting source file ...");
                    try { if (bSerious) File.Delete(Path.Combine(Settings.Settings.Instance.SourceDir, ep.SourcePath)); }
                    catch (Exception ex)
                    {
                        Console.WriteLine("  ERROR: Unable to delete source file \"" + ep.SourcePath + "\" (" + ex.Message + ")");
                        continue;
                    }
                }
            }

            try { Settings.Settings.SaveInstance(sCfgFile); }
            catch { Console.WriteLine("ERROR: Unable to save settings file \"" + sCfgFile + "\"."); }
        }
    }
}
