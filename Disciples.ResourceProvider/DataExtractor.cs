using System.Data;
using System.Text;
using NDbfReader;

namespace Disciples.ResourceProvider;

/// <summary>
/// Класс для извлечения данных из текстовых ресурсов.
/// </summary>
public class DataExtractor
{
    private readonly string _path;

    /// <inheritdoc />
    public DataExtractor(string path)
    {
        _path = path;
    }


    /// <summary>
    /// Извлечь данные из указанной таблицы.
    /// </summary>
    public DataTable GetData(string dbName)
    {
        using (var table = Table.Open($"{_path}\\{dbName}")) {
            return table.AsDataTable(CodePagesEncodingProvider.Instance.GetEncoding(866));
        }
    }
}