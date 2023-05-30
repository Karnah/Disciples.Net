using System.Drawing;
using Disciples.Engine;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Implementation.Resources;
using Disciples.Engine.Platform.Factories;
using Disciples.Resources.Sounds.Models;
using Disciples.Scene.Battle.Resources.SoundKeys;
using Disciples.Scene.Battle.Resources.SoundKeys.Extensions;

namespace Disciples.Scene.Battle.Providers;

/// <inheritdoc cref="IBattleResourceProvider" />
internal class BattleResourceProvider : BaseSupportLoading, IBattleResourceProvider
{
    private readonly BattleImagesExtractor _imagesExtractor;
    private readonly IBitmapFactory _bitmapFactory;
    private readonly BattleSoundsExtractor _soundsExtractor;

    private readonly Dictionary<string, IReadOnlyList<Frame>> _animations;
    private readonly Dictionary<string, Frame> _images;
    private readonly Dictionary<string, RawSound?> _rawSounds;

    /// <inheritdoc />
    public BattleResourceProvider(BattleImagesExtractor imagesExtractor, IBitmapFactory bitmapFactory, BattleSoundsExtractor soundsExtractor)
    {
        _imagesExtractor = imagesExtractor;
        _bitmapFactory = bitmapFactory;
        _soundsExtractor = soundsExtractor;

        _animations = new Dictionary<string, IReadOnlyList<Frame>>();
        _images = new Dictionary<string, Frame>();
        _rawSounds = new Dictionary<string, RawSound?>();
    }

    #region Анимации

    /// <inheritdoc />
    public IReadOnlyList<Frame> GetBattleAnimation(string animationName)
    {
        if (!_animations.ContainsKey(animationName))
            _animations[animationName] = ExtractAnimationFrames(animationName);

        return _animations[animationName];
    }

    /// <inheritdoc />
    public Frame GetBattleFrame(string frameName)
    {
        if (!_images.ContainsKey(frameName))
        {
            var image = _imagesExtractor.GetImage(frameName);
            _images[frameName] = _bitmapFactory.FromRawBitmap(image);
        }

        return _images[frameName];
    }

    /// <summary>
    /// Извлечь кадры анимации из ресурсов.
    /// </summary>
    /// <param name="animationName">Имя анимации в ресурсах игры.</param>
    private IReadOnlyList<Frame> ExtractAnimationFrames(string animationName)
    {
        var images = _imagesExtractor.GetAnimationFrames(animationName);
        if (images == null)
            throw new ArgumentException($"Не найдена анимация {animationName}", nameof(animationName));

        return _bitmapFactory.ConvertToFrames(images);
    }

    #endregion

    #region Изображения

    /// <inheritdoc />
    public IReadOnlyList<IBitmap> GetRandomBattleground()
    {
        var battlegrounds = _imagesExtractor.GetAllFilesNames()
            .Where(name => name.StartsWith("BG_"))
            .ToList();
        var index = RandomGenerator.Get(battlegrounds.Count);
        var battleground = _imagesExtractor.GetImageParts(battlegrounds[index]);

        // Картинка поля боя имеет размер 950 * 600. Если игрок атакует, то первые 150 пикселей высоты пропускаются.
        // Если игрок защищается, то откидываются последние 150 пикселей. В данном случае, мы всегда берём как будто игрок атакует.
        return battleground.Select(p => _bitmapFactory.FromRawToBitmap(p.Value, new Rectangle(150, 0, 800, 600))).ToList();
    }

    #endregion

    #region Звуки

    /// <inheritdoc />
    public RawSound UnitDeathSound { get; private set; } = null!;

    /// <inheritdoc />
    public RawSound? GetSound(string soundName)
    {
        if (!_rawSounds.TryGetValue(soundName, out var rawSound))
        {
            rawSound = _soundsExtractor.GetSound(soundName);
            _rawSounds[soundName] = rawSound;
        }

        return rawSound;
    }

    /// <inheritdoc />
    public RawSound? GetAttackTypeSound(UnitAttackType attackType)
    {
        if (!attackType.HasResourceKey())
            return null;

        var soundName = new UnitAttackTypeSoundResourceKey(attackType).Key;
        return GetSound(soundName);
    }

    #endregion

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        var soundName = new UnitDeathSoundResourceKey().Key;
        UnitDeathSound = GetSound(soundName)
                         ?? throw new Exception($"Не найден ключ {soundName} в ресурсах звука");
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }
}