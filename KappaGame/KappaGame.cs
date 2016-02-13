﻿using FarseerPhysics;
using Kappa.gui;
using Kappa.gui.scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Kappa {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class KappaGame : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private static KappaGame instance;
        public static KappaGame Instance { get { return instance; } }
        public float FPS { get; private set; }

        public KappaGame() {
            graphics = new GraphicsDeviceManager(this);
            //graphics.PreferredBackBufferWidth = 1920;
            //graphics.PreferredBackBufferHeight = 1080;
            //Window.IsBorderless = true;
            //graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
            instance = this;
            ConvertUnits.SetDisplayUnitToSimUnitRatio(32f);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            SceneController.Instance.UnloadContent();
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            float dt = gameTime.ElapsedGameTime.Milliseconds / 1000f;
            FPS = 1 / dt;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Quit();

            SceneController.Instance.Update(dt);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            SceneController.Instance.Render(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void Quit() {
            SceneController.Instance.Exit();
            Exit();
        }
    }
}
