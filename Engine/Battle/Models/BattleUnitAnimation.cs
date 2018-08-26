using System.Collections.Generic;

using Engine.Battle.Enums;
using Engine.Common.Models;

namespace Engine.Battle.Models
{
    public class BattleUnitAnimation
    {
        public BattleUnitAnimation(
            Dictionary<BattleAction, BattleUnitFrames> battleUnitFrames,
            BattleUnitTargetAnimation targetAnimation,
            IReadOnlyList<Frame> deathFrames)
        {
            BattleUnitFrames = battleUnitFrames;
            TargetAnimation = targetAnimation;
            DeathFrames = deathFrames;
        }


        public Dictionary<BattleAction, BattleUnitFrames> BattleUnitFrames { get; }

        public BattleUnitTargetAnimation TargetAnimation { get; }

        public IReadOnlyList<Frame> DeathFrames { get; }
    }
}
