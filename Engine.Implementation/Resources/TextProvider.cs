using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

using Engine.Common.Providers;
using Engine.Implementation.Helpers;
using ResourceProvider;

namespace Engine.Implementation.Resources
{
    /// <inheritdoc />
    public class TextProvider : ITextProvider
    {
        private readonly SortedDictionary<string, ResourceText> _resourceTexts;

        /// <inheritdoc />
        public TextProvider()
        {
            _resourceTexts = new SortedDictionary<string, ResourceText>(StringComparer.OrdinalIgnoreCase);

            LoadResourceText($"{Directory.GetCurrentDirectory()}\\Resources\\Globals", "Tglobal.dbf");
            LoadResourceText($"{Directory.GetCurrentDirectory()}\\Resources\\interf", "TApp.dbf");
        }


        /// <inheritdoc />
        public string GetText(string textId)
        {
            return _resourceTexts[textId].Text;
        }


        /// <summary>
        /// Загрузить текстовую информацию из ресурсов.
        /// </summary>
        private void LoadResourceText(string path, string fileName)
        {
            var dataExtractor = new DataExtractor(path);
            var resourceTexts = dataExtractor.GetData(fileName);

            foreach (DataRow resourceTextRow in resourceTexts.Rows)
            {
                var textId = resourceTextRow.GetClass<string>("TXT_ID");
                var text = resourceTextRow.GetClass<string>("TEXT");
                var isVerified = resourceTextRow.GetStruct<bool>("VERIFIED") ?? false;
                var context = resourceTextRow.GetClass<string>("CONTEXT");

                _resourceTexts.Add(textId, new ResourceText(textId, DestroyTag(text), isVerified, context));
            }
        }

        /// <summary>
        /// Удалить все специфичные теги.
        /// </summary>
        private string DestroyTag(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return Regex.Replace(text, @"\\\w+?;", string.Empty);
        }


        /// <summary>
        /// Информация о тексте в файле ресурсов.
        /// </summary>
        private class ResourceText
        {
            public ResourceText(string textId, string text, bool isVerified, string context)
            {
                TextId = textId;
                Text = text;
                IsVerified = isVerified;
                Context = context;
            }

            /// <summary>
            /// Идентификатор текста.
            /// </summary>
            public string TextId { get; }

            /// <summary>
            /// Текст.
            /// </summary>
            public string Text { get; }

            public bool IsVerified { get; }

            public string Context { get; }
        }
    }
}
