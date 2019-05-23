using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace yes
{
    public partial class Form1 : Form
    {
        private List<KeyValuePair<string, int>> Headers = new List<KeyValuePair<string, int>>()
        {
            new KeyValuePair<string, int>("Username", 110),
            new KeyValuePair<string, int>("Time", 80),
            new KeyValuePair<string, int>("Wins", 60),
            new KeyValuePair<string, int>("Rank", 60),
        };

        private List<AccountDetails> Accounts;

        private int OldColumn { get; set; } = -1;
        private bool SortDecending { get; set; } = true;

        public Form1()
        {
            InitializeComponent();

            foreach (var header in Headers)
            {
                listView1.Columns.Add(new ColumnHeader
                {
                    Text = header.Key,
                    Width = header.Value,
                    TextAlign = HorizontalAlignment.Center
                });
            }

            string json = File.ReadAllText("accounts.json");
            Accounts = JsonConvert.DeserializeObject<List<AccountDetails>>(json);

            MessageBox.Show(Accounts.Count.ToString());

            LoadListView();
        }

        private void ListView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && listView1.FocusedItem.Bounds.Contains(e.Location))
            {
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void ListView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if(OldColumn == e.Column)
            {
                SortDecending = !SortDecending;
            }
            else
            {
                SortDecending = true;
            }

            OldColumn = e.Column;

            switch (e.Column)
            {
                case 0: SortListView(o => o.login.Split(':')[0]); break;
                case 1: SortListView(o => o.penalty_seconds); break;
                case 2: SortListView(o => o.wins); break;
                case 3: SortListView(o => o.rank); break;
                default: break;
            }
        }

        private void OpenSteamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var process in Process.GetProcesses().Where(pr => pr.ProcessName == "Steam" || pr.ProcessName == "csgo"))
            {
                process.Kill();
            }

            var login = Accounts.First(x => x.login.Split(':')[0] == listView1.SelectedItems[0].Text).login.Split(':');
            ProcessStartInfo info = new ProcessStartInfo(@"C:\Program Files (x86)\Steam\Steam.exe")
            {
                Arguments = string.Format("-login {0} {1} -silent -applaunch 730 -novid -freq 144 +exec hack +developer 1", login[0], login[1])
            };
            Process.Start(info);
        }

        private string ConvertToTime(int seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);

            if (t.Days > 0)
            {
                return t.ToString(@"dd\:hh\:mm\:ss");
            }

            return t.ToString(@"hh\:mm\:ss");
        }

        private void CopyLoginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var login = Accounts.First(x => x.login.Split(':')[0] == listView1.SelectedItems[0].Text).login;
            Clipboard.SetText(login);
        }

        private void LoadListView()
        {
            listView1.Items.Clear();

            foreach (AccountDetails account in Accounts)
            {
                var seconds = account.penalty_seconds;

                string[] row = {
                    account.login.Split(':')[0],
                    ConvertToTime(seconds),
                    account.wins.ToString(),
                    account.rank.ToString()
                };

                listView1.Items.Add(new ListViewItem(row));
            }
        }

        private void SortListView<T>(Func<AccountDetails, T> keySelector)
        {
            if (SortDecending)
            {
                Accounts = Accounts.OrderByDescending(keySelector).ToList();
            }
            else
            {
                Accounts = Accounts.OrderBy(keySelector).ToList();
            }

            LoadListView();
        }
    }
}
