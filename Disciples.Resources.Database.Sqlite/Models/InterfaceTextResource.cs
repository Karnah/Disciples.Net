﻿namespace Disciples.Resources.Database.Sqlite.Models;

/// <summary>
/// Текстовая информация для интерфейса.
/// </summary>
public class InterfaceTextResource : IEntity
{
    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Текст.
    /// </summary>
    public string Text { get; init; } = null!;

    /// <summary>
    /// Верифицирована ли запись.
    /// </summary>
    /// <remarks>
    /// Никак не используется.
    /// </remarks>
    public bool IsVerified { get; init; }

    /// <summary>
    /// Контекст записи.
    /// </summary>
    /// <remarks>
    /// Никак не используется.
    /// </remarks>
    public string? Context { get; init; }
}