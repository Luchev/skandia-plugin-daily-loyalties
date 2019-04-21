using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Plugins;
using System.IO;

namespace DailyLoyalties
{
    public partial class MainUI : Form
    {
        public List<DailyAchievement> daily { get; set; }
        DayOfWeek day { get; set; }
        public MainUI()
        {
            InitializeComponent();
            MaximizeBox = false;
            comboBox1.Items.AddRange(H.CombatProfiles().ToArray());
            comboBox1.Text = comboBox1.Items[0].ToString();
            Width = 320;
            Height = 260;
            button6.ForeColor = Color.Green;

            // Loading circle
            loadingCircle.NumberSpoke = 20;
            loadingCircle.SpokeThickness = 3;
            loadingCircle.InnerCircleRadius = 3;
            loadingCircle.OuterCircleRadius = 15;
            loadingCircle.Active = false;
            loadingCircle.Visible = true;
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            // Hide instead of close the window
            e.Cancel = true;
            Hide();
            base.OnClosing(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Add current target
            var newDaily = new DailyAchievement();
            if (Skandia.Me.GotTarget)
                newDaily.EntityID = Skandia.Me.CurrentTarget.Template.Id;
            else
                MessageBox.Show("No target!");
            newDaily.Count = int.Parse(textBox1.Text);
            newDaily.Day = day;
            newDaily.LevelRequired = (int)ObjectManager.GetCurrentMapInfo().MinLevel;
            if (comboBox2.SelectedItem.ToString() == "WipeOut")
                newDaily.Type = DailyAchievementType.WipeOut;
            if (comboBox2.SelectedItem.ToString() == "TopKills")
                newDaily.Type = DailyAchievementType.TopKills;
            if (comboBox2.SelectedItem.ToString() == "Gathering")
                newDaily.Type = DailyAchievementType.Gathering;
            if (comboBox2.SelectedItem.ToString() == "Exploration")
                newDaily.Type = DailyAchievementType.Exploration;
            newDaily.Map = ObjectManager.GetCurrentMapInfo().Id;
            newDaily.Location3D = Skandia.Me.CurrentTarget.Location3D;
            if (daily == null)
                daily = new List<DailyAchievement>();
            daily.Add(newDaily);
            H.Log("[DailyLoyalties]Added daily target (" + ObjectManager.GetTemplateInfo(newDaily.EntityID).Name + ") for {" + newDaily.Day + "}", true);
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            // Save daily list to  file
            H.SerializeToFile(textBox2.Text, daily);
            MessageBox.Show("File saved to profiles/plugins/DailyLoyalties/" + textBox2.Text + ".xml");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Increment day
            day = day + 1;
            if ((int)day > 6)
                day = DayOfWeek.Sunday;
            label1.Text = day.ToString();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // Always on top
            TopMost = !TopMost;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Main.Manager.Toggle();
        }
        public void SetStartStopButton(string text, Color color)
        {
            if (button6.InvokeRequired)
            {
                button6.Invoke((MethodInvoker)delegate
                {
                    button6.ForeColor = color;
                    button6.Text = text;
                });
            }
            else
            {
                button6.ForeColor = color;
                button6.Text = text;
            }
        }
 
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update combat profile to use
            Main.settings.CombatProfile = comboBox1.SelectedItem.ToString();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            // Skip target
            Main.Manager.SkipModule();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // Check ID to Name
            string input = Microsoft.VisualBasic.Interaction.InputBox("Input the Mob ID to check", "Check mob ID", "", -1, -1);
            if (input == "")
                return;
            uint id = uint.Parse(input);
            MessageBox.Show(ObjectManager.GetTemplateInfo(id).Name);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            // Reset
            Main.Manager.Reset();
        }
        
        private void button11_Click(object sender, EventArgs e)
        {
            // Test Mover
            string input = Microsoft.VisualBasic.Interaction.InputBox("Input the Mob ID to move to", "Move to mob", "", -1, -1);
            if (input == "")
                return;
            uint id = UInt32.Parse(input);
            Skandia.Core.Mover.MoveTo(id);
        } 

        private void button12_Click(object sender, EventArgs e)
        {
            // Get Current Location
            MessageBox.Show("Map: " + ObjectManager.GetCurrentMapInfo().Id + Environment.NewLine + "Location: " + Skandia.Me.Location3D.ToString());
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            // Auto Start
            Main.settings.AutoStart = checkBox2.Checked;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // Save Profile
            H.SaveProfile();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Main.settings.DetailedLogs = checkBox3.Checked;
        }
        public void SetCombatProfile(string combatProfile)
        {
            if (comboBox1.InvokeRequired)
            {
                comboBox1.Invoke((MethodInvoker)delegate
                {
                    comboBox1.SelectedItem = combatProfile;
                });
            }
            else
            {
                comboBox1.SelectedItem = combatProfile;
            }
        }
        public void SetAutoStart(bool autoStart)
        {
            if (checkBox2.InvokeRequired)
            {
                checkBox2.Invoke((MethodInvoker)delegate
                {
                    checkBox2.Checked = autoStart;
                });
            }
            else
            {
                checkBox2.Checked = autoStart;
            }
        }
        public void SetDetailedLogs(bool detailedLogs)
        {
            if (checkBox3.InvokeRequired)
            {
                checkBox3.Invoke((MethodInvoker)delegate
                {
                    checkBox3.Checked = detailedLogs;
                });
            }
            else
            {
                checkBox3.Checked = detailedLogs;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // TEST
            //H.LogDailiesInformation();
        }
        public class ArcheoNPC
        {
            public uint Id { get; set; }
            public Vector3 Location { get; set; }
            public short Map { get; set; }
            public DayOfWeek Day { get; set; }
        }

            private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            // Wipe Out
            Main.settings.WipeOut = checkBox5.Checked;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            // Top Kills
            Main.settings.TopKills = checkBox6.Checked;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            // Gathering
            Main.settings.Gathering = checkBox7.Checked;
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            // Exploration
            Main.settings.Exploration = checkBox8.Checked;
        }
        public void SetWipeOut(bool _checked)
        {
            if (checkBox5.InvokeRequired)
            {
                checkBox5.Invoke((MethodInvoker)delegate
                {
                    checkBox5.Checked = _checked;
                });
            }
            else
            {
                checkBox5.Checked = _checked;
            }
        }
        public void SetTopKills(bool _checked)
        {
            if (checkBox6.InvokeRequired)
            {
                checkBox6.Invoke((MethodInvoker)delegate
                {
                    checkBox6.Checked = _checked;
                });
            }
            else
            {
                checkBox6.Checked = _checked;
            }
        }
        public void SetGathering(bool _checked)
        {
            if (checkBox7.InvokeRequired)
            {
                checkBox7.Invoke((MethodInvoker)delegate
                {
                    checkBox7.Checked = _checked;
                });
            }
            else
            {
                checkBox7.Checked = _checked;
            }
        }
        public void SetExploration(bool _checked)
        {
            if (checkBox8.InvokeRequired)
            {
                checkBox8.Invoke((MethodInvoker)delegate
                {
                    checkBox8.Checked = _checked;
                });
            }
            else
            {
                checkBox8.Checked = _checked;
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            // Ignore PVP
            Main.settings.IgnorePVPChannel = checkBox4.Checked;
        }
        public void SetIgnorePVP(bool _checked)
        {
            if (checkBox4.InvokeRequired)
            {
                checkBox4.Invoke((MethodInvoker)delegate
                {
                    checkBox4.Checked = _checked;
                });
            }
            else
            {
                checkBox4.Checked = _checked;
            }
        }
    }
}