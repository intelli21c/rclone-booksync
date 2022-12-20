using System.Diagnostics;

namespace rsync_booksync
{
    public partial class Form1 : Form
    {
        Process rsync = null;
        Thread rsyncwatch = null;
        string command = "";
        void rsyncwatchf()
        {
            rsync.Start();
            while (!rsync.HasExited)
            {
                richTextBox1.Text += rsync.StandardOutput.Read();
                richTextBox1.Text += rsync.StandardError.Read();
            }
        }

        bool startrsync()
        {
            if (rsyncwatch != null)
            {
                if (MessageBox.Show("Previous instance seems to be running", "", MessageBoxButtons.YesNo) == DialogResult.No)
                    return false;
            }
            rsync = new Process();
            rsync.Exited += (object sender, EventArgs e) => { rsync = null; rsyncwatch = null; };
            if (!File.Exists(textBox2.Text))
            {
                MessageBox.Show("Invalid rsync path provided", "Error", MessageBoxButtons.OK);
                return false;
            }
            rsync.StartInfo.FileName = textBox2.Text;
            //rsync.StartInfo.UseShellExecute = true;
            rsync.StartInfo.UseShellExecute = false;
            rsync.StartInfo.RedirectStandardOutput = true;
            rsync.StartInfo.RedirectStandardError = true;
            //rsync.StartInfo.CreateNoWindow = true;

            //rsyncwatch = new Thread(rsyncwatchf);
            //rsyncwatch.Start();
            //stdout redirection yet not working. remove if it works 

            return true;
        }

        public Form1()
        {
            InitializeComponent();

            string bookdir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Books";
            string programdir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (Directory.Exists(bookdir))
                textBox4.Text = bookdir;
            if (Directory.Exists(programdir + "\\cwrsync"))
            {
                textBox2.Text = programdir + "\\cwrsync\\bin\\rsync.exe";
            }
            else if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\cwrsync"))
            {
                textBox2.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\cwrsync";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = openFileDialog1.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox4.Text))
            {
                MessageBox.Show("Invalid local path provided", "Error", MessageBoxButtons.OK);
                return;
            }
            if (textBox3.Text == "")
            {
                MessageBox.Show("No remote path provided", "Error", MessageBoxButtons.OK);
                return;
            }
            if (textBox3.Text.EndsWith('/'))
            {
                MessageBox.Show("Remote path has trailing slash, and may result in undesired behaviour", "Notice", MessageBoxButtons.OK);
            }
            command = textBox5.Text + " ";
            if (checkBox1.Checked)
                command += "--dry-run ";
            command += textBox3.Text + " ";
            if (textBox4.Text.StartsWith('"') && textBox4.Text.EndsWith('"'))
                command += new string(textBox4.Text.ToList().GetRange(0, textBox4.Text.LastIndexOf("\\") + 1).ToArray());
            else if (textBox4.Text.Contains(' '))
                command += (" \"" + (new string(textBox4.Text.ToList().GetRange(0, textBox4.Text.LastIndexOf("\\") + 1).ToArray())) + "\"");
            else
                command += (" \"" + textBox4.Text + "\"");
            richTextBox1.Text += ("\"" + textBox2.Text + "\" " + command + "\n");
            startrsync();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox4.Text))
            {
                MessageBox.Show("Invalid local path provided", "Error", MessageBoxButtons.OK);
                return;
            }
            if (textBox3.Text == "")
            {
                MessageBox.Show("No remote path provided", "Error", MessageBoxButtons.OK);
                return;
            }
            if (textBox3.Text.EndsWith('/'))
            {
                MessageBox.Show("Remote path has trailing slash, and may result in undesired behaviour", "Notice", MessageBoxButtons.OK);
            }
            command = textBox5.Text + " ";
            if (checkBox1.Checked)
                command += "--dry-run ";
            if (textBox4.Text.StartsWith('"') && textBox4.Text.EndsWith('"'))
                command += textBox4.Text;
            else if (textBox4.Text.Contains(' '))
                command += (" \"" + textBox4.Text + "\"");
            else
                command += (" \"" + textBox4.Text + "\"");
            command += " " + new string(textBox3.Text.ToList().GetRange(0, textBox3.Text.LastIndexOf("/") + 1).ToArray());
            richTextBox1.Text += ("\"" + textBox2.Text + "\" " + command + "\n");
            startrsync();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (rsync != null) rsync.Dispose();
        }
    }
}