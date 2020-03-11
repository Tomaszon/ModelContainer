using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ModelContainer
{
	/// <summary>
	/// Base class for viewmodel classes to inherit from.
	/// </summary>
	/// <typeparam name="TModel">Type of model.</typeparam>
	public abstract class ViewModelBase : InitableBase
	{
		private readonly ViewModelPropertyDictionary _accessors = new ViewModelPropertyDictionary();

		private readonly Dictionary<string, PropertyInfo> _modelProperties = new Dictionary<string, PropertyInfo>();

		private readonly ModelBase _model;

		/// <summary>
		/// Provides format strings for getter accessors.
		/// </summary>
		public FormatDictionary FormatStrings { get; } = new FormatDictionary();

		public ViewModelBase(ModelBase model)
		{
			model.PropertyChanged += Model_PropertyChanged;

			_model = model;

			Array.ForEach(model.GetType().GetProperties(), p => _modelProperties.Add(p.Name, p));
		}

		/// <summary>
		/// Sets the value of the corresponding variable of the underlying model.
		/// </summary>
		/// <param name="value">New value.</param>
		/// <param name="inverseTransform">Transformation func from TOut to TSource. If no type conversion needed use the single template format.</param>
		/// <param name="setDefaultOnError">Defines wheter to return the given default value of the variable or throw exception if an error occurs in the inverse transformation.</param>
		/// <param name="accessorName">Do NOT modify this parameter! The [CallerMemberName] attribute will handle this.</param>
		/// <exception cref="Exception">Throws exception if something went wrong with the inverse transformation.</exception> 
		protected void Set<TOut, TStored>(TOut value, Func<TOut, TStored> inverseTransform, [CallerMemberName] string accessorName = "accessorName")
		{
			var property = _accessors[accessorName];

			property.SetValue(_model, inverseTransform is null ? value : (object)inverseTransform.Invoke(value));

		}

		protected void Set<T>(T value, Func<T, T> inverseTransform = null, [CallerMemberName] string accessorName = "accessorName")
		{
			Set<T, T>(value, inverseTransform, accessorName);
		}

		/// <summary>
		/// Returns the value of the corresponding variable of the underlying model.
		/// </summary>
		/// <typeparam name="TOut">Type of the viewmodel accessor which given to the view.</typeparam>
		/// <typeparam name="TStored">Type of the variable of the underlying model.</typeparam>
		/// <param name="transform">Transformation func from TSource to TOut. If no type conversion needed use the single template format.</param>
		/// <param name="propertyName">Name of linked model property.</param>
		/// <param name="accessorName">Do NOT modify this parameter! The [CallerMemberName] attribute will handle this.</param>
		/// <exception cref="InvalidCastException">Throws InvalidCastException if no implicit conversion found between TSource and TOut types or the underlying property type differs from the given TStored.</exception>
		/// <returns>Returns the value of the corresponding variable.</returns>
		protected TOut Get<TStored, TOut>(Func<TStored, TOut> transform, [CallerMemberName] string propertyName = null, [CallerMemberName] string accessorName = "accessorName")
		{
			CacheModelVariable(propertyName, accessorName);

			var property = _accessors[accessorName];

			return transform is null ? (TOut)property.GetValue(_model) : transform.Invoke((TStored)property.GetValue(_model));
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
		protected T Get<T>([CallerMemberName] string propertyName = null, Func<T, T> transform = null, [CallerMemberName] string accessorName = "accessorName")
		{
			return Get(transform, propertyName, accessorName);
		}

		private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertiesChanged(_accessors.GetByPropertyName(e.PropertyName));
		}

		internal void OnPropertiesChanged(ViewModelProperty keyValuePair)
		{
			keyValuePair.AccessorNames.ForEach(k => OnPropertyChanged(k));
		}

		private void CacheModelVariable(string propertyName, string accessorName)
		{
			PropertyInfo pi = _modelProperties[propertyName];
			_accessors.Add(accessorName, pi, pi.GetValue(_model));
		}
	}
}