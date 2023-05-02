using Disciples.Engine.Base;

namespace Disciples.Scene.Battle;

/// <summary>
/// Сцена битвы двух отрядов.
/// </summary>
public interface IBattleScene : IScene, ISupportLoadingWithParameters<BattleSceneParameters>
{
}