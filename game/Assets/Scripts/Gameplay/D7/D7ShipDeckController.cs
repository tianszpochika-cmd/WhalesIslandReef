// ============================================================
// D7ShipDeckController —— 舵位、甲板和上下船状态诊断
//
// 它不裁决航行数值，只提供船上交互锚点和表现层状态，供输入、HUD
// 与运行时验收读取。船舶速度/方向仍由 D2 SailingSystem 所有。
// ============================================================

using UnityEngine;
using Whalebone.Gameplay.D2;
using Whalebone.Systems.D2;

namespace Whalebone.Gameplay.D7
{
    public sealed class D7ShipDeckController : MonoBehaviour
    {
        private Transform _ship;
        private Transform _player;
        private D2ControlRouter _router;
        private D2ControlMode _lastMode = D2ControlMode.Character;
        // Keep a deterministic fallback for scenes without a presentation
        // helm anchor. R03 publishes D7_VisibleHelmAnchor below the stable
        // ship root; when it exists, the runtime must consume that authored
        // presentation point instead of this historical placeholder offset.
        // The old fixed value placed the Player at z=-1.47 while the visible
        // R03 wheel was at z=1.15, which made boarding look like control of an
        // unrelated prop even though D2/D7 state transitions passed.
        private static readonly Vector3 DefaultWorldHelmOffset = new Vector3(0f, 0.96f, -1.47f);

        public Transform HelmPoint { get; private set; }
        public bool IsReady { get; private set; }
        public bool HelmReady => HelmPoint != null;
        public bool PlayerBoarded { get; private set; }
        public bool DeckCollisionReady { get; private set; }
        public int TransitionCount { get; private set; }

        public void Initialize(Transform ship, Transform player, D2ControlRouter router)
        {
            _ship = ship;
            _player = player;
            _router = router;
            if (_ship == null) return;
            EnsureHelmPoint();
            IsReady = HelmPoint != null && _player != null && _router != null;
            DeckCollisionReady = _ship.GetComponentsInChildren<Collider>(true).Length > 0;
        }

        public bool IsWithinBoardingRange(Transform player, float range)
        {
            if (player == null || _ship == null) return false;
            Vector3 a = player.position;
            a.y = 0f;
            Vector3 shipPosition = _ship.position;
            shipPosition.y = 0f;
            float shipDistance = Vector3.Distance(a, shipPosition);

            // 舵位是最明确的交互入口，但大型/缩放后的占位船可能让舵位
            // 偏离船体中心。靠近船体本身也应允许按 E，避免“看得到船却
            // 永远无法登船”的体验断点。
            float helmDistance = float.PositiveInfinity;
            if (HelmPoint != null)
            {
                Vector3 helmPosition = HelmPoint.position;
                helmPosition.y = 0f;
                helmDistance = Vector3.Distance(a, helmPosition);
            }

            return Mathf.Min(shipDistance, helmDistance) <= Mathf.Max(1f, range);
        }

        /// <summary>由 D2 控制路由在切换瞬间调用，避免同帧上下船被 Update 采样遗漏。</summary>
        public void RecordControlTransition(D2ControlMode mode)
        {
            if (mode == _lastMode)
            {
                PlayerBoarded = _player != null && _player.parent == _ship;
                return;
            }

            TransitionCount++;
            _lastMode = mode;
            PlayerBoarded = _player != null && _player.parent == _ship;
            Debug.Log("[D7/Ship] 控制权切换：" + mode + "，甲板状态=" + PlayerBoarded);
        }

        private void Update()
        {
            if (!IsReady || _router == null || _player == null) return;
            PlayerBoarded = _player.parent == _ship;
            RecordControlTransition(_router.Mode);
        }

        private void EnsureHelmPoint()
        {
            HelmPoint = _ship.Find("D7_HelmPoint");
            if (HelmPoint == null)
            {
                var go = new GameObject("D7_HelmPoint");
                HelmPoint = go.transform;
                HelmPoint.SetParent(_ship, false);
            }

            var authoredVisibleAnchor = FindAuthoredVisibleHelmAnchor();
            if (authoredVisibleAnchor != null)
            {
                // D19 owns the presentation anchor; D7 only consumes its
                // read-only world position and keeps the actual HelmPoint on
                // the stable gameplay ship root. InverseTransformPoint keeps
                // this correct under the scaled/rotated ship root.
                HelmPoint.localPosition = _ship.InverseTransformPoint(authoredVisibleAnchor.position);
            }
            else
            {
                // Preserve the scale-independent fallback contract when a
                // scene has no authored presentation anchor.
                HelmPoint.localPosition = _ship.InverseTransformVector(DefaultWorldHelmOffset);
            }
            HelmPoint.localRotation = Quaternion.identity;
        }

        private Transform FindAuthoredVisibleHelmAnchor()
        {
            var transforms = _ship.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var candidate = transforms[i];
                if (candidate != null && candidate != HelmPoint && candidate.name == "D7_VisibleHelmAnchor")
                    return candidate;
            }

            return null;
        }
    }
}
