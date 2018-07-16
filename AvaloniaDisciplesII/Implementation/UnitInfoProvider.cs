using System.Collections.Generic;
using System.Data;
using System.IO;

using AvaloniaDisciplesII.Helpers;
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
            _units = new SortedDictionary<string, UnitType>();

            var units = _dataExtractor.GetData("Gunits.dbf");
            foreach (DataRow unitInfoRow in units.Rows) {
                var unitId = unitInfoRow.GetClass<string>("UNIT_ID");
                var nameId = unitInfoRow.GetClass<string>("NAME_TXT");
                var face = _facesExtractor.GetImage($"{unitId}FACEB").ToBitmap();

                var unit = new UnitType(unitId, _resourceTexts[nameId].Text, face);
                _units.Add(unitId, unit);
            }
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
