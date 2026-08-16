using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WhalesIslandReef.Presentation.D20.PlayerAnimation;

namespace D20.Tests.A02.PlayMode
{
    public sealed class D20A02PlayerAnimationPlayModeTests
    {
        [UnityTest]
        public IEnumerator TenReadOnlyActionSnapshotsNeverMoveThePhysicalRoot()
        {
            var go = new GameObject("D20-A02-PlayMode-Root");
            go.transform.position = new Vector3(4f, 2f, -3f);
            var adapter = go.AddComponent<D20PlayerAnimationPresentationAdapter>();
            var origin = go.transform.position;
            var snapshots = new[]
            {
                new D20PlayerAnimationSnapshot { grounded = true, speed01 = 0f },
                new D20PlayerAnimationSnapshot { grounded = true, speed01 = 0.4f },
                new D20PlayerAnimationSnapshot { grounded = true, speed01 = 0.9f },
                new D20PlayerAnimationSnapshot { grounded = false, verticalSpeed = 2f },
                new D20PlayerAnimationSnapshot { grounded = false, verticalSpeed = -2f },
                new D20PlayerAnimationSnapshot { grounded = true, actionIntent = "interact" },
                new D20PlayerAnimationSnapshot { grounded = true, actionIntent = "talk" },
                new D20PlayerAnimationSnapshot { grounded = true, actionIntent = "fish" },
                new D20PlayerAnimationSnapshot { grounded = true, actionIntent = "boarding" },
                new D20PlayerAnimationSnapshot { grounded = true, actionIntent = "helm" }
            };

            try
            {
                foreach (var snapshot in snapshots)
                {
                    adapter.ApplyReadOnlySnapshot(snapshot);
                    yield return null;
                    Assert.AreEqual(origin, go.transform.position);
                }
                Assert.AreEqual(D20PlayerAnimationState.Helm, adapter.CurrentState);
                adapter.SetPaused(true);
                yield return null;
                Assert.IsTrue(adapter.IsPaused);
                Assert.AreEqual(origin, go.transform.position);
            }
            finally
            {
                Object.Destroy(go);
            }
        }
    }
}
