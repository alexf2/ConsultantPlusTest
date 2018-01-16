using System;
using System.Collections;
using System.Collections.Generic;

namespace Ifx.FoundationHelpers.General
{
	/// <summary>
	/// Разбивает коллекцию на равные части заданного размера и отдаёт коллекцию массивов.
	/// </summary>
	/// <typeparam name="T">Тип элементов коллекции</typeparam>
	public class Partitioner<T>: IEnumerable<T[]>, IDisposable
	{
		readonly int _size;
		readonly IEnumerable<T> _en;
		IEnumerator<T> _curr;

		public Partitioner (IEnumerable<T> enumerable, int size)
		{
			_size = size;
			_en = enumerable;

			_curr = _en.GetEnumerator();
		}

		public IEnumerator<T[]> GetEnumerator ()
		{						
			bool end = false;
			while (!end)
			{
				int count = 0;
				List<T> tmp = new List<T>(_size);

				while (count++ < _size)
				{
					if (_curr.MoveNext())
					{
						tmp.Add(_curr.Current);
					}
					else
					{
						end = true;
						break;
					}
				}
				yield return tmp.ToArray();
			}
		}
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return (IEnumerator)GetEnumerator();
		}

		public void Dispose ()
		{
			if (_curr != null)
			{
				_curr.Dispose();
				_curr = null;
			}
		}
	}

}
