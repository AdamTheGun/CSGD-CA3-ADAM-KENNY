#region File Description
//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using GameStateManagement;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class CreditsMenuScreen : MenuScreen
    {
        #region Fields


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public CreditsMenuScreen(SoundBank soundbank)
            : base("",soundbank)
        {
            MenuEntry back = new MenuEntry(string.Empty);

            // Hook up menu event handlers.
           
            back.Selected += backButtonSelected;
            
            // Add entries to the menu.
            MenuEntries.Add(back);
        }

        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ScreenManager.ScreenInCounter = 0;
            LoadingScreen.Load(ScreenManager, false, ControllingPlayer, new BackgroundScreen(), new MainMenuScreen(ScreenManager.SoundBank));
        }

        public override void Activate(bool instancePreserved)
        {
            base.Activate(instancePreserved);
        }

        public void backButtonSelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.ScreenInCounter = 0;
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreen(ScreenManager.SoundBank));
        }

        #endregion
    }
}
