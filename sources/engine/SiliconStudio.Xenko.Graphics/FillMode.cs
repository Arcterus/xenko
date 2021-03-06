// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>	
    /// <p>Determines the fill mode to use when rendering triangles.</p>	
    /// </summary>	
    /// <remarks>	
    /// <p>This enumeration is part of a rasterizer-state object description (see <strong><see cref="RasterizerStateDescription"/></strong>).</p>	
    /// </remarks>
    [DataContract]
    public enum FillMode : int
    {

        /// <summary>	
        /// <dd> <p>Draw lines connecting the vertices. Adjacent vertices are not drawn.</p> </dd>	
        /// </summary>	
        Wireframe = unchecked((int)2),

        /// <summary>	
        /// <dd> <p>Fill the triangles formed by the vertices. Adjacent vertices are not drawn.</p> </dd>	
        /// </summary>	
        Solid = unchecked((int)3),
    }
}