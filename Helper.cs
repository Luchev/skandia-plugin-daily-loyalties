using Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace DailyLoyalties
{
    public static class H
    {
        private static XmlSerializer serializer;
        public static void Log(string message, bool isDetailed = false)
        {
            if (!isDetailed || Main.settings.DetailedLogs)
                Skandia.MessageLog("[DL]" + message);
            else return;
        }
        internal class DailyCounter
        {
            public DayOfWeek Day;
            public DailyAchievementType Type;
            public int Dailies;
            public DailyCounter(DayOfWeek day, DailyAchievementType type, int dailies)
            {
                Day = day;
                Type = type;
                Dailies = dailies;
            }
        }
        public static void LogDailiesInformation()
        {
            List<DailyAchievement> _dailyAchievs = DeserializeFromFile<List<DailyAchievement>>(H.DataFile);
            List<DailyCounter> CountList = new List<DailyCounter>();
            foreach (var _daily in _dailyAchievs)
            {
                var s = CountList.FirstOrDefault(x => x.Day == _daily.Day && x.Type == _daily.Type);
                if (s == null)
                    CountList.Add(new DailyCounter(_daily.Day, _daily.Type, 1));
                else
                    s.Dailies++;
            }
            CountList.OrderBy(x => x.Day);
            foreach (var item in CountList)
            {
                Log("[H] " + item.Day + " - " + item.Type + " " + item.Dailies);
            }
        }
        public static T DeserializeFromFile<T>(string file, bool useCutomDirectory = false)
        {
            if (!file.EndsWith(".xml"))
                file = file + ".xml";

            try
            {
                serializer = new XmlSerializer(typeof(T));
                if (useCutomDirectory)
                    using (var reader = new StreamReader(file))
                        return (T)serializer.Deserialize(reader);
                else
                    using (var reader = new StreamReader(Path.Combine(ProfilesDirectory, file)))
                        return (T)serializer.Deserialize(reader);
            }
            catch
            {
                Log("[H]Failed to deserialize XML");
                MessageBox.Show("Your profile.xml is corrupted. Please delete it and let the plugin generate a new one for you.");
                return default(T);
            }
        }

        public static void SerializeToFile<T>(string fileName, T instance, bool useCustomDirectory = false)
        {
            if (!fileName.EndsWith(".xml"))
                fileName = fileName + ".xml";
            if (!Directory.Exists(ProfilesDirectory))
                Directory.CreateDirectory(ProfilesDirectory);

            serializer = new XmlSerializer(typeof(T));
            if (useCustomDirectory)
                using (var writer = new StreamWriter(fileName))
                    serializer.Serialize(writer, instance);
            else
                using (var writer = new StreamWriter(Path.Combine(ProfilesDirectory, fileName)))
                    serializer.Serialize(writer, instance);
        }

        public static string SerializeToString<T>(T instance)
        {
            serializer = new XmlSerializer(typeof(T));
            using (var writer = new StringWriter()) {
                serializer.Serialize(writer, instance);
                return writer.ToString();
            }
        }

        public static T DeserializeFromString<T>(string data)
        {
            try
            {
                serializer = new XmlSerializer(typeof(T));
                using (var reader = new StringReader(data))
                    return (T)serializer.Deserialize(reader);
            }
            catch
            {
                Log("[H]Failed to deserialize XML");
                MessageBox.Show("Internal XML data is corrupted. Please report the the developer.");
                return default(T);
            }
        }
        
        public static void SaveProfile()
        {
            SerializeToFile(Skandia.Me.Name, Main.settings);
            Log("[H]Saved settings for character " + Skandia.Me.Name);
        }
        public static void LoadProfile()
        {
            if (File.Exists(Path.Combine(ProfilesDirectory, Skandia.Me.Name + ".xml")))
            {
                if (Main.mainUI == null)
                {
                    Log("[H]Main UI is null", true);
                    return;
                }
                Main.settings = DeserializeFromFile<Settings>(Skandia.Me.Name);
                Main.mainUI.SetDetailedLogs(Main.settings.DetailedLogs);
                Main.mainUI.SetCombatProfile(Main.settings.CombatProfile);
                Main.mainUI.SetAutoStart(Main.settings.AutoStart);
                Main.mainUI.SetWipeOut(Main.settings.WipeOut);
                Main.mainUI.SetTopKills(Main.settings.TopKills);
                Main.mainUI.SetGathering(Main.settings.Gathering);
                Main.mainUI.SetExploration(Main.settings.Exploration);
                Main.mainUI.SetIgnorePVP(Main.settings.IgnorePVPChannel);
                Main.PluginStartedOnce = false;
                Log("[H]Loaded settings for character " + Skandia.Me.Name);
            }
            else
            {
                Main.settings = new Settings();
                SaveProfile();
            }
            Main.Character = Skandia.Me.Name;
        }

        public static string ProfilesDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.Combine(Directory.GetParent(Path.GetDirectoryName(path)).FullName, "profiles", "plugins", "DailyLoyalties");
            }
        }
        public static string CombatProfilesDir
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.Combine(Directory.GetParent(Path.GetDirectoryName(path)).FullName, "profiles", "combat");
            }
        }

        public static List<string> CombatProfiles()
        {
            List<string> filePaths = Directory.GetFiles(CombatProfilesDir, "*.akcs").ToList();
            List<string> files = new List<string>();
            foreach (var file in filePaths)
            {
                files.Add(Path.GetFileNameWithoutExtension(file));
            }
            return files;
        }

        public class AutoClosingMessageBox
        {
            System.Threading.Timer _timeoutTimer;
            string _caption;
            AutoClosingMessageBox(string text, string caption, int timeout)
            {
                _caption = caption;
                _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                    null, timeout, System.Threading.Timeout.Infinite);
                using (_timeoutTimer)
                    MessageBox.Show(text, caption);
            }
            public static void Show(string text, string caption, int timeout)
            {
                new AutoClosingMessageBox(text, caption, timeout);
            }
            void OnTimerElapsed(object state)
            {
                IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
                if (mbWnd != IntPtr.Zero)
                    SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                _timeoutTimer.Dispose();
            }
            const int WM_CLOSE = 0x0010;
            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        }
        public static string DataDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.Combine(Directory.GetParent(Path.GetDirectoryName(path)).FullName, "profiles", "plugins", "DailyLoyalties", "Data");
            }
        }
        public static string DataFile
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.Combine(Directory.GetParent(Path.GetDirectoryName(path)).FullName, "profiles", "plugins", "DailyLoyalties", "Data", "Data.xml");
            }
        }
    }
}
