using System.IO;
using System.Runtime.Serialization.Json;

namespace DST_PMPortal.Services
{
    class JsonOperations
    {
        internal static void WritePortfolioToFile(string _filePath, Portfolio _Portfolio)
        {
            using (var stream = new FileStream(_filePath, FileMode.Create, FileAccess.ReadWrite))
            {
                var ser = new DataContractJsonSerializer(typeof(Portfolio));
                ser.WriteObject(stream, _Portfolio);
            }
        }
        internal static Portfolio ReadPortfolioFromFile(string _filePath, Portfolio _Portfolio)
        {
            using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
            {
                var ser = new DataContractJsonSerializer(typeof(Portfolio));
                _Portfolio = (Portfolio)ser.ReadObject(stream);
            }
            return _Portfolio;
        }
    }

}
