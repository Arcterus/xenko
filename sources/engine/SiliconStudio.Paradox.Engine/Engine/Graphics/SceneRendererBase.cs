// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine.Graphics.Composers;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// Base implementation for a <see cref="ISceneRenderer"/>.
    /// </summary>
    public abstract class SceneRendererBase : RendererBase, ISceneRenderer
    {
        protected SceneRendererBase()
        {
            Output = new CurrentRenderFrameProvider();
            Parameters = new ParameterCollection();
            Viewport = new RectangleF(0, 0, 100f, 100f);
            IsViewportInPercentage = true;
        }

        [DataMember(100)]
        public ISceneRendererOutput Output { get; set; }

        /// <summary>
        /// Gets or sets the viewport in percentage or pixel.
        /// </summary>
        /// <value>The viewport in percentage or pixel.</value>
        [DataMember(110)]
        public RectangleF Viewport { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the viewport is in fixed pixels instead of percentage.
        /// </summary>
        /// <value><c>true</c> if the viewport is in pixels instead of percentage; otherwise, <c>false</c>.</value>
        /// <userdoc>When this value is true, the Viewport size is a percentage (0-100) calculated relatively to the size of the Output, else it is a fixed size in pixels.</userdoc>
        [DataMember(50)]
        [DefaultValue(true)]
        [Display("Viewport in percentage?")]
        public bool IsViewportInPercentage { get; set; }

        /// <summary>
        /// Gets the parameters used to in place of the default <see cref="RenderContext.Parameters"/>.
        /// </summary>
        /// <value>The parameters.</value>
        [DataMemberIgnore]
        public ParameterCollection Parameters { get; private set; }

        protected override void DrawCore(RenderContext context)
        {
            var output = Output.GetSafeRenderFrame(context);
            if (output != null)
            {
                try
                {
                    context.PushParameters(Parameters);

                    // Setup the render target
                    context.GraphicsDevice.SetDepthAndRenderTarget(output.DepthStencil, output.RenderTarget);

                    Viewport viewport;
                    var rect = Viewport;
                    // Setup the viewport
                    if (IsViewportInPercentage)
                    {
                        var width = output.RenderTarget.Width;
                        var height = output.RenderTarget.Height;
                        viewport = new Viewport((int)(rect.X * width / 100.0f), (int)(rect.Y * height / 100.0f), (int)(rect.Width * width / 100.0f), (int)(rect.Height * height / 100.0f));
                    }
                    else
                    {
                        viewport = new Viewport((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
                    }
                    context.GraphicsDevice.SetViewport(viewport);

                    DrawCore(context, output);
                }
                finally
                {
                    context.PopParameters();

                    // Make sure that states are clean after this rendering
                    context.GraphicsDevice.ResetStates();
                }
            }
        }

        protected abstract void DrawCore(RenderContext context, RenderFrame output);

        protected override void Destroy()
        {
            if (Output != null)
            {
                Output.Dispose();
                Output = null;
            }

            base.Destroy();
        }
    }
}