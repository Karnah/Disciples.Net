using DryIoc;

namespace Disciples.Engine.Base;

/// <summary>
/// Модуль для регистрации зависимостей.
/// </summary>
public interface IGameModule
{
    /// <summary>
    /// Зарегистрировать все необходимые зависимости для модуля сцены.
    /// </summary>
    void Initialize(IRegistrator containerRegistrator);
}