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

        private Rectangle bounds;
        private Texture2D texture;
        private int releasedFrames = 0;

        public int ReleasedLength { get; set; } = 5;
        public Color BackgroundColor { get; set; } = Color.White;
        public Color PressedColor { get; set; } = Color.DarkGray;
        public Color ReleasedColor { get; set; } = Color.LightGray;
        public Color HoverColor { get; set; } = Color.LightGray;

        public Action ButtonPressed { get; set; }

        State state;

        public Rectangle Bounds { get { return bounds; } }

        public Button(Rectangle bounds_, Texture2D texture_ = null) {
            bounds = bounds_;
            texture = texture_;
            state = State.NOT_PRESSED;
        }

        public void Update(float dt) {
            if(state == State.RELEASED) {
                if (--releasedFrames <= 0) {
                    state = State.NOT_PRESSED; // Could change from released to not pressed before it it drawn
                    releasedFrames = 0;
                }
            }
            
            if (bounds.Left < Mouse.GetState().Position.X && bounds.Right > Mouse.GetState().Position.X &&
                bounds.Top < Mouse.GetState().Position.Y && bounds.Bottom > Mouse.GetState().Position.Y) {
                if (state == State.NOT_PRESSED) {
                    state = State.HOVER;
                }
                if (Mouse.GetState().LeftButton == ButtonState.Pressed) {
                    state = State.PRESSED;
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
                    spriteBatch.Draw(texture, Bounds, PressedColor);
                    break;
                case State.HOVER:
                    spriteBatch.Draw(texture, Bounds, HoverColor);
                    break;
                case State.RELEASED:
                    spriteBatch.Draw(texture, Bounds, ReleasedColor);
                    break;
                case State.NOT_PRESSED:
                    spriteBatch.Draw(texture, Bounds, BackgroundColor);
                    break;
            }
        }
    }
}
