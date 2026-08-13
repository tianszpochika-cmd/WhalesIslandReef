using UnityEngine;
using Whalebone.Systems.D2;
using Whalebone.Systems.D16;
using Whalebone.Systems.Save;
using Whalebone.Gameplay.D7;
using Whalebone.Gameplay.UI.D16;

namespace Whalebone.Gameplay.D2
{
    public sealed class D2ShipController : MonoBehaviour
    {
        private SailingSystem _sailing;
        private D2ControlMode _mode = D2ControlMode.Character;

        public SailingSnapshot Snapshot => _sailing == null ? new SailingSnapshot() : _sailing.Snapshot;

        public void Initialize(Vector3 position, float heading)
        {
            var ship = GameState.CurrentShip;
            var type = ship == null ? null : ShipTypeSeed.Find(ship.ShipTypeId);
            string shipId = ship == null ? "d2_ship" : ship.Id;
            float maxSpeed = type == null ? 4f : type.Speed;
            float turnRate = (type == null ? 6f : type.Turn) * 10f;
            _sailing = new SailingSystem(shipId, maxSpeed, turnRate);
            var location = GameState.CurrentPlayer == null ? null : GameState.CurrentPlayer.Location;
            bool hasSavedOpenSea = location != null && location.Type == "open_sea";
            _sailing.ApplySnapshot(new SailingSnapshot
            {
                PositionX = hasSavedOpenSea ? location.X : position.x,
                PositionZ = hasSavedOpenSea ? location.Y : position.z,
                Heading = hasSavedOpenSea ? location.Heading : heading,
                Speed = hasSavedOpenSea ? location.Speed : 0f,
                ControlMode = D2ControlMode.Character,
            });
            ApplyTransform(_sailing.Snapshot);
        }

        public void SetControlMode(D2ControlMode mode)
        {
            _mode = mode;
            if (_sailing != null) _sailing.SetControlMode(mode);
        }

        public void ApplySnapshot(SailingSnapshot snapshot)
        {
            if (_sailing == null) return;
            _sailing.ApplySnapshot(snapshot);
            ApplyTransform(_sailing.Snapshot);
        }

        private void Update()
        {
            if (_sailing == null || _mode != D2ControlMode.Ship || Time.timeScale <= 0f ||
                !D7ControlCoordinator.GameplayInputAllowed) return;
            float throttle = 0f;
            float rudder = 0f;
            if (D16InputRouter.IsPressed(D16InputAction.MoveForward)) throttle += 1f;
            if (D16InputRouter.IsPressed(D16InputAction.MoveBack)) throttle -= 1f;
            if (D16InputRouter.IsPressed(D16InputAction.MoveLeft)) rudder -= 1f;
            if (D16InputRouter.IsPressed(D16InputAction.MoveRight)) rudder += 1f;
            _sailing.Tick(new SailingInput(throttle, rudder), Time.deltaTime);
            ApplyTransform(_sailing.Snapshot);
        }

        private void ApplyTransform(SailingSnapshot snapshot)
        {
            transform.position = new Vector3(snapshot.PositionX, transform.position.y, snapshot.PositionZ);
            transform.rotation = Quaternion.Euler(0f, snapshot.Heading, 0f);
        }
    }
}
