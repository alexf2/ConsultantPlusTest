using System;
using System.Xml;
using System.Text;
using ConsPlus.TaskShowerModel;
using Ifx.FoundationHelpers.General;
using System.Xml.Schema;
using System.IO;
using System.Xml.Xsl;


namespace ConsPlus.TaskShowerRuntime
{
    public sealed class XmlProcessor : IXmlProcessor
    {
        sealed class ValidatorExtension
        {
            public string ValidatingFilter(string xhtml)
            {
                XmlReaderSettings settings = new XmlReaderSettings();                
                settings.ValidationType = ValidationType.None;
                settings.ValidationFlags = XmlSchemaValidationFlags.ProcessInlineSchema | XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ConformanceLevel = System.Xml.ConformanceLevel.Fragment;
                StringBuilder bld = new StringBuilder();
                bool hasError = false;
                settings.ValidationEventHandler += (s, args) =>
                {
                    if (bld.Length > 0)
                        bld.Append("<br/>");
                    bld.Append(args.Message);
                    hasError = true;
                };

                try
                {
                    using (XmlReader sr = XmlReader.Create(new StringReader(xhtml), settings))
                        while (sr.Read()) ;
                }
                catch (Exception ex)
                {
                    hasError = true;
                    if (bld.Length > 0)
                        bld.Append("<br/>");
                    bld.Append(ex.Message);
                }

                return hasError ? string.Format("<div class=\"err\">[{0}]</div>", bld.ToString()) : xhtml;
            }
        };

        XmlReaderSettings _settings;        
        bool _res;
        StringBuilder _bld = new StringBuilder();
        string _lineSep;
        readonly XslCompiledTransform _xslTransform;
        readonly ValidatorExtension _validator = new ValidatorExtension();

        public XmlProcessor ()
        {
            _settings = new XmlReaderSettings();
            //_settings.XmlResolver = _resolver;
            _settings.ValidationType = ValidationType.Schema;
            _settings.ValidationFlags = XmlSchemaValidationFlags.ProcessInlineSchema | XmlSchemaValidationFlags.ReportValidationWarnings;
            _settings.ConformanceLevel = System.Xml.ConformanceLevel.Document;
            _settings.ValidationEventHandler += schemaValidationCallback;
            _settings.Schemas.Add(@"http:/consultant.ru/nris/test", XmlReader.Create(
                new StringReader(
                    ResourceHelpers.GetStringResource("schema.xsd", GetType().Namespace + ".Resources")
            )));
            

            _xslTransform = new XslCompiledTransform(false);
            //_xslTransform.OutputSettings.Encoding = UTF8Encoding.UTF8;
            //_xslTransform.OutputSettings.Indent = true;            
            using (var rd = getTemplate())
                _xslTransform.Load(rd, XsltSettings.TrustedXslt, null);
        }

        XmlReader getTemplate()
		{            
            return XmlReader.Create(
                ResourceHelpers.GetStringResourceAsStream("ConsPlusTasks.xslt", GetType().Namespace + ".Resources")
            );
		}

        void schemaValidationCallback (object sender, ValidationEventArgs e)
        {            
            if (_bld.Length > 0)
                _bld.Append(_lineSep);
            _bld.Append(e.Message);
            _res = false;
        }

        #region IXmlProcessor
        public Tuple<bool, string> ValidateSchema (string path, string lineSep)
        {
            _res = true;
            _bld.Clear();
            _lineSep = lineSep;

            using (XmlReader sr = XmlReader.Create(path, _settings))
                while (sr.Read());

            return new Tuple<bool, string>(_res, _bld.ToString());
        }

        public Stream RenderXml (string path)
        {
            MemoryStream stmOut = new MemoryStream();
            
            using (Stream stm = File.OpenRead(path))
                using (XmlReader rd = XmlReader.Create(stm))
                {
                    XsltArgumentList args = new XsltArgumentList();
                    args.AddExtensionObject("urn:ValidatorExtensionObj", _validator);

                    StreamWriter wrPref = new StreamWriter(stmOut, Encoding.UTF8);
                    wrPref.WriteLine("<!DOCTYPE html>");
                    wrPref.Flush();

                    _xslTransform.Transform(rd, args, stmOut);
                }

            stmOut.Seek(0, SeekOrigin.Begin);
            return stmOut;
        }
        #endregion IXmlProcessor
    }
}
