using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace Ifx.FoundationHelpers.General
{
	/// <summary>
	/// Представляет хэлперы для работы с перечислениями.
	/// </summary>
	public static class EnumUtils
	{
		/// <summary>
		/// Возвращает все значения перечисления.
		/// </summary>
		public static IEnumerable<T> GetValues<T> ()
		{
			foreach (FieldInfo fieldInfo in typeof(T).GetFields(BindingFlags.Static|BindingFlags.Public))
			{
				yield return (T)fieldInfo.GetValue(typeof(T));
			}
		}

		/// <summary>
		/// Возвращает описание элемента перечисления.
		/// </summary>
		/// <typeparam name="T">Тип перечисления</typeparam>
		/// <param name="enumValue">Поле.</param>
		/// <returns>Текстовое описание из DescriptionAttribute.</returns>
		public static string GetDescription<T> (T enumValue) where T: struct, IConvertible
		{
			FieldInfo fi = typeof(T).GetField(enumValue.ToString());
			DescriptionAttribute da = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));
			return da == null ? string.Empty:da.Description;
		}

		public static TAttr GetMemberAttribute<TEnum, TAttr> (this TEnum val) where TEnum: struct, IComparable where TAttr: Attribute
		{
			CheckIsEnum<TEnum>();

			MemberInfo info = val.GetType().GetMember(val.ToString(), MemberTypes.Field, BindingFlags.Public|BindingFlags.Static).FirstOrDefault();
			if (info == null)
				throw new ArgumentException(string.Format("Enum '{0}' does not have the member '{1}'", typeof(TEnum).FullName, val));

			TAttr res = (TAttr)info.GetCustomAttributes(typeof(TAttr), false).FirstOrDefault();
			return res;
		}

		public static TAttr[] GetMemberAttributes<TEnum, TAttr> (this TEnum val) where TEnum: struct, IComparable where TAttr: Attribute
		{
			CheckIsEnum<TEnum>();

			MemberInfo info = val.GetType().GetMember(val.ToString(), MemberTypes.Field, BindingFlags.Public|BindingFlags.Static).FirstOrDefault();
			if (info == null)
				throw new ArgumentException(string.Format("Enum '{0}' does not have the member '{1}'", typeof(TEnum).FullName, val));

			return info.GetCustomAttributes(typeof(TAttr), false).Select( (a) => (TAttr)a ).ToArray();
		}

		public static void CheckIsEnum<T> (bool withFlags = false)
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(string.Format("Type '{0}' is not an enum", typeof(T).FullName));

            if (withFlags)
				if (!Attribute.IsDefined(typeof(T), typeof(FlagsAttribute)))
					throw new ArgumentException(string.Format("Type '{0}' does not have the 'Flags' attribute", typeof(T).FullName));
        }

	}
}
