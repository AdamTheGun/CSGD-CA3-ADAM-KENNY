#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class StartScreen : MenuScreen
    {
        #region Initialization
        SoundBank sb;

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public StartScreen(SoundBank soundbank)
            : base("",soundbank)
        {
            // Create our menu entries.
            MenuEntry playGameMenuEntry = new MenuEntry("");
            sb = soundbank;
            // Hook up menu event handlers.
            playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            // Add entries to the menu.
            MenuEntries.Add(playGameMenuEntry);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new BackgroundScreen(), null);
            ScreenManager.AddScreen(new MainMenuScreen(sb), e.PlayerIndex);

            ScreenManager.ScreenInCounter = 0;
        }


        

        


        


        #endregion
    }
}
