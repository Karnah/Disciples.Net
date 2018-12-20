namespace Engine.Models
{
    /// <summary>
    /// Тип действия на игровым объектом.
    /// </summary>
    public enum GameObjectActionType
    {
        /// <summary>
        /// Объект был выбран.
        /// </summary>
        Selected,

        /// <summary>
        /// С объекта было снято выделение.
        /// </summary>
        Unselected,

        /// <summary>
        /// Была нажата ЛКМ.
        /// </summary>
        LeftButtonPressed,

        /// <summary>
        /// Была отпущена ЛКМ.
        /// </summary>
        LeftButtonReleased,

        /// <summary>
        /// Была нажата ПКМ.
        /// </summary>
        RightButtonPressed,

        /// <summary>
        /// Была отпущена ПКМ.
        /// </summary>
        RightButtonReleased,
    }
}