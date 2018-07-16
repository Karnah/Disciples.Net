using System.Data;
using System.Text;

using NDbfReader;


namespace ResourceProvider
{
    public class DataExtractor
    {
        private readonly string _path;

        public DataExtractor(string path)
        {
            _path = path;
        }


        public DataTable GetData(string dbName)
        {
            using (var table = Table.Open($"{_path}\\{dbName}")) {
                return table.AsDataTable(CodePagesEncodingProvider.Instance.GetEncoding(866));
            }
        }
    }
}
