using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace rclone_booksync
{
	public partial class Form1 : Form
	{
		Process rclone = null;
		Thread rclonewatch = null;
		string command = "";
		List<ConfigParser.RemoteEntry> remoteEntries;
		void rclonewatchf()
		{
			rclone.Start();
			label2.Text = "RUNNING";
			richTextBox1.Text += rclone.StandardError.ReadToEnd(); //synchronous workaround, not good.
			while (!rclone.HasExited) ;
			//MessageBox.Show(rclone.StandardOutput.ReadToEnd());
			/*while (!rclone.HasExited)
			{
				//richTextBox1.Text += rclone.StandardOutput.Read();
				richTextBox1.Text += (char)rclone.StandardError.Read();
				//stderr reading seems to cause error
			}*/
			rclone.Kill();
			rclone.Dispose();
			rclone = null;
			richTextBox1.Text += "\n";
			label2.Text = "IDLE";
			rclonewatch = null;
		}

		void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
		{
			//https://stackoverflow.com/questions/4291912/process-start-how-to-get-the-output
			//Console.WriteLine(outLine.Data);
		}

		bool startrclone()
		{
			if (rclonewatch != null)
			{
				if (MessageBox.Show("Previous instance seems to be running", "Notice", MessageBoxButtons.YesNo) == DialogResult.No)
					return false;
				rclone.Kill(); //errs.
				rclone.Dispose();
				rclone = null;
				Thread.Sleep(100);
			}
			rclone = new Process();
			if (!File.Exists(textBox2.Text))
			{
				MessageBox.Show("Invalid rclone path provided", "Error", MessageBoxButtons.OK);
				return false;
			}
			rclone.StartInfo.FileName = textBox2.Text;
			rclone.StartInfo.Arguments = command;
			rclone.StartInfo.UseShellExecute = false;
			rclone.StartInfo.RedirectStandardOutput = true;
			rclone.StartInfo.RedirectStandardError = true;
			rclone.StartInfo.CreateNoWindow = true;

			//rclone.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
			//rclone.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

			rclonewatch = new Thread(rclonewatchf);
			rclonewatch.Start();
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
			if (File.Exists(bookdir + "\\rclone.config") || File.Exists(bookdir + "\\.meta\\rclone.config"))
			{
				textBox1.Text = bookdir + "\\.meta\\rclone.config";
				checkBox2.Checked = false;
				remoteEntries = ConfigParser.load(textBox1.Text);
				for (int i = 0; i < remoteEntries.Count; i++)
				{
					domainUpDown1.Items.Add(remoteEntries[i].name);
				}
				domainUpDown1.SelectedIndex = 0;
				checkBox3.Checked = true;
			}

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
			if (!checkBox2.Checked)
				command += "--config " + "\"" + textBox1.Text + "\" ";
			if (checkBox1.Checked)
				command += "--dry-run ";
			command += textBox3.Text + " ";
			if (textBox4.Text.StartsWith('"') && textBox4.Text.EndsWith('"'))
				command += textBox4.Text;
			else if (textBox4.Text.Contains(' '))
				command += " \"" + textBox4.Text + "\"";
			else
				command += " \"" + textBox4.Text + "\"";
			richTextBox1.Text += textBox2.Text + " " + command + "\n";
			startrclone();
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
			if (!checkBox2.Checked)
				command += "--config " + "\"" + textBox1.Text + "\" ";
			if (checkBox1.Checked)
				command += "--dry-run ";
			if (textBox4.Text.StartsWith('"') && textBox4.Text.EndsWith('"'))
				command += textBox4.Text;
			else if (textBox4.Text.Contains(' '))
				command += (" \"" + textBox4.Text + "\"");
			else
				command += (" \"" + textBox4.Text + "\"");
			command += " " + textBox3.Text;
			richTextBox1.Text += textBox2.Text + " " + command + "\n";
			startrclone();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (rclone != null)
			{
				rclone.Kill();
				rclone.Dispose();
				rclone = null;
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			richTextBox1.Text = "";
		}

		private void button2_Click(object sender, EventArgs e)
		{
			if (rclone != null)
				rclone.Kill();
		}

		private void checkBox2_CheckedChanged(object sender, EventArgs e)
		{
			textBox1.Enabled = !checkBox2.Checked;
		}
		private void domainUpDown1_SelectedItemChanged(object sender, EventArgs e)
		{
			if (checkBox3.Checked)
			{
				textBox3.Text = (remoteEntries[domainUpDown1.SelectedIndex].name + ":Books");
			}
		}

		private void checkBox3_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBox3.Checked)
			{
				textBox3.Text = (remoteEntries[domainUpDown1.SelectedIndex].name + ":Books");
			}
		}

		private void button7_Click(object sender, EventArgs e)
		{
			//TODO handel default case.
			//maybe reinitalisation need be done every time other place too...
			openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
				(Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Books") ? "\\Books\\.meta" : "");
			openFileDialog1.FileName = "rclone.config";
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				textBox1.Text = openFileDialog1.FileName;
			}

			remoteEntries = ConfigParser.load(textBox1.Text);
			domainUpDown1.Items.Clear();
			for (int i = 0; i < remoteEntries.Count; i++)
			{
				domainUpDown1.Items.Add(remoteEntries[i].name);
			}
			domainUpDown1.SelectedIndex = 0;
		}
	}
}