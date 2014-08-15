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
        public const int EditorToolbarTopOffset = 30;
        public const int EditorVerticalMarginBottomLeft = 124;
        public const int EditorHorizontalMarginBottomLeft = 284;
        public const int EditorHorizontalMarginBottomRight = 115; 

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

        public static string IconFolderPath
        {
            get
            {
                return CatalogDataPath + "/Icons";
            }
        }

        public static string ConfigButtonName
        {
            get
            {
                return "ConfigButton";
            }
        }
        public static string DefaultIconName
        {
            get
            {
                return "TagBlank";
            }
        }

        public static string ErrorIconName
        {
            get
            {
                return "UnknownIcon";
            }
        }

        public static string SearchIconName
        {
            get
            {
                return "Search";
            }
        }
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
