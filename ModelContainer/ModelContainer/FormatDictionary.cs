using System;
using System.Collections.Generic;
using System.Text;

namespace ModelContainer
{
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