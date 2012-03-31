﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Vocaluxe.Base;
using Vocaluxe.Lib.Draw;
using Vocaluxe.Menu;

namespace Vocaluxe.Screens
{
    class CScreenNames : CMenu
    {
        // Version number for theme files. Increment it, if you've changed something on the theme files!
        const int ScreenVersion = 2;

        private CStatic chooseAvatarStatic;
        private int OldMouseX;
        private int OldMouseY;

        private const string SelectSlidePlayerNumber = "SelectSlidePlayerNumber";
        private const string NameSelection = "NameSelection";
        private const string ButtonBack = "ButtonBack";
        private const string ButtonStart = "ButtonStart";
        private const string TextWarning = "TextWarning";
        private const string StaticWarning = "StaticWarning";
        private readonly string[] StaticPlayer = new string[] {"StaticPlayer1", "StaticPlayer2", "StaticPlayer3", "StaticPlayer4", "StaticPlayer5", "StaticPlayer6"};
        private readonly string[] TextPlayer = new string[] { "TextPlayer1", "TextPlayer2", "TextPlayer3", "TextPlayer4", "TextPlayer5", "TextPlayer6" };

        private bool selectingMouseActive = false;
        private bool selectingKeyboardActive = false;
        private int selectingKeyboardPlayerNr = 0;
        private int SelectedPlayerNr;

        
        private int[] _PlayerNr;
        
        public CScreenNames()
        {
            Init();
        }

        protected override void Init()
        {
            base.Init();

            _ThemeName = "ScreenNames";
            _ScreenVersion = ScreenVersion;

            List<string> statics = new List<string>();
            foreach(string text in StaticPlayer){
                statics.Add(text);
            }
            statics.Add(StaticWarning);
            _ThemeStatics = statics.ToArray();

            List<string> texts = new List<string>();

            _ThemeSelectSlides = new string[] { SelectSlidePlayerNumber };

            texts.Clear();
            texts.Add(TextWarning);
            foreach(string text in TextPlayer){
                texts.Add(text);
            }
            _ThemeTexts = texts.ToArray();

            texts.Clear();
            texts.Add(ButtonBack);
            texts.Add(ButtonStart);

            _ThemeButtons = texts.ToArray();

            texts.Clear();
            texts.Add(NameSelection);
            _ThemeNameSelections = texts.ToArray();

            chooseAvatarStatic = new CStatic();
            chooseAvatarStatic.Visible = false;
        }

        public override void LoadTheme()
        {
            base.LoadTheme();
            
            _PlayerNr = new int[CSettings.MaxNumPlayer];
            for (int i = 0; i < _PlayerNr.Length; i++)
            {
                _PlayerNr[i] = i;
            }
        }

        public override bool HandleInput(KeyEvent KeyEvent)
        {
            //Check if selecting with keyboard is active
            if (selectingKeyboardActive)
            {
                //Handle left/right/up/down
                NameSelections[htNameSelections(NameSelection)].HandleInput(KeyEvent);
                switch (KeyEvent.Key)
                {
                    case Keys.Enter:
                        //Check, if a player is selected
                        if (NameSelections[htNameSelections(NameSelection)].Selection > -1)
                        {
                            SelectedPlayerNr = NameSelections[htNameSelections(NameSelection)].Selection;
                            //Update Game-infos with new player
                            CGame.Player[selectingKeyboardPlayerNr-1].Name = CProfiles.Profiles[SelectedPlayerNr].PlayerName;
                            CGame.Player[selectingKeyboardPlayerNr-1].Difficulty = CProfiles.Profiles[SelectedPlayerNr].Difficulty;
                            CGame.Player[selectingKeyboardPlayerNr-1].ProfileID = SelectedPlayerNr;
                            //Update texture and name
                            Statics[htStatics(StaticPlayer[selectingKeyboardPlayerNr-1])].Texture = NameSelections[htNameSelections(NameSelection)].TilePlayerAvatar(SelectedPlayerNr).Texture;
                            Texts[htTexts(TextPlayer[selectingKeyboardPlayerNr-1])].Text = CProfiles.Profiles[SelectedPlayerNr].PlayerName;
                        }
                        //Reset all values
                        selectingKeyboardPlayerNr = 0;
                        selectingKeyboardActive = false;
                        NameSelections[htNameSelections(NameSelection)].KeyboardSelection(false, -1);
                        break;

                    case Keys.Escape:
                        //Reset all values
                        selectingKeyboardPlayerNr = 0;
                        selectingKeyboardActive = false;
                        NameSelections[htNameSelections(NameSelection)].KeyboardSelection(false, -1);
                        break;
                }
            }
            //Normal Keyboard handling
            else
            {
                base.HandleInput(KeyEvent);
                bool processed = false;
                switch (KeyEvent.Key)
                {
                    case Keys.Escape:
                    case Keys.Back:
                        CGraphics.FadeTo(EScreens.ScreenSong);
                        break;

                    case Keys.Enter:

                        if (!processed && Buttons[htButtons(ButtonBack)].Selected)
                        {
                            processed = true;
                            CGraphics.FadeTo(EScreens.ScreenSong);
                        }

                        if (!processed && Buttons[htButtons(ButtonStart)].Selected)
                        {
                            processed = true;
                            StartSong();
                        }

                        break;

                    case Keys.D1:
                        selectingKeyboardPlayerNr = 1;
                        break;

                    case Keys.D2:
                        selectingKeyboardPlayerNr = 2;
                        break;

                    case Keys.D3:
                        selectingKeyboardPlayerNr = 3;
                        break;

                    case Keys.D4:
                        selectingKeyboardPlayerNr = 4;
                        break;

                    case Keys.D5:
                        selectingKeyboardPlayerNr = 5;
                        break;

                    case Keys.D6:
                        selectingKeyboardPlayerNr = 6;
                        break;
                }

                if (!processed)
                    UpdatePlayerNumber();

                if (selectingKeyboardPlayerNr > 0 && selectingKeyboardPlayerNr <= CConfig.NumPlayer)
                {
                    selectingKeyboardActive = true;
                    NameSelections[htNameSelections(NameSelection)].KeyboardSelection(true, selectingKeyboardPlayerNr);
                }
            }

            return true;
        }

        public override bool HandleMouse(MouseEvent MouseEvent)
        {
            base.HandleMouse(MouseEvent);

            //Check if LeftButton is hold and Select-Mode inactive
            if (MouseEvent.LBH && !selectingMouseActive)
            {
                //Save mouse-coords
                OldMouseX = MouseEvent.X;
                OldMouseY = MouseEvent.Y;
                //Check if mouse if over tile
                if (NameSelections[htNameSelections(NameSelection)].isOverTile(MouseEvent))
                {
                    //Get player-number of tile
                    SelectedPlayerNr = NameSelections[htNameSelections(NameSelection)].TilePlayerNr(MouseEvent);
                    if (SelectedPlayerNr != -1)
                    {
                        //Activate mouse-selecting
                        selectingMouseActive = true;

                        //Update of Drag/Drop-Texture
                        CStatic SelectedPlayer = NameSelections[htNameSelections(NameSelection)].TilePlayerAvatar(MouseEvent);
                        chooseAvatarStatic.Visible = true;
                        chooseAvatarStatic.Rect = SelectedPlayer.Rect;
                        chooseAvatarStatic.Rect.Z = CSettings.zNear - 5;
                        chooseAvatarStatic.Color = new SColorF(1, 1, 1, 1);
                        chooseAvatarStatic.Texture = SelectedPlayer.Texture;
                    }
                }
            }

            //Check if LeftButton is hold and Select-Mode active
            if (MouseEvent.LBH && selectingMouseActive)
            {
                //Update coords for Drag/Drop-Texture
                chooseAvatarStatic.Rect.X += (MouseEvent.X - OldMouseX);
                chooseAvatarStatic.Rect.Y += (MouseEvent.Y - OldMouseY);
                OldMouseX = MouseEvent.X;
                OldMouseY = MouseEvent.Y;
            }
            // LeftButton isn't hold anymore, but Selec-Mode is still active -> "Drop" of Avatar
            else if (selectingMouseActive)
            {
                //Check if really a player was selected
                if (SelectedPlayerNr != -1)
                {
                    //Foreach Drop-Area
                    for (int i = 0; i < StaticPlayer.Length; i++)
                    {
                        //Check first, if area is "active"
                        if (Statics[htStatics(StaticPlayer[i])].Visible == true)
                        {
                            //Check if Mouse is in area
                            if (CHelper.IsInBounds(Statics[htStatics(StaticPlayer[i])].Rect, MouseEvent))
                            {
                                //Update Game-infos with new player
                                CGame.Player[i].Name = CProfiles.Profiles[SelectedPlayerNr].PlayerName;
                                CGame.Player[i].Difficulty = CProfiles.Profiles[SelectedPlayerNr].Difficulty;
                                CGame.Player[i].ProfileID = SelectedPlayerNr;
                                //Update texture and name
                                Statics[htStatics(StaticPlayer[i])].Texture = chooseAvatarStatic.Texture;
                                Texts[htTexts(TextPlayer[i])].Text = CProfiles.Profiles[SelectedPlayerNr].PlayerName;
                            }
                        }
                    }
                    SelectedPlayerNr = -1;
                }
                //Reset variables
                selectingMouseActive = false;
                chooseAvatarStatic.Visible = false;
            }

            if (MouseEvent.LB && IsMouseOver(MouseEvent))
            {

            }

            if (MouseEvent.LB && IsMouseOver(MouseEvent))
            {
                bool processed = false;

                if (!processed && Buttons[htButtons(ButtonBack)].Selected)
                {
                    processed = true;
                    CGraphics.FadeTo(EScreens.ScreenSong);
                }

                if (!processed && Buttons[htButtons(ButtonStart)].Selected)
                {
                    processed = true;
                    StartSong();
                }

                if (!processed)
                    UpdatePlayerNumber();
            }

            if (MouseEvent.RB)
            {
                CGraphics.FadeTo(EScreens.ScreenSong);
            }

            //Check mouse-wheel for scrolling
            if (MouseEvent.Wheel != 0)
            {
                if (CHelper.IsInBounds(NameSelections[htNameSelections(NameSelection)].Rect, MouseEvent))
                {
                    int offset = NameSelections[htNameSelections(NameSelection)]._Offset + MouseEvent.Wheel;
                    NameSelections[htNameSelections(NameSelection)].UpdateList(offset);
                }
            }
            return true;
        }

        public override bool UpdateGame()
        {

            return true;
        }

        public override void OnShow()
        {
            base.OnShow();

            NameSelections[htNameSelections(NameSelection)].Init();

            UpdateSlides();
            UpdatePlayerNumber();
            CheckMics();
        }

        public override bool Draw()
        {
            base.Draw();

            if (chooseAvatarStatic.Visible)
            {
                chooseAvatarStatic.Draw();
            }

            return true;           
        }

        private void StartSong()
        {
            for (int i = 0; i < CGame.NumPlayer; i++)
            {
                if (CGame.Player[i].ProfileID < 0)
                {
                    CGame.Player[i].Name = "Player " + i.ToString();
                    CGame.Player[i].Difficulty = EGameDifficulty.TR_CONFIG_EASY;
                    CGame.Player[i].ProfileID = -1;
                }
                else
                {
                    //Just a work-around for not crashing the game
                    if (CGame.Player[i].Name == null)
                    {
                        CGame.Player[i].Name = "";
                    }
                }
            }

            CGraphics.FadeTo(EScreens.ScreenSing);
        }

        private void UpdateSlides()
        {
            SelectSlides[htSelectSlides(SelectSlidePlayerNumber)].Clear();
            for (int i = 1; i <= CSettings.MaxNumPlayer; i++)
            {
                SelectSlides[htSelectSlides(SelectSlidePlayerNumber)].AddValue(CLanguage.Translate("TR_SCREENNAMES_" + i + "PLAYER"));
            }
            SelectSlides[htSelectSlides(SelectSlidePlayerNumber)].Selection = CConfig.NumPlayer - 1;
        }

        private void UpdatePlayerNumber()
        {
            CConfig.NumPlayer = (int)SelectSlides[htSelectSlides(SelectSlidePlayerNumber)].Selection + 1;
            CGame.NumPlayer = (int)SelectSlides[htSelectSlides(SelectSlidePlayerNumber)].Selection + 1;
            for (int i = 1; i <= CSettings.MaxNumPlayer; i++)
            {
                if (i <= CGame.NumPlayer)
                {
                    Statics[htStatics("StaticPlayer" + i)].Visible = true;
                    Texts[htTexts("TextPlayer" + i)].Visible = true;
                }
                else
                {
                    Statics[htStatics("StaticPlayer" + i)].Visible = false;
                    Texts[htTexts("TextPlayer" + i)].Visible = false;
                }
            }
            CConfig.SaveConfig();
            CheckMics();
        }

        private void CheckMics()
        {
            List<int> _PlayerWithoutMicro = new List<int>();
            for (int player = 0; player < CConfig.NumPlayer; player++)
            {
                if (!CConfig.IsMicConfig(player + 1))
                {
                    _PlayerWithoutMicro.Add(player + 1);
                }
            }
            if (_PlayerWithoutMicro.Count > 0)
            {
                Statics[htStatics(StaticWarning)].Visible = true;
                Texts[htTexts(TextWarning)].Visible = true;

                if (_PlayerWithoutMicro.Count > 1)
                {
                    string PlayerNums = string.Empty;
                    for (int i = 0; i < _PlayerWithoutMicro.Count; i++)
                    {
                        if (_PlayerWithoutMicro.Count - 1 == i)
                        {
                            PlayerNums += _PlayerWithoutMicro[i].ToString();
                        }
                        else if (_PlayerWithoutMicro.Count - 2 == i)
                        {
                            PlayerNums += _PlayerWithoutMicro[i].ToString() + " " + CLanguage.Translate("TR_GENERAL_AND") + " ";
                        }
                        else
                        {
                            PlayerNums += _PlayerWithoutMicro[i].ToString() + ", ";
                        }
                    }

                    Texts[htTexts(TextWarning)].Text = CLanguage.Translate("TR_SCREENNAMES_WARNING_PL").Replace("%v", PlayerNums);
                }
                else
                {
                    Texts[htTexts(TextWarning)].Text = CLanguage.Translate("TR_SCREENNAMES_WARNING_SG").Replace("%v", _PlayerWithoutMicro[0].ToString());
                }
            }
            else
            {
                Statics[htStatics(StaticWarning)].Visible = false;
                Texts[htTexts(TextWarning)].Visible = false;
            }
        }
    }
}
