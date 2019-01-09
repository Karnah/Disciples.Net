using System.Collections.Generic;
using System.IO;
using System.Linq;

using Disciples.Engine.Battle.Providers;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Platform.Factories;
using Disciples.Engine.Platform.Models;
using Disciples.ResourceProvider;

namespace Disciples.Engine.Implementation.Battle.Providers
{
    /// <inheritdoc cref="IBattleResourceProvider" />
    public class BattleResourceProvider : BaseSupportLoading, IBattleResourceProvider
    {
        private readonly IBitmapFactory _bitmapFactory;

        private ImagesExtractor _extractor;
        private IDictionary<string, IReadOnlyList<Frame>> _animations;
        private IDictionary<string, Frame> _images;

        /// <inheritdoc />
        public BattleResourceProvider(IBitmapFactory bitmapFactory)
        {
            _bitmapFactory = bitmapFactory;
        }


        /// <inheritdoc />
        public override bool OneTimeLoading => false;

        /// <inheritdoc />
        protected override void LoadInternal()
        {
            _extractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Resources\\Imgs\\Battle.ff");
            _animations = new SortedDictionary<string, IReadOnlyList<Frame>>();
            _images = new SortedDictionary<string, Frame>();
        }

        /// <inheritdoc />
        protected override void UnloadInternal()
        {
            _animations = null;
            _images = null;
        }

        /// <inheritdoc />
        public IReadOnlyList<Frame> GetBattleAnimation(string animationName)
        {
            if (!_animations.ContainsKey(animationName)) {
                _animations[animationName] = ExtractAnimationFrames(animationName);
            }

            return _animations[animationName];
        }

        /// <summary>
        /// Извлечь кадры анимации из ресурсов.
        /// </summary>
        /// <param name="animationName">Имя анимации в ресурсах игры.</param>
        private IReadOnlyList<Frame> ExtractAnimationFrames(string animationName)
        {
            var images = _extractor.GetAnimationFrames(animationName);
            return _bitmapFactory.ConvertToFrames(images);
        }


        /// <inheritdoc />
        public Frame GetBattleFrame(string frameName)
        {
            if (!_images.ContainsKey(frameName)) {
                var image = _extractor.GetImage(frameName);
                _images[frameName] = _bitmapFactory.FromRawBitmap(image);
            }

            return _images[frameName];
        }


        /// <inheritdoc />
        public IReadOnlyList<IBitmap> GetRandomBattleground()
        {
            var battlegrounds = _extractor.GetAllFilesNames()
                .Where(name => name.StartsWith("BG_"))
                .ToList();
            var index = RandomGenerator.Next(battlegrounds.Count);
            var battleground = _extractor.GetImageParts(battlegrounds[index]);

            // Картинка поля боя имеет размер 950 * 600. Если игрок атакует, то первые 150 пикселей высоты пропускаются.
            // Если игрок защищается, то откидываются последние 150 пикселей. В данном случае, мы всегда берём как будто игрок атакует.
            return battleground.Select(p => _bitmapFactory.FromRawToBitmap(p.Value, new Bounds(0, 600, 150, 950))).ToList();
        }
    }
}