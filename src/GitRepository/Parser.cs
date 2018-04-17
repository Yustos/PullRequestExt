using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace YL.GitRepository
{
	public class Parser
	{
		private readonly FileReader _reader;
		private readonly Dictionary<string, Section> _sections;

		public Parser(string filePath)
		{
			_reader = new FileReader(filePath);
			_sections = new SectionParser(_reader).Sections;
		}

		public string GetString(string section, string key)
		{
			return _sections[section].GetString(key);
		}

		public float GetFloat(string section, string key)
		{
			return _sections[section].GetFloat(key);
		}

		public int GetInt(string section, string key)
		{
			return _sections[section].GetInt(key);
		}

		public void SetString(string section, string key, string value)
		{
			SetAndWrite(section, key, value);
		}

		public void SetFloat(string section, string key, float value)
		{
			SetString(section, key, value.ToString());
		}

		public void SetInt(string section, string key, int value)
		{
			SetString(section, key, value.ToString());
		}

		private void SetAndWrite(string section, string key, string value)
		{
			var s = _sections[section];
			if (s.ContainsKey(key) && s.GetString(key) == value) return;
			s.SetString(key, value);
			var sb = new StringBuilder();
			_sections.All(kvp => { sb.AppendFormat("{0}\r\n", kvp.Value.ToString()); return true; });
			File.WriteAllText(_reader.FilePath, sb.ToString());
		}
	}
}

