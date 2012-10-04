using System;
using System.Collections;
using System.Collections.Generic;
using ToolBelt;

namespace XnaBuildContent
{
	public class PropertyGroup : IEnumerable<KeyValuePair<string, string>>
	{
        #region Private Fields
        private Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        #endregion

		#region Construction
		public PropertyGroup()
		{
		}

		public PropertyGroup(PropertyGroup other)
		{
			IEnumerator<KeyValuePair<string, string>> e = ((IEnumerable<KeyValuePair<string, string>>)other).GetEnumerator();

			while (e.MoveNext())
			{
				KeyValuePair<string, string> pair = e.Current;

				dictionary.Add(pair.Key, pair.Value);
			}
		}

		#endregion

		#region Methods
        public string ReplaceVariables(string s)
        {
            return s.ReplaceTags("$(", ")", this.dictionary, TaggedStringOptions.ThrowOnUnknownTags);
        }

        public void AddWellKnownProperties(
            ParsedPath buildContentInstallDir,
            ParsedPath contentFileDir)
        {
            this.Set("BuildContentInstallDir", buildContentInstallDir.ToString());
            this.Set("InputDir", contentFileDir.ToString());
            this.Set("OutputDir", contentFileDir.ToString());
        }

        public void ExpandAndAddFromList(List<Tuple<string, string>> pairs, PropertyGroup propGroup)
        {
            foreach (var pair in pairs)
            {
                dictionary[pair.Item1] = propGroup.ReplaceVariables(pair.Item2);
            }
        }

        public void AddFromEnvironment()
        {
            IDictionary entries = Environment.GetEnvironmentVariables();

            foreach (DictionaryEntry entry in entries)
            {
                if (!String.IsNullOrEmpty((string)entry.Key))
                    dictionary[(string)entry.Key] = (string)entry.Value;
            }
        }

        public void AddFromPropertyString(string keyValuePairString)
        {
            if (String.IsNullOrEmpty(keyValuePairString))
                return;

            string[] keyValuePairs = keyValuePairString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string keyValuePair in keyValuePairs)
            {
                string[] keyAndValue = keyValuePair.Split('=');

                if (keyAndValue.Length == 2)
                {
                    dictionary[keyAndValue[0]] = keyAndValue[1].Trim();
                }
            }
        }

		public string GetRequiredValue(string name)
		{
			string value = null;

			if (!dictionary.TryGetValue(name, out value))
				throw new InvalidOperationException("Required property '{0}' not found".CultureFormat(name));

			return value;
		}
		
		public void GetOptionalValue(string name, out string value, string defaultValue)
		{
			if (!dictionary.TryGetValue(name, out value))
				value = defaultValue;
		}
		
		public string GetOptionalValue(string name, string defaultValue)
		{
			string value;

			GetOptionalValue(name, out value, defaultValue);

			return value;
		}

		public void GetOptionalValue(string name, out int value, int defaultValue)
		{
			string s;
			
			if (!dictionary.TryGetValue(name, out s) || !Int32.TryParse(s, out value))
				value = defaultValue;
		}
		
		public void GetRequiredValue(string name, out int value)
		{
			string s;
			
			if (!dictionary.TryGetValue(name, out s))
				throw new InvalidOperationException("Property '{0}' not present".CultureFormat(name));
			
			if (!Int32.TryParse(s, out value))
				throw new InvalidOperationException("Property '{0}' value '{1}' is not a valid integer".CultureFormat(name, s));
		}

		public void GetRequiredValue(string name, out string value)
		{
			if (!dictionary.TryGetValue(name, out value))
				throw new InvalidOperationException("Property '{0}' not present".CultureFormat(name));
		}
		
		public void GetRequiredValue(string name, out string[] value)
		{
			string s;
			
			if (!dictionary.TryGetValue(name, out s))
				throw new InvalidOperationException("Property '{0}' not present".CultureFormat(name));
			
			value = s.Split(';');
		}

		#endregion

		#region IEnumerable Implementation
		public IEnumerator GetEnumerator()
		{
			return (IEnumerator)GetEnumerator();
		}

		IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		#endregion

		#region Methods
		public void Clear()
		{
			dictionary.Clear();
		}

		public bool Contains(string name)
		{
			return dictionary.ContainsKey(name);
		}

		public int Count
		{
			get
			{
				return dictionary.Count;
			}
		}

		public void Set(string key, string value)
		{
			dictionary.Add(key, value);
		}

		public bool Remove(string key)
		{
			return dictionary.Remove(key);
		}

		#endregion
	}
}

