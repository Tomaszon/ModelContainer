using ModelContainer;
using System;
using System.ComponentModel;

namespace ESAWriter.Models
{
	/// <summary>
	/// Wrapper class for model viewmodel classes.
	/// </summary>
	/// <typeparam name="TViewModel">Type of the viewmodel</typeparam>
	/// <typeparam name="TModel">Type of the model.</typeparam>
	public class ModelContainer<TViewModel, TModel> where TModel : InitableBase, INotifyPropertyChanged, new() where TViewModel : InitableBase
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

			ViewModel.Init();

			Model.Init();
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
}