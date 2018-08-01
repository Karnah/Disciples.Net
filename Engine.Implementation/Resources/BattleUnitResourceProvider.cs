using System;
using System.Collections.Generic;
using System.IO;

using Engine.Battle.Enums;
using Engine.Battle.Models;
using Engine.Battle.Providers;
using Engine.Implementation.Helpers;
using Engine.Models;
using ResourceProvider;

namespace Engine.Implementation.Resources
{
    public class BattleUnitResourceProvider : IBattleUnitResourceProvider
    {
        private readonly SortedDictionary<(string unidId, BattleDirection direction), BattleUnitAnimation> _unitsAnimations;
        private readonly ImagesExtractor _extractor;

        public BattleUnitResourceProvider()
        {
            _unitsAnimations = new SortedDictionary<(string unidId, BattleDirection direction), BattleUnitAnimation>();
            _extractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Imgs\\BatUnits.ff");
        }


        public BattleUnitAnimation GetBattleUnitAnimation(string unitId, BattleDirection direction)
        {
            if (_unitsAnimations.ContainsKey((unitId, direction)) == false) {
                _unitsAnimations[(unitId, direction)] = GetUnitAnimation(unitId, direction);
            }

            return _unitsAnimations[(unitId, direction)];
        }


        private BattleUnitAnimation GetUnitAnimation(string unitId, BattleDirection direction)
        {
            var unitFrames = new Dictionary<BattleAction, BattleUnitFrames>();
            foreach (BattleAction action in Enum.GetValues(typeof(BattleAction))) {
                unitFrames.Add(action, GetUnitFrames(unitId, direction, action));
            }

            // todo Добавить анимацию для атаки цели
            return new BattleUnitAnimation(unitFrames, null);
        }


        // g000uu0015 - ид в верхнем регистре
        // HHIT - ограбает | HMOVE - атакует | IDLE - ждёт | STIL - замер | TUCH - бьёт 1 врага | HEFF - бьёт площадь
        // A - объект или аура | S - тень
        // 1 - объект | 2 -аура
        // A - юго-восток, лицом | D - северо-запад, спиной | B - симметрично
        // 00
        private BattleUnitFrames GetUnitFrames(string unitId, BattleDirection direction, BattleAction action)
        {
            var shadowImagesName = $"{unitId.ToUpper()}{ConvertAction(action)}S1{ConvertDirection(direction)}00";
            var shadowFrames = GetAnimationFrames(shadowImagesName);

            var unitImagesName = $"{unitId.ToUpper()}{ConvertAction(action)}A1{ConvertDirection(direction)}00";
            var unitFrames = GetAnimationFrames(unitImagesName);

            var auraImagesName = $"{unitId.ToUpper()}{ConvertAction(action)}A2{ConvertDirection(direction)}00";
            var auraFrames = GetAnimationFrames(auraImagesName);

            return new BattleUnitFrames(shadowFrames, unitFrames, auraFrames);
        }

        private IReadOnlyList<Frame> GetAnimationFrames(string fileName)
        {
            var images = _extractor.GetAnimationFrames(fileName);
            return images?.ConvertToFrames();
        }


        private static string ConvertAction(BattleAction action)
        {
            switch (action) {
                case BattleAction.Waiting:
                    return "IDLE";
                case BattleAction.Attacking:
                    return "HMOV";
                case BattleAction.TakingDamage:
                    return "HHIT";
                case BattleAction.Paralized:
                    return "STIL";
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private static string ConvertDirection(BattleDirection direction)
        {
            switch (direction) {
                case BattleDirection.Attacker:
                    return "A";
                case BattleDirection.Defender:
                    return "D";
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
