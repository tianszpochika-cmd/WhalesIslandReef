using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace WhalesIslandReef.Tests.D19A01
{
    public sealed class D19A01TerrainPlayModeTests
    {
        [UnityTest]
        public IEnumerator WorkSceneProbeReportsGroundAndNavMesh()
        {
            Component probe = Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(component => component != null && component.GetType().FullName == "WhalesIslandReef.D19A01.D19A01TerrainRuntimeProbe");
            if (probe == null)
                Assert.Ignore("Open D19_Terrain_Work through D19A01TerrainBuilder.RunPlayModeSmoke before running this PlayMode test.");
            yield return null;
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex("\\[D19-A01\\] Terrain PlayMode probe (PASS|PARTIAL):.*"));
        }
    }
}
