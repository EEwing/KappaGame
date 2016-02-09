using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kappa.world {
    class Camera {
        private const float ZOOM_MAX = 1.5f;
        private const float ZOOM_MIN = .5f;

        private float _zoom;
        private Matrix _transform;
        private Vector2 _position;
        private float _rotation;
        private int _viewportWidth;
        private int _viewportHeight;
        private int _worldWidth;
        private int _worldHeight;
        private int _previousScroll;
        private float _zoomIncrement;

        public Camera(Viewport viewport, int worldWidth, int worldHeight, float initialZoom) {
            _zoom = initialZoom;
            _rotation = 0.0f;
            _position = Vector2.Zero;
            _viewportWidth = viewport.Width;
            _viewportHeight = viewport.Height;
            _worldWidth = worldWidth;
            _worldHeight = worldHeight;
            _previousScroll = 0;
            _zoomIncrement = 0.1f;
        }

        public void Update() {
            if(Mouse.GetState().ScrollWheelValue > _previousScroll)
                Zoom += _zoomIncrement;
            else if(Mouse.GetState().ScrollWheelValue < _previousScroll)
                Zoom -= _zoomIncrement;
            _previousScroll = Mouse.GetState().ScrollWheelValue;

            KeyboardState currentKeyboardState = Keyboard.GetState();
            if(currentKeyboardState.IsKeyDown(Keys.Left))
                _position = new Vector2(_position.X - 2, _position.Y);
            if(currentKeyboardState.IsKeyDown(Keys.Right))
                _position = new Vector2(_position.X + 2, _position.Y);
            if(currentKeyboardState.IsKeyDown(Keys.Up))
                _position = new Vector2(_position.X, _position.Y - 2);
            if(currentKeyboardState.IsKeyDown(Keys.Down))
                _position = new Vector2(_position.X, _position.Y + 2);
        }

        public float Zoom {
            get { return _zoom; }
            set { _zoom = value; _zoom = (_zoom < ZOOM_MIN) ? ZOOM_MIN : (_zoom > ZOOM_MAX) ? ZOOM_MAX : _zoom; }
        }

        public float Rotation {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public void Move(Vector2 amount) {
            _position += amount;
        }

        public Vector2 Position {
            get { return _position; }
            set {
                // Doesn't limit the camera's position to the world's end yet. YET.
                _position = value;
            }
        }

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