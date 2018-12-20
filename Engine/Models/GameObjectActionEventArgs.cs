using Engine.Common.GameObjects;

namespace Engine.Models
{
    /// <summary>
    /// Аргументы события действия над игровым объектом.
    /// </summary>
    public class GameObjectActionEventArgs
    {
        /// <inheritdoc />
        public GameObjectActionEventArgs(GameObjectActionType actionType, GameObject gameObject)
        {
            ActionType = actionType;
            GameObject = gameObject;
        }

        /// <summary>
        /// Тип действия над игровым объектом.
        /// </summary>
        public GameObjectActionType ActionType { get; }

        /// <summary>
        /// Игровой объект над которым было совершено действие.
        /// </summary>
        public GameObject GameObject { get; }
    }
}