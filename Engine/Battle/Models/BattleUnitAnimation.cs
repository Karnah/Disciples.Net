using System.Collections.Generic;

using Engine.Battle.Enums;
using Engine.Models;

namespace Engine.Battle.Models
{
    public class BattleUnitAnimation
    {
        public BattleUnitAnimation(
            Dictionary<BattleAction, BattleUnitFrames> battleUnitFrameses,
            BattleUnitTargetAnimation targetAnimation,
            IReadOnlyList<Frame> deathFrames)
        {
            BattleUnitFrameses = battleUnitFrameses;
            TargetAnimation = targetAnimation;
            DeathFrames = deathFrames;
        }


        public Dictionary<BattleAction, BattleUnitFrames> BattleUnitFrameses { get; }

        public BattleUnitTargetAnimation TargetAnimation { get; }

        public IReadOnlyList<Frame> DeathFrames { get; }
    }
}
