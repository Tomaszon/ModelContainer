using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace ESAWriter.Models
{
	public class ModelContainer<TViewModel, TModel> where TModel : INotifyPropertyChanged, new() where TViewModel : IViewModelInit
	{
		public TViewModel ViewModel { get; }

		public TModel Model { get; }

		public ModelContainer()
		{
			Model = new TModel();

			ViewModel = (TViewModel)Activator.CreateInstance(typeof(TViewModel), Model);

			ViewModel.InitAccessors();
		}
	}

	public class ViewModel<TModel> : ViewModelBase<TModel> where TModel : INotifyPropertyChanged
	{
		public string A { get { return Get<string, int>(p => _formatDictionary.Format(nameof(A), p, (B * 2).ToString() + "zz0")); } }

		public int B { get { return Get<int>(p => p * 2, nameof(Model.A)); } }

		public int C { get { return Get<int, int>(s=> s, nameof(Model.A)); } set { Set(value); } }

		public int D { get { return Get<int>(); } set { Set(value); } }

		public ViewModel(TModel model) : base(model) { }
	}

	public abstract class ViewModelBase<TModel> : IViewModelInit, INotifyPropertyChanged where TModel : INotifyPropertyChanged
	{
		private readonly ListKeyDictionary _accessors = new ListKeyDictionary();

		private readonly Dictionary<string, PropertyInfo> _modelProperties = new Dictionary<string, PropertyInfo>();

		private readonly TModel _model;

		public FormatDictionary _formatDictionary = new FormatDictionary();

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

		public void AddFormat(string key, string value)
		{
			_formatDictionary.Add(key, value);
		}

		public void RemoveFormat(string key)
		{
			_formatDictionary.Remove(key);
		}

		public void ModifyFormat(string key, string newValue)
		{
			_formatDictionary.Modify(key, newValue);
		}

		protected void Set(object value, [CallerMemberName]string accessorName = null)
		{
			PropertyInfo pi = _accessors[accessorName];

			pi.SetValue(_model, value);
		}

		protected TOut Get<TOut, TStored>(Func<TStored, TOut> transform, [CallerMemberName] string propertyName = null, [CallerMemberName] string accessorName = null)
		{
			PropertyInfo pi = _modelProperties[propertyName];
			_accessors.Add(accessorName, pi);

			return transform is null ? (TOut)_accessors[accessorName].GetValue(_model) : transform.Invoke((TStored)_accessors[accessorName].GetValue(_model));
		}

		protected T Get<T>(Func<T, T> transform, [CallerMemberName] string propertyName = null, [CallerMemberName] string accessorName = null)
		{
			return Get<T, T>(transform, propertyName, accessorName);
		}

		protected T Get<T>([CallerMemberName] string propertyName = null, [CallerMemberName] string accessorName = null)
		{
			return Get<T>(p => p, propertyName, accessorName);
		}

		private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertiesChanged(_accessors.GetByPropertyName(e.PropertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName]string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		protected void OnPropertiesChanged(ListKeyValuePair keyValuePair)
		{
			keyValuePair.Key.ForEach(k => OnPropertyChanged(k));
		}
	}

	public interface IViewModelInit
	{
		void InitAccessors();
	}

	public class Model : ModelBase
	{
		public int A { get { return Get<int>(); } set { Set(value); } }


		public int B { get { return Get<int>(); } set { Set(value); } }
	}

	public class ModelBase : INotifyPropertyChanged
	{
		private readonly Dictionary<string, object> _vars = new Dictionary<string, object>();

		protected void Set(object value, [CallerMemberName]string name = null)
		{
			if (_vars.ContainsKey(name))
			{
				_vars.Remove(name);
			}

			_vars.Add(name, value);

			OnPropertyChanged(name);
		}

		protected T Get<T>([CallerMemberName] string name = null)
		{
			if (!_vars.ContainsKey(name))
			{
				_vars.Add(name, default(T));
			}

			return (T)_vars[name];
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName]string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}

	public class ListKeyDictionary
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

	public class ListKeyValuePair
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

		public string this[string key, int argsCount = 1]
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

		public void Add(string key, string format)
		{
			_dic.Add(key, format);
		}

		public void Remove(string key)
		{
			_dic.Remove(key);
		}

		public void Modify(string key, string newValue)
		{
			Remove(key);

			Add(key, newValue);
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


