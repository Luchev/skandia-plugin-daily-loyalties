using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugins;
using PluginsCommon;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace DailyLoyalties
{
    public class Main : IPlugin
    {
        public static Thread mainUIThread;
        public static MainUI mainUI;
        public static ModuleManager Manager;
        public static Settings settings;
        public static string Character;
        public static bool PluginStartedOnce;
        public static bool PluginFinished;
        public string Author
        {
            get
            {
                return "Hachiman";
            }
        }

        public string Description
        {
            get
            {
                return "The way loyalties are meant to be farmed";
            }
        }

        public string Name
        {
            get
            {
                return "DailyLoyalties";
            }
        }

        public Version Version
        {
            get
            {
                return new Version(2, 2, 2);
            }
        }

        public void OnButtonClick()
        {
            SettingsButtonClick();
        }
        public static void SettingsButtonClick()
        {
            if (mainUI == null)
            {
                StartMainUIThread();
                return;
            }
            else if (mainUI.Visible)
            {
                mainUI.Hide();
            }
            else
            {
                mainUI.Show();
            }
        }
        public static bool StartMainUIThread()
        {
            if (mainUIThread == null ||
                mainUIThread.ThreadState == System.Threading.ThreadState.Aborted ||
                mainUIThread.ThreadState == System.Threading.ThreadState.Stopped ||
                mainUIThread.ThreadState == System.Threading.ThreadState.Unstarted)
            {
                mainUIThread = new Thread(MainUIThreadStartMethod);
                mainUIThread.SetApartmentState(ApartmentState.STA);
                mainUIThread.Start();
                return true;
            }
            // If something fails return false
            else
                return false;
        }
        private static void MainUIThreadStartMethod()
        {
            if (mainUI == null)
            {
                mainUI = new MainUI();
            }
            mainUI.ShowDialog();
        }

        public void OnStart()
        {
            // Initializing variables to avoid null exception
            Manager = new ModuleManager();
            settings = new Settings();
            PluginStartedOnce = false;
            PluginFinished = false;
            if (!Directory.Exists(H.DataDirectory))
                Directory.CreateDirectory(H.DataDirectory);
            if (!File.Exists(H.DataFile))
                File.WriteAllText(H.DataFile, Properties.Resources.DailyAchievements);

            //SettingsButtonClick();

            // Launch the UI if starting the plugin in game
        }

        public void OnStop(bool off)
        {
            // Stop all core methods

            // Kill UI
            //try
            //{
            //    if (mainUI != null && mainUI.InvokeRequired)
            //    {
            //        mainUI.Invoke((MethodInvoker)delegate
            //        {
            //            mainUI.Close();
            //            mainUI.Dispose();
            //        });
            //    }
            //    else
            //    {
            //        if (mainUI != null && !mainUI.IsDisposed)
            //        {
            //            mainUI.Close();
            //            mainUI.Dispose();
            //        }
            //    }
            //}
            //catch { }
            //if (!Skandia.Core.Mover.IsIdle())
            //    Skandia.Core.Mover.Stop();
            //if (Skandia.Core.GetSelfDefenseState())
            //    Skandia.Core.ToggleSelfDefenseBot(false);
            //if (!Skandia.Core.Fighter.IsIdle())
            //    Skandia.Core.Fighter.Stop();
            //if (!Skandia.Core.GetArchaeologyBotState())
            //    Skandia.Core.ToggleArchaeologyBot(false);
            if (mainUI != null)
            {
                mainUI.Close();
                mainUI = null;
            }
        }

        public void Pulse()
        {
            Skandia.Update();
            if (!Skandia.IsInGame)
                return;
            // Auto Load settings
            if (Character == null || Character != Skandia.Me.Name)
            {
                H.LoadProfile();
                if (settings.AutoStart)
                {
                    Manager.Toggle();
                }
            }
            // ModuleManager call
            Manager.Call();
        }
    }
}
