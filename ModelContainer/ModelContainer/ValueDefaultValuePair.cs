namespace ESAWriter.Models
{
	internal class ValueDefaultValuePair<T>
	{
		public T Value { get; set; }

		public T DefaultValue { get; }

		public ValueDefaultValuePair(T value, T defaultValue)
		{
			Value = value;
			DefaultValue = defaultValue;
		}
	}
}