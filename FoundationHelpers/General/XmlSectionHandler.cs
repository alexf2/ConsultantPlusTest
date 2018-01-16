using System.Xml.Serialization;
using System.Xml;
using System.Configuration;

namespace Ifx.FoundationHelpers.General
{
	public class XmlSectionHandler<T>: IConfigurationSectionHandler
	{
		#region Properties and fields

		XmlSerializer _serializer = new XmlSerializer(typeof(T));		

		#endregion

		#region IConfigurationSectionHandler Members

		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			return _serializer.Deserialize(new XmlNodeReader(section));
		}

		#endregion
	}
}
