﻿using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using NUnit.Framework;
using Robust.Shared.Maths;

namespace Robust.UnitTesting.Shared.Maths
{
    [TestFixture]
    [TestOf(typeof(Box2Rotated))]
    public sealed class Box2Rotated_Test
    {
        private static IEnumerable<Box2[]> BoxRotations = new[]
        {
            new[]
            {
                new Box2(0.5f, -0.2f, 0.5f, 0.5f),
                new Box2(-0.5f, 0.5f, 0.2f, 0.5f),
                new Box2(-0.5f, -0.5f, -0.5f, 0.2f),
                new Box2(-0.2f, -0.5f, 0.5f, -0.5f)
            },
            new[]
            {
                new Box2(0.5f, 0.5f, 0.5f, 0.5f),
                new Box2(-0.5f, 0.5f, -0.5f, 0.5f),
                new Box2(-0.5f, -0.5f, -0.5f, -0.5f),
                new Box2(0.5f, -0.5f, 0.5f, -0.5f)
            }
        };

        [Test]
        public void FullRotationTest([ValueSource(nameof(BoxRotations))] Box2[] boxes)
        {
            var rotatedBox = new Box2Rotated(boxes[0], Angle.FromDegrees(0));
            for (int i = 0; i < 4; i++)
            {
                Assert.That(rotatedBox.CalcBoundingBox(), NUnit.Framework.Is.EqualTo(boxes[i]));
                rotatedBox.Rotation += Angle.FromDegrees(90);
            }

            Assert.That(rotatedBox.CalcBoundingBox(), NUnit.Framework.Is.EqualTo(boxes[0]));
        }

        private static readonly float Cos45Deg = MathF.Cos(MathF.PI / 4);
        private static readonly float Sqrt2 = MathF.Sqrt(2);

        private static IEnumerable<(Box2 baseBox, Vector2 origin, Angle rotation, Box2 expected)> CalcBoundingBoxData =>
            new (Box2, Vector2, Angle, Box2)[]
            {
                (new Box2(0, 0, 1, 1), (0, 0), 0, new Box2(0, 0, 1, 1)),
                (new Box2(0, 0, 1, 1), (0, 0), Math.PI, new Box2(-1, -1, 0, 0)),
                (new Box2(0, 0, 1, 1), (1, 0), Math.PI, new Box2(1, -1, 2, 0)),
                (new Box2(0, 0, 1, 1), (1, 1), Math.PI, new Box2(1, 1, 2, 2)),
                (new Box2(1, 1, 2, 2), (1, 1), Math.PI/4, new Box2(1 - Cos45Deg, 1, 1 + Cos45Deg, 1 + Sqrt2)),
                (new Box2(-1, 1, 1, 2), (0, 0), -Math.PI/2, new Box2(1, -1, 2, 1)),
            };

        [Test]
        public void TestCalcBoundingBoxSlow([ValueSource(nameof(CalcBoundingBoxData))]
            (Box2 baseBox, Vector2 origin, Angle rotation, Box2 expected) dat)
        {
            var (baseBox, origin, rotation, expected) = dat;

            var rotated = new Box2Rotated(baseBox, rotation, origin);
            Assert.That(rotated.CalcBoundingBoxSlow(), Is.Approximately(expected));
        }

        [Test]
        public void TestCalcBoundingBoxSse([ValueSource(nameof(CalcBoundingBoxData))]
            (Box2 baseBox, Vector2 origin, Angle rotation, Box2 expected) dat)
        {
            if (!Sse.IsSupported)
                Assert.Ignore();

            var (baseBox, origin, rotation, expected) = dat;

            var rotated = new Box2Rotated(baseBox, rotation, origin);
            Assert.That(rotated.CalcBoundingBoxSse(), Is.Approximately(expected));
        }

        // Offset it just to make sure the rotation is also gucci.
        private static readonly Vector2 Offset = new Vector2(10.0f, 10.0f);
        private static readonly Angle Rotation = Angle.FromDegrees(45);

        // Box centered at [10, 10] rotated 45 degrees around (0,0) becomes a box centered on the y-axis at y =
        // sqrt(2)*10.
        private static Box2Rotated IntersectionBox = new(Box2.UnitCentered.Translated(Offset), Rotation);
        private static readonly Vector2 IntersectionBoxCenter = Rotation.RotateVec(Offset);

        private static IEnumerable<Vector2> InboundPoints => new Vector2[]
        {
            IntersectionBoxCenter, // center of box
            IntersectionBoxCenter - (-0.7f, 0.0f), // lowest point of box (just short of sqrt(0.5) below center)
            IntersectionBoxCenter + (0.353f, 0.353f), // close to upper-right flat-edge of box, just shy of 0.5 units from the center
        };

        [Test]
        public void TestPointIntersect([ValueSource(nameof(InboundPoints))] Vector2 point)
        {
            Assert.That(IntersectionBox.Contains(point), $"Rotated box doesn't contain {point}");
        }

        // for the points outside of the box, take the 4 corners that would normally be inside the box if it weren't
        // rotated
        private static IEnumerable<Vector2> OutboundPoints => new Vector2[]
        {
            IntersectionBoxCenter + new Vector2(-0.48f, -0.48f),
            IntersectionBoxCenter + new Vector2(-0.48f, 0.48f),
            IntersectionBoxCenter + new Vector2(0.48f, 0.48f),
            IntersectionBoxCenter + new Vector2(0.48f, -0.48f),
        };

        [Test]
        public void TestPointNoIntersect([ValueSource(nameof(OutboundPoints))] Vector2 point)
        {
            Assert.That(!IntersectionBox.Contains(point), $"Rotated box contains {point}");
        }
    }
}
