using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

using AvaloniaDisciplesII.Helpers;
using Engine.Enums;
using Engine.Interfaces;
using Engine.Models;
using ResourceProvider;

namespace AvaloniaDisciplesII.Implementation
{
    public class UnitInfoProvider : IUnitInfoProvider
    {
        private readonly DataExtractor _dataExtractor;
        private readonly ImagesExtractor _facesExtractor;

        private SortedDictionary<string, ResourceText> _resourceTexts;
        private SortedDictionary<string, UnitType> _units;

        public UnitInfoProvider()
        {
            _dataExtractor = new DataExtractor($"{Directory.GetCurrentDirectory()}\\Globals");
            _facesExtractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Imgs\\Faces.ff");

            LoadResourceText();
            LoadUnitTypes();
        }

        private void LoadResourceText()
        {
            _resourceTexts = new SortedDictionary<string, ResourceText>();

            var resourceTexts = _dataExtractor.GetData("Tglobal.dbf");
            foreach (DataRow resourceTextRow in resourceTexts.Rows) {
                var textId = resourceTextRow.GetClass<string>("TXT_ID");
                var text = resourceTextRow.GetClass<string>("TEXT");
                var isVerified = resourceTextRow.GetStruct<bool>("VERIFIED") ?? false;
                var context = resourceTextRow.GetClass<string>("CONTEXT");

                _resourceTexts.Add(textId, new ResourceText(textId, text, isVerified, context));
            }
        }

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

            var face = _facesExtractor.GetImage($"{unitId}FACEB").ToBitmap();


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
                _resourceTexts[nameId].Text,
                _resourceTexts[descriptionId].Text,
                _resourceTexts[abilId].Text,
                firstAttackId,
                secondAttackId,
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
                face
            );

            return unit;
        }




        public UnitType GetUnitType(string unitTypeId)
        {
            return _units[unitTypeId];
        }


        private class ResourceText
        {
            public ResourceText(string textId, string text, bool isVerified, string context)
            {
                TextId = textId;
                Text = text;
                IsVerified = isVerified;
                Context = context;
            }

            public string TextId { get; }

            public string Text { get; }

            public bool IsVerified { get; }

            public string Context { get; }
        }
    }
}
