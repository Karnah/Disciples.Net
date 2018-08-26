using System.Collections.Generic;

using Avalonia.Media.Imaging;

using Engine.Common.Models;

namespace Engine.Battle.Providers
{
    public interface IBattleResourceProvider
    {
        IReadOnlyList<Frame> GetBattleAnimation(string animationName);

        Frame GetBattleFrame(string frameName);


        /// <summary>
        /// Получить случайную картинку поля боя
        /// </summary>
        Bitmap GetRandomBattleground();
    }
}
