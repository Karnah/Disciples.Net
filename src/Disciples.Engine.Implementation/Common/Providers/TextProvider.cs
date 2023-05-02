using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Resources.Database.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Disciples.Engine.Implementation.Common.Providers;

/// <inheritdoc cref="ITextProvider" />
public class TextProvider : BaseSupportLoading,  ITextProvider
{
    private readonly GameDataContextFactory _gameDataContextFactory;
    private IReadOnlyDictionary<string, string> _interfaceTextResources = null!;
    private IReadOnlyDictionary<string, string> _globalTextResources = null!;

    /// <inheritdoc />
    public TextProvider(GameDataContextFactory gameDataContextFactory)
    {
        _gameDataContextFactory = gameDataContextFactory;
    }

    /// <inheritdoc />
    public override bool IsSharedBetweenScenes => true;


    /// <inheritdoc />
    public string GetText(string textId)
    {
        textId = textId.ToUpperInvariant();

        if (_interfaceTextResources.TryGetValue(textId, out var interfaceTextResource))
            return RemoveTags(interfaceTextResource);

        if (_globalTextResources.TryGetValue(textId, out var globalTextResource))
            return RemoveTags(globalTextResource);

        throw new ArgumentException($"Не найден текст с ключом {textId}");
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        using (var context = _gameDataContextFactory.Create())
        {
            _interfaceTextResources = context
                .InterfaceTextResources
                .AsNoTracking()
                .Select(tr => new { tr.Id, tr.Text })
                .ToDictionary(tr => tr.Id, tr => tr.Text);

            _globalTextResources = context
                .GlobalTextResources
                .AsNoTracking()
                .Select(tr => new { tr.Id, tr.Text })
                .ToDictionary(tr => tr.Id, tr => tr.Text);
        }
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Удалить все специфичные теги.
    /// </summary>
    /// <remarks>
    /// TODO Вообще они должны остаться и отображаться особым образом на UI.
    /// </remarks>
    private static string RemoveTags(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return Regex.Replace(text, @"\\\w+?;", string.Empty);
    }
}