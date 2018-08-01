using System.Collections.Generic;

namespace Engine
{
    public interface IGame
    {
        /// <summary>
        /// Все объекты, размещённые на сцене
        /// </summary>
        IReadOnlyCollection<GameObject> GameObjects { get; }


        /// <summary>
        /// Инициализировать новый объект и разместить его на сцене
        /// </summary>
        void CreateObject(GameObject gameObject);

        /// <summary>
        /// Убрать объект со сцены
        /// </summary>
        void DestroyObject(GameObject gameObject);


        /// <summary>
        /// Полностью очистить сцену от всех объектов
        /// </summary>
        void ClearScene();
    }
}
