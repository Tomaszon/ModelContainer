using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModelContainer
{
	public abstract class InitableBase : INotifyPropertyChanged
	{
		public void Init()
		{
			Array.ForEach(GetType().GetProperties(), p => p.GetValue(this));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName]string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public void Refresh(string fieldName)
		{
			OnPropertyChanged(fieldName);
		}
	}
}