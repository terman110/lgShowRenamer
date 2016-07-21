using System;
using System.Collections.Generic;
using System.IO;

namespace lgShowRenamer.TVShows
{
    static public class Parser
    {
        #region Settings
        static private lgShowRenamer.Settings.Settings Settings { get { return lgShowRenamer.Settings.Settings.Instance; } }
        #endregion

        static public List<Episode> GetFiles()
        {
            if (Settings == null || string.IsNullOrWhiteSpace(Settings.SourceDir) || !Directory.Exists(Settings.SourceDir))
                return new List<Episode>();

            List<string> lAll = new List<string>(Directory.GetFiles(Settings.SourceDir, "*", SearchOption.AllDirectories));
            if (lAll == null || lAll.Count < 1)
                return new List<Episode>();

            List<string> lFiles;
            try { lFiles = lAll.FindAll(x => Settings.Extensions.Contains(Path.GetExtension(x))); }
            catch { return new List<Episode>(); }
            finally { lAll.Clear(); }

            List<Episode> lEpisodes = new List<Episode>(lFiles.Count);

            List<string> lDirs;
            for (int i = 0; i < lFiles.Count; ++i)
            {
                lFiles[i] = lFiles[i].Replace(Settings.SourceDir, string.Empty);

                lDirs = new List<string>(Path.GetDirectoryName(lFiles[i]).Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries));
                foreach (string inv in Settings.InvalidSubDir)
                    if (lDirs.Contains(inv))
                        continue;

                Episode ep = new Episode(lFiles[i]);
                if (ep == null || !ep.IsValid)
                    continue;
                
                lEpisodes.Add(ep);
                
            }
            return lEpisodes;
        }
    }
}

