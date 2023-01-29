namespace rclone_booksync
{
	internal static class ConfigParser
	{
		public class RemoteEntry
		{
			public string name;
			public string user;
			public string host;

			public RemoteEntry()
			{ }
		}

		static RemoteEntry parseentry(string[] text, int start, int end)
		{
			RemoteEntry remoteEntry = new();
			remoteEntry.name = text[start].Substring(1, text[start].Length - 2);
			for (int i = start + 1; i < end; i++)
			{
				int p = text[i].IndexOf("=");
				if (p == -1) continue;
				string word = text[i].Substring(0, (text[i][p - 1] == ' ') ? (p - 1) : p);
				string rest = text[i].Substring((text[i][p + 1] == ' ') ? (p + 2) : (p + 1),
					(text[i][p + 1] == ' ') ? (text[i].Length - p - 2) : (text[i].Length - p - 1));
				if (word == "type")
				{
					if (rest != "sftp")
						return null;
				}
				if (word == "user")
					remoteEntry.user = rest;
				if (word == "host")
					remoteEntry.host = rest;
			}
			return remoteEntry;
		}
		static List<RemoteEntry> parseentries(string[] text)
		{
			List<RemoteEntry> remoteEntries = new List<RemoteEntry>();
			List<int> entryindex = new List<int>();
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i].StartsWith('[') && text[i].EndsWith(']'))
				{
					entryindex.Add(i);
				}
			}
			RemoteEntry c;
			for (int i = 0; i < entryindex.Count - 1; i++)
			{
				c = parseentry(text, entryindex[i], entryindex[i + 1] - 1);
				if (c != null) remoteEntries.Add(c);
			}
			c = parseentry(text, entryindex[entryindex.Count - 1], text.Length);
			if (c != null) remoteEntries.Add(c);
			return remoteEntries;
		}

		public static List<RemoteEntry> load(string path)
		{
			string[] text = System.IO.File.ReadAllLines(path);
			List<RemoteEntry> remoteEntries = parseentries(text);
			return remoteEntries;
		}
	}
}
