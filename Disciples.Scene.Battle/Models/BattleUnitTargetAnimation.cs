﻿using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Класс анимации, которая применяется к атакуемому юниту (например, удары магов или лечение целителей).
/// </summary>
internal class BattleUnitTargetAnimation
{
    public BattleUnitTargetAnimation(bool isSingle, IReadOnlyList<Frame> frames)
    {
        IsSingle = isSingle;
        Frames = frames;
    }


    /// <summary>
    /// Анимация применяется к одному юниту, а не к площади.
    /// </summary>
    public bool IsSingle { get; }

    /// <summary>
    /// Кадры анимации.
    /// </summary>
    public IReadOnlyList<Frame> Frames { get; }
}