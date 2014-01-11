using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PartCatalog
{
    class GUIEditorControls
    {
        static readonly GUIEditorControls instance = new GUIEditorControls();

        public static GUIEditorControls Instance
        {
            get
            {
                return instance;
            }
        }
        private GUIEditorControls()
        {

            OverlayStyle = new GUIStyle(HighLogic.Skin.label);
            OverlayStyle.alignment = TextAnchor.MiddleCenter;
            OverlayStyle.font = HighLogic.Skin.button.font;
            OverlayStyle.fontSize = 17;
            OverlayStyle.fontStyle = FontStyle.Bold;
            OverlayStyle.normal.textColor = Color.black;
            OverlayStyleEnabled = new GUIStyle(OverlayStyle);
            OverlayStyleEnabled.normal.textColor = HighLogic.Skin.label.normal.textColor;


            LabelStyle = new GUIStyle(HighLogic.Skin.label);
            LabelStyle.alignment = TextAnchor.LowerLeft;
            LabelStyle.fontSize = 10;
            LabelStyle.normal.textColor = Color.white;
            LabelStyleEnabled = new GUIStyle(LabelStyle);
            LabelStyleEnabled.normal.textColor = HighLogic.Skin.label.normal.textColor;

            ButtonStyle = new GUIStyle(HighLogic.Skin.button);
            ButtonStyleEnabled = new GUIStyle(HighLogic.Skin.button);
            ButtonStyleEnabled.normal.textColor = HighLogic.Skin.label.normal.textColor;

            iconStyle = new GUIStyle();
            iconStyle.alignment = TextAnchor.MiddleCenter;
            iconStyle.margin.top = 6;
        }

        private class MouseOverStackEntry
        {
            public PartTag Tag;
            private Vector2 position;

            public Vector2 Position
            {
                get { return GUIUtility.ScreenToGUIPoint(position); }
            }
            public Rect WindowPos = new Rect(0, 0, 40, 40);
            public MouseOverStackEntry(PartTag tag, Vector2 pos) { Tag = tag; position = GUIUtility.GUIToScreenPoint(pos); }
        }

        GUIStyle LabelStyle;
        GUIStyle LabelStyleEnabled;
        GUIStyle OverlayStyle;
        GUIStyle OverlayStyleEnabled;
        GUIStyle ButtonStyle;
        GUIStyle ButtonStyleEnabled;
        GUIStyle iconStyle;

        public bool configPressed = false;
        private bool MouseOverClear = true;
        private int MouseOverTimer = 0;
        float shiftAmount = 0;
        bool nextPageAvailable;
        LinkedList<PartTag> currentPageTags = new LinkedList<PartTag>();
        List<MouseOverStackEntry> MouseOverStack = new List<MouseOverStackEntry>();

        #region Drawing

        public void Draw()
        {
            GUI.skin = HighLogic.Skin;
            MouseOverClear = true;
            DrawToolBar();
            if (MouseOverTimer == 0)
            {
                MouseOverStack.Clear();
            }
        }


        public int MaxTagCount
        {
            get
            {
                if (ConfigHandler.Instance.ToolBarDirection == ToolBarDirections.Right || ConfigHandler.Instance.ToolBarDirection == ToolBarDirections.Left)
                {
                    return (int)(GetToolbarRectNoConfig().width / (ConfigHandler.Instance.ButtonSize.x + 1));
                }
                else
                {
                    return (int)(GetToolbarRectNoConfig().height / (ConfigHandler.Instance.ButtonSize.y + 1));
                }
            }
        }


        private void DrawToolBar()
        {
            if (GUILayoutSettings.Instance.IsOpen)
            {
                Rect toolBarRect = GetToolbarRectRaw();
                GUI.Box(toolBarRect, "Toolbar", HighLogic.Skin.box);
            }

            if (!ConfigHandler.Instance.AutoHideToolBar || shiftAmount != 0)
            {
                Rect toolbarRect = GetToolbarRectRaw();
                GUI.BeginGroup(toolbarRect);
                Rect shiftRect = new Rect(0, 0, toolbarRect.width, toolbarRect.height);
                if (ConfigHandler.Instance.AutoHideToolBar)
                {
                    switch (ConfigHandler.Instance.ToolBarPreset)
                    {
                        case ToolBarPositions.HorizontalTop:
                            shiftRect.y = shiftAmount - toolbarRect.height;
                            break;
                        case ToolBarPositions.HorizontalBottom:
                            shiftRect.y = toolbarRect.height - shiftAmount;
                            break;
                        case ToolBarPositions.VerticalLeft:
                            shiftRect.x = shiftAmount - toolbarRect.width;
                            break;
                        case ToolBarPositions.VerticalRight:
                            shiftRect.x = toolbarRect.width - shiftAmount;
                            break;
                    }
                }
                GUI.BeginGroup(shiftRect);
                if (ConfigHandler.Instance.ConfigButtonPreset == ConfigButtonPositions.CompoundStart || ConfigHandler.Instance.ConfigButtonPreset == ConfigButtonPositions.CompoundEnd)
                {
                    DrawConfigButton();
                }

                Rect configOffsetRect = GetToolbarRectNoConfig();
                GUI.BeginGroup(configOffsetRect);
                if (ConfigHandler.Instance.AutoHideToolBar)
                {
                    GUI.Box(configOffsetRect, "");
                }

                TextAnchor old = GUI.skin.label.alignment;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(GetPageNumberPos(), (ConfigHandler.Instance.DisplayedPage + 1).ToString());
                GUI.skin.label.alignment = old;
                Rect pageOffsetRect = GetToolbarRectNoPageNumber();
                GUI.BeginGroup(pageOffsetRect);
                Rect innerRect = new Rect(0, 0, pageOffsetRect.width, pageOffsetRect.height);

                DrawToolBarTags(innerRect);
                GUI.EndGroup();
                GUI.EndGroup();
                GUI.EndGroup();
                GUI.EndGroup();
            }
            if (!(ConfigHandler.Instance.ConfigButtonPreset == ConfigButtonPositions.CompoundStart || ConfigHandler.Instance.ConfigButtonPreset == ConfigButtonPositions.CompoundEnd))
            {
                DrawConfigButton();
            }
            DrawMouseOverTags();

        }

        private void DrawMouseOverTags()
        {
            if (GUILayoutSettings.Instance.IsOpen)
            {
                return;
            }
            for (int i = 0; i < MouseOverStack.Count; i++)
            {
                MouseOverStackEntry Entry = MouseOverStack[i];
                if (i != 0 && Entry.Tag.ChildTags.Count == 0)
                {
                    break;
                }
                Entry.WindowPos = GUILayout.Window(ConfigHandler.Instance.MouseOverWindow + i, Entry.WindowPos, DrawMouseOverWindow, Entry.Tag.Name, GUILayout.MinHeight(200), GUILayout.MinWidth(200), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                if (i > 0)
                {
                    MouseOverStackEntry LastEntry = MouseOverStack[i - 1];
                    if (ConfigHandler.Instance.ToolBarPreset == ToolBarPositions.VerticalRight)
                    {
                        Entry.WindowPos.x = Entry.Position.x - Entry.WindowPos.width;
                    }
                    else
                    {
                        Entry.WindowPos.x = Entry.Position.x + LastEntry.WindowPos.width;
                    }
                    Entry.WindowPos.y = Entry.Position.y - Entry.WindowPos.height * 0.5f;
                }
                else
                {
                    switch (ConfigHandler.Instance.ToolBarPreset)
                    {
                        case ToolBarPositions.HorizontalTop:
                            Entry.WindowPos.x = Entry.Position.x - Entry.WindowPos.width * 0.5f + ConfigHandler.Instance.ButtonSize.x * 0.5f;
                            Entry.WindowPos.y = Entry.Position.y + ConfigHandler.Instance.ButtonSize.y;
                            break;
                        case ToolBarPositions.HorizontalBottom:
                            Entry.WindowPos.x = Entry.Position.x - Entry.WindowPos.width * 0.5f + ConfigHandler.Instance.ButtonSize.x * 0.5f;
                            Entry.WindowPos.y = Entry.Position.y - Entry.WindowPos.height;
                            break;
                        case ToolBarPositions.VerticalLeft:
                            Entry.WindowPos.x = Entry.Position.x + ConfigHandler.Instance.ButtonSize.x;
                            Entry.WindowPos.y = Entry.Position.y - Entry.WindowPos.height * 0.5f;
                            break;
                        case ToolBarPositions.VerticalRight:
                            Entry.WindowPos.x = Entry.Position.x - Entry.WindowPos.width;
                            Entry.WindowPos.y = Entry.Position.y - Entry.WindowPos.height * 0.5f;
                            break;
                        default:
                            break;
                    }
                }
                if (Entry.WindowPos.x < GUIConstants.EditorPartListWidth)
                {
                    Entry.WindowPos.x = GUIConstants.EditorPartListWidth;
                }
                if (Entry.WindowPos.xMax > Screen.width)
                {
                    Entry.WindowPos.x -= Screen.width - Entry.WindowPos.xMax;
                }

                if (Entry.WindowPos.y < GUIConstants.EditorToolbarHeight)
                {
                    Entry.WindowPos.y = GUIConstants.EditorToolbarHeight;
                }
                if (Entry.WindowPos.yMax > Screen.height - GUIConstants.EditorToolbarHeight)
                {
                    Entry.WindowPos.y += (Screen.height - GUIConstants.EditorToolbarHeight) - Entry.WindowPos.yMax;
                }


                Entry.WindowPos = GUILayout.Window(ConfigHandler.Instance.MouseOverWindow + i, Entry.WindowPos, DrawMouseOverWindow, Entry.Tag.Name);

                if (Entry.WindowPos.Contains(Event.current.mousePosition))
                {
                    MouseOverClear = false;
                }
            }
        }

        private void DrawMouseOverWindow(int id)
        {
            int index = id - ConfigHandler.Instance.MouseOverWindow;
            if (index < MouseOverStack.Count)
            {
                MouseOverStackEntry Entry = MouseOverStack[index];
                if (index == 0 && Entry.Tag.ChildTags.Count == 0)
                {
                    GUILayout.Space(15);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(Entry.Tag.Name.Length * 10);
                    GUILayout.EndHorizontal();
                    //GUILayout.Label(Entry.Tag.Name, GUILayout.ExpandWidth(true));
                }
                GUILayout.BeginVertical(GUILayout.MinWidth(Entry.Tag.Name.Length * 9));
                foreach (PartTag subTag in Entry.Tag.ChildTags)
                {
                    if (ConfigHandler.Instance.HideUnresearchedTags && !subTag.Researched)
                    {
                       continue;
                    }
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    bool pushed = false;
                    if (subTag.IconName != "")
                    {
                        pushed |= GUILayout.Button(ResourceProxy.Instance.GetIconTexture(subTag.IconName, subTag.Enabled), iconStyle, GUILayout.Width(ConfigHandler.Instance.ButtonSize.x), GUILayout.Height(ConfigHandler.Instance.ButtonSize.y));
                    }
                    pushed |= GUILayout.Button(subTag.Name, subTag.Enabled ? ButtonStyleEnabled : ButtonStyle,GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();



                    if (pushed)
                    {
                        PartFilterManager.Instance.ToggleFilter(subTag);
                    }
                    //GUILayout.Box(ResourceProxy.Instance.GetTagIcon(subTag)); 
                    if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    {
                        while (index + 1 < MouseOverStack.Count)
                        {
                            MouseOverStack.RemoveAt(index + 1);
                        }

                        MouseOverStack.Insert(index + 1, new MouseOverStackEntry(subTag, new Vector2(0, GUILayoutUtility.GetLastRect().y + GUILayoutUtility.GetLastRect().height * 0.5f)));
                        MouseOverClear = false;
                    }
                }
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.Space(1);
            }
        }

        public void UpdateDisplayedTags()
        {
            int maxNumTags = MaxTagCount;
            currentPageTags = PartCatalog.Instance.GetTagsForPage(ConfigHandler.Instance.DisplayedPage, maxNumTags, out nextPageAvailable);
        }

        private void DrawToolBarTags(Rect drawInto)
        {
            Rect curPos = new Rect(0, 0, ConfigHandler.Instance.ButtonSize.x, ConfigHandler.Instance.ButtonSize.y);
            if (ConfigHandler.Instance.ToolBarDirection == ToolBarDirections.Left)
            {
                curPos.x = drawInto.xMax - curPos.width;
            }
            else if (ConfigHandler.Instance.ToolBarDirection == ToolBarDirections.Up)
            {
                curPos.y = drawInto.yMax - curPos.height;
            }
            int count = 0;
            foreach (PartTag tag in currentPageTags)
            {
                count++;
                if (GUI.Button(curPos, ResourceProxy.Instance.GetTagIcon(tag), GUIStyle.none))
                {
                    PartFilterManager.Instance.ToggleFilter(tag);
                }

                if(tag.IconName == "" && tag.IconOverlay != "")
                {
                    Rect overlayPos = new Rect(curPos);
                    overlayPos.x -= 1;
                    overlayPos.y += 2;
                    GUI.Label(overlayPos, tag.IconOverlay, tag.Enabled ? OverlayStyleEnabled : OverlayStyle);
                }

                Rect LabelPos = new Rect(curPos);
                LabelPos.x += 3;
                LabelPos.y += LabelPos.height * 0.2f;
                GUI.Label(LabelPos, count.ToString(), tag.Enabled ? LabelStyleEnabled : LabelStyle);
                
                
                if (curPos.Contains(Event.current.mousePosition))
                {
                    if (MouseOverStack.Count == 0 || MouseOverStack[0].Tag != tag)
                    {
                        MouseOverStack.Clear();
                        MouseOverStack.Add(new MouseOverStackEntry(tag, new Vector2(curPos.x, curPos.y)));                        
                    }
                    MouseOverClear = false;
                }



                switch (ConfigHandler.Instance.ToolBarDirection)
                {
                    case ToolBarDirections.Up:
                        curPos.y -= curPos.height;
                        break;
                    case ToolBarDirections.Down:
                        curPos.y += curPos.height;
                        break;
                    case ToolBarDirections.Left:
                        curPos.x -= curPos.width;
                        break;
                    case ToolBarDirections.Right:
                        curPos.x += curPos.width;
                        break;
                    default:
                        break;
                }
            }
        }

        private void DrawConfigButton()
        {
            if (GUI.Button(GetConfigButtonPos(), ResourceProxy.Instance.GetIconTexture(GUIConstants.ConfigButtonName, GUITagEditor.Instance.isOpen), GUIStyle.none))
            {
                if (GUITagEditor.Instance.isOpen)
                {
                    GUITagEditor.Instance.Close();
                }
                else
                {
                    GUITagEditor.Instance.Open();
                }
                GUILayoutSettings.Instance.Close();
            }
        }

        #endregion

        #region Positioning
        private Rect GetToolbarRectRaw()
        {
            Rect toReturn = new Rect();
            switch (ConfigHandler.Instance.ToolBarPreset)
            {
                case ToolBarPositions.HorizontalTop:
                    toReturn.x = GUIConstants.EditorPartListWidth;
                    toReturn.y = GUIConstants.EditorToolbarTop;
                    toReturn.width = Screen.width - GUIConstants.EditorButtonsWidth - toReturn.x;
                    toReturn.height = ConfigHandler.Instance.ButtonSize.y;
                    break;
                case ToolBarPositions.HorizontalBottom:
                    toReturn.x = GUIConstants.EditorPartListWidth;
                    toReturn.height = ConfigHandler.Instance.ButtonSize.y;
                    toReturn.y = Screen.height - toReturn.height - 1;
                    toReturn.width = Screen.width - GUIConstants.EditorResetButtonWidth - toReturn.x;
                    break;
                case ToolBarPositions.VerticalLeft:
                    toReturn.x = GUIConstants.EditorPartListWidth;
                    toReturn.width = ConfigHandler.Instance.ButtonSize.x;
                    toReturn.y = GUIConstants.EditorToolbarHeight;
                    toReturn.height = Screen.height - toReturn.y;
                    break;
                case ToolBarPositions.VerticalRight:
                    toReturn.x = Screen.width - ConfigHandler.Instance.ButtonSize.x;
                    toReturn.width = ConfigHandler.Instance.ButtonSize.x;
                    toReturn.y = GUIConstants.EditorButtonsHeight;
                    toReturn.height = Screen.height - toReturn.y - GUIConstants.EditorButtonsHeight;
                    break;
                default:
                    break;
            }
            return toReturn;
        }

        private Rect GetToolbarRectNoConfig()
        {
            Rect toReturn = GetToolbarRectRaw();
            toReturn.x = 0;
            toReturn.y = 0;
            if (ConfigHandler.Instance.ConfigButtonPreset == ConfigButtonPositions.CompoundStart || ConfigHandler.Instance.ConfigButtonPreset == ConfigButtonPositions.CompoundEnd)
            {
                if (ConfigHandler.Instance.ToolBarDirection == ToolBarDirections.Right || ConfigHandler.Instance.ToolBarDirection == ToolBarDirections.Left)
                {
                    toReturn.width -= ConfigHandler.Instance.ButtonSize.x;
                    if ((ConfigHandler.Instance.ToolBarDirection == ToolBarDirections.Right && ConfigHandler.Instance.ConfigButtonPreset == ConfigButtonPositions.CompoundStart) || (ConfigHandler.Instance.ToolBarDirection == ToolBarDirections.Left && ConfigHandler.Instance.ConfigButtonPreset == ConfigButtonPositions.CompoundEnd))
                    {
                        toReturn.x += ConfigHandler.Instance.ButtonSize.x;
                    }
                }
                else
                {
                    toReturn.height -= ConfigHandler.Instance.ButtonSize.y;
                    if ((ConfigHandler.Instance.ToolBarDirection == ToolBarDirections.Down && ConfigHandler.Instance.ConfigButtonPreset == ConfigButtonPositions.CompoundStart) || (ConfigHandler.Instance.ToolBarDirection == ToolBarDirections.Up && ConfigHandler.Instance.ConfigButtonPreset == ConfigButtonPositions.CompoundEnd))
                    {
                        toReturn.y += ConfigHandler.Instance.ButtonSize.y;
                    }
                }
            }
            return toReturn;
        }

        private Rect GetToolbarRectNoPageNumber()
        {
            Rect toReturn = GetToolbarRectNoConfig();
            toReturn.x = 0;
            toReturn.y = 0;

            switch (ConfigHandler.Instance.ToolBarDirection)
            {
                case ToolBarDirections.Up:
                    toReturn.height -= ConfigHandler.Instance.ButtonSize.y;
                    if (ConfigHandler.Instance.PageNumberOnToolbarEnd)
                    {
                        toReturn.y += ConfigHandler.Instance.ButtonSize.y;
                    }
                    break;
                case ToolBarDirections.Down:

                    toReturn.height -= ConfigHandler.Instance.ButtonSize.y;
                    if (!ConfigHandler.Instance.PageNumberOnToolbarEnd)
                    {
                        toReturn.y += ConfigHandler.Instance.ButtonSize.y;
                    }
                    break;
                case ToolBarDirections.Left:
                    toReturn.width -= GUIConstants.PageNumberWidth;
                    if (ConfigHandler.Instance.PageNumberOnToolbarEnd)
                    {
                        toReturn.x += GUIConstants.PageNumberWidth;
                    }
                    break;
                case ToolBarDirections.Right:
                    toReturn.width -= GUIConstants.PageNumberWidth;
                    if (!ConfigHandler.Instance.PageNumberOnToolbarEnd)
                    {
                        toReturn.x += GUIConstants.PageNumberWidth;
                    }
                    break;
                default:
                    break;
            }

            return toReturn;
        }

        private Rect GetPageNumberPos()
        {
            Rect toReturn = new Rect(0, 1.5f, GUIConstants.PageNumberWidth, ConfigHandler.Instance.ButtonSize.y);
            Rect toolBarRect = GetToolbarRectNoConfig();
            if (ConfigHandler.Instance.PageNumberOnToolbarEnd)
            {
                switch (ConfigHandler.Instance.ToolBarDirection)
                {
                    case ToolBarDirections.Down:
                        toReturn.y += toolBarRect.height - ConfigHandler.Instance.ButtonSize.y;
                        break;
                    case ToolBarDirections.Right:
                        toReturn.x = toolBarRect.width - GUIConstants.PageNumberWidth;
                        break;
                }
            }
            else //On start of Toolbar
            {
                switch (ConfigHandler.Instance.ToolBarDirection)
                {
                    case ToolBarDirections.Up:
                        toReturn.y += toolBarRect.height - ConfigHandler.Instance.ButtonSize.y;
                        break;
                    case ToolBarDirections.Left:
                        toReturn.x = toolBarRect.width - GUIConstants.PageNumberWidth;
                        break;
                }

            }
            return toReturn;
        }

        private Rect GetConfigButtonPos()
        {
            Rect toReturn = new Rect(0, 0, ConfigHandler.Instance.ButtonSize.x, ConfigHandler.Instance.ButtonSize.y);
            Rect toolBarRect = GetToolbarRectRaw();
            switch (ConfigHandler.Instance.ConfigButtonPreset)
            {
                case ConfigButtonPositions.CompoundStart:
                    switch (ConfigHandler.Instance.ToolBarDirection)
                    {
                        case ToolBarDirections.Up:
                            toReturn.y = toolBarRect.height - ConfigHandler.Instance.ButtonSize.y;
                            break;
                        case ToolBarDirections.Left:
                            toReturn.x = toolBarRect.width - ConfigHandler.Instance.ButtonSize.x;
                            break;
                    }
                    break;
                case ConfigButtonPositions.CompoundEnd:
                    switch (ConfigHandler.Instance.ToolBarDirection)
                    {
                        case ToolBarDirections.Down:
                            toReturn.y = toolBarRect.height - ConfigHandler.Instance.ButtonSize.y;
                            break;
                        case ToolBarDirections.Right:
                            toReturn.x = toolBarRect.width - ConfigHandler.Instance.ButtonSize.x;
                            break;
                    }
                    break;
                case ConfigButtonPositions.TopMiddle:
                    toReturn.x = Screen.width - toReturn.width - GUIConstants.EditorButtonsWidth;
                    toReturn.y = GUIConstants.EditorToolbarTop;
                    break;
                case ConfigButtonPositions.TopLeft:
                    toReturn.x = GUIConstants.EditorPartListWidth;
                    toReturn.y = GUIConstants.EditorToolbarTop;
                    break;
                case ConfigButtonPositions.BottomLeft:
                    toReturn.x = GUIConstants.EditorPartListWidth;
                    toReturn.y = Screen.height - toReturn.height;
                    break;
                case ConfigButtonPositions.BottomRight:
                    toReturn.x = Screen.width - toReturn.width - GUIConstants.EditorResetButtonWidth;
                    toReturn.y = Screen.height - toReturn.height;
                    break;
            }
            return toReturn;
        }
        #endregion

        #region Input
        public void Update()
        {
            if (ConfigHandler.Instance.AutoHideToolBar)
            {
                Rect ToolbarRect = GetToolbarRectRaw();
                if (ToolbarRect.Contains(Event.current.mousePosition))
                {
                    shiftAmount = Math.Min(shiftAmount + ConfigHandler.Instance.ToolbarShiftSpeed * 0.1f, Math.Min(ToolbarRect.height, ToolbarRect.width));
                }
                else
                {
                    shiftAmount = Math.Max(shiftAmount - ConfigHandler.Instance.ToolbarShiftSpeed * 0.1f, 0);
                }
            }
            if (!MouseOverClear)
            {
                MouseOverTimer = 50;
            }
            else
            {
                if (MouseOverTimer > 0)
                {
                    
                    MouseOverTimer--;
                }
            }
            HandleKeyBindings();
        }
        private PartCategories[] EditorCategories = { PartCategories.Pods, PartCategories.Propulsion, PartCategories.Control, PartCategories.Structural, PartCategories.Aero, PartCategories.Utility, PartCategories.Science };
        private KeyCode[] shortCutsKeyCodes = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0 };
        private void MoveCategory(int step)
        {
            int curIndex = Array.FindIndex<PartCategories>(EditorCategories, (cat) => cat == (PartCategories)EditorPartList.Instance.categorySelected);
            if (curIndex != -1)
            {
                int newIndex = curIndex + step;
                while (newIndex >= 0 && newIndex < EditorCategories.Length && !PartFilterManager.Instance.CategoryEnabled(EditorCategories[newIndex]))
                {
                    newIndex += step;
                }
                if (newIndex >= 0 && newIndex < EditorCategories.Length)
                {
                    EditorPartList.Instance.SelectTab(EditorCategories[newIndex]);
                }
            }
        }

        private static float CurMouseScroll = 0;
        private static float AccumulatedMouseScroll = 0;

        private void HandleKeyBindings()
        {
            HandleMouseScroll();
            if (ConfigHandler.Instance.EnableShortcuts)
            {
                for (int i = 0; i < shortCutsKeyCodes.Length; i++)
                {
                    int index = i;
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        index += 10;
                    }
                    if (index < currentPageTags.Count)
                    {
                        if (Input.GetKeyUp(shortCutsKeyCodes[i]))
                        {
                            PartTag tag = currentPageTags.ElementAt(index);
                            PartFilterManager.Instance.ToggleFilter(tag);
                        }
                    }
                }
            }
            Vector2 MousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            if (GetToolbarRectRaw().Contains(MousePos))
            {
                if (CurMouseScroll < 0.0f)
                {
                    if (nextPageAvailable)
                    {
                        ConfigHandler.Instance.DisplayedPage++;
                        UpdateDisplayedTags();
                    }
                }
                else if (CurMouseScroll > 0.0f && ConfigHandler.Instance.DisplayedPage > 0)
                {
                    ConfigHandler.Instance.DisplayedPage--;
                    UpdateDisplayedTags();
                }
            }
            else if (GUIConstants.EditorScrollRegion.Contains(MousePos))
            {
                if (CurMouseScroll < 0.0f)
                {
                    if (ConfigHandler.Instance.EnableCategoryScrolling && Input.GetKey(KeyCode.LeftControl))
                    {
                        MoveCategory(1);
                    }
                    else if (ConfigHandler.Instance.EnablePartListScrolling)
                    {
                        EditorPartList.Instance.NextPage();
                    }
                }
                else if (CurMouseScroll > 0.0f)
                {
                    if (ConfigHandler.Instance.EnableCategoryScrolling && Input.GetKey(KeyCode.LeftControl))
                    {
                        MoveCategory(-1);
                    }
                    else if (ConfigHandler.Instance.EnablePartListScrolling)
                    {
                        EditorPartList.Instance.PrevPage();
                    }
                }
            }
        }


        private static void HandleMouseScroll()
        {
            AccumulatedMouseScroll += Input.GetAxis("Mouse ScrollWheel");
            CurMouseScroll = 0;
            if (AccumulatedMouseScroll > ConfigHandler.Instance.MouseWheelPrescaler / 10f)
            {
                CurMouseScroll = 1;
                AccumulatedMouseScroll = 0;
            }
            if (AccumulatedMouseScroll < ConfigHandler.Instance.MouseWheelPrescaler / -10f)
            {
                CurMouseScroll = -1;
                AccumulatedMouseScroll = 0;
            }
        }
        #endregion
    }

}
