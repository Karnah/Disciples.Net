using System.Collections.Generic;
using System.IO;

using Engine.Battle.Providers;
using Engine.Implementation.Helpers;
using Engine.Models;
using ResourceProvider;

namespace Engine.Implementation.Resources
{
    public class BattleResourceProvider : IBattleResourceProvider
    {
        private readonly IDictionary<string, IReadOnlyList<Frame>> _animations;
        private readonly IDictionary<string, Frame> _images;
        private readonly ImagesExtractor _extractor;

        public BattleResourceProvider()
        {
            _animations = new SortedDictionary<string, IReadOnlyList<Frame>>();
            _images = new SortedDictionary<string, Frame>();
            _extractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Imgs\\Battle.ff");
        }


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


        public Frame GetBattleFrame(string frameName)
        {
            if (_images.ContainsKey(frameName) == false) {
                var image = _extractor.GetImage(frameName);
                _images[frameName] = image?.ConvertToFrame();
            }

            return _images[frameName];
        }
    }
}
