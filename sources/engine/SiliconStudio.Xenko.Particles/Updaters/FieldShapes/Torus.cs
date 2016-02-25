﻿using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.DebugDraw;

namespace SiliconStudio.Xenko.Particles.Updaters.FieldShapes
{
    [DataContract("FieldShapeTorus")]
    public class Torus : FieldShape
    {
        public override DebugDrawShape GetDebugDrawShape(out Vector3 pos, out Quaternion rot, out Vector3 scl)
        {
            pos = new Vector3(0, 0, 0);
            rot = new Quaternion(0, 0, 0, 1);
            scl = new Vector3(2 * BigRadius, 2 * BigRadius, 2 * BigRadius); // The default torus for drawing has a small radius of 0.5f
            return DebugDrawShape.Torus;
        }

        /// <summary>
        /// Big radius of the torus
        /// </summary>
        /// <userdoc>
        /// Big radius of the torus (defines the circle around which the torus is positioned)
        /// </userdoc>
        [DataMember(10)]
        [Display("Big radius")]
        public float BigRadius { get; set; } = 1f;

        [DataMemberIgnore]
        private float smallRadius { get; set; } = 0.33333f;

        [DataMemberIgnore]
        private float smallRadiusSquared = 0.11111f;

        /// <summary>
        /// Small radius of the torus, given as a relative size to the big radius
        /// </summary>
        /// <userdoc>
        /// Small radius of the torus, given as a relative to the big radius (percentage between 0 and 1)
        /// </userdoc>
        [DataMember(20)]
        [DataMemberRange(0, 1, 0.001, 0.1)]
        [Display("Small radius")]
        public float SmallRadius
        {
            get { return smallRadius; }
            set { smallRadius = value; smallRadiusSquared = value * value; }
        }

        [DataMemberIgnore]
        private Vector3 fieldPosition;

        [DataMemberIgnore]
        private Quaternion fieldRotation;

        [DataMemberIgnore]
        private Quaternion inverseRotation;

        [DataMemberIgnore]
        private Vector3 fieldSize;

        public override void PreUpdateField(Vector3 position, Quaternion rotation, Vector3 size)
        {
            this.fieldSize = size * BigRadius;
            this.fieldPosition = position;
            this.fieldRotation = rotation;
            inverseRotation = new Quaternion(-rotation.X, -rotation.Y, -rotation.Z, rotation.W);
        }

        public override float GetDistanceToCenter(
            Vector3 particlePosition, Vector3 particleVelocity,
            out Vector3 alongAxis, out Vector3 aroundAxis, out Vector3 awayAxis)
        {
            alongAxis = new Vector3(0, 1, 0);

            particlePosition -= fieldPosition;            
            inverseRotation.Rotate(ref particlePosition);
            particlePosition /= fieldSize;

            // Start by positioning hte particle on the torus' plane
            var projectedPosition = new Vector3(particlePosition.X, 0, particlePosition.Z);
            var distanceFromOrigin = projectedPosition.Length();
            var distSquared = 1 + distanceFromOrigin * distanceFromOrigin - 2 * distanceFromOrigin + particlePosition.Y * particlePosition.Y;

            var totalStrength = (distSquared >= smallRadiusSquared) ? 1 : ((float) Math.Sqrt(distSquared) / smallRadius);

            // Fix the field's axis back to world space
            var forceAxis = Vector3.Cross(alongAxis, projectedPosition);
            fieldRotation.Rotate(ref forceAxis);
            forceAxis.Normalize();
            alongAxis = forceAxis;

            projectedPosition = (distanceFromOrigin > 0) ? (projectedPosition/(float)distanceFromOrigin) : projectedPosition;
            projectedPosition -= particlePosition;
            projectedPosition *= fieldSize;
            fieldRotation.Rotate(ref projectedPosition);
            awayAxis = -projectedPosition;
            awayAxis.Normalize();

            aroundAxis = Vector3.Cross(alongAxis, awayAxis);

            return totalStrength;
        }

        public override bool IsPointInside(Vector3 particlePosition, out Vector3 surfacePoint, out Vector3 surfaceNormal)
        {
            particlePosition -= fieldPosition;
            inverseRotation.Rotate(ref particlePosition);
            particlePosition /= fieldSize;

            var projectedPosition = new Vector3(particlePosition.X, 0, particlePosition.Z);
            var distanceFromOrigin = projectedPosition.Length();
            projectedPosition = (distanceFromOrigin > MathUtil.ZeroTolerance) ? (projectedPosition / distanceFromOrigin) : projectedPosition;


            surfaceNormal = particlePosition - projectedPosition;
            surfacePoint = surfaceNormal;
            var coef = surfacePoint.Length();

            var isInside = (coef <= smallRadius);

            coef = (coef > MathUtil.ZeroTolerance) ? smallRadius/coef : smallRadius;
            surfacePoint *= coef;
            surfacePoint += projectedPosition;


            // Fix the surface point and normal to world space
            fieldRotation.Rotate(ref surfaceNormal);
            surfaceNormal *= fieldSize;
            surfaceNormal.Normalize();

            fieldRotation.Rotate(ref surfacePoint);
            surfacePoint *= fieldSize;
            surfacePoint += fieldPosition;

            return isInside;
        }
    }
}