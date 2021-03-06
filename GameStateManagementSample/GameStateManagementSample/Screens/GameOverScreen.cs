#region File Description
//-----------------------------------------------------------------------------
// PauseMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using GameStateManagementSample;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// The pause menu comes up over the top of the game,
    /// giving the player options to resume or quit.
    /// </summary>
    class GameOverScreen : MenuScreen
    {
        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameOverScreen(SoundBank soundbank)
            : base("Game Over",soundbank)
        {
            //IsFirstGame = true;
            // Create our menu entries.
            MenuEntry restartGameMenuEntry = new MenuEntry("Restart Game");
            MenuEntry quitGameMenuEntry = new MenuEntry("Quit Game");
            
            // Hook up menu event handlers.
            restartGameMenuEntry.Selected += OnRestart;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(restartGameMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
           // ScreenManager.SoundBank.GetCue("UI Click").Play();
            const string message = "Are you sure you want to quit this game?";

            MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(message);

            confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmQuitMessageBox, ControllingPlayer);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to quit" message box. This uses the loading screen to
        /// transition from the game back to the main menu screen.
        /// </summary>
        void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            //ScreenManager.SoundBank.GetCue("UI Click").Play();
            ScreenManager.MainMenu.Stop(AudioStopOptions.Immediate);
            ScreenManager.MainMenu = ScreenManager.SoundBank.GetCue("MainMenu");
            ScreenManager.MainMenu.Play();
            ScreenManager.Winner = -1;
            ScreenManager.ScreenInCounter = 0;
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreen(ScreenManager.SoundBank));
        }

        void OnRestart(object sender, PlayerIndexEventArgs e)
        {
           // ScreenManager.SoundBank.GetCue("UI Click").Play();
            ScreenManager.Winner = -1;
            ScreenManager.MainMenu.Stop(AudioStopOptions.Immediate);
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
                                   new GameplayScreen());
        }

       


        #endregion
    }
}
