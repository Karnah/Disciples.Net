using System.Drawing;
using Disciples.Engine;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Implementation.Resources;
using Disciples.Engine.Platform.Factories;
using Disciples.Resources.Common.Exceptions;
using Disciples.Resources.Sounds.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Resources.SoundKeys;
using Disciples.Scene.Battle.Resources.SoundKeys.Extensions;

namespace Disciples.Scene.Battle.Providers;

/// <inheritdoc cref="IBattleResourceProvider" />
internal class BattleResourceProvider : BaseSupportLoading, IBattleResourceProvider
{
    private readonly BattleImagesExtractor _imagesExtractor;
    private readonly IBitmapFactory _bitmapFactory;
    private readonly BattleSoundsExtractor _soundsExtractor;
    private readonly BattleContext _context;

    private readonly Dictionary<string, AnimationFrames> _animations;
    private readonly Dictionary<string, IBitmap> _images;
    private readonly Dictionary<string, RawSound?> _rawSounds;

    /// <inheritdoc />
    public BattleResourceProvider(BattleImagesExtractor imagesExtractor, IBitmapFactory bitmapFactory, BattleSoundsExtractor soundsExtractor, BattleContext context)
    {
        _imagesExtractor = imagesExtractor;
        _bitmapFactory = bitmapFactory;
        _soundsExtractor = soundsExtractor;
        _context = context;

        _animations = new Dictionary<string, AnimationFrames>();
        _images = new Dictionary<string, IBitmap>();
        _rawSounds = new Dictionary<string, RawSound?>();
    }

    #region Анимации

    /// <inheritdoc />
    public AnimationFrames GetBattleAnimation(string animationName)
    {
        if (!_animations.ContainsKey(animationName))
            _animations[animationName] = ExtractAnimationFrames(animationName);

        return _animations[animationName];
    }

    /// <inheritdoc />
    public IBitmap GetBattleBitmap(string frameName)
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
    private AnimationFrames ExtractAnimationFrames(string animationName)
    {
        var images = _imagesExtractor.TryGetAnimationFrames(animationName);
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

        // Картинка поля боя имеет размер 950 * 600. Если игрок атакует, то первые 150 пикселей ширины пропускаются.
        // Если игрок защищается, то откидываются последние 150 пикселей.
        var bounds = _context.PlayerSquadPosition == BattleSquadPosition.Attacker
            ? new Rectangle(150, 0, 800, 600)
            : new Rectangle(0, 0, 800, 600);
        return battleground.Select(p => _bitmapFactory.FromRawToBitmap(p.Value, bounds)).ToList();
    }

    #endregion

    #region Звуки

    /// <inheritdoc />
    public RawSound UnitDeathSound { get; private set; } = null!;

    /// <inheritdoc />
    public RawSound UnitLevelUpSound { get; private set; } = null!;

    /// <inheritdoc />
    public RawSound UnitUnsummonSound { get; private set; } = null!;

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
        var unitDeathSoundKey = new UnitDeathSoundResourceKey().Key;
        UnitDeathSound = GetSound(unitDeathSoundKey)
                         ?? throw new ResourceException($"Не найден ключ {unitDeathSoundKey} в ресурсах звука");

        var unitLevelUpSoundKey = new UnitLevelUpSoundResourceKey().Key;
        UnitLevelUpSound = GetSound(unitLevelUpSoundKey)
                         ?? throw new ResourceException($"Не найден ключ {unitLevelUpSoundKey} в ресурсах звука");

        var unitUnsummonSoundKey = new UnitUnsummonSoundResourceKey().Key;;
        UnitUnsummonSound = GetSound(unitUnsummonSoundKey)
                            ?? throw new ResourceException($"Не найден ключ {unitUnsummonSoundKey} в ресурсах звука");
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }
}