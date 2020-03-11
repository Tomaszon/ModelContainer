using System.Collections.Generic;
using System.Reflection;

namespace ModelContainer
{
	internal class ViewModelProperty
	{
		public List<string> AccessorNames { get; } = new List<string>();

		public PropertyInfo PropertyInfo { get; }

		public object DefaultValue { get; }

		public ViewModelProperty(string accessorNames, PropertyInfo propertyInfo, object defaultValue)
		{
			AccessorNames.Add(accessorNames);
			PropertyInfo = propertyInfo;
			DefaultValue = defaultValue;
		}
	}
}