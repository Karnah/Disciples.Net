﻿using System;
using System.Collections.Generic;
using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Extensions;

namespace Disciples.Engine.Common.GameObjects;

/// <summary>
/// Список строк.
/// </summary>
/// <remarks>
/// TODO Контрол сделан только для одноколоночного варианта.
/// Также отсутствует разбиение по страницам, а высоты элемента захардкожены.
/// </remarks>
public class TextListBoxObject : GameObject
{
    private readonly IGameObjectContainer _gameObjectContainer;
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly TextListBoxSceneElement _textListBox;
    private readonly int _layer;

    private ButtonObject? _scrollUpButton;
    private ButtonObject? _scrollDownButton;
    private ButtonObject? _scrollLeftButton;
    private ButtonObject? _scrollRightButton;
    private ButtonObject? _pageUpButton;
    private ButtonObject? _pageDownButton;
    private ButtonObject? _doubleClickButton;

    private int? _selectedItemIndex;
    private IReadOnlyList<TextListBoxItem> _items = Array.Empty<TextListBoxItem>();
    private List<TextListBoxItemObject> _itemObjects = new();

    /// <summary>
    /// Создать объект типа <see cref="ImageObject" />.
    /// </summary>
    public TextListBoxObject(
        IGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        TextListBoxSceneElement textListBox,
        int layer
    ) : base(textListBox)
    {
        _gameObjectContainer = gameObjectContainer;
        _sceneObjectContainer = sceneObjectContainer;
        _textListBox = textListBox;
        _layer = layer;
    }

    /// <summary>
    /// Выбранный объект.
    /// </summary>
    public TextListBoxItem? SelectedItem => _selectedItemIndex == null
        ? null
        : _items[_selectedItemIndex.Value];

    /// <summary>
    /// Обработать выбор элемента.
    /// </summary>
    public Action<TextListBoxItem?>? ItemSelected { get; set; }

    /// <summary>
    /// Выбранный объект.
    /// </summary>
    private int? SelectedItemIndex
    {
        get => _selectedItemIndex;
        set
        {
            if (_selectedItemIndex != null)
                _itemObjects[_selectedItemIndex.Value].IsSelected = false;

            _selectedItemIndex = value;

            if (_selectedItemIndex != null)
                _itemObjects[_selectedItemIndex.Value].IsSelected = true;

            ItemSelected?.Invoke(SelectedItem);
            UpdateButtonStates();
        }
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        _scrollUpButton = GetButton(_textListBox.ScrollUpButtonName, ExecuteItemScrollUp);
        _scrollDownButton = GetButton(_textListBox.ScrollDownButtonName, ExecuteItemScrollDown);
        _scrollLeftButton = GetButton(_textListBox.ScrollLeftButtonName);
        _scrollRightButton = GetButton(_textListBox.ScrollRightButtonName);
        _pageUpButton = GetButton(_textListBox.PageUpButtonName);
        _pageDownButton = GetButton(_textListBox.PageDownButtonName);
        _doubleClickButton = GetButton(_textListBox.DoubleClickButtonName);
    }

    /// <inheritdoc />
    public override void Destroy()
    {
        base.Destroy();

        foreach (var item in _itemObjects)
            item.Destroy();
    }

    /// <summary>
    /// Установить список элементов.
    /// </summary>
    public void SetItems(IReadOnlyList<TextListBoxItem> items)
    {
        SelectedItemIndex = null;
        foreach (var itemObject in _itemObjects)
            itemObject.Destroy();

        var itemsObjects = new List<TextListBoxItemObject>(items.Count);
        for (int itemIndex = 0; itemIndex < items.Count; itemIndex++)
        {
            var itemObject = _gameObjectContainer.AddObject(
                new TextListBoxItemObject(_sceneObjectContainer,
                    _textListBox.SelectedTextStyle,
                    _textListBox.CommonTextStyle,
                    items[itemIndex],
                    ExecuteItemSelected,
                    ExecuteItemLeftMouseButtonDoubleClicked,
                    new RectangleD(X, Y + itemIndex * (18 + _textListBox.VerticalSpacing), Width, 18),
                    _layer + 1));
            itemsObjects.Add(itemObject);
        }

        _items = items;
        _itemObjects = itemsObjects;

        if (_itemObjects.Count > 0)
            SelectedItemIndex = 0;

        UpdateButtonStates();
    }

    /// <summary>
    /// Получить объект кнопки.
    /// </summary>
    private ButtonObject? GetButton(string? buttonName, Action? clickedAction = null)
    {
        if (buttonName == null)
            return null;

        return _gameObjectContainer.GameObjects.GetButton(buttonName, clickedAction);
    }

    /// <summary>
    /// Выбрать объект выше.
    /// </summary>
    private void ExecuteItemScrollUp()
    {
        --SelectedItemIndex;
    }

    /// <summary>
    /// Выбрать объект ниже.
    /// </summary>
    private void ExecuteItemScrollDown()
    {
        ++SelectedItemIndex;
    }

    /// <summary>
    /// Обработать выбор элемента.
    /// </summary>
    private void ExecuteItemSelected(TextListBoxItemObject itemObject)
    {
        SelectedItemIndex = _itemObjects.IndexOf(itemObject);
    }

    /// <summary>
    /// Обработать двойной клик на элементе.
    /// </summary>
    private void ExecuteItemLeftMouseButtonDoubleClicked(TextListBoxItemObject itemObject)
    {
        _doubleClickButton?.ClickedAction?.Invoke();
    }

    /// <summary>
    /// Обновить состояние кнопок.
    /// </summary>
    private void UpdateButtonStates()
    {
        if (SelectedItemIndex == null)
        {
            DisableButtons(
                _scrollUpButton,
                _scrollDownButton,
                _scrollLeftButton,
                _scrollRightButton,
                _pageUpButton,
                _pageDownButton,
                _doubleClickButton);
            return;
        }

        _doubleClickButton?.SetActive();

        if (SelectedItemIndex > 0)
            _scrollUpButton?.SetActive();
        else
            _scrollUpButton?.SetDisabled();

        if (SelectedItemIndex < _items.Count - 1)
            _scrollDownButton?.SetActive();
        else
            _scrollDownButton?.SetDisabled();
    }

    /// <summary>
    /// Деактивировать указанные кнопки.
    /// </summary>
    private static void DisableButtons(params ButtonObject?[] buttons)
    {
        foreach (var button in buttons)
        {
            button?.SetDisabled();
        }
    }
}