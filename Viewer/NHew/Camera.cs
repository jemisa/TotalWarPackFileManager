using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewer.NHew
{

    public class ArcBallCamera
    {

        public ArcBallCamera(float aspectRation, Vector3 lookAt)
            : this(aspectRation, MathHelper.PiOver4, lookAt, Vector3.Up, 0.1f, float.MaxValue) { }

        public ArcBallCamera(float aspectRatio, float fieldOfView, Vector3 lookAt, Vector3 up, float nearPlane, float farPlane)
        {
            this.aspectRatio = aspectRatio;
            this.fieldOfView = fieldOfView;
            this.lookAt = lookAt;
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;
        }

        /// <summary>
        /// Recreates our view matrix, then signals that the view matrix
        /// is clean.
        /// </summary>
        private void ReCreateViewMatrix()
        {
            //Calculate the relative position of the camera                        
            position = Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(yaw, pitch, 0));
            //Convert the relative position to the absolute position
            position *= zoom;
            position += lookAt;

            //Calculate a new viewmatrix
            viewMatrix = Matrix.CreateLookAt(position, lookAt, Vector3.Up);
            viewMatrixDirty = false;
        }

        /// <summary>
        /// Recreates our projection matrix, then signals that the projection
        /// matrix is clean.
        /// </summary>
        private void ReCreateProjectionMatrix()
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, AspectRatio, nearPlane, farPlane);
            projectionMatrixDirty = false;
        }

        #region HelperMethods

        /// <summary>
        /// Moves the camera and lookAt at to the right,
        /// as seen from the camera, while keeping the same height
        /// </summary>        
        public void MoveCameraRight(float amount)
        {
            Vector3 right = Vector3.Normalize(LookAt - Position); //calculate forward
            right = Vector3.Cross(right, Vector3.Up); //calculate the real right
            right.Y = 0;
            right.Normalize();
            LookAt += right * amount;
        }

        public void MoveCameraUp(float amount)
        {
            lookAt.Y += amount;
            viewMatrixDirty = true;
        }

        /// <summary>
        /// Moves the camera and lookAt forward,
        /// as seen from the camera, while keeping the same height
        /// </summary>        
        public void MoveCameraForward(float amount)
        {
            Vector3 forward = Vector3.Normalize(LookAt - Position);
            forward.Y = 0;
            forward.Normalize();
            LookAt += forward * amount;
        }

        #endregion

        #region FieldsAndProperties
        //We don't need an update method because the camera only needs updating
        //when we change one of it's parameters.
        //We keep track if one of our matrices is dirty
        //and reacalculate that matrix when it is accesed.
        private bool viewMatrixDirty = true;
        private bool projectionMatrixDirty = true;

        public float MinPitch = -MathHelper.PiOver2 + 0.3f;
        public float MaxPitch = MathHelper.PiOver2 - 0.3f;
        private float pitch;
        public float Pitch
        {
            get { return pitch; }
            set
            {
                viewMatrixDirty = true;
                pitch = MathHelper.Clamp(value, MinPitch, MaxPitch);
            }
        }

        private float yaw;
        public float Yaw
        {
            get { return yaw; }
            set
            {
                viewMatrixDirty = true;
                yaw = value;
            }
        }

        private float fieldOfView;
        public float FieldOfView
        {
            get { return fieldOfView; }
            set
            {
                projectionMatrixDirty = true;
                fieldOfView = value;
            }
        }

        private float aspectRatio;
        public float AspectRatio
        {
            get { return aspectRatio; }
            set
            {
                projectionMatrixDirty = true;
                aspectRatio = value;
            }
        }

        private float nearPlane;
        public float NearPlane
        {
            get { return nearPlane; }
            set
            {
                projectionMatrixDirty = true;
                nearPlane = value;
            }
        }

        private float farPlane;
        public float FarPlane
        {
            get { return farPlane; }
            set
            {
                projectionMatrixDirty = true;
                farPlane = value;
            }
        }

        public float MinZoom = 1;
        public float MaxZoom = float.MaxValue;
        private float zoom = 1;
        public float Zoom
        {
            get { return zoom; }
            set
            {
                viewMatrixDirty = true;
                zoom = MathHelper.Clamp(value, MinZoom, MaxZoom);
            }
        }


        private Vector3 position;
        public Vector3 Position
        {
            get
            {
                if (viewMatrixDirty)
                {
                    ReCreateViewMatrix();
                }
                return position;
            }
        }

        private Vector3 lookAt;
        public Vector3 LookAt
        {
            get { return lookAt; }
            set
            {
                viewMatrixDirty = true;
                lookAt = value;
            }
        }
        #endregion

        #region ICamera Members        
        public Matrix ViewProjectionMatrix
        {
            get { return ViewMatrix * ProjectionMatrix; }
        }

        private Matrix viewMatrix;
        public Matrix ViewMatrix
        {
            get
            {
                if (viewMatrixDirty)
                {
                    ReCreateViewMatrix();
                }
                return viewMatrix;
            }
        }

        private Matrix projectionMatrix;
        public Matrix ProjectionMatrix
        {
            get
            {
                if (projectionMatrixDirty)
                {
                    ReCreateProjectionMatrix();
                }
                return projectionMatrix;
            }
        }
        #endregion
    }

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Camera
    {
        public Matrix view { get; protected set; }
        public Matrix projection { get; protected set; }

        public Vector3 cameraPosition { get; protected set; }
        Vector3 cameraDirection;
        Vector3 cameraUp;

        //defines speed of camera movement
        float speed = 0.5F;

        MouseState prevMouseState;

        WpfMouse _mouse;
        WpfKeyboard _keyboard;

        public Camera(WpfMouse mouse, WpfKeyboard keyboard, Vector3 pos, Vector3 target, Vector3 up)
        {
            _keyboard = keyboard;
            _mouse = mouse;

            // TODO: Construct any child components here

            // Build camera view matrix
            cameraPosition = pos;
            cameraDirection = target - pos;
            cameraDirection.Normalize();
            cameraUp = up;
            CreateLookAt();

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 800/600, 1, 100);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize()
        {
            // TODO: Add your initialization code here

            // Set mouse position and do initial get state
            //_mouse.SetPosition(800 / 2, 600 / 2);

            prevMouseState = _mouse.GetState();

         
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            // TODO: Add your update code here


            // Move forward and backward

            var speedM = 0.5f;
            float mouseDiffXCopy = 0;
            float mouseDiffYCopy = 0;
            if (_mouse.GetState().RightButton == ButtonState.Pressed)
            {

                mouseDiffXCopy = (float)(_mouse.GetState().X - prevMouseState.X) * speedM;
                mouseDiffYCopy = (float)(_mouse.GetState().Y - prevMouseState.Y) * speedM;
            }

            if (_keyboard.GetState().IsKeyDown(Keys.W))
                cameraPosition += cameraDirection * speed;
            if (_keyboard.GetState().IsKeyDown(Keys.S))
                cameraPosition -= cameraDirection * speed;
            
            if (_keyboard.GetState().IsKeyDown(Keys.A))
                cameraPosition += Vector3.Cross(cameraUp, cameraDirection) * speed;
            if (_keyboard.GetState().IsKeyDown(Keys.D))
                cameraPosition -= Vector3.Cross(cameraUp, cameraDirection) * speed;

            var mouseDiffX = 0;//_mouse.GetState().X - prevMouseState.X);
            var mouseDiffY = 0;//(_mouse.GetState().Y - prevMouseState.Y);


            //var speedM = 0.05f;
            //var mouseDiffXCopy = (float) (_mouse.GetState().X - prevMouseState.X)*speedM;
            //var mouseDiffYCopy = (float) (_mouse.GetState().Y - prevMouseState.Y)* speedM;

            Console.WriteLine($"{mouseDiffXCopy }, {mouseDiffYCopy}");

            // Rotation in the world
            cameraDirection = Vector3.Transform(cameraDirection,
                Matrix.CreateFromAxisAngle(cameraUp, (-MathHelper.PiOver4 / 150) * mouseDiffXCopy));

            cameraDirection = Vector3.Transform(cameraDirection, 
                Matrix.CreateFromAxisAngle(Vector3.Cross(cameraUp, cameraDirection), (MathHelper.PiOver4 / 100) * mouseDiffYCopy));

            cameraUp = Vector3.Transform(cameraUp, 
                Matrix.CreateFromAxisAngle(Vector3.Cross(cameraUp, cameraDirection), (MathHelper.PiOver4 / 100) * mouseDiffYCopy));

            // Reset prevMouseState
            prevMouseState = _mouse.GetState();

            CreateLookAt();

       
        }

        private void CreateLookAt()
        {
            view = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraDirection, cameraUp);
        }

    }
}
