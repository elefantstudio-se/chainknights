using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;

using Common;
using FarseerGames.FarseerPhysics.Factories;


namespace chainknights
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        PhysicsSimulator physicsSimulator = new PhysicsSimulator(new Vector2(0, 4000));
        PhysicsSimulatorView psView;
        InputHelper input = new InputHelper();
        Camera2D camera;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Knight knight = new Knight();

        Contraption level = new Contraption();

        Body bodyCamera;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            knight.LoadContent(physicsSimulator, Content);
            level.Build("level.xml", physicsSimulator, Content);

            bodyCamera = BodyFactory.Instance.CreateBody(physicsSimulator, 1f, 1f);
            bodyCamera.IgnoreGravity = true;
            camera = new Camera2D(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
            camera.TrackingBody = bodyCamera;
            camera.Zoom = .25f;

            psView = new PhysicsSimulatorView(physicsSimulator);
            psView.LoadContent(GraphicsDevice, Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float dtime = gameTime.ElapsedGameTime.Milliseconds * 0.001f;

            knight.Update(dtime, input);

            physicsSimulator.Update(dtime);
            camera.Update(input);
            input.Update();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, camera.CameraMatrix);

            level.Draw(spriteBatch);
            knight.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
