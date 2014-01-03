using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PartCatalog
{
    class GUIConstants
    {
        public const int EditorPartListWidth = 255;
        public const int EditorToolbarHeight = 28;
        public const int EditorResetButtonWidth = 50;
        public const int EditorButtonsWidth = 750;
        public const int EditorButtonsHeight = 40;
        public const int EditorToolbarTop = 1;
        public const int PageNumberWidth = 25;

        public readonly static Rect EditorScrollRegion = new Rect(0, 0, EditorPartListWidth, Screen.height);

        public static string CatalogDataPath
        {
            get
            {
                return KSPUtil.ApplicationRootPath + "GameData/PartCatalog/Plugins/PluginData/PartCatalog";
            }
        }
        public static string ModFolderPath
        {
            get
            {
                return KSPUtil.ApplicationRootPath + "GameData";
            }
        }
        public const string ConfigButtonName = "ConfigButton";
        public const string DefaultIconName = "TagDefault";
        public const string ErrorIconName = "UnknownIcon";
    }
    public enum ToolBarPositions
    {
        HorizontalTop,
        HorizontalBottom,
        VerticalLeft,
        VerticalRight
    }
    public enum ConfigButtonPositions
    {
        CompoundStart,
        CompoundEnd,        
        TopMiddle,
        TopLeft,
        BottomLeft,
        BottomRight
    }
    public enum ToolBarDirections
    {
        Up,
        Down,
        Left,
        Right
    }

}
