using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Resources.Database;
using Disciples.Resources.Database.Models;

namespace Disciples.Engine.Implementation.Common.Providers;

/// <inheritdoc cref="ITextProvider" />
public class TextProvider : BaseSupportLoading,  ITextProvider
{
    private readonly IReadOnlyDictionary<string, InterfaceTextResource> _interfaceTextResources;
    private readonly IReadOnlyDictionary<string, GlobalTextResource> _globalTextResources;

    /// <inheritdoc />
    public TextProvider(Database database)
    {
        _interfaceTextResources = database.InterfaceTextResources;
        _globalTextResources = database.GlobalTextResources;
    }

    /// <inheritdoc />
    public override bool IsSharedBetweenScenes => true;


    /// <inheritdoc />
    public string GetText(string textId)
    {
        textId = textId.ToUpperInvariant();

        if (_interfaceTextResources.TryGetValue(textId, out var interfaceTextResource))
            return RemoveTags(interfaceTextResource.Text);

        if (_globalTextResources.TryGetValue(textId, out var globalTextResource))
            return RemoveTags(globalTextResource.Text);

        throw new ArgumentException($"Не найден текст с ключом {textId}");
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
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