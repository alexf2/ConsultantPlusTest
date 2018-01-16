using System;
using System.Reflection;

namespace Ifx.FoundationHelpers.General
{
	/// <summary>
	/// Представляет вспомогательный класс для работы с атрибутами.
	/// </summary>
	public static class AssemblyAttributes
	{
		public static string Product
		{
			get {
				dynamic attr = getAssemblyAttribute(typeof(AssemblyProductAttribute));
				return attr.Product;
			}
		}

		public static string Copyright
		{
			get {
				dynamic attr = getAssemblyAttribute(typeof(AssemblyCopyrightAttribute));
				return attr.Copyright;
			}
		}

		/// <summary>
		/// По типу атрибута возвращает атрибут, ассоциированный с главной (исполняемой) сборкой приложения.
		/// </summary>
		/// <param name="attrType">Тип атрибута.</param>
		/// <returns>Атрибут, ассоциированный со сборкой.</returns>
		private static Attribute getAssemblyAttribute (Type attrType)
		{
			object[] attrs = Assembly.GetEntryAssembly().GetCustomAttributes(attrType, false);
			return attrs.Length > 0 && (attrs[0] is Attribute) ? (Attribute)attrs[ 0 ]:null;
		}
	}
}
