﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// An abstract window.
    /// </summary>
    internal class GameWindowOpenTK : GameWindow
    {
        private bool isMouseVisible;

        private bool isMouseCurrentlyHidden;

        private OpenTK.GameWindow gameForm;
        private WindowHandle nativeWindow;

        internal GameWindowOpenTK()
        {
        }

        public override WindowHandle NativeWindow
        {
            get
            {
                return nativeWindow;
            }
        }

        public override Int2 Position
        {
            get
            {
                if (gameForm == null)
                    return base.Position;

                return new Int2(gameForm.X, gameForm.Y);
            }
            set
            {
                if (gameForm != null)
                {
                    gameForm.X = value.X;
                    gameForm.Y = value.Y;
                }

                base.Position = value;
            }
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {

        }

        public override void EndScreenDeviceChange(int clientWidth, int clientHeight)
        {

        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // Desktop doesn't have orientation (unless on Windows 8?)
        }

        internal override bool CanHandle(GameContext gameContext)
        {
            return gameContext.ContextType == AppContextType.DesktopOpenTK;
        }

        internal override void Initialize(GameContext gameContext)
        {
            this.GameContext = gameContext;

            gameForm = (OpenTK.GameWindow)gameContext.Control;
            nativeWindow = new WindowHandle(AppContextType.DesktopOpenTK, gameForm);

            // Setup the initial size of the window
            var width = gameContext.RequestedWidth;
            if (width == 0)
            {
                width = gameForm.Width;
            }

            var height = gameContext.RequestedHeight;
            if (height == 0)
            {
                height = gameForm.Height;
            }

            gameForm.ClientSize = new System.Drawing.Size(width, height);

            gameForm.MouseEnter += GameWindowForm_MouseEnter;
            gameForm.MouseLeave += GameWindowForm_MouseLeave;

            gameForm.Resize += OnClientSizeChanged;
        }

        internal override void Run()
        {
            Debug.Assert(InitCallback != null);
            Debug.Assert(RunCallback != null);

            // Initialize the init callback
            InitCallback();

            // Make the window visible
            gameForm.Visible = true;

            // Run the rendering loop
            try
            {
                while (!Exiting)
                {
                    gameForm.ProcessEvents();

                    RunCallback();
                }

                if (gameForm != null)
                {
                    if (OpenTK.Graphics.GraphicsContext.CurrentContext == gameForm.Context)
                        gameForm.Context.MakeCurrent(null);
                    gameForm.Close();
                    gameForm.Dispose();
                    gameForm = null;
                }
            }
            finally
            {
                if (ExitCallback != null)
                {
                    ExitCallback();
                }
            }
        }

        private void GameWindowForm_MouseEnter(object sender, System.EventArgs e)
        {
            if (!isMouseVisible && !isMouseCurrentlyHidden)
            {
                Cursor.Hide();
                isMouseCurrentlyHidden = true;
            }
        }

        private void GameWindowForm_MouseLeave(object sender, System.EventArgs e)
        {
            if (isMouseCurrentlyHidden)
            {
                Cursor.Show();
                isMouseCurrentlyHidden = false;
            }
        }

        public override bool IsMouseVisible
        {
            get
            {
                return isMouseVisible;
            }
            set
            {
                if (isMouseVisible != value)
                {
                    isMouseVisible = value;
                    if (isMouseVisible)
                    {
                        if (isMouseCurrentlyHidden)
                        {
                            Cursor.Show();
                            isMouseCurrentlyHidden = false;
                        }
                    }
                    else if (!isMouseCurrentlyHidden)
                    {
                        Cursor.Hide();
                        isMouseCurrentlyHidden = true;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="GameWindow" /> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        public override bool Visible
        {
            get
            {
                return gameForm.Visible;
            }
            set
            {
                gameForm.Visible = value;
            }
        }

        protected override void SetTitle(string title)
        {
            gameForm.Title = title;
        }

        internal override void Resize(int width, int height)
        {
            gameForm.ClientSize = new System.Drawing.Size(width, height);
        }

        public override bool IsBorderLess
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        public override bool AllowUserResizing
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        public override Rectangle ClientBounds
        {
            get
            {
                return new Rectangle(0, 0, gameForm.ClientSize.Width, gameForm.ClientSize.Height);
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get
            {
                return DisplayOrientation.Default;
            }
        }

        public override bool IsMinimized
        {
            get
            {
                return gameForm.WindowState == OpenTK.WindowState.Minimized;
            }
        }

        protected override void Destroy()
        {
            if (gameForm != null)
            {
                gameForm.Context.MakeCurrent(null);
                gameForm.Close();
                gameForm.Dispose();
                gameForm = null;
            }

            base.Destroy();
        }
    }
}
#endif