using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace lgShowRenamer.TVShows
{
    public class Episode
    {
        public Episode(string sPath)
        {
            Parse(sPath);
        }

        #region Settings
        static private lgShowRenamer.Settings.Settings Settings { get { return lgShowRenamer.Settings.Settings.Instance; } }
        #endregion

        #region Helper
        // Find S01E01
        static private Regex m_idxRegex = new Regex("S([0-9]{1,2})E([0-9]{1,2})", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        static private List<string> m_dstDir = null;

        static protected Regex IdxRegex { get { return m_idxRegex; } }
        static private List<string> DestDirs
        {
            get
            {
                if(m_dstDir == null)
                {
                    if (!Directory.Exists(Settings.Shows.DestDir))
                        return null;
                    m_dstDir = new List<string>(Directory.GetDirectories(Settings.Shows.DestDir)); // , "*", SearchOption.TopDirectoryOnly));
                    if (m_dstDir == null)
                        return null;
                    for (int i = 0; i < m_dstDir.Count; ++i)
                        m_dstDir[i] = m_dstDir[i].Replace(Settings.Shows.DestDir, string.Empty);
                }
                return m_dstDir;
            }
        }
        #endregion

        #region Properties
        public bool IsValid { get; protected set; }

        public string SourcePath { get; protected set; }

        public string ShowName { get; protected set; }
        public int SeasonNo { get; protected set; }
        public int EpisodeNo { get; protected set; }
        //public int Title { get; protected set; }
        #endregion

        public void Parse(string sPath) 
        {
            IsValid = false;
            try
            {
                if (string.IsNullOrWhiteSpace(sPath))
                    return;

                string sFileName = Path.GetFileNameWithoutExtension(sPath);
                if (string.IsNullOrWhiteSpace(sPath))
                    return;

                Match idx = IdxRegex.Match(sFileName);
                if (idx.Groups.Count < 3)
                    return;

                int nTmp;
                if (!int.TryParse(idx.Groups[1].Value, out nTmp))
                    return;
                SeasonNo = nTmp;

                if (!int.TryParse(idx.Groups[2].Value, out nTmp))
                    return;
                EpisodeNo = nTmp;

                ShowName = sFileName.Substring(0, idx.Index);
                ShowName = ShowName.Replace('.', ' ').Trim();
                ShowName = new CultureInfo("en-US", false).TextInfo.ToTitleCase(ShowName);
                if (string.IsNullOrEmpty(ShowName) || ShowName.Length < 2)
                    return;
            }
            catch { return; }

            SourcePath = sPath;
            IsValid = true;
        }

        public string GetDestPath()
        {
            if (!IsValid)
                return null;

            string sExistingDir;
            try { sExistingDir = DestDirs.Find(x => x.StartsWith(ShowName, StringComparison.InvariantCultureIgnoreCase)); }
            catch { sExistingDir = null; }
            if(sExistingDir == null)
            {
                foreach (string existing in DestDirs)
                {
                    string sNeedle = existing;
                    int vI = sNeedle.IndexOf(" (", StringComparison.InvariantCultureIgnoreCase);
                    if (vI > 0)
                        sNeedle = sNeedle.Substring(0, vI);
                    if(Helper.LevenshteinDistance.Compute(sNeedle, ShowName) <= Settings.Shows.MinLevenshteinDistance)
                    {
                        sExistingDir = existing;
                        break;
                    }
                }                      
            }
            if (string.IsNullOrWhiteSpace(sExistingDir))
                sExistingDir = ShowName;
            
            return Path.Combine(sExistingDir, Settings.Shows.SeasonDirPrefix + SeasonNo.ToString(Settings.Shows.SeasonDirIndexFormat), ShowName + " S" + SeasonNo.ToString("00") + "E" + EpisodeNo.ToString("00") + Path.GetExtension(SourcePath));
        }
    }
}

