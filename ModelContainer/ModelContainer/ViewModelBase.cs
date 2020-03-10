using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ESAWriter.Models
{
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
}


