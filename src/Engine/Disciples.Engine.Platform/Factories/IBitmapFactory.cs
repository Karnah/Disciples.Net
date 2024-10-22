﻿using System.Drawing;
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
    IBitmap FromRawBitmap(RawBitmap rawBitmap, Rectangle? bounds = null);


    /// <summary>
    /// Сохранить изображение в файл на диске.
    /// </summary>
    /// <param name="bitmap">Изображение, которое необходимо сохранить.</param>
    /// <param name="fileName">Имя файла.</param>
    /// <remarks>
    /// Используется только для тестов и подготовки данных.
    /// </remarks>
    void SaveToFile(IBitmap bitmap, string fileName);
}