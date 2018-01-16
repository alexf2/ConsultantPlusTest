using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;


namespace Ifx.FoundationHelpers.General
{
	public abstract class ConfigCollectionBase<TElement>: ConfigurationElementCollection where TElement:ConfigurationElement, new()
	{
		/// <summary>
		/// Возвращает имя элемента в XML.
		/// </summary>
		/// <returns></returns>
		protected abstract string GetElementName ();
		
		public override ConfigurationElementCollectionType CollectionType
		{
			get {return ConfigurationElementCollectionType.BasicMap;}
		}
		/// <summary>
		/// Проверяет принадлежность элемента коллекции.
		/// </summary>
		/// <param name="elementName">Имя элемента, прочитанное из XML.</param>
		/// <returns>true - если XML-элемет принадлежит коллекции.</returns>
		protected override bool IsElementName (string elementName)
		{
			return !String.IsNullOrEmpty(elementName) && elementName == GetElementName();
		}
		/// <summary>
		/// Создаёт новый элемент.
		/// </summary>		
		protected override ConfigurationElement CreateNewElement ()
		{
			return new TElement();
		}
		protected override ConfigurationElement CreateNewElement (string elementName)
		{
			if (GetElementName() == elementName)
			{
				return new TElement();
			}
			else
			{
				throw new Exception("Unsupported element type in custom config setcion: '" + elementName + "'.");
			}
		}

		public TElement this [int index]
		{
			get { return (TElement)BaseGet(index); }
			set{
				if (BaseGet(index) != null) BaseRemoveAt(index);
				BaseAdd(index, value);
			}
		}

		new public TElement this [string name]
		{
			get { return (TElement)BaseGet(name); }
		}

		public override bool Equals (object compareTo)
		{
			ConfigCollectionBase<TElement> o2 = compareTo as ConfigCollectionBase<TElement>;
			if (o2 == null || Count != o2.Count)
			{
				return false;
			}

			return !((this as IEnumerable).OfType<TElement>().Except(o2.OfType<TElement>()).Any());			
		}

		public override int GetHashCode ()
		{
		    return (this as IEnumerable).OfType<TElement>().Aggregate(179, (h, el) => h ^ el.GetHashCode());
		}
	}
}
