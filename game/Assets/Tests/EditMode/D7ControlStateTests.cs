using NUnit.Framework;
using Whalebone.Systems.D7;

namespace Save.Tests
{
    public sealed class D7ControlStateTests
    {
        [Test]
        public void CharacterAuthority_AllowsOnlyCharacterInput()
        {
            var state = new D7ControlStateMachine();

            Assert.That(state.Snapshot.CharacterInputAllowed, Is.True);
            Assert.That(state.Snapshot.ShipInputAllowed, Is.False);
            Assert.That(state.Snapshot.CameraInputAllowed, Is.True);
        }

        [Test]
        public void ShipAuthority_AllowsOnlyShipInput()
        {
            var state = new D7ControlStateMachine();
            state.SetAuthority(D7ControlAuthority.Ship);

            Assert.That(state.Snapshot.CharacterInputAllowed, Is.False);
            Assert.That(state.Snapshot.ShipInputAllowed, Is.True);
            Assert.That(state.AuthorityLabel, Is.EqualTo("船舶"));
        }

        [Test]
        public void ModalInputBlocksGameplayAndCamera()
        {
            var state = new D7ControlStateMachine();
            state.SetAuthority(D7ControlAuthority.Ship);
            state.SetModal(D7ModalBlock.Dialogue);

            Assert.That(state.Snapshot.GameplayInputAllowed, Is.False);
            Assert.That(state.Snapshot.ShipInputAllowed, Is.False);
            Assert.That(state.Snapshot.CameraInputAllowed, Is.False);
            Assert.That(state.ModalLabel, Is.EqualTo("对话"));
        }

        [Test]
        public void ClearingModalRestoresPreviousAuthority()
        {
            var state = new D7ControlStateMachine();
            state.SetAuthority(D7ControlAuthority.Ship);
            state.SetModal(D7ModalBlock.Trade);
            state.SetModal(D7ModalBlock.None);

            Assert.That(state.Snapshot.ShipInputAllowed, Is.True);
            Assert.That(state.Snapshot.CharacterInputAllowed, Is.False);
        }

        [Test]
        public void LockedAuthorityNeverAllowsMovement()
        {
            var state = new D7ControlStateMachine();
            state.SetAuthority(D7ControlAuthority.Locked);

            Assert.That(state.Snapshot.CharacterInputAllowed, Is.False);
            Assert.That(state.Snapshot.ShipInputAllowed, Is.False);
            Assert.That(state.Snapshot.CameraInputAllowed, Is.True);
        }
    }
}
