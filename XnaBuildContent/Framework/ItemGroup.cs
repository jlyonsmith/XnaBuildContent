using System;
using System.Collections;
using System.Collections.Generic;
using ToolBelt;

namespace XnaBuildContent
{
	public class ItemGroup : IEnumerable<KeyValuePair<string, ParsedPathList>>
	{
		#region Fields
		private Dictionary<string, ParsedPathList> dictionary;

		#endregion

		#region Construction
		public ItemGroup()
		{
			dictionary = new Dictionary<string, ParsedPathList>(StringComparer.OrdinalIgnoreCase);
		}

		public ItemGroup(ItemGroup itemGroup) : this()
		{
			IEnumerator<KeyValuePair<string, ParsedPathList>> e = ((IEnumerable<KeyValuePair<string, ParsedPathList>>)itemGroup).GetEnumerator();

			while (e.MoveNext())
			{
				KeyValuePair<string, ParsedPathList> pair = e.Current;

				dictionary.Add(pair.Key, pair.Value);
			}
		}

		#endregion

		#region Methods
		public void ExpandAndAddFromList(List<ContentFileV2.Item> items, PropertyGroup propGroup)
		{
			foreach (var item in items)
			{
				ParsedPathList pathList;

				if (!dictionary.TryGetValue(item.Name, out pathList))
				{
					pathList = new ParsedPathList();
					dictionary.Add(item.Name, pathList);
				}

				string[] parts = item.Include.Split(';');

				foreach (var part in parts)
				{
					ParsedPath pathSpec = new ParsedPath(propGroup.ReplaceVariables(part), PathType.File);
					ParsedPathList paths;

					if (pathSpec.HasWildcards)
					{
						paths = new ParsedPathList(DirectoryUtility.GetFiles(pathSpec, SearchScope.DirectoryOnly));

						foreach (var path in paths)
						{
							pathList.Add(path);
						}
					}
					else
					{
						pathList.Add(pathSpec);
					}
				}

				if (!String.IsNullOrEmpty(item.Exclude))
				{
					parts = item.Exclude.Split(';');

					foreach (var part in parts)
					{
						ParsedPath path = new ParsedPath(propGroup.ReplaceVariables(part), PathType.File);

						pathList.Remove(path);
					}
				}
			}
		}

		#endregion

		#region IEnumerable Implementation
		public IEnumerator GetEnumerator()
		{
			return (IEnumerator)GetEnumerator();
		}

		IEnumerator<KeyValuePair<string, ParsedPathList>> IEnumerable<KeyValuePair<string, ParsedPathList>>.GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		#endregion

		#region Methods Implementation
		public ParsedPathList GetRequiredValue(string name)
		{
			ParsedPathList list;

			if (!dictionary.TryGetValue(name, out list))
				throw new InvalidOperationException("ItemGroup '{0}' not found".CultureFormat(name));

			return list;
		}

		public void Clear()
		{
			dictionary.Clear();
		}

		public bool Remove(KeyValuePair<string, ParsedPathList> item)
		{
			return dictionary.Remove(item.Key);
		}

		public int Count
		{
			get
			{
				return dictionary.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public void Set(string key, ParsedPathList value)
		{
			dictionary.Add(key, value);
		}

		public bool Contains(string key)
		{
			return dictionary.ContainsKey(key);
		}

		public bool Remove(string key)
		{
			return dictionary.Remove(key);
		}
		#endregion
	}
}

