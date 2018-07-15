using System.Collections.Generic;

using Engine.Models;

namespace Engine.Battle.Models
{
    public class BattleUnitFrames
    {
        public BattleUnitFrames(
            IReadOnlyList<Frame> shadowFrames,
            IReadOnlyList<Frame> unitFrames,
            IReadOnlyList<Frame> auraFrames)
        {
            ShadowFrames = shadowFrames;
            UnitFrames = unitFrames;
            AuraFrames = auraFrames;
        }


        public IReadOnlyList<Frame> ShadowFrames { get; }

        public IReadOnlyList<Frame> UnitFrames { get; }

        public IReadOnlyList<Frame> AuraFrames { get; }
    }
}