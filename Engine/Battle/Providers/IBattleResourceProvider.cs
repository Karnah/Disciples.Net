using System.Collections.Generic;

using Engine.Models;

namespace Engine.Battle.Providers
{
    public interface IBattleResourceProvider
    {
        IReadOnlyList<Frame> GetBattleAnimation(string animationName);

        Frame GetBattleFrame(string frameName);
    }
}
