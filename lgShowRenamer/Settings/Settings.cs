using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace lgShowRenamer.Settings
{
    public class Settings
    {
        public class TVShows
        {
            #region Declaration
            private string m_destDir = "/Volumes/TV Shows/";
            private string m_sDirPrefix = "Season ";
            private string m_sDirFormat = "00";
            private uint m_minLevenshteinDistance = 2;
            #endregion

            #region Properties
            public string DestDir { get { return m_destDir; } set { if (string.IsNullOrWhiteSpace(value)) return; m_destDir = value; if (!m_destDir.EndsWith("/", StringComparison.InvariantCultureIgnoreCase)) m_destDir += "/"; } }
            public string SeasonDirPrefix { get { return m_sDirPrefix; } set { if (value == null) return; m_sDirPrefix = value; } }
            public string SeasonDirIndexFormat { get { return m_sDirFormat; } set { if (value == null) return; m_sDirFormat = value; } }
            public uint MinLevenshteinDistance { get { return m_minLevenshteinDistance; } set { m_minLevenshteinDistance = value; } }
            #endregion
        }

        #region Declaration
        private string m_sourceDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "Transmission") + "/";
        private List<string> m_extensions = new List<string>(new string[] { ".mp4", ".m4v", ".avi", ".mkv", ".mov", ".mpg", ".mpeg" , ".webm" });
        private List<string> m_invSubDir = new List<string>(new string[] { "tmp" });
        private TVShows m_shows = new TVShows();
        #endregion

        #region Properties
        public string SourceDir { get { return m_sourceDir; } set { if (string.IsNullOrWhiteSpace(value)) return; m_sourceDir = value; if (!m_sourceDir.EndsWith("/", StringComparison.InvariantCultureIgnoreCase)) m_sourceDir += "/"; } }
        public List<string> Extensions 
        { 
            get { return m_extensions; } 
            set
            {
                if (value == null)
                    return;
                m_extensions = value;
                for (int i = 0; i < m_extensions.Count; ++i)
                    m_extensions[i] = m_extensions[i].ToLower().Trim();
            } 
        }
        public List<string> InvalidSubDir { get { return m_invSubDir; } set { if (value == null) return; m_invSubDir = value; } }
        public TVShows Shows { get { return m_shows; } }
        #endregion

        #region Instance
        private static volatile Settings m_instance = null;
        public static Settings Instance 
        {
            get 
            {
                if (m_instance == null)
                    m_instance = new Settings();
                return m_instance;
            }
        }
        #endregion

        #region File Managing
        static public void SaveInstance(string sPath)
        {
            string sJson = JsonConvert.SerializeObject(Instance, Formatting.Indented);
            if (string.IsNullOrWhiteSpace(sJson))
                return;
            using (StreamWriter writer = new StreamWriter(sPath, false, Encoding.Unicode))
                writer.Write(sJson);
        }

        static public void LoadInstance(string sPath)
        {
            string sJson;
            using (StreamReader reader = new StreamReader(sPath, Encoding.Unicode))
                sJson = reader.ReadToEnd();
            if (string.IsNullOrWhiteSpace(sJson))
                return;
            Settings obj = JsonConvert.DeserializeObject(sJson) as Settings;
            if (obj != null)
                m_instance = obj;
        }
        #endregion
    }
}

