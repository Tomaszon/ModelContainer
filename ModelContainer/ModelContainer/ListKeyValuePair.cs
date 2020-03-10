using System.Collections.Generic;
using System.Reflection;

namespace ESAWriter.Models
{
	internal class ListKeyValuePair
	{
		public List<string> Key { get; set; } = new List<string>();

		public PropertyInfo Value { get; set; }

		public ListKeyValuePair(string key, PropertyInfo value)
		{
			Key.Add(key);
			Value = value;
		}
	}
}


