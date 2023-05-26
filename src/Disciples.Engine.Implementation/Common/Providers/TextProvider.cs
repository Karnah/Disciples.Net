using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Resources.Database.Sqlite;

namespace Disciples.Engine.Implementation.Common.Providers;

/// <inheritdoc cref="ITextProvider" />
public class TextProvider : BaseSupportLoading,  ITextProvider
{
    private readonly GameDataContextFactory _gameDataContextFactory;
    private IReadOnlyDictionary<string, TextContainer> _interfaceTextResources = null!;
    private IReadOnlyDictionary<string, TextContainer> _globalTextResources = null!;

    /// <inheritdoc />
    public TextProvider(GameDataContextFactory gameDataContextFactory)
    {
        _gameDataContextFactory = gameDataContextFactory;
    }

    /// <inheritdoc />
    public TextContainer GetText(string textId)
    {
        textId = textId.ToUpperInvariant();

        if (_interfaceTextResources.TryGetValue(textId, out var interfaceTextResource))
            return interfaceTextResource;

        if (_globalTextResources.TryGetValue(textId, out var globalTextResource))
            return globalTextResource;

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
                .ToDictionary(tr => tr.Id, tr => new TextContainer(tr.Text));

            _globalTextResources = context
                .GlobalTextResources
                .AsNoTracking()
                .Select(tr => new { tr.Id, tr.Text })
                .ToDictionary(tr => tr.Id, tr => new TextContainer(tr.Text));
        }
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }
}