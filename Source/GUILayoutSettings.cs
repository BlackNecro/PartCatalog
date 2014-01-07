using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP;
using UnityEngine;

namespace PartCatalog
{
    class GUILayoutSettings
    {
        #region Members
        #region Instance
        static readonly GUILayoutSettings instance = new GUILayoutSettings();
        internal static GUILayoutSettings Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion
        #region Base
        bool display = false;
        public bool IsOpen {
            get
            {
                return display;
            }
    }
        Rect windowPosition;
        #endregion
        #endregion

        #region Constructor
        private GUILayoutSettings()
        {
            
        }
        #endregion

        #region Drawing

        public void Draw()
        {
            if (display)
            {
                GUI.skin = HighLogic.Skin;
                windowPosition = GUILayout.Window(ConfigHandlerHandler.Instance.LayoutWindow, windowPosition, DrawWindow, "Catalog GUI Layout", GUILayout.ExpandHeight(true));
            }
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("Toolbar Position");
            /*  RIP 24/7/2013 DEATH BY 0.21
            if (GUILayout.Button("Top"))
            {
                ConfigHandler.Instance.ToolBarPreset = ToolBarPositions.HorizontalTop;
                ConfigHandler.Instance.ToolBarDirection = ToolBarDirections.Right;
            } */
            if (GUILayout.Button("Bottom"))
            {
                ConfigHandlerHandler.Instance.ToolBarPreset = ToolBarPositions.HorizontalBottom;
                ConfigHandlerHandler.Instance.ToolBarDirection = ToolBarDirections.Right;
            }
            if (GUILayout.Button("Left"))
            {
                ConfigHandlerHandler.Instance.ToolBarPreset = ToolBarPositions.VerticalLeft;
                ConfigHandlerHandler.Instance.ToolBarDirection = ToolBarDirections.Down;
            }
            if (GUILayout.Button("Right"))
            {
                ConfigHandlerHandler.Instance.ToolBarPreset = ToolBarPositions.VerticalRight;
                ConfigHandlerHandler.Instance.ToolBarDirection = ToolBarDirections.Down;
            }
            GUILayout.Label("Tag Alignment");
            if (ConfigHandlerHandler.Instance.ToolBarPreset == ToolBarPositions.HorizontalTop || ConfigHandlerHandler.Instance.ToolBarPreset == ToolBarPositions.HorizontalBottom)
            {
                if (ConfigHandlerHandler.Instance.ToolBarDirection == ToolBarDirections.Right)
                {
                    if (GUILayout.Button("Left to Right"))
                    {
                        ConfigHandlerHandler.Instance.ToolBarDirection = ToolBarDirections.Left;
                    }
                }
                else
                {
                    if (GUILayout.Button("Right to Left"))
                    {
                        ConfigHandlerHandler.Instance.ToolBarDirection = ToolBarDirections.Right;
                    }
                }
            }
            else
            {
                if (ConfigHandlerHandler.Instance.ToolBarDirection == ToolBarDirections.Down)
                {
                    if (GUILayout.Button("Top to Bottom"))
                    {
                        ConfigHandlerHandler.Instance.ToolBarDirection = ToolBarDirections.Up;
                    }
                }
                else
                {
                    if (GUILayout.Button("Bottom to Top"))
                    {
                        ConfigHandlerHandler.Instance.ToolBarDirection = ToolBarDirections.Down;
                    }
                }
            }            
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label("Config Button Position");
            if (GUILayout.Button("Start of Toolbar"))
            {
                ConfigHandlerHandler.Instance.ConfigButtonPreset = ConfigButtonPositions.CompoundStart;

            }
            if (GUILayout.Button("End of Toolbar"))
            {
                ConfigHandlerHandler.Instance.ConfigButtonPreset = ConfigButtonPositions.CompoundEnd;
            }
             /*  RIP 24/7/2013 DIED BY 0.21
            if (GUILayout.Button("Top Middle"))
            {
                ConfigHandler.Instance.ConfigButtonPreset = ConfigButtonPositions.TopMiddle;
            }
              */
            if (GUILayout.Button("Top Left"))
            {
                ConfigHandlerHandler.Instance.ConfigButtonPreset = ConfigButtonPositions.TopLeft;
            }
            if (GUILayout.Button("Bottom Left"))
            {
                ConfigHandlerHandler.Instance.ConfigButtonPreset = ConfigButtonPositions.BottomLeft;
            }
            if (GUILayout.Button("Bottom Right"))
            {
                ConfigHandlerHandler.Instance.ConfigButtonPreset = ConfigButtonPositions.BottomRight;
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            ConfigHandlerHandler.Instance.AutoHideToolBar = GUILayout.Toggle(ConfigHandlerHandler.Instance.AutoHideToolBar, "Autohide Toolbar");
            ConfigHandlerHandler.Instance.PageNumberOnToolbarEnd = GUILayout.Toggle(ConfigHandlerHandler.Instance.PageNumberOnToolbarEnd, "Page number on end");
            if (GUILayout.Button("Close"))
            {
                Close();
            }
            GUI.DragWindow(new Rect(0,0,Screen.width,Screen.height));
        }
        public void Open()
        {
            display = true;   
            windowPosition = new Rect(Screen.width * 0.4f,Screen.width * 0.2f,Screen.width * 0.2f, 0);
        }
        
        #endregion

        internal void Close()
        {
            display = false;
            ConfigHandlerHandler.Instance.SaveConfig();
        }
    }
}
