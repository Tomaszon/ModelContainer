using ModelContainer;
using System.Runtime.CompilerServices;

namespace ESAWriter.Models
{
	/// <summary>
	/// Base class for model classes to inherit from./>
	/// </summary>
	public class ModelBase : InitableBase
	{
		private readonly ModelVariableDictionary _vars = new ModelVariableDictionary();

		/// <summary>
		/// Sets the value of the corresponding variable.
		/// </summary>
		/// <param name="value">New value</param>
		/// <param name="name">Do NOT modify this parameter! The [CallerMemberName] attribute will handle this.</param>
		protected void Set(object value, [CallerMemberName] string name = "propertyName")
		{
			_vars.Get(name).Value = value;

			OnPropertyChanged(name);
		}

		/// <summary>
		/// Returns the value of the corresponding variable.
		/// </summary>
		/// <typeparam name="T">Type of the variable.</typeparam>
		/// <param name="defaultValue">Default value.</param>
		/// <param name="name">Do NOT modify this parameter! The [CallerMemberName] attribute will handle this.</param>
		/// <returns>Returns the value of the corresponding variable.</returns>
		protected T Get<T>(T defaultValue = default, [CallerMemberName] string name = "propertyName")
		{
			if (_vars.Get(name) is var var && var is null)
			{
				_vars.Add(name, defaultValue, defaultValue);
			}

			return (T)_vars[name];
		}

		public T GetDefaultValue<T>(string name)
		{
			return (T)_vars.Get(name).DefaultValue;
		}
	}
}