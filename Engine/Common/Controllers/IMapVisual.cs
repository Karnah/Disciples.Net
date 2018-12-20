using System.Collections.Generic;
using Engine.Common.VisualObjects;

namespace Engine.Common.Controllers
{
    /// <summary>
    /// Интерфейс для объектов, которые отрисовываются на сцене.
    /// </summary>
    public interface IMapVisual
    {
        /// <summary>
        /// Список всех объектов на сцене.
        /// </summary>
        IReadOnlyCollection<VisualObject> Visuals { get; }


        /// <summary>
        /// Добавить объект на сцену.
        /// </summary>
        void AddVisual(VisualObject visual);

        /// <summary>
        /// Удалить объект со сцены.
        /// </summary>
        /// <param name="visual"></param>
        void RemoveVisual(VisualObject visual);
    }
}