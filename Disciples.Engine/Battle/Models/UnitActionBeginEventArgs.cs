using System;
using Disciples.Engine.Battle.Enums;

namespace Disciples.Engine.Battle.Models
{
    /// <summary>
    /// Аргументы события действия юнита.
    /// </summary>
    public class UnitActionBeginEventArgs : EventArgs
    {
        /// <inheritdoc />
        public UnitActionBeginEventArgs(UnitActionType unitActionType)
        {
            UnitActionType = unitActionType;
        }

        /// <summary>
        /// Тип действия юнита.
        /// </summary>
        public UnitActionType UnitActionType { get; }
    }
}