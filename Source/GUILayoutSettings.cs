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
                windowPosition = GUILayout.Window(ConfigHandler.Instance.LayoutWindow, windowPosition, DrawWindow, "Catalog GUI Layout", GUILayout.ExpandHeight(true));
            }
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("Toolbar Position");

            if (GUILayout.Button("Bottom"))
            {
                ConfigHandler.Instance.ToolBarPreset = ToolBarPositions.HorizontalBottom;
                ConfigHandler.Instance.ToolBarDirection = ToolBarDirections.Right;
            }
            if (GUILayout.Button("Left"))
            {
                ConfigHandler.Instance.ToolBarPreset = ToolBarPositions.VerticalLeft;
                ConfigHandler.Instance.ToolBarDirection = ToolBarDirections.Down;
            }
            if (GUILayout.Button("Right"))
            {
                ConfigHandler.Instance.ToolBarPreset = ToolBarPositions.VerticalRight;
                ConfigHandler.Instance.ToolBarDirection = ToolBarDirections.Down;
            }
            GUILayout.Label("Tag Alignment");
            if (ConfigHandler.Instance.ToolBarPreset == ToolBarPositions.HorizontalTop || ConfigHandler.Instance.ToolBarPreset == ToolBarPositions.HorizontalBottom)
            {
                if (ConfigHandler.Instance.ToolBarDirection == ToolBarDirections.Right)
                {
                    if (GUILayout.Button("Left to Right"))
                    {
                        ConfigHandler.Instance.ToolBarDirection = ToolBarDirections.Left;
                    }
                }
                else
                {
                    if (GUILayout.Button("Right to Left"))
                    {
                        ConfigHandler.Instance.ToolBarDirection = ToolBarDirections.Right;
                    }
                }
            }
            else
            {
                if (ConfigHandler.Instance.ToolBarDirection == ToolBarDirections.Down)
                {
                    if (GUILayout.Button("Top to Bottom"))
                    {
                        ConfigHandler.Instance.ToolBarDirection = ToolBarDirections.Up;
                    }
                }
                else
                {
                    if (GUILayout.Button("Bottom to Top"))
                    {
                        ConfigHandler.Instance.ToolBarDirection = ToolBarDirections.Down;
                    }
                }
            }            
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label("Config Button Position");

            if (GUILayout.Button("Start of Toolbar"))
            {
                ConfigHandler.Instance.ConfigButtonPreset = ConfigButtonPositions.CompoundStart;

            }
            if (GUILayout.Button("End of Toolbar"))
            {
                ConfigHandler.Instance.ConfigButtonPreset = ConfigButtonPositions.CompoundEnd;
            }
            if (GUILayout.Button("Top Left"))
            {
                ConfigHandler.Instance.ConfigButtonPreset = ConfigButtonPositions.TopLeft;
            }
            if (GUILayout.Button("Bottom Left"))
            {
                ConfigHandler.Instance.ConfigButtonPreset = ConfigButtonPositions.BottomLeft;
            }
            if (GUILayout.Button("Bottom Right"))
            {
                ConfigHandler.Instance.ConfigButtonPreset = ConfigButtonPositions.BottomRight;
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            ConfigHandler.Instance.AutoHideToolBar = GUILayout.Toggle(ConfigHandler.Instance.AutoHideToolBar, "Autohide Toolbar");
            ConfigHandler.Instance.PageNumberOnToolbarEnd = GUILayout.Toggle(ConfigHandler.Instance.PageNumberOnToolbarEnd, "Page number on end");
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
            ConfigHandler.Instance.SaveConfig();
        }
    }
}
