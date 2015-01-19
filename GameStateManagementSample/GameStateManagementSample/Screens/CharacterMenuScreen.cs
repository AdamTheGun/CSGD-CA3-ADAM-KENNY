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
    class CharacterMenuScreen : MenuScreen
    {
        #region Fields

        MenuEntry shipMenuEntry;
        MenuEntry enterMenuEntry;
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public CharacterMenuScreen(): base("Choose Your Ship")
        {
            // Create our menu entries.
            shipMenuEntry = new MenuEntry(string.Empty);
            enterMenuEntry = new MenuEntry(string.Empty);
            MenuEntry back = new MenuEntry("Back");

            // Hook up menu event handlers.
            shipMenuEntry.Selected += shipMenuEntrySelected;
            enterMenuEntry.Selected += enterMenuEntrySelected;
            back.Selected += OnCancel;
            
            // Add entries to the menu.
            MenuEntries.Add(shipMenuEntry);
            MenuEntries.Add(enterMenuEntry);
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
            if (ScreenManager.CurrentShipChoosing == 1)
            {
                if (ScreenManager.shipChosenbool1 == false)
                    shipMenuEntry.Text = "Blue Seal";
                else
                    shipMenuEntry.Text = "Red Dragon";
            }
            else if (ScreenManager.CurrentShipChoosing == 2)
            {
                if (ScreenManager.shipChosenbool2 == false)
                    shipMenuEntry.Text = "Blue Seal";
                else
                    shipMenuEntry.Text = "Red Dragon";
            }
            if (ScreenManager.CurrentShipChoosing == 1)
            {
                enterMenuEntry.Text = "Player 1 : Go";
            }
            else if (ScreenManager.CurrentShipChoosing == 2)
            {
                enterMenuEntry.Text = "Player 2 : Go";
            }
        }

        #endregion

        #region Handle Input

        void shipMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            if (ScreenManager.CurrentShipChoosing == 1)
            {
                ScreenManager.shipChosenbool1 = !ScreenManager.shipChosenbool1;
            }
            else if (ScreenManager.CurrentShipChoosing == 2)
            {
                ScreenManager.shipChosenbool2 = !ScreenManager.shipChosenbool2;
            }
            SetMenuEntryText();
            //ScreenManager.AddScreen(new CharacterMenuScreen(), PlayerIndex.One);
        }
        void enterMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            if (ScreenManager.CurrentShipChoosing == 1) 
            {
                LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                          new CharacterMenuScreen());
                
                ScreenManager.CurrentShipChoosing = 2;
            }
            else if (ScreenManager.CurrentShipChoosing == 2)
            {
                LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
                                   new GameplayScreen());
                
                ScreenManager.CurrentShipChoosing = 1;

                ScreenManager.MainMenu.Stop(AudioStopOptions.Immediate);
            }
            SetMenuEntryText();
        }

        #endregion
    }
}
