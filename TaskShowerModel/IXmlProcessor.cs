using System;
using System.IO;

namespace ConsPlus.TaskShowerModel
{
    /// <summary>
    /// Интерфейс обработчика XML
    /// </summary>
    public interface IXmlProcessor
    {
        /// <summary>
        /// Проверяет XML-файл на соотвествие схеме
        /// </summary>        
        Tuple<bool, string> ValidateSchema(string path, string lineSep);
        /// <summary>
        /// Преобразует XML в XHTML
        /// </summary>        
        Stream RenderXml(string path);
    }
}
