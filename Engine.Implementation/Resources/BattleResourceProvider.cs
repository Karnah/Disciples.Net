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
        private readonly SortedDictionary<string, IReadOnlyList<Frame>> _animations;
        private readonly ImagesExtractor _extractor;

        public BattleResourceProvider()
        {
            _animations = new SortedDictionary<string, IReadOnlyList<Frame>>();
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
    }
}
