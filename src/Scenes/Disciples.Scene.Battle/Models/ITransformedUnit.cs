using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Превращённый юнит.
/// </summary>
internal interface ITransformedUnit
{
    /// <summary>
    /// Исходный юнит, который был превращён.
    /// </summary>
    Unit OriginalUnit { get; }

    /// <summary>
    /// Во что юнит превращён.
    /// </summary>
    /// <remarks>
    /// Свойство заведено для удобства, чтобы постоянно не cast'ить от интерфейса к  классу <see cref="Engine.Common.Models.Unit" />.
    /// </remarks>
    Unit Unit { get; }
}