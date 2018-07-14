using System;

using Avalonia;

using Engine.Enums;

namespace Engine.Components
{
    public class MovementComponent : Component
    {
        private readonly double _speed;
        private MapObject _mapObject;
        private double _targetX = -1000; //todo doudlbe constants?
        private double _targetY = -1000;

        public MovementComponent(GameObject gameObject, double speed) : base(gameObject)
        {
            _speed = speed;
        }


        public override void OnInitialize()
        {
            base.OnInitialize();

            _mapObject = GetComponent<MapObject>();
            GetNewTarget();
        }

        public override void OnUpdate(long tickCount)
        {
            var prevPosition = _mapObject.Position;
            var offset = _speed * tickCount / 1000;

            double x, y;
            if (Math.Abs(prevPosition.X - _targetX) < offset && Math.Abs(prevPosition.Y - _targetY) < offset) {
                x = _targetX;
                y = _targetY;

                GetNewTarget();
            }
            else {
                x = prevPosition.X + offset * GetXDirection(_mapObject.Direction);
                y = prevPosition.Y + offset * GetYDirection(_mapObject.Direction);
            }

            var width = prevPosition.Width;
            var height = prevPosition.Height;

            var position = new Rect(x, y, width, height);
            _mapObject.Position = position;
        }


        private int GetXDirection(Direction direction)
        {
            switch (direction) {
                case Direction.Southwest:
                case Direction.West:
                case Direction.Northwest:
                    return -1;

                case Direction.North:
                case Direction.South:
                    return 0;

                case Direction.Northeast:
                case Direction.East:
                case Direction.Southeast:
                    return 1;

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        private int GetYDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Northwest:
                case Direction.North:
                case Direction.Northeast:
                    return -1;

                case Direction.West:
                case Direction.East:
                    return 0;

                case Direction.Southwest:
                case Direction.Southeast:
                case Direction.South:
                    return 1;

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }


        private void GetNewTarget()
        {
            const int size = 100;
            int steps = RandomGenerator.Next(1, 3);
            int offset = steps * size;


            var direction = (Direction)RandomGenerator.Next(0, 8);

            var curPosition = _mapObject.Position.Position;
            _targetX = curPosition.X + offset * GetXDirection(direction);
            _targetY = curPosition.Y + offset * GetYDirection(direction);
            _mapObject.Direction = direction;
        }
    }
}
