using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YL.GitRepository
{
	public class SectionParser
	{
		private static readonly Regex SectionPattern = new Regex(@"^\[(.*)\]$", RegexOptions.Compiled);
		private static readonly Regex PairPattern = new Regex(@"^\s*([\S][^=]+)[\s]*=[\s]*(.*)$", RegexOptions.Compiled);

		private readonly FileReader _reader;

		public Dictionary<string, Section> Sections { get; private set; } = new Dictionary<string, Section>();

		public SectionParser(FileReader reader)
		{
			_reader = reader;
			EnsureFileBeginsWithSection();
			InitSections();
		}

		private void InitSections()
		{
			foreach (var line in _reader.Lines)
			{
				ParseLine(line);
			}
		}

		private void ParseLine(string line)
		{
			if (SectionPattern.IsMatch(line))
				InitNewSectionFromLine(line);
			if (PairPattern.IsMatch(line))
				AddKeyValuePairToLastSectionFromLine(line);
		}

		private void InitNewSectionFromLine(string line)
		{
			var match = SectionPattern.Match(line);
			var sectionKey = match.Groups[1].Value.Trim();
			if (!Sections.ContainsKey(sectionKey))
			{
				Sections.Add(sectionKey, new Section(sectionKey));
			}
		}

		private void AddKeyValuePairToLastSectionFromLine(string line)
		{
			var match = PairPattern.Match(line);
			var key = match.Groups[1].Value.Trim();
			var value = match.Groups[2].Value;
			Sections[Sections.Last().Key].Add(key, value);
		}

		private void AppendToLastSectionValueFromLine(string line)
		{
			var sectionKey = Sections.Last().Key;
			var lastPair = Sections[sectionKey].LastOrDefault();
			if (lastPair.Equals(default(KeyValuePair<string, string>))) throw new ArgumentException("Section contains invalid key format");
			Sections[sectionKey][lastPair.Key] += line;
		}

		private void EnsureFileBeginsWithSection()
		{
			if (!SectionPattern.IsMatch(_reader.Lines[0]))
				throw new ArgumentException(string.Format("{0} - Beginning line is not a valid section heading", _reader.Lines[0]));
		}
	}
}
