using System.Collections.Generic;
using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Battle.Models
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
