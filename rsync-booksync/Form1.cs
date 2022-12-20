using System.Diagnostics;
using System.Linq.Expressions;

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
            label2.Text = "RClone is running";
            while (!rsync.HasExited)
            {
                richTextBox1.Text += (char)rsync.StandardOutput.Read();
                richTextBox1.Text += (char)rsync.StandardError.Read();
            }
            rsync.Kill();
            rsync.Dispose();
            rsync = null;
            richTextBox1.Text += "\n";
            label2.Text = "RClone not running";
            rsyncwatch = null;
        }

        bool startrsync()
        {
            if (rsyncwatch != null)
            {
                if (MessageBox.Show("Previous instance seems to be running", "", MessageBoxButtons.YesNo) == DialogResult.No)
                    return false;
                rsync.Kill();
            }
            rsync = new Process();
            if (!File.Exists(textBox2.Text))
            {
                MessageBox.Show("Invalid rclone path provided", "Error", MessageBoxButtons.OK);
                return false;
            }
            rsync.StartInfo.FileName = textBox2.Text;
            rsync.StartInfo.Arguments = command;
            rsync.StartInfo.UseShellExecute = false;
            rsync.StartInfo.RedirectStandardOutput = true;
            rsync.StartInfo.RedirectStandardError = true;
            rsync.StartInfo.CreateNoWindow = true;

            rsyncwatch = new Thread(rsyncwatchf);
            rsyncwatch.Start();
            return true;
        }

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

            string bookdir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Books";
            string programdir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (Directory.Exists(bookdir))
                textBox4.Text = bookdir;
            if (File.Exists(programdir + "\\rclone.exe"))
            {
                textBox2.Text = programdir + "\\rclone.exe";
            }
            else if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Programs\\rclone.exe"))
            {
                textBox2.Text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Programs\\rclone.exe";
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
                MessageBox.Show("No remote profile and path provided", "Error", MessageBoxButtons.OK);
                return;
            }
            command = "copy " + textBox5.Text + " ";
            if (checkBox1.Checked)
                command += "--dry-run ";
            command += textBox3.Text + " ";
            if (textBox4.Text.StartsWith('"') && textBox4.Text.EndsWith('"'))
                command += textBox4.Text;
            else if (textBox4.Text.Contains(' '))
                command += " \"" + textBox4.Text + "\"";
            else
                command += " \"" + textBox4.Text + "\"";
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
                MessageBox.Show("No remote profile and path provided", "Error", MessageBoxButtons.OK);
                return;
            }
            command = "copy " + textBox5.Text + " ";
            if (checkBox1.Checked)
                command += "--dry-run ";
            if (textBox4.Text.StartsWith('"') && textBox4.Text.EndsWith('"'))
                command += textBox4.Text;
            else if (textBox4.Text.Contains(' '))
                command += (" \"" + textBox4.Text + "\"");
            else
                command += (" \"" + textBox4.Text + "\"");
            command += " " + textBox3.Text;
            richTextBox1.Text += ("\"" + textBox2.Text + "\" " + command + "\n");
            startrsync();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (rsync != null)
            {
                rsync.Kill();
                rsync.Dispose();
                rsync = null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (rsync != null)
                rsync.Kill();
        }
    }
}