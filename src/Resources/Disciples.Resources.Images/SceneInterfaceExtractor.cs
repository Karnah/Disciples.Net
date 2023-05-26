using Disciples.Resources.Common;
using Disciples.Resources.Common.Exceptions;
using Disciples.Resources.Images.Extensions;
using Disciples.Resources.Images.Models;
using Disciples.Resources.Images.Parsers;

namespace Disciples.Resources.Images;

/// <summary>
/// Класс для извлечения данных об интерфейсе сцен/диалогов (.dlg).
/// </summary>
public class SceneInterfaceExtractor : BaseResourceExtractor
{
    private const string DIALOG_HEADER = "DIALOG";
    private const string DIALOG_BEGIN = "BEGIN";
    private const string DIALOG_END = "END";

    private Dictionary<string, SceneInterface> _dialogs = null!;

    private readonly IReadOnlyList<BaseSceneElementParser> _elementParsers = new BaseSceneElementParser[]
    {
        new ImageSceneElementParser(),
        new ButtonSceneElementParser(),
        new TextBlockSceneElementParser()
    };

    /// <summary>
    /// Создать объект типа <see cref="SceneInterfaceExtractor" />.
    /// </summary>
    public SceneInterfaceExtractor(string resourceFilePath) : base(resourceFilePath)
    {
    }

    /// <summary>
    /// Получить данные сцены или диалога.
    /// </summary>
    public SceneInterface GetSceneInterface(string name)
    {
        if (_dialogs.TryGetValue(name, out var sceneInterface))
            return sceneInterface;

        throw new ResourceNotFoundException($"Не найден интерфейс сцены с именем {name}");
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        using var stream = new StreamReader(ResourceFilePath);

        var dialogs = new Dictionary<string, SceneInterface>();
        while (stream.ReadLine() is { } line)
        {
            if (string.IsNullOrEmpty(line))
                continue;

            if (!line.StartsWith(DIALOG_HEADER))
                throw new ResourceException($"Некорректная строка: {line}, ожидалось, что начинается с {DIALOG_HEADER}");

            var dialogHeader = line[(DIALOG_HEADER.Length + 1)..];
            var sceneInterface = ParseSceneInterface(dialogHeader, stream);
            dialogs.Add(sceneInterface.Name, sceneInterface);
        }

        _dialogs = dialogs;
    }

    /// <summary>
    /// Распарсить информацию интерфейса.
    /// </summary>
    private SceneInterface ParseSceneInterface(string dialogHeader, StreamReader stream)
    {
        var begin = stream.ReadLine();
        if (begin != DIALOG_BEGIN)
            throw new ResourceException($"Некорректная строка: {begin}, ожидалось: {DIALOG_BEGIN}");

        // Сцена может занимать меньше места, чем на 800*600.
        // Координаты объектов при этом будут рассчитываться от нового границы сцены.
        // Пересчитываем их глобальные.
        // TODO Использовать константы из GameInfo.
        var headerData = dialogHeader.Split(',');
        var bounds = headerData.ParseBounds(1);
        var offsetX = (800 - bounds.Width) / 2;
        var offsetY = (600 - bounds.Height) / 2;

        var elements = new List<SceneElement>();
        while (stream.ReadLine() is { } line)
        {
            if (string.IsNullOrEmpty(line))
                continue;

            if (line == DIALOG_END)
                break;

            line = line.TrimStart('\t');

            var elementTypeLength = line.IndexOf('\t');
            var elementTypeName = line[..elementTypeLength];
            var elementParser = _elementParsers.FirstOrDefault(elementParser => elementParser.ElementTypeName == elementTypeName);
            if (elementParser == null)
                continue;

            var elementData = line[(elementTypeName.Length + 1)..];
            elements.Add(elementParser.Parse(elementData, offsetX, offsetY));
        }

        return new SceneInterface
        {
            Name = headerData[0],
            Bounds = bounds,
            BackgroundImageName = headerData[5].ParseImageName(),
            CursorImageName = headerData[6].ParseImageName(),
            CursorHotSpot = headerData.ParsePoint(7),
            Position = headerData.ParseBounds(9),
            IsSelfDrawn = headerData[13].ParseBoolean(),
            Elements = elements
        };
    }
}