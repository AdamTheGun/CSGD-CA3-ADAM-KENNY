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
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class OptionsMenuScreen : MenuScreen
    {
        #region Fields

        MenuEntry enableAudioMenuEntry;
        MenuEntry audioVolumeMenuEntry;
        MenuEntry soundEffectVolumeEntry;
        MenuEntry splitScreenMenuEntry;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenuScreen()
            : base("Options")
        {
            // Create our menu entries.
            enableAudioMenuEntry = new MenuEntry(string.Empty);
            audioVolumeMenuEntry = new MenuEntry(string.Empty);
            soundEffectVolumeEntry = new MenuEntry(string.Empty);
            splitScreenMenuEntry = new MenuEntry(string.Empty);

            
            MenuEntry back = new MenuEntry("Back");

            // Hook up menu event handlers.
            enableAudioMenuEntry.Selected += EnableAudioMenuEntrySelected;
            audioVolumeMenuEntry.Selected += AudioVolumeMenuEntrySelected;
            soundEffectVolumeEntry.Selected += SoundEffectMenuEntrySelected;
            splitScreenMenuEntry.Selected += SplitScreenMenuEntrySelected;
            back.Selected += OnCancel;
            
            // Add entries to the menu.
            MenuEntries.Add(enableAudioMenuEntry);
            MenuEntries.Add(audioVolumeMenuEntry);
            MenuEntries.Add(soundEffectVolumeEntry);
            MenuEntries.Add(splitScreenMenuEntry);
            MenuEntries.Add(back);
        }

        public override void Activate(bool instancePreserved)
        {
            SetMenuEntryText();

            base.Activate(instancePreserved);
        }

        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>
        void SetMenuEntryText()
        {
            if (ScreenManager.AudioEnabled == true)
                enableAudioMenuEntry.Text = "Audio: On";
            else
                enableAudioMenuEntry.Text = "Audio: Off";

            audioVolumeMenuEntry.Text = "Music Volume: " + (int)(ScreenManager.AudioVolume * 10);
            soundEffectVolumeEntry.Text = "SFX Volume: " + (int)(ScreenManager.SFXVolume * 10);

            if (ScreenManager.ScreenHorizontal == true)
                splitScreenMenuEntry.Text = "Split Screens Orientation: Horizontal";
            else
                splitScreenMenuEntry.Text = "Split Screens Orientation: Vertical";
        }


        #endregion

        #region Handle Input

        void EnableAudioMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AudioEnabled = !ScreenManager.AudioEnabled;

            SetMenuEntryText();
        }

        void AudioVolumeMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AudioVolume += 0.1f;

            if (ScreenManager.AudioVolume >= 1.1f)
                ScreenManager.AudioVolume = 0.0f;

            SetMenuEntryText();
        }

        void SoundEffectMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.SFXVolume += 0.1f;

            if (ScreenManager.SFXVolume >= 1.1f)
                ScreenManager.SFXVolume = 0.0f;

            SetMenuEntryText();
        }

        void SplitScreenMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.ScreenHorizontal = !ScreenManager.ScreenHorizontal;

            SetMenuEntryText();
        }

        #endregion
    }
}
