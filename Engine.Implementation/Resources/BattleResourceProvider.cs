using System.Collections.Generic;
using System.IO;
using System.Linq;

using Avalonia.Media.Imaging;

using Engine.Battle.Providers;
using Engine.Common.Models;
using Engine.Implementation.Helpers;
using ResourceProvider;

namespace Engine.Implementation.Resources
{
    /// <inheritdoc />
    public class BattleResourceProvider : IBattleResourceProvider
    {
        private readonly IDictionary<string, IReadOnlyList<Frame>> _animations;
        private readonly IDictionary<string, Frame> _images;
        private readonly ImagesExtractor _extractor;

        /// <inheritdoc />
        public BattleResourceProvider()
        {
            _animations = new SortedDictionary<string, IReadOnlyList<Frame>>();
            _images = new SortedDictionary<string, Frame>();
            _extractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Resources\\Imgs\\Battle.ff");
        }


        /// <inheritdoc />
        public IReadOnlyList<Frame> GetBattleAnimation(string animationName)
        {
            if (_animations.ContainsKey(animationName) == false) {
                _animations[animationName] = GetAnimationFrames(animationName);
            }

            return _animations[animationName];
        }

        private IReadOnlyList<Frame> GetAnimationFrames(string fileName)
        {
            var images = _extractor.GetAnimationFrames(fileName);
            return images?.ConvertToFrames();
        }


        /// <inheritdoc />
        public Frame GetBattleFrame(string frameName)
        {
            if (_images.ContainsKey(frameName) == false) {
                var image = _extractor.GetImage(frameName);
                _images[frameName] = image?.ConvertToFrame();
            }

            return _images[frameName];
        }


        /// <inheritdoc />
        public IReadOnlyList<Bitmap> GetRandomBattleground()
        {
            var battlegrounds = _extractor.GetAllFilesNames()
                .Where(name => name.StartsWith("BG_"))
                .ToList();
            var index = RandomGenerator.Next(battlegrounds.Count);
            var battleground = _extractor.GetImageParts(battlegrounds[index]);

            // Картинка поля боя имеет размер 950 * 600. Если игрок атакует, то первые 150 пикселей высоты пропускаются.
            // Если игрок защищается, то откидываются последние 150 пикселей. В данном случае, мы всегда берём как будто игрок атакует.
            return battleground.Select(p => p.Value.ToBitmap(new ImageExtensions.Bounds(0, 600, 150, 950))).ToList();
        }
    }
}