using System.Collections.Generic;

using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Battle.Providers
{
    /// <summary>
    /// Поставщик ресурсов для сцены битвы.
    /// </summary>
    public interface IBattleResourceProvider : ISupportLoading
    {
        /// <summary>
        /// Получить анимацию для сцены битвы.
        /// </summary>
        /// <param name="animationName">Имя анимации.</param>
        IReadOnlyList<Frame> GetBattleAnimation(string animationName);

        /// <summary>
        /// Получить изображение для сцены битвы.
        /// </summary>
        /// <param name="frameName">Имя изображения.</param>
        Frame GetBattleFrame(string frameName);


        /// <summary>
        /// Получить случайный фон для поля боя.
        /// </summary>
        /// <remarks>Фон может состоять из нескольких изображений, поэтому возвращается список.</remarks>
        IReadOnlyList<IBitmap> GetRandomBattleground();
    }
}