using NUnit.Framework;
using UnityEngine;
using WhalesIslandReef.D19.Water;

namespace WhalesIslandReef.Tests.EditMode.D19Water
{
    public sealed class D19WaterPresentationTests
    {
        [Test]
        public void DefaultFrameUsesFrozenSeaLevelAndBoundedValues()
        {
            D19WaterFrameParameters frame = D19WaterFrameParameters.Default;
            Assert.That(frame.SeaLevelY, Is.EqualTo(0f));
            Assert.That(frame.WindDirection.magnitude, Is.EqualTo(1f).Within(0.001f));
            Assert.That(frame.Transparency, Is.InRange(0f, 1f));
            Assert.That(frame.Turbidity, Is.InRange(0f, 1f));
        }

        [Test]
        public void PresentationAcceptsWindWaveInterfaceWithoutWeatherDependency()
        {
            GameObject root = new GameObject("D19WaterTest");
            try
            {
                D19WaterPresentation presentation = root.AddComponent<D19WaterPresentation>();
                D19WaterFrameParameters frame = D19WaterFrameParameters.Default;
                frame.WindDirection = new Vector2(0f, 0f);
                frame.WindSpeed = -4f;
                frame.WaveAmplitude = 9f;
                presentation.ApplyExternalFrame(frame);
                Assert.That(presentation.SeaLevelY, Is.EqualTo(0f).Within(0.001f));
                Assert.That(presentation.WindDirection.magnitude, Is.EqualTo(1f).Within(0.001f));
                Assert.That(presentation.WindSpeed, Is.EqualTo(0f).Within(0.001f));
                Assert.That(presentation.WaveAmplitude, Is.EqualTo(2f).Within(0.001f));
                Assert.That(presentation.IsSeaLevelConsistent(0f), Is.True);
                Assert.That(presentation.IsSeaLevelConsistent(0.1f), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void QualityTierCanBeChangedWithoutChangingSeaLevel()
        {
            GameObject root = new GameObject("D19WaterQualityTest");
            try
            {
                D19WaterPresentation presentation = root.AddComponent<D19WaterPresentation>();
                presentation.SetQuality(D19WaterQualityTier.Low);
                Assert.That(presentation.Quality, Is.EqualTo(D19WaterQualityTier.Low));
                Assert.That(presentation.SeaLevelY, Is.EqualTo(0f).Within(0.001f));
                presentation.SetQuality(D19WaterQualityTier.High);
                Assert.That(presentation.Quality, Is.EqualTo(D19WaterQualityTier.High));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
