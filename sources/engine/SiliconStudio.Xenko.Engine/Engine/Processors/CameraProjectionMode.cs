// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Engine.Processors
{
    /// <summary>
    /// Projection of a <see cref="CameraComponent"/>.
    /// </summary>
    [DataContract("CameraProjectionMode")]
    public enum CameraProjectionMode
    {
        /// <summary>
        /// A perspective projection.
        /// </summary>
        /// <userdoc>A perspective projection (usually used for 3D games).</userdoc>
        Perspective,

        /// <summary>
        /// An orthographic projection.
        /// </summary>
        /// <userdoc>An orthographic projection (usually used for 2D games).</userdoc>
        Orthographic
    }
}