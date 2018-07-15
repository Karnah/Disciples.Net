using System.Collections.Generic;

using Engine.Battle.Enums;
using Engine.Models;

namespace Engine.Battle.Models
{
    public class BattleUnitAnimation
    {
        public BattleUnitAnimation(
            Dictionary<BattleAction, BattleUnitFrames> battleUnitFrameses,
            IReadOnlyList<Frame> targetFrames)
        {
            BattleUnitFrameses = battleUnitFrameses;
            TargetFrames = targetFrames;
        }


        public Dictionary<BattleAction, BattleUnitFrames> BattleUnitFrameses { get; }

        public IReadOnlyList<Frame> TargetFrames { get; }
    }
}
