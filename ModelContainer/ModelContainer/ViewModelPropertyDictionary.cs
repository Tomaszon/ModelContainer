using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModelContainer
{
	internal class ViewModelPropertyDictionary
	{
		private readonly List<ViewModelProperty> _dic = new List<ViewModelProperty>();

		public PropertyInfo this[string key]
		{
			get
			{
				return Get(key)?.PropertyInfo;
			}
		}

		public ViewModelProperty Get(string key)
		{
			return _dic.FirstOrDefault(p => p.AccessorNames.Contains(key));
		}

		public ViewModelProperty Get(PropertyInfo propertyInfo)
		{
			return _dic.FirstOrDefault(p => p.PropertyInfo == propertyInfo);
		}

		public ViewModelProperty GetByPropertyName(string propertyName)
		{
			return _dic.FirstOrDefault(p => p.PropertyInfo.Name == propertyName);
		}

		public void Add(string key, PropertyInfo propertyInfo, object defaultValue)
		{
			if (Get(key) is var k && k == null)
			{
				if (Get(propertyInfo) is var k2 && k2 != null)
				{
					k2.AccessorNames.Add(key);
				}
				else
				{
					_dic.Add(new ViewModelProperty(key, propertyInfo, defaultValue));
				}
			}
			else if (k.PropertyInfo != propertyInfo)
			{
				throw new ArgumentException("An element with the same key already exists in the collection with different value!");
			}
		}
	}
}