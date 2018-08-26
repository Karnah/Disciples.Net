using System;
using System.Collections.Generic;

using Engine.Common.GameObjects;

namespace Engine
{
    public interface IGame
    {
        /// <summary>
        /// Все объекты, размещённые на сцене
        /// </summary>
        IReadOnlyCollection<GameObject> GameObjects { get; }


        /// <summary>
        /// Событие, которое возникает после того, как сцена была обновлена
        /// </summary>
        event EventHandler SceneEndUpdating;

        // Сделано, так как EventHandler не поддерживает приоритет в вызове, а делать на Action не хочется
        /// <summary>
        /// Событие возникает после того, как вся логика была выполнена и можно перерисовать сцену
        /// </summary>
        event EventHandler SceneRedraw;


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
