#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    class BackgroundScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        Texture2D background;
        Texture2D backgroundTexture;
        Texture2D controlsBackground;
        Texture2D creditsBackground;
        Texture2D Ship1Background;
        Texture2D Ship2Background;
        Texture2D StartBackground;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public BackgroundScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");

                background = content.Load<Texture2D>("MainMenuScreen");
                backgroundTexture = background;
                Ship1Background = content.Load<Texture2D>("Ship1Screen");
                Ship2Background = content.Load<Texture2D>("Ship2Screen");
                creditsBackground = content.Load<Texture2D>("CREDITS");
                controlsBackground = content.Load<Texture2D>("Controls");
                StartBackground = content.Load<Texture2D>("StartScreen");
            }
        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void Unload()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {

            if (ScreenManager.ScreenInCounter == -1)
            {
                backgroundTexture = StartBackground;
            }

            if (ScreenManager.ScreenInCounter == 3)
            {
                backgroundTexture = controlsBackground;
            }
            else if (ScreenManager.ScreenInCounter == 4)
            {
                backgroundTexture = creditsBackground;
            }
            else if (ScreenManager.ScreenInCounter == 1) 
            {
                
                if (ScreenManager.CurrentShipChoosing == 1)
                {
                    if (ScreenManager.shipChosenbool1 == true)
                    {
                        backgroundTexture = Ship1Background;
                    }
                    else
                    {
                        backgroundTexture = Ship2Background;
                    }
                }
                else if (ScreenManager.CurrentShipChoosing == 2)
                {
                    if (ScreenManager.shipChosenbool2 == true)
                    {
                        backgroundTexture = Ship1Background;
                    }
                    else
                    {
                        backgroundTexture = Ship2Background;
                    }
                }
            }
            
            else if (ScreenManager.ScreenInCounter == 0)
            {
                backgroundTexture = background;
            }
            base.Update(gameTime, otherScreenHasFocus, false);
        }


        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            spriteBatch.Begin();

            spriteBatch.Draw(backgroundTexture, fullscreen,
                             new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

            spriteBatch.End();
        }


        #endregion
    }
}
