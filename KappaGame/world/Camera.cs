using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Kappa.entity;
using FarseerPhysics;
using System.Diagnostics;

namespace Kappa.world {
    class Camera {
        private const float ZOOM_MAX = 1.5f;
        private const float ZOOM_MIN = .5f;
        private const float ZOOM_SPEED = 0.075f;
        private const float MOVE_SPEED = 10f; // Higher is slower.

        private float _zoom;
        private Matrix _transform;
        private Vector2 _position;
        private float _rotation;
        private int _viewportWidth;
        private int _viewportHeight;
        private int _worldWidth;
        private int _worldHeight;
        private int _previousScroll;

        public Entity trackTarget;

        public Camera(Viewport viewport, int worldWidth, int worldHeight, float initialZoom, Entity player) {
            _zoom = initialZoom;
            _rotation = 0.0f;
            _position = Vector2.Zero;
            _viewportWidth = viewport.Width;
            _viewportHeight = viewport.Height;
            _worldWidth = worldWidth;
            _worldHeight = worldHeight;
            _previousScroll = 0;
            trackTarget = player;
        }

        public void Update() {
            // Zoom with the mouse wheel.
            if(Mouse.GetState().ScrollWheelValue > _previousScroll)
                Zoom += ZOOM_SPEED;
            else if(Mouse.GetState().ScrollWheelValue < _previousScroll)
                Zoom -= ZOOM_SPEED;

            _previousScroll = Mouse.GetState().ScrollWheelValue;

            // Camera movement.
            if(trackTarget != null) {
                // Try to track the said target.
                Position = new Vector2(Position.X - ((Position.X - ConvertUnits.ToDisplayUnits(trackTarget.body.Position).X) / MOVE_SPEED), Position.Y - ((Position.Y - ConvertUnits.ToDisplayUnits(trackTarget.body.Position).Y) / MOVE_SPEED));
            } else {
                // If you're not tracking anything, allow moving the camera with the arrow keys (Probably never).
                KeyboardState currentKeyboardState = Keyboard.GetState();
                if(currentKeyboardState.IsKeyDown(Keys.Left))
                    Position = new Vector2(Position.X - MOVE_SPEED, Position.Y);
                if(currentKeyboardState.IsKeyDown(Keys.Right))
                    Position = new Vector2(Position.X + MOVE_SPEED, Position.Y);
                if(currentKeyboardState.IsKeyDown(Keys.Up))
                    Position = new Vector2(Position.X, Position.Y - MOVE_SPEED);
                if(currentKeyboardState.IsKeyDown(Keys.Down))
                    Position = new Vector2(Position.X, Position.Y + MOVE_SPEED);
            }
        }

        // Setter limits the zoom to ZOOM_MIN and ZOOM_MAX.
        public float Zoom {
            get { return _zoom; }
            set { _zoom = value; _zoom = (_zoom < ZOOM_MIN) ? ZOOM_MIN : (_zoom > ZOOM_MAX) ? ZOOM_MAX : _zoom; }
        }

        public float Rotation {
            get { return _rotation; }
            set { _rotation = value; }
        }

        // Setter limits the position to the world using worldWidth and worldHeight.
        public Vector2 Position {
            get { return _position; }
            set { _position = new Vector2((value.X < 0) ? 0 : (value.X > _worldWidth) ? _worldWidth : value.X, (value.Y < 0) ? 0 : (value.Y > _worldHeight) ? _worldHeight : value.Y); }
        }

        // Returns the matrix according to all the transforms, use this as the last param of the spritebatch.Begin() call.
        public Matrix GetMatrix() {
            _transform =
                Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                Matrix.CreateTranslation(new Vector3(_viewportWidth * 0.5f, _viewportHeight * 0.5f, 0));
            return _transform;
        }
    }
}