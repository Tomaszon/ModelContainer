using System;
using System.Collections.Generic;

namespace ESAWriter.Models
{
	internal class ModelVariableDictionary
	{
		private readonly Dictionary<string, ValueDefaultValuePair<object>> _dic = new Dictionary<string, ValueDefaultValuePair<object>>();

		public object this[string key]
		{
			get
			{
				return Get(key)?.Value;
			}
		}

		public ValueDefaultValuePair<object> Get(string key)
		{
			return _dic[key];
		}

		public void Add<T>(string key, T value, T defaultValue)
		{
			if (Get(key) is var k && k == null)
			{
				_dic.Add(key, new ValueDefaultValuePair<object>(value, defaultValue));
			}
			else
			{
				throw new ArgumentException("An element with the same key already exists in the collection with different value!");
			}
		}
	}
}