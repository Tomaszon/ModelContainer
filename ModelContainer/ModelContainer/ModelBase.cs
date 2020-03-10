using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ESAWriter.Models
{
	/// <summary>
	/// Base class for model classes to inherit from./>
	/// </summary>
	public class ModelBase : INotifyPropertyChanged
	{
		private readonly Dictionary<string, object> _vars = new Dictionary<string, object>();

		/// <summary>
		/// Sets the value of the corresponding variable.
		/// </summary>
		/// <param name="value">New value</param>
		/// <param name="name">Do NOT modify this parameter! The [CallerMemberName] attribute will handle this.</param>
		protected void Set(object value, [CallerMemberName] string name = "propertyName")
		{
			if (_vars.ContainsKey(name))
			{
				_vars.Remove(name);
			}

			_vars.Add(name, value);

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
			if (!_vars.ContainsKey(name))
			{
				_vars.Add(name, defaultValue);
			}

			return (T)_vars[name];
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName]string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}


