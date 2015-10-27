﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Engine.Tests
{
    [TestFixture]
    public class SpriteTestGame : EngineTestBase
    {
        private SpriteSheet ballSprite1;
        private SpriteSheet ballSprite2;

        private Entity ball;

        private SpriteComponent spriteComponent;

        private Vector2 areaSize;

        private TransformComponent transfoComponent;

        private Vector2 ballSpeed = new Vector2(-300, -200);

        private Entity foreground;

        private Entity background;

        private SpriteSheet groundSprites;

        public SpriteTestGame()
        {   
            CurrentVersion = 7;
            GraphicsDeviceManager.PreferredDepthStencilFormat = PixelFormat.D24_UNorm_S8_UInt;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();
            
            // sets the virtual resolution
            areaSize = new Vector2(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);

            // Creates the camera
            CameraComponent.UseCustomProjectionMatrix = true;
            CameraComponent.ProjectionMatrix = Matrix.OrthoRH(areaSize.X, areaSize.Y, -2, 2);

            // Load assets
            groundSprites = Asset.Load<SpriteSheet>("GroundSprite");
            ballSprite1 = Asset.Load<SpriteSheet>("BallSprite1");
            ballSprite2 = Asset.Load<SpriteSheet>("BallSprite2");
            ball = new Entity { new SpriteComponent { SpriteProvider = new SpriteFromSheet { Sheet = Asset.Load<SpriteSheet>("BallSprite1") } } };

            // create fore/background entities
            foreground = new Entity();
            background = new Entity();
            foreground.Add(new SpriteComponent { SpriteProvider = new SpriteFromSheet { Sheet = groundSprites }, CurrentFrame = 1 });
            background.Add(new SpriteComponent { SpriteProvider = new SpriteFromSheet { Sheet = groundSprites }, CurrentFrame = 0 });

            Scene.AddChild(ball);
            Scene.AddChild(foreground);
            Scene.AddChild(background);

            spriteComponent = ball.Get(SpriteComponent.Key);
            transfoComponent = ball.Get(TransformComponent.Key);
            
            var decorationScalings = new Vector3(areaSize.X, areaSize.Y, 1);
            background.Get(TransformComponent.Key).Scale = decorationScalings;
            foreground.Get(TransformComponent.Key).Scale = decorationScalings/2;
            background.Get(TransformComponent.Key).Position = new Vector3(0, 0, -1);
            foreground.Get(TransformComponent.Key).Position = new Vector3(0, 0, 1);

            SpriteAnimation.Play(spriteComponent, 0, spriteComponent.SpriteProvider.SpritesCount-1, AnimationRepeatMode.LoopInfinite, 30);
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.DrawOrder = -1;
            FrameGameSystem.Draw(() => SpriteAnimation.Stop(spriteComponent)).TakeScreenshot();
            FrameGameSystem.Update(() => SetFrameAndUpdateBall(20, 15)).TakeScreenshot();
            FrameGameSystem.Update(() => SetSpriteImage(ballSprite2)).TakeScreenshot();
        }

        private void SetSpriteImage(SpriteSheet sprite)
        {
            spriteComponent.SpriteProvider = new SpriteFromSheet { Sheet = sprite };
        }

        protected override void Update(GameTime time)
        {
            base.Update(time);

            if(!ScreenShotAutomationEnabled)
                UpdateBall((float)time.Elapsed.TotalSeconds);

            if (Input.IsKeyPressed(Keys.D1))
                SetSpriteImage(ballSprite1);
            if (Input.IsKeyPressed(Keys.D2))
                SetSpriteImage(ballSprite2);

            if (Input.IsKeyDown(Keys.Space))
                spriteComponent.CurrentFrame = 0;
        }

        private void SetFrameAndUpdateBall(int updateTimes, int frame)
        {
            spriteComponent.CurrentFrame = frame;

            for (int i = 0; i < updateTimes; i++)
                UpdateBall(0.033f);
        }

        private void UpdateBall(float totalSeconds)
        {
            const float RotationSpeed = (float)Math.PI / 2;

            var deltaRotation = RotationSpeed * totalSeconds;

            transfoComponent.RotationEulerXYZ = new Vector3(0,0, transfoComponent.RotationEulerXYZ.Z + deltaRotation);

            var sprite = spriteComponent.SpriteProvider.GetSprite(spriteComponent.CurrentFrame);
            var spriteSize = new Vector2(sprite.Region.Width, sprite.Region.Height);

            for (int i = 0; i < 2; i++)
            {
                var nextPosition = transfoComponent.Position[i] + totalSeconds * ballSpeed[i];

                var infBound = -areaSize[i] / 2 + sprite.Center[i];
                var supBound =  areaSize[i] / 2 - sprite.Center[i];

                if (nextPosition > supBound || nextPosition<infBound)
                {
                    ballSpeed[i] = -ballSpeed[i];

                    if (nextPosition > supBound)
                        nextPosition = supBound - (nextPosition - supBound);
                    else
                        nextPosition = infBound + (infBound - nextPosition);
                }

                transfoComponent.Position[i] = nextPosition;
            }
        }

        [Test]
        public void RunTestGame()
        {
            RunGameTest(new SpriteTestGame());
        }

        public static void Main()
        {
            using (var testGame = new SpriteTestGame())
                testGame.Run();
        }
    }
}