using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace ESAWriter.Models
{
	/// <summary>
	/// Wrapper class for model viewmodel classes.
	/// </summary>
	/// <typeparam name="TViewModel">Type of the viewmodel</typeparam>
	/// <typeparam name="TModel">Type of the model.</typeparam>
	public class ModelContainer<TViewModel, TModel> where TModel : class, INotifyPropertyChanged, new() where TViewModel : class, IViewModelInit
	{
		/// <summary>
		/// The transformed model.
		/// </summary>
		public TViewModel ViewModel { get; private set; }

		/// <summary>
		/// The underlying model.
		/// </summary>
		public TModel Model { get; private set; }

		/// <summary>
		/// Creates new instances of templated types if no instances passed as parameter.
		/// </summary>
		/// <param name="model">Optional existing model.</param>
		/// <param name="viewModel">Optional existing viewmodel.</param>
		public ModelContainer(TModel model = null, TViewModel viewModel = null)
		{
			Model = model ?? new TModel();

			ViewModel = viewModel ?? (TViewModel)Activator.CreateInstance(typeof(TViewModel), Model);

			ViewModel.InitAccessors();
		}

		public void ChangeModel(TModel newModel)
		{
			//TODO
		}

		public void ChangeViewModel(TViewModel newViewModel)
		{
			//TODO
		}
	}

	/// <summary>
	/// Base class for viewmodels classes to inherit from.
	/// </summary>
	/// <typeparam name="TModel">Type of model.</typeparam>
	public abstract class ViewModelBase<TModel> : IViewModelInit, INotifyPropertyChanged where TModel : class, INotifyPropertyChanged
	{
		private readonly ListKeyDictionary _accessors = new ListKeyDictionary();

		private readonly Dictionary<string, PropertyInfo> _modelProperties = new Dictionary<string, PropertyInfo>();

		private readonly TModel _model;

		/// <summary>
		/// Provides format strings for getter accessors.
		/// </summary>
		public FormatDictionary FormatStrings { get; } = new FormatDictionary();

		public ViewModelBase(TModel model)
		{
			model.PropertyChanged += Model_PropertyChanged;

			_model = model;

			Array.ForEach(model.GetType().GetProperties(), p => _modelProperties.Add(p.Name, p));
		}

		public void InitAccessors()
		{
			Array.ForEach(GetType().GetProperties(), p => p.GetValue(this));
		}

		/// <summary>
		/// Sets the value of the corresponding variable of the underlying model.
		/// </summary>
		/// <param name="value">New value.</param>
		/// <param name="accessorName">Do NOT modify this parameter! The [CallerMemberName] attribute will handle this.</param>
		protected void Set(object value, [CallerMemberName] string accessorName = "accessorName")
		{
			PropertyInfo pi = _accessors[accessorName];

			pi.SetValue(_model, value);
		}

		/// <summary>
		/// Returns the value of the corresponding variable of the underlying model.
		/// </summary>
		/// <typeparam name="TOut">Type of the viewmodel accessor which given to the view.</typeparam>
		/// <typeparam name="TStored">Type of the variable of the underlying model.</typeparam>
		/// <param name="transform">Transformation func from TSource to TOut. If no type conversion needed use the single template format.</param>
		/// <param name="propertyName">Name of linked model property.</param>
		/// <param name="accessorName">Do NOT modify this parameter! The [CallerMemberName] attribute will handle this.</param>
		/// <exception cref="InvalidCastException">Throws InvalidCastException if no implicit conversion found between TSource and TOut types or the underlying property type differs from give TStored.</exception>
		/// <returns>Returns the value of the corresponding variable.</returns>
		protected TOut Get<TOut, TStored>(Func<TStored, TOut> transform, [CallerMemberName] string propertyName = null, [CallerMemberName] string accessorName = "accessorName")
		{
			PropertyInfo pi = _modelProperties[propertyName];
			_accessors.Add(accessorName, pi);

			return transform is null ? (TOut)_accessors[accessorName].GetValue(_model) : transform.Invoke((TStored)_accessors[accessorName].GetValue(_model));
		}

		/// <summary>
		/// Returns the value of the corresponding variable of the underlying model.
		/// </summary>
		/// <typeparam name="T">Type of the viewmodel accessor and the underlying model property which given to the view.</typeparam>
		/// <param name="transform">Transformation func from TSource to TOut. If no transformation needed use the single propertyName format.</param>
		/// <param name="propertyName">Name of linked model property.</param>
		/// <param name="accessorName">Do NOT modify this parameter! The [CallerMemberName] attribute will handle this.</param>
		/// <exception cref="InvalidCastException">Throws InvalidCastException if the underlying property type differs from give T.</exception>
		/// <returns>Returns the value of the corresponding variable.</returns>
		protected T Get<T>(Func<T, T> transform, [CallerMemberName] string propertyName = null, [CallerMemberName] string accessorName = "accessorName")
		{
			return Get<T, T>(transform, propertyName, accessorName);
		}

		/// <summary>
		/// Returns the value of the corresponding variable of the underlying model.
		/// </summary>
		/// <typeparam name="T">Type of the viewmodel accessor and the underlying model property which given to the view.</typeparam>
		/// <param name="propertyName">Name of linked model property.</param>
		/// <param name="accessorName">Do NOT modify this parameter! The [CallerMemberName] attribute will handle this.</param>
		/// <exception cref="InvalidCastException">Throws InvalidCastException if the underlying property type differs from give T.</exception>
		/// <returns></returns>
		protected T Get<T>([CallerMemberName] string propertyName = null, [CallerMemberName] string accessorName = "accessorName")
		{
			return Get<T>(p => p, propertyName, accessorName);
		}

		private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertiesChanged(_accessors.GetByPropertyName(e.PropertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		internal void OnPropertyChanged([CallerMemberName]string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		internal void OnPropertiesChanged(ListKeyValuePair keyValuePair)
		{
			keyValuePair.Key.ForEach(k => OnPropertyChanged(k));
		}
	}

	public interface IViewModelInit
	{
		void InitAccessors();
	}

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

		public List<string> this[PropertyInfo propertyInfo]
		{
			get
			{
				return Get(propertyInfo)?.Key;
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

	public class FormatDictionary
	{
		private readonly Dictionary<string, string> _dic = new Dictionary<string, string>();

		internal FormatDictionary() { }

		internal string this[string key, int argsCount = 1]
		{
			get
			{
				if (key is null)
				{
					return BuildDefaultFormatString(argsCount);
				}

				return _dic.TryGetValue(key, out string s) ? s : BuildDefaultFormatString(argsCount);
			}
		}

		/// <summary>
		/// Replaces the format item in a specified string with the string representation of a corresponding object in a specified array.
		/// </summary>
		/// <param name="formatStringKey">Key for the dictionary to look after.</param>
		/// <param name="args">An object array that contains zero or more objects to format.</param>
		/// <returns>A copy of format in which the format items have been replaced by the string representation of the corresponding objects in args.</returns>
		public string Format(string formatStringKey, params object[] args)
		{
			string f = this[formatStringKey, args.Length];
			try
			{
				return string.Format(f, args);
			}
			catch
			{
				return string.Format(BuildDefaultFormatString(args.Length), args);
			}
		}

		/// <summary>
		/// Adds a format option to the viewmodel.
		/// </summary>
		/// <param name="key">Key for the format string to be refered from accessors.</param>
		/// <param name="value">Format string.</param>
		/// <exception cref="ArgumentException">Throws ArgumentException if an element with the same key already exists in the collection.</exception>
		public void Add(string key, string format)
		{
			if (key is null)
			{
				return;
			}

			_dic.Add(key, format);
		}

		/// <summary>
		/// Removes a format option from the viewmodel.
		/// </summary>
		/// <param name="key">Key for the format to be removed.</param>
		public void Remove(string key)
		{
			if (key is null)
			{
				return;
			}

			_dic.Remove(key);
		}

		/// <summary>
		/// Modifies the value of a format string.
		/// </summary>
		/// <param name="key">Key for the format to be modeified.</param>
		/// <param name="newValue">New format string.</param>
		public void Modify(string key, string newValue)
		{
			Remove(key);

			Add(key, newValue);
		}

		/// <summary>
		/// Removes all format option from the viewmodel.
		/// </summary>
		public void Clear()
		{
			_dic.Clear();
		}

		private string BuildDefaultFormatString(int count)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < count; i++)
			{
				sb.Append($"{{{i}}} ");
			}

			return sb.ToString().TrimEnd(' ');
		}
	}
}


