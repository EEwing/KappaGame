using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace Kappa.gui.interaction {
    class Button : IRenderable, IDynamicObject {

        enum State {
            PRESSED,
            HOVER,
            RELEASED,
            NOT_PRESSED
        }
        
        //private Texture2D texture;
        private int releasedFrames = 0;

        public Rectangle Bounds { get; set; } = new Rectangle(0, 0, 0, 0);
        public Vector2 Location { get; set; }
        public int ReleasedLength { get; set; } = 5;
        public Color BackgroundColor { get; set; } = Color.White;
        public Color PressedColor { get; set; } = Color.DarkGray;
        public Color ReleasedColor { get; set; } = Color.LightGray;
        public Color HoverColor { get; set; } = Color.LightGray;
        public Texture2D Texture { set; get; }

        public Action ButtonPressed { get; set; }

        State state;

        public Button() {
            state = State.NOT_PRESSED;
        }

        public Button(Rectangle bounds_) : this() {
            Bounds = bounds_;
        }

        public Button(Vector2 loc) : this(new Rectangle(loc.ToPoint(), Point.Zero)) {}

        public void Update(float dt) {
            if(state == State.RELEASED) {
                if (--releasedFrames <= 0) {
                    state = State.NOT_PRESSED; // Could change from released to not pressed before it it drawn
                    releasedFrames = 0;
                }
            }
            
            if (Bounds.Left < Mouse.GetState().Position.X && Bounds.Right > Mouse.GetState().Position.X &&
                Bounds.Top < Mouse.GetState().Position.Y && Bounds.Bottom > Mouse.GetState().Position.Y) {
                if (state == State.NOT_PRESSED) {
                    state = State.HOVER;
                }
                if (Mouse.GetState().LeftButton == ButtonState.Pressed) {
                    state = State.PRESSED;
                    if(ButtonPressed != null)
                        ButtonPressed();
                } else {
                    if (state == State.PRESSED) {
                        state = State.RELEASED;
                        releasedFrames = ReleasedLength;
                    }
                }

            } else {
                if(state != State.NOT_PRESSED) {
                    state = State.NOT_PRESSED;
                    releasedFrames = 0;
                }
            }
        }

        public void LoadContent(ContentManager content) {
            throw new NotImplementedException();
        }

        public void Render(SpriteBatch spriteBatch) {
            switch (state) {
                case State.PRESSED:
                    spriteBatch.Draw(Texture, Bounds, PressedColor);
                    break;
                case State.HOVER:
                    spriteBatch.Draw(Texture, Bounds, HoverColor);
                    break;
                case State.RELEASED:
                    spriteBatch.Draw(Texture, Bounds, ReleasedColor);
                    break;
                case State.NOT_PRESSED:
                    spriteBatch.Draw(Texture, Bounds, BackgroundColor);
                    break;
            }
        }

        public bool HasTexture() {
            return Texture != null;
        }
    }
}
