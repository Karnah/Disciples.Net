using System;
using System.Collections.Generic;
using System.IO;

using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Battle.Models;
using Disciples.Engine.Battle.Providers;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Platform.Factories;
using Disciples.ResourceProvider;

namespace Disciples.Engine.Implementation.Battle.Providers
{
    /// <inheritdoc cref="IBattleUnitResourceProvider" />
    public class BattleUnitResourceProvider : BaseSupportLoading, IBattleUnitResourceProvider
    {
        private readonly string[] _deathAnimationNames = {
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

        private readonly IBitmapFactory _bitmapFactory;
        private readonly IUnitInfoProvider _unitInfoProvider;
        private readonly IBattleResourceProvider _battleResourceProvider;

        private ImagesExtractor _extractor;
        private SortedDictionary<(string unidId, BattleDirection direction), BattleUnitAnimation> _unitsAnimations;

        /// <inheritdoc />
        public BattleUnitResourceProvider(IBitmapFactory bitmapFactory, IUnitInfoProvider unitInfoProvider, IBattleResourceProvider battleResourceProvider)
        {
            _bitmapFactory = bitmapFactory;
            _unitInfoProvider = unitInfoProvider;
            _battleResourceProvider = battleResourceProvider;
        }


        /// <inheritdoc />
        public override bool IsSharedBetweenScenes => false;


        /// <inheritdoc />
        protected override void LoadInternal()
        {
            _extractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Resources\\Imgs\\BatUnits.ff");
            _unitsAnimations = new SortedDictionary<(string unidId, BattleDirection direction), BattleUnitAnimation>();
        }

        /// <inheritdoc />
        protected override void UnloadInternal()
        {
            _unitsAnimations = null;
        }


        /// <inheritdoc />
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
            // Анимация после смерти - это просто кости. Они одинаковы для всех юнитов, поэтому извлекаем отдельно.
            var unitFrames = new Dictionary<BattleAction, BattleUnitFrames> {
                { BattleAction.Dead, new BattleUnitFrames(null, GetDeadFrames(unitType.SizeSmall), null) }
            };

            foreach (BattleAction action in Enum.GetValues(typeof(BattleAction))) {
                if (action == BattleAction.Dead)
                    continue;

                unitFrames.Add(action, GetUnitFrames(unitId, direction, action));
            }

            // Методом перебора смотрим, есть ли кадры атаки, применяемые к одному юниту.
            var singleTargetFrames =
                // Анимация зависит от положения.
                GetAnimationFrames($"{unitId.ToUpper()}TUCHA1{ConvertDirection(direction)}00") ??
                // Анимация симметрична.
                GetAnimationFrames($"{unitId.ToUpper()}TUCHA1B00");
            BattleUnitTargetAnimation unitTargetAnimation = null;
            if (singleTargetFrames != null) {
                unitTargetAnimation = new BattleUnitTargetAnimation(true, singleTargetFrames);
            }
            else {
                // Кадры атаки, которые применяется целиком на площадь.
                var areaTargetFrames =
                    // Анимация зависит от положения.
                    GetAnimationFrames($"{unitId.ToUpper()}HEFFA1{ConvertDirection(direction)}00") ??
                    // Анимация симметрична.
                    GetAnimationFrames($"{unitId.ToUpper()}HEFFA1B00");

                if (areaTargetFrames != null) {
                    unitTargetAnimation = new BattleUnitTargetAnimation(false, areaTargetFrames);
                }
            }


            var deathAnimation = GetDeathFrames(unitType.DeathAnimationId);


            return new BattleUnitAnimation(unitFrames, unitTargetAnimation, deathAnimation);
        }


        // g000uu0015 - ид в верхнем регистре.
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
            return _bitmapFactory.ConvertToFrames(images);
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

            // Необходимо задать дополнительно смещение, чтобы кости оказались в нужном месте.
            var frame = new Frame(
                deadFrame.Width,
                deadFrame.Height,
                deadFrame.OffsetX - 30,
                deadFrame.OffsetY - 10,
                deadFrame.Bitmap);

            return new []{ frame };
        }

        private IReadOnlyList<Frame> GetDeathFrames(int deathAnimationId)
        {
            return _battleResourceProvider.GetBattleAnimation(_deathAnimationNames[deathAnimationId]);
        }
    }
}