using Disciples.Engine.Common.Controllers;

namespace Disciples.Engine.Base
{
    /// <summary>
    /// Расширенный класс для контроллера сцены, поддерживающий инициализацию.
    /// </summary>
    /// <typeparam name="TData">Тип параметров инициализации.</typeparam>
    public interface ISceneController<in TData> : ISceneController, ISupportLoading<ISceneContainer, TData>
    {
    }
}