using System;

using Engine.Interfaces;

using Action = Engine.Enums.Action;

namespace Engine.Components
{
    public class SoundsComponent : Component
    {
        private readonly IAudioService _audioService;
        private readonly string[] _attackSounds;

        private MapObject _mapObject;
        private Action _action;

        public SoundsComponent(GameObject gameObject, IAudioService audioService, string[] attackSounds)
            : base(gameObject)
        {
            _audioService = audioService;
            _attackSounds = attackSounds;
        }


        public override void OnInitialize()
        {
            base.OnInitialize();

            _mapObject = GetComponent<MapObject>();
        }

        public override void OnUpdate(long tickCount)
        {
            base.OnUpdate(tickCount);

            if (_mapObject.Action != _action)
            {
                _action = _mapObject.Action;
                switch (_action)
                {
                    case Action.Waiting:
                        break;
                    case Action.Moving:
                        break;
                    case Action.Attacking:
                        _audioService.PlaySound(_attackSounds[RandomGenerator.Next(0, _attackSounds.Length)]);
                        break;
                    case Action.TakingDamage:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
