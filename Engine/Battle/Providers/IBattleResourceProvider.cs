using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Engine.Common.Models;

namespace Engine.Battle.Providers
{
    /// <summary>
    /// Поставщик ресурсов для сцены битвы.
    /// </summary>
    public interface IBattleResourceProvider
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
        IReadOnlyList<Bitmap> GetRandomBattleground();
    }
}