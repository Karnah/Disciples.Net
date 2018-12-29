using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.ResourceProvider;
using Disciples.Engine.Implementation.Helpers;
using Disciples.Engine.Platform.Factories;

namespace Disciples.Engine.Implementation.Resources
{
    public class UnitInfoProvider : IUnitInfoProvider
    {
        private readonly ITextProvider _textProvider;
        private readonly IBitmapFactory _bitmapFactory;
        private readonly DataExtractor _dataExtractor;
        private readonly ImagesExtractor _facesExtractor;
        private readonly ImagesExtractor _portraitExtractor;

        private SortedDictionary<string, Attack> _attacks;
        private SortedDictionary<string, UnitType> _units;

        public UnitInfoProvider(ITextProvider textProvider, IBitmapFactory bitmapFactory)
        {
            _textProvider = textProvider;
            _bitmapFactory = bitmapFactory;

            _dataExtractor = new DataExtractor($"{Directory.GetCurrentDirectory()}\\Resources\\Globals");
            _facesExtractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Resources\\Imgs\\Faces.ff");
            _portraitExtractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Resources\\Imgs\\Events.ff");

            LoadAttacks();
            LoadUnitTypes();
        }


        /// <summary>
        /// Загрузить информацию о типах атаки.
        /// </summary>
        private void LoadAttacks()
        {
            _attacks = new SortedDictionary<string, Attack>(StringComparer.InvariantCultureIgnoreCase) {
                {"G000000000", null }
            };

            var attacks = _dataExtractor.GetData("Gattacks.dbf");
            foreach (DataRow attackInfoRow in attacks.Rows) {
                var attack = ExtractAttack(attackInfoRow);
                _attacks.Add(attack.AttackId, attack);
            }
        }

        /// <summary>
        /// Загрузить информацию о юнитах.
        /// </summary>
        private void LoadUnitTypes()
        {
            _units = new SortedDictionary<string, UnitType>(StringComparer.InvariantCultureIgnoreCase) {
                {"G000000000", null }
            };


            var units = _dataExtractor.GetData("Gunits.dbf");
            foreach (DataRow unitInfoRow in units.Rows) {
                var unit = ExtractUnitType(unitInfoRow);
                _units.Add(unit.UnitTypeId, unit);
            }
        }


        /// <summary>
        /// Извлечь из строки информацию об атаке.
        /// </summary>
        private Attack ExtractAttack(DataRow attackInfo)
        {
            var attackId = attackInfo.GetClass<string>("ATT_ID");
            var nameId = attackInfo.GetClass<string>("NAME_TXT");
            var descriptionId = attackInfo.GetClass<string>("DESC_TXT");
            var initiative = attackInfo.GetStruct<int>("INITIATIVE") ?? 10;
            var source = (AttackSource)(attackInfo.GetStruct<int>("SOURCE") ?? 1);
            var attackClass = (AttackClass)(attackInfo.GetStruct<int>("CLASS") ?? 1);
            var accuracy = attackInfo.GetStruct<int>("POWER") ?? 80;
            var reach = (Reach)(attackInfo.GetStruct<int>("REACH") ?? 1);
            var heal = attackInfo.GetStruct<int>("QTY_HEAL") ?? 0;
            var damage = attackInfo.GetStruct<int>("QTY_DAM") ?? 0;


            return new Attack(
                attackId,
                _textProvider.GetText(nameId),
                _textProvider.GetText(descriptionId),
                initiative,
                source,
                attackClass,
                accuracy,
                reach,
                heal,
                damage
                );
        }

        /// <summary>
        /// Извлечь из строки информацию о юните.
        /// </summary>
        private UnitType ExtractUnitType(DataRow unitInfo)
        {
            var unitId = unitInfo.GetClass<string>("UNIT_ID");
            var unitCategory = (UnitCategory)(unitInfo.GetStruct<int>("UNIT_CAT") ?? 0);
            var level = unitInfo.GetStruct<int>("LEVEL") ?? 1;
            var prevUnitId = unitInfo.GetClass<string>("PREV_ID");
            var raceId = unitInfo.GetClass<string>("RACE_ID");
            var subrace = (Subrace)(unitInfo.GetStruct<int>("SUBRACE") ?? 0);
            var branch = (UnitBranch)(unitInfo.GetStruct<int>("BRANCH") ?? 0);
            var sizeSmall = unitInfo.GetStruct<bool>("SIZE_SMALL") ?? true;
            var isMale = unitInfo.GetStruct<bool>("SEX_M") ?? true;
            var enrollCost = unitInfo.GetClass<string>("ENROLL_C");
            var enrollBuilding = unitInfo.GetClass<string>("ENROLL_B");
            var nameId = unitInfo.GetClass<string>("NAME_TXT");
            var descriptionId = unitInfo.GetClass<string>("DESC_TXT");
            var abilId = unitInfo.GetClass<string>("ABIL_TXT");
            var firstAttackId = unitInfo.GetClass<string>("ATTACK_ID");
            var secondAttackId = unitInfo.GetClass<string>("ATTACK2_ID");
            var attackTwice = unitInfo.GetStruct<bool>("ATCK_TWICE") ?? false;
            var hitpoint = unitInfo.GetStruct<int>("HIT_POINT") ?? 0;
            var baseUnitId = unitInfo.GetClass<string>("BASE_UNIT");
            var armor = unitInfo.GetStruct<int>("ARMOR") ?? 0;
            var regen = unitInfo.GetStruct<int>("REGEN") ?? 0;
            var reviveCost = unitInfo.GetClass<string>("REVIVE_C");
            var healCost = unitInfo.GetClass<string>("HEAL_C");
            var trainingCost = unitInfo.GetClass<string>("TRAINING_C");
            var xpKilled = unitInfo.GetStruct<int>("XP_KILLED") ?? 0;
            var upgradeBuildingId = unitInfo.GetClass<string>("UPGRADE_B");
            var xpNext = unitInfo.GetStruct<int>("XP_NEXT") ?? 0;
            var deathAnim = unitInfo.GetStruct<int>("DEATH_ANIM") ?? 1;

            // Лицо юнита дополнительно обрабатывать не надо.
            // Кроме того, там есть проблемы с некоторым портретами, если их получать обычным путём.
            var face = new Lazy<IBitmap>(() => _bitmapFactory.FromByteArray(_facesExtractor.GetFileContent($"{unitId}FACE")));
            var battleFace = new Lazy<IBitmap>(() => _bitmapFactory.FromRawToBitmap(_facesExtractor.GetImage($"{unitId}FACEB")));
            var portrait = new Lazy<IBitmap>(() => _bitmapFactory.FromRawToOriginalBitmap(_portraitExtractor.GetImage(unitId.ToUpper())));

            var unit = new UnitType(
                unitId,
                unitCategory,
                level,
                null, //todo Просто так делать нельзя. Иногда базовый тип юнита идёт после следующего GetUnitType(prevUnitId),
                raceId,
                subrace,
                branch,
                sizeSmall,
                isMale,
                enrollCost,
                enrollBuilding,
                _textProvider.GetText(nameId),
                _textProvider.GetText(descriptionId),
                _textProvider.GetText(abilId),
                _attacks[firstAttackId],
                _attacks[secondAttackId],
                attackTwice,
                hitpoint,
                null, //todo GetUnitType(baseUnitId),
                armor,
                regen,
                reviveCost,
                healCost,
                trainingCost,
                xpKilled,
                upgradeBuildingId,
                xpNext,
                deathAnim,
                face,
                battleFace,
                portrait
            );

            return unit;
        }


        /// <inheritdoc />
        public UnitType GetUnitType(string unitTypeId)
        {
            return _units[unitTypeId];
        }
    }
}