using System;
using System.Collections.Generic;
using System.Text;

namespace YL.GitRepository
{
	public class Section : Dictionary<string, string>
	{
		public string Name { get; private set; }

		public Section(string name)
		{
			Name = name;
		}

		public string GetString(string key)
		{
			return this[key];
		}

		public int GetInt(string key)
		{
			var value = this[key];
			int result = 0;
			var success = int.TryParse(value, out result);
			return result;
		}

		public float GetFloat(string key)
		{
			var value = this[key];
			float result = 0F;
			var success = float.TryParse(value, out result);
			return result;
		}

		public void SetString(string key, string value)
		{
			this[key] = value;
		}

		public void SetInt(string key, int value)
		{
			SetString(key, value.ToString());
		}

		public void SetFloat(string key, float value)
		{
			SetString(key, value.ToString());
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("[{0}]\r\n", Name);
			foreach (var kvp in this)
				sb.AppendFormat("{0}: {1}\r\n", kvp.Key, kvp.Value);

			return sb.ToString();
		}
	}
}
