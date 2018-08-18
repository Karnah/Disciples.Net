using System;
using System.Collections.Generic;
using System.IO;

using Engine.Battle.Enums;
using Engine.Battle.Models;
using Engine.Battle.Providers;
using Engine.Implementation.Helpers;
using Engine.Interfaces;
using Engine.Models;
using ResourceProvider;

namespace Engine.Implementation.Resources
{
    public class BattleUnitResourceProvider : IBattleUnitResourceProvider
    {
        private readonly string[] DeathAnimationNames = new[] {
            string.Empty,
            "DEATH_HUMAN_S13",
            "DEATH_HERETIC_S13",
            "DEATH_DWARF_S15",
            "DEATH_UNDEAD_S15",
            "DEATH_NEUTRAL_S10",
            "DEATH_DRAGON_S15",
            "DEATH_GHOST_S15",
            "DEATH_ELF_S15"
        };

        private readonly IUnitInfoProvider _unitInfoProvider;
        private readonly IBattleResourceProvider _battleResourceProvider;

        private readonly SortedDictionary<(string unidId, BattleDirection direction), BattleUnitAnimation> _unitsAnimations;
        private readonly ImagesExtractor _extractor;

        public BattleUnitResourceProvider(IUnitInfoProvider unitInfoProvider, IBattleResourceProvider battleResourceProvider)
        {
            _unitInfoProvider = unitInfoProvider;
            _battleResourceProvider = battleResourceProvider;

            _unitsAnimations = new SortedDictionary<(string unidId, BattleDirection direction), BattleUnitAnimation>();
            _extractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Imgs\\BatUnits.ff");
        }


        public BattleUnitAnimation GetBattleUnitAnimation(string unitId, BattleDirection direction)
        {
            if (_unitsAnimations.ContainsKey((unitId, direction)) == false) {
                _unitsAnimations[(unitId, direction)] = ExtractUnitAnimation(unitId, direction);
            }

            return _unitsAnimations[(unitId, direction)];
        }


        private BattleUnitAnimation ExtractUnitAnimation(string unitId, BattleDirection direction)
        {
            var unitType = _unitInfoProvider.GetUnitType(unitId);
            // Анимация после смерти - это просто кости. Они одинаковы для всех юнитов, поэтому извлекаем отдельно
            var unitFrames = new Dictionary<BattleAction, BattleUnitFrames> {
                { BattleAction.Dead, new BattleUnitFrames(null, GetDeadFrames(unitType.SizeSmall), null) }
            };

            foreach (BattleAction action in Enum.GetValues(typeof(BattleAction))) {
                if (action == BattleAction.Dead)
                    continue;

                unitFrames.Add(action, GetUnitFrames(unitId, direction, action));
            }

            // Методом перебора смотрим, есть ли кадры атаки, применяемые к одному юниту
            var singleTargetFrames =
                // Анимация зависит от положения
                GetAnimationFrames($"{unitId.ToUpper()}TUCHA1{ConvertDirection(direction)}00") ??
                // Анимация симметрична
                GetAnimationFrames($"{unitId.ToUpper()}TUCHA1B00");
            BattleUnitTargetAnimation unitTargetAnimation = null;
            if (singleTargetFrames != null) {
                unitTargetAnimation = new BattleUnitTargetAnimation(true, singleTargetFrames);
            }
            else {
                // Кадры атаки, которые применяется целиком на площадь
                var areaTargetFrames =
                    // Анимация зависит от положения
                    GetAnimationFrames($"{unitId.ToUpper()}HEFFA1{ConvertDirection(direction)}00") ??
                    // Анимация симметрична
                    GetAnimationFrames($"{unitId.ToUpper()}HEFFA1B00");

                if (areaTargetFrames != null) {
                    unitTargetAnimation = new BattleUnitTargetAnimation(false, areaTargetFrames);
                }
            }


            var deathAnimation = GetDeathFrames(unitType.DeathAnimationId);


            return new BattleUnitAnimation(unitFrames, unitTargetAnimation, deathAnimation);
        }


        // g000uu0015 - ид в верхнем регистре
        // HHIT - юнита ударили | HMOVE - атакует | IDLE - ждёт | STIL - замер(например, паралич) | TUCH - бьёт 1 врага | HEFF - бьёт площадь
        // A - объект или аура | S - тень
        // 1 - объект | 2 -аура
        // A - атакующий, лицом | D - защищающийся, спиной | B - симметрично
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

        private IReadOnlyList<Frame> GetDeadFrames(bool isSmall)
        {
            var sizeChar = isSmall ? 'S' : 'L';
            var imageIndex = RandomGenerator.Next(2);
            var deadFrame = _battleResourceProvider.GetBattleFrame($"DEAD_HUMAN_{sizeChar}A{imageIndex:00}");

            // Необходимо задать дополнительно смещение, так как картинка не 800*600, как анимации юнитов
            var frame = new Frame(
                deadFrame.Width,
                deadFrame.Height,
                deadFrame.OffsetX + 350 * GameInfo.Scale,
                deadFrame.OffsetY + 400 * GameInfo.Scale,
                deadFrame.Bitmap);

            return new []{ frame };
        }

        private IReadOnlyList<Frame> GetDeathFrames(int deathAnimationId)
        {
            return _battleResourceProvider.GetBattleAnimation(DeathAnimationNames[deathAnimationId]);
        }
    }
}
