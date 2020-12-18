using System.Collections.Generic;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Battle.Models
{
    /// <summary>
    /// Кадры для анимации юнита во время битвы.
    /// </summary>
    public class BattleUnitFrames
    {
        /// <inheritdoc />
        public BattleUnitFrames(
            IReadOnlyList<Frame> shadowFrames,
            IReadOnlyList<Frame> unitFrames,
            IReadOnlyList<Frame> auraFrames)
        {
            ShadowFrames = shadowFrames;
            UnitFrames = unitFrames;
            AuraFrames = auraFrames;
        }


        /// <summary>
        /// Кадры для анимации тени.
        /// </summary>
        public IReadOnlyList<Frame> ShadowFrames { get; }

        /// <summary>
        /// Кадры для анимации самого юнита.
        /// </summary>
        public IReadOnlyList<Frame> UnitFrames { get; }

        /// <summary>
        /// Кадры для анимации ауры юнита.
        /// </summary>
        public IReadOnlyList<Frame> AuraFrames { get; }
    }
}