using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ESAWriter.Models
{
	internal class ListKeyDictionary
	{
		private readonly List<ListKeyValuePair> _dic = new List<ListKeyValuePair>();

		public PropertyInfo this[string key]
		{
			get
			{
				return Get(key)?.Value;
			}
		}

		public ListKeyValuePair Get(string key)
		{
			return _dic.FirstOrDefault(p => p.Key.Contains(key));
		}

		public ListKeyValuePair Get(PropertyInfo propertyInfo)
		{
			return _dic.FirstOrDefault(p => p.Value == propertyInfo);
		}

		public ListKeyValuePair GetByPropertyName(string propertyName)
		{
			return _dic.FirstOrDefault(p => p.Value.Name == propertyName);
		}

		public void Add(string key, PropertyInfo propertyInfo)
		{
			if (Get(key) is var k && k == null)
			{
				if (Get(propertyInfo) is var k2 && k2 != null)
				{
					k2.Key.Add(key);
				}
				else
				{
					_dic.Add(new ListKeyValuePair(key, propertyInfo));
				}
			}
			else if (k.Value != propertyInfo)
			{
				throw new ArgumentException("An element with the same key already exists in the collection with different value!");
			}
		}
	}
}