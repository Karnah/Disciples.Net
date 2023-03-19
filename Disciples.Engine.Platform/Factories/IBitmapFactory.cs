using Disciples.Engine.Common.Models;
using Disciples.Engine.Platform.Models;
using Disciples.Resources.Images.Models;

namespace Disciples.Engine.Platform.Factories;

/// <summary>
/// Фабрика для создания изображений.
/// </summary>
public interface IBitmapFactory
{
    /// <summary>
    /// Загрузить изображение из массива байт.
    /// </summary>
    /// <param name="bitmapData">Массив байт (формат предположительно PNG).</param>
    IBitmap FromByteArray(byte[] bitmapData);

    /// <summary>
    /// Загрузить изображение из файла.
    /// </summary>
    /// <param name="filePath">Путь до файла.</param>
    IBitmap FromFile(string filePath);

    /// <summary>
    /// Конвертировать изображение из сырых данных.
    /// </summary>
    /// <param name="rawBitmap">Сырая информация об изображении.</param>
    /// <param name="bounds">Границы изображения.</param>
    Frame FromRawBitmap(RawBitmap rawBitmap, Bounds? bounds = null);


    /// <summary>
    /// Сохранить изображение в файл на диске.
    /// </summary>
    /// <param name="bitmap">Изображение, которое необходимо сохранить.</param>
    /// <param name="filePath">Путь до файла.</param>
    /// <remarks>
    /// Используется только для тестов и подготовки данных.
    /// </remarks>
    void SaveToFile(IBitmap bitmap, string filePath);
}