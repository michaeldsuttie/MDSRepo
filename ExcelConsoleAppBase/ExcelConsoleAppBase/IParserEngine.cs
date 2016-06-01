using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelConsoleAppBase
{
    public interface IParserEngine
    {
        IList<string> ExtractRecords(char lineDelimiter, string csvText);
        IList<string> ExtractFields(char delimiter, char quote, string csvLine);
    }
}
