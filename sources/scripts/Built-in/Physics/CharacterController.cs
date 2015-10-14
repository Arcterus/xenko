using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.Physics;
using System;
using System.Linq;

namespace Xenko.Scripts
{
    public class CharacterController : SyncScript
    {
        private static readonly Vector3 UpVector = new Vector3(0, 1, 0);
        private static readonly Vector3 ForwardVector = new Vector3(0, 0, -1);

        public float Speed { get; set; } = 5.0f;
        public Entity CameraEntity { get; set; }

        private Character character;
        private CharacterElement characterElement;
        private Quaternion baseCameraRotation;

        public override void Start()
        {
            desiredYaw =
                yaw =
                    (float)
                        Math.Asin(2 * Entity.Transform.Rotation.X * Entity.Transform.Rotation.Y +
                                  2 * Entity.Transform.Rotation.Z * Entity.Transform.Rotation.W);

            desiredPitch =
                pitch =
                    (float)
                        Math.Atan2(
                            2 * Entity.Transform.Rotation.X * Entity.Transform.Rotation.W -
                            2 * Entity.Transform.Rotation.Y * Entity.Transform.Rotation.Z,
                            1 - 2 * Entity.Transform.Rotation.X * Entity.Transform.Rotation.X -
                            2 * Entity.Transform.Rotation.Z * Entity.Transform.Rotation.Z);

            if (!Platform.IsWindowsDesktop)
            {
                Input.ActivatedGestures.Add(new GestureConfigDrag());
            }

            Input.LockMousePosition(true);
            Game.IsMouseVisible = false;

            var physComponent = Entity.Get<PhysicsComponent>();
            if (physComponent == null)
            {
                physComponent = new PhysicsComponent();
                Entity.Add(physComponent);
            }

            var element = Entity.Get<PhysicsComponent>().Elements.FirstOrDefault(x => x is CharacterElement);
            if (element != null)
            {
                characterElement = (CharacterElement)element;
            }
            else
            {
                Log.Error("Could not find a physics character element. (" + Entity.Name + ")");

                characterElement = new CharacterElement();
                characterElement.ColliderShapes.Add(new ColliderShapeAssetDesc { Shape = PhysicsColliderShape.New(new CapsuleColliderShapeDesc()) });
                physComponent.Elements.Add(characterElement);

                //this is currently the only way to make sure the physics element is actually created.
                SceneSystem.SceneInstance.Scene.Entities.Remove(Entity);
                SceneSystem.SceneInstance.Scene.Entities.Add(Entity);
            }

            character = characterElement.Character;

            if (CameraEntity == null)
            {
                for (int index = 0; index < Entity.Transform.Children.Count; index++)
                {
                    var child = Entity.Transform.Children[index];
                    var camera = child.Entity.Get<CameraComponent>();
                    if (camera != null)
                    {
                        CameraEntity = child.Entity;
                        break;
                    }
                }

                if (CameraEntity == null)
                {
                    Log.Error("Could not find a camera component. (" + Entity.Name + ")");
                }
            }

            if (CameraEntity != null)
            {
                baseCameraRotation = CameraEntity.Transform.Rotation;
            }
        }

        private float yaw, desiredYaw;
        private float pitch, desiredPitch;

        /// <summary>
        /// Gets or sets the rate at which orientation is adapted to a target value.
        /// </summary>
        /// <value>
        /// The adaptation rate.
        /// </value>
        public float RotationAdaptationSpeed { get; set; } = 5.0f;

        /// <summary>
        /// Gets or sets the rotation speed of the camera (in radian/screen units)
        /// </summary>
        public float RotationSpeed { get; set; } = 2.355f;

        public override void Update()
        {
            var rotationDelta = Input.MouseDelta;
            foreach (var gestureEvent in Input.GestureEvents)
            {
                switch (gestureEvent.Type)
                {
                    case GestureType.Drag:
                        {
                            var drag = (GestureEventDrag)gestureEvent;
                            rotationDelta = drag.DeltaTranslation;
                        }
                        break;

                    case GestureType.Flick:
                        break;

                    case GestureType.LongPress:
                        break;

                    case GestureType.Composite:
                        break;

                    case GestureType.Tap:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Compute translation speed according to framerate and modifiers
            var translationSpeed = Speed * (float)Game.UpdateTime.Elapsed.TotalSeconds;

            // Take shortest path
            var deltaPitch = desiredPitch - pitch;
            var deltaYaw = (desiredYaw - yaw) % MathUtil.TwoPi;
            if (deltaYaw < 0)
                deltaYaw += MathUtil.TwoPi;
            if (deltaYaw > MathUtil.Pi)
                deltaYaw -= MathUtil.TwoPi;
            desiredYaw = yaw + deltaYaw;

            // Perform orientation transition
            var rotationAdaptation = (float)Game.UpdateTime.Elapsed.TotalSeconds * RotationAdaptationSpeed;
            yaw = Math.Abs(deltaYaw) < rotationAdaptation ? desiredYaw : yaw + rotationAdaptation * Math.Sign(deltaYaw);
            pitch = Math.Abs(deltaPitch) < rotationAdaptation ? desiredPitch : pitch + rotationAdaptation * Math.Sign(deltaPitch);

            desiredYaw = yaw -= 1.333f * rotationDelta.X * RotationSpeed; // we want to rotate faster Horizontally and Vertically
            desiredPitch = pitch = MathUtil.Clamp(pitch - rotationDelta.Y * RotationSpeed, -MathUtil.PiOverTwo, MathUtil.PiOverTwo);

            if (CameraEntity != null)
            {
                //we need to pitch only the camera node
                CameraEntity.Transform.Rotation = baseCameraRotation * Quaternion.RotationYawPitchRoll(0, pitch, 0);
            }
            Entity.Transform.Rotation = Quaternion.RotationYawPitchRoll(yaw, 0, 0); //do not apply pitch to our controller

            var move = new Vector3();

            var forward = Vector3.Transform(ForwardVector, Entity.Transform.Rotation);
            var projectedForward = Vector3.Normalize(new Vector3(forward.X, 0, forward.Z));
            var up = Vector3.TransformNormal(UpVector, Matrix.RotationQuaternion(Entity.Transform.Rotation));
            var right = Vector3.Cross(forward, up);

            if (Input.IsKeyDown(Keys.A) || Input.IsKeyDown(Keys.Left))
            {
                move += -right;
            }
            if (Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right))
            {
                move += right;
            }
            if (Input.IsKeyDown(Keys.W) || Input.IsKeyDown(Keys.Up))
            {
                move += projectedForward;
            }
            if (Input.IsKeyDown(Keys.S) || Input.IsKeyDown(Keys.Down))
            {
                move += -projectedForward;
            }

            //            if (Input.PointerEvents.Any())
            //            {
            //                var last = Input.PointerEvents.Last();
            //                if (last != null)
            //                {
            //                    switch (last.State)
            //                    {
            //                        case PointerState.Down:
            //                            if (last.Position.X < 0.5)
            //                            {
            //                                pointerVector = -Vector3.UnitX;
            //                            }
            //                            else
            //                            {
            //                                pointerVector = Vector3.UnitX;
            //                            }
            //                            break;
            //                        case PointerState.Up:
            //                        case PointerState.Out:
            //                        case PointerState.Cancel:
            //                            pointerVector = Vector3.Zero;
            //                            break;
            //                    }
            //                }
            //            }
            //
            //            if (pointerVector != Vector3.Zero)
            //            {
            //                move = pointerVector;
            //            }

            move *= translationSpeed;

            //please note that the default character controller ignores rotation, in the case of complex collisions you would have more kinematic elements within your model anyway.
            character.Move(move);
        }
    }
}