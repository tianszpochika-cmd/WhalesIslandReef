// ============================================================
// SailingSystem —— D2 最小航行强逻辑
//
// 归属：Whalebone.Systems.D2
// 规则：只接受输入意图并修改自己的运行时快照；不依赖 Unity、Crest、网络或 LLM。
// 表现层通过 Snapshot 读取位置/速度/朝向，再把结果应用到 Transform。
// ============================================================

using System;

namespace Whalebone.Systems.D2
{
    public sealed class SailingSystem
    {
        private const float MinDeltaTime = 0.0001f;
        private readonly float _turnRate;
        private readonly float _acceleration;
        private readonly float _deceleration;
        private readonly SailingSnapshot _state;

        public SailingSystem(
            string shipId,
            float maxSpeed,
            float turnRate,
            float acceleration = 2.5f,
            float deceleration = 4f)
        {
            _turnRate = SanitizePositive(turnRate, 1f);
            _acceleration = SanitizePositive(acceleration, 1f);
            _deceleration = SanitizePositive(deceleration, 1f);
            _state = new SailingSnapshot
            {
                ShipId = shipId ?? string.Empty,
                MaxSpeed = SanitizePositive(maxSpeed, 1f),
                ControlMode = D2ControlMode.Character,
            };
        }

        public SailingSnapshot Snapshot => _state.Clone();

        public void SetControlMode(D2ControlMode mode)
        {
            _state.ControlMode = mode;
            if (mode == D2ControlMode.Locked)
                _state.Speed = 0f;
        }

        /// <summary>
        /// 用固定语义推进一帧航行。Throttle&lt;=0 表示停止/减速，D2 不允许倒车。
        /// </summary>
        public void Tick(SailingInput rawInput, float deltaTime)
        {
            if (_state.ControlMode != D2ControlMode.Ship) return;
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime <= 0f) return;

            var input = rawInput.Normalized();
            float targetSpeed = input.Throttle > 0f ? input.Throttle * _state.MaxSpeed : 0f;
            float rate = targetSpeed < _state.Speed || input.Brake ? _deceleration : _acceleration;
            _state.Speed = MoveTowards(_state.Speed, input.Brake ? 0f : targetSpeed, rate * deltaTime);

            float speedRatio = _state.MaxSpeed <= 0f ? 0f : _state.Speed / _state.MaxSpeed;
            float turnScale = 0.2f + 0.8f * Clamp01(speedRatio);
            _state.Heading = NormalizeHeading(_state.Heading + input.Rudder * _turnRate * turnScale * deltaTime);

            double radians = _state.Heading * Math.PI / 180.0;
            _state.PositionX += (float)(Math.Sin(radians) * _state.Speed * deltaTime);
            _state.PositionZ += (float)(Math.Cos(radians) * _state.Speed * deltaTime);
        }

        public void ApplySnapshot(SailingSnapshot snapshot)
        {
            if (snapshot == null) return;
            if (!string.IsNullOrEmpty(snapshot.ShipId)) _state.ShipId = snapshot.ShipId;
            _state.PositionX = SanitizeFinite(snapshot.PositionX);
            _state.PositionZ = SanitizeFinite(snapshot.PositionZ);
            _state.Heading = NormalizeHeading(snapshot.Heading);
            _state.Speed = Clamp(SanitizeFinite(snapshot.Speed), 0f, _state.MaxSpeed);
            _state.ControlMode = snapshot.ControlMode;
            if (_state.ControlMode == D2ControlMode.Locked) _state.Speed = 0f;
        }

        private static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta) return target;
            return current + Math.Sign(target - current) * maxDelta;
        }

        private static float NormalizeHeading(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            value %= 360f;
            if (value < 0f) value += 360f;
            return value;
        }

        private static float SanitizePositive(float value, float fallback)
        {
            return float.IsNaN(value) || float.IsInfinity(value) || value <= 0f ? fallback : value;
        }

        private static float SanitizeFinite(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }
    }
}
