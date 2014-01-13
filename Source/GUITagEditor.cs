using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace PartCatalog
{
    class GUITagEditor
    {
        #region Members
        #region Instance
        static readonly GUITagEditor instance = new GUITagEditor();
        internal static GUITagEditor Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion
        #region Base

        bool display = false;
        #region HelpRelated
        string CurHelpField = null;
        #endregion
        bool editFilter = false;
        bool editTags = false;
        bool editMods = false;
        bool editIcon = false;
        Rect windowPosition;
        private PartTag selectedPartTag;
        GUIStyle iconStyle = new GUIStyle();

        Dictionary<string, string> HelpTexts = new Dictionary<string, string>();
        Vector2[] ScrollPositions = new Vector2[3];

        string inputText = "";
        private bool DisplaySettingsPanel;
        private PartTag selectedPartTagTransfer;

        private List<string> manufacturerFilter = new List<string>();

        private List<string> moduleFilter = new List<string>();

        private List<string> modFilter = new List<string>();
        private string FilterCaption;
        private static HashSet<string> collapsedFilter = new HashSet<string>();

        #endregion
        #endregion
        #region Constructor
        private GUITagEditor()
        {
            FillHelpTexts();
            iconStyle.alignment = TextAnchor.MiddleCenter;
            iconStyle.margin.top = 6;
        }
        #endregion

        public void Open()
        {
            display = true;
            windowPosition = new Rect(GUIConstants.EditorPartListWidth, GUIConstants.EditorToolbarHeight + GUIConstants.PageNumberWidth, Screen.width - GUIConstants.EditorPartListWidth - GUIConstants.PageNumberWidth, Screen.height - (GUIConstants.EditorToolbarHeight + GUIConstants.PageNumberWidth * 3));
        }

        public void Close()
        {
            display = false;
            PartCatalog.Instance.SavePartTags();
            ConfigHandler.Instance.SaveConfig();
        }

        public void Draw()
        {
            if (display)
            {
                EditorLockManager.Instance.LockGUI();
                GUILayout.Window(ConfigHandler.Instance.TagEditorWindow, windowPosition, DrawWindow, "Tag Editor", GUILayout.Width(windowPosition.width), GUILayout.Height(windowPosition.height));
            }
        }
        public void DrawWindow(int id)
        {
            if(Event.current.type == EventType.KeyDown)
            {
                Update();
            }

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            DrawTagColumn();


            if (DisplaySettingsPanel)
            {
                DrawSettings();
            }
            else if (editIcon)
            {
                if (selectedPartTag != null)
                {
                    DrawIconColumn();
                }
            }
            else
            {
                if (!editFilter)
                {

                    if (selectedPartTagTransfer == null)
                    {
                        DrawAvailablePartColumn();
                        DrawIncludedPartColumn();
                    }
                    else
                    {
                        DrawTagTransferColumn(selectedPartTag, selectedPartTagTransfer);
                        DrawTagTransferColumn(selectedPartTagTransfer, selectedPartTag);

                    }
                }
                else //Draw Filter Editor
                {
                    GUILayout.BeginVertical();
                    if (GUILayout.Button("Close", GUILayout.Width((float)(windowPosition.width / 4 * 3 / 2))))
                    {
                        editFilter = false;
                        editTags = false;
                        ScrollPositions[1] = new Vector2();
                        ScrollPositions[2] = new Vector2();
                        UpdateFilterCaption();
                    }
                    RegisterHelp("FilterClose");

                    GUILayout.BeginHorizontal();
                    if (editTags) //Part Modules
                    {
                        DrawAvailableMods();
                        DrawFilteredMods();
                    }
                    else // Manufacturers
                    {
                        DrawAvailableManufacturers();
                        DrawFilteredManufacturers();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndHorizontal();
            if (ConfigHandler.Instance.HelpActive)
            {
                string label = "";
                if (CurHelpField != null)
                {
                    label = HelpTexts.ContainsKey(CurHelpField) ? HelpTexts[CurHelpField] : String.Format("Missing Help: {0}", CurHelpField);
                }
                GUILayout.Label(label, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndVertical();

            CurHelpField = null;
        }


        private HashSet<string> CollapsedPaths = new HashSet<string>();
        private void DrawIconColumn()
        {
            GUILayout.BeginVertical();
            ScrollPositions[1] = GUILayout.BeginScrollView(ScrollPositions[1], GUILayout.Width((float)(windowPosition.width / 4 * 3 / 2)));
            if (GUILayout.Button("None"))
            {
                selectedPartTag.IconName = "";
            }

            List<string> icons = new List<string>();
            foreach (var icon in ResourceProxy.Instance.LoadedTextures)
            {
                string iconName = icon.Key;

                string sanitized = iconName.Replace('\\', '/');
                if (iconName != sanitized && ResourceProxy.Instance.LoadedTextures.ContainsKey(sanitized))
                {
                    continue;
                }
                if (iconName != "TagDefault")
                {
                    GUILayout.BeginHorizontal();
                    bool pushed = false;
                    pushed |= GUILayout.Button(ResourceProxy.Instance.GetIconTexture(iconName, true), iconStyle, GUILayout.Width(ConfigHandler.Instance.ButtonSize.x), GUILayout.Height(ConfigHandler.Instance.ButtonSize.y));
                    if (icon.Value.IsToggle)
                    {
                        GUILayout.Button(ResourceProxy.Instance.GetIconTexture(iconName, false), iconStyle, GUILayout.Width(ConfigHandler.Instance.ButtonSize.x), GUILayout.Height(ConfigHandler.Instance.ButtonSize.y));
                    }
                    pushed |= GUILayout.Button(sanitized);
                    GUILayout.EndHorizontal();
                    RegisterHelp("IconSelect");
                    if (pushed)
                    {
                        selectedPartTag.IconName = iconName;
                    }
                }
            }
            GUILayout.EndScrollView();
            if(GUILayout.Button("Reload Icons"))
            {
                ResourceProxy.Instance.Reload();
            }
            RegisterHelp("ReloadIcons");
            GUILayout.EndVertical();
        }

        private void DrawFilteredMods()
        {
            GUILayout.BeginVertical(GUILayout.Width((float)(windowPosition.width / 4 * 3 / 2)));
            GUILayout.Label("Filtered Mods", GUILayout.ExpandWidth(true));
            RegisterHelp("FilteredMods");
            ScrollPositions[2] = GUILayout.BeginScrollView(ScrollPositions[2]);
            string toDelete = null;
            foreach (string mod in modFilter)
            {
                if (GUILayout.Button(mod))
                {
                    toDelete = mod;
                }
                RegisterHelp("RemoveModFilter");
            }
            if (toDelete != null)
            {
                modFilter.Remove(toDelete);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawAvailableMods()
        {
            GUILayout.BeginVertical(GUILayout.Width((float)(windowPosition.width / 4 * 3 / 2)));
            GUILayout.Label("Available Mods", GUILayout.ExpandWidth(true));
            RegisterHelp("AvailableMods");
            ScrollPositions[1] = GUILayout.BeginScrollView(ScrollPositions[1]);
            foreach (string mod in PartCatalog.Instance.HashedModCatalog.Keys)
            {
                if (!modFilter.Contains(mod))
                {
                    if (GUILayout.Button(mod))
                    {
                        modFilter.Add(mod);
                    }
                    RegisterHelp("AddModFilter");
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawFilteredManufacturers()
        {
            GUILayout.BeginVertical(GUILayout.Width((float)(windowPosition.width / 4 * 3 / 2)));
            GUILayout.Label("Available Manufacturers", GUILayout.ExpandWidth(true));
            ScrollPositions[2] = GUILayout.BeginScrollView(ScrollPositions[2]);
            string toDelete = null;
            foreach (string manufacturer in manufacturerFilter)
            {
                if (GUILayout.Button(manufacturer))
                {
                    toDelete = manufacturer;
                }
            }
            if (toDelete != null)
            {
                manufacturerFilter.Remove(toDelete);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawAvailableManufacturers()
        {
            GUILayout.BeginVertical(GUILayout.Width((float)(windowPosition.width / 4 * 3 / 2)));
            GUILayout.Label("Filtered Manufacturers", GUILayout.ExpandWidth(true));
            ScrollPositions[1] = GUILayout.BeginScrollView(ScrollPositions[1]);
            foreach (string manufacturer in PartCatalog.Instance.HashedManufacturerCatalog.Keys)
            {
                if (!manufacturerFilter.Contains(manufacturer))
                {
                    if (GUILayout.Button(manufacturer))
                    {
                        manufacturerFilter.Add(manufacturer);
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }


        private void UpdateFilterCaption()
        {
            List<string> filters = new List<string>();
            if (modFilter.Count == 1)
            {
                filters.Add("Mod");
            }
            else if (modFilter.Count > 1)
            {
                filters.Add("Mods");
            }
            if (manufacturerFilter.Count == 1)
            {
                filters.Add("Manufacturer");
            }
            else if (manufacturerFilter.Count > 1)
            {
                filters.Add("Manufacturers");
            }
            if (moduleFilter.Count == 1)
            {
                filters.Add("PartModule");
            }
            else if (moduleFilter.Count > 1)
            {
                filters.Add("PartModules");
            }
            if (filters.Count == 0)
            {
                FilterCaption = "No Filter";
            }
            else
            {
                FilterCaption = string.Format("Filtered by: {0}", string.Join(",", filters.ToArray()));
            }
        }

        private void DrawTagTransferColumn(PartTag tagToList, PartTag tagMoveTo)
        {
            if (tagToList == selectedPartTag)
            {
                GUILayout.BeginVertical(GUILayout.Width((float)(windowPosition.width / 4 * 3 / 2)));
            }
            else
            {
                GUILayout.BeginVertical(GUILayout.Width((float)(windowPosition.width / 6 * 5 / 2)));
            }
            //DrawAddAllButton(selected);
            //DrawRemoveAllButton(selected);
            if (tagMoveTo == selectedPartTag)
            {
                DrawSortButton();
            }
            else
            {
                DrawFilterButton();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label(String.Format("{0} {1}", tagToList.Name, GetCategoryString()), GUILayout.ExpandWidth(true));
            RegisterHelp("TransferPartList");
            if (tagToList == selectedPartTag)
            {
                if (ConfigHandler.Instance.SynchronizePartCategories)
                {
                    if (GUILayout.Button("S", GUILayout.Width(30)))
                    {
                        ConfigHandler.Instance.SynchronizePartCategories = false;
                    }
                    RegisterHelp("SyncCategoriesOn");
                }
                else
                {
                    if (GUILayout.Button("A", GUILayout.Width(30)))
                    {
                        ConfigHandler.Instance.SynchronizePartCategories = true;
                    }
                    RegisterHelp("SyncCategoriesOff");
                }

                if (GUILayout.Button("+", GUILayout.Width(30)))
                {
                    collapsedFilter.Clear();
                }
                RegisterHelp("ExpandAll");
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    foreach (AvailablePart part in PartCatalog.Instance.SortedPartList)
                    {
                        string category = GetPartCategory(part);
                        if (category != null)
                        {
                            collapsedFilter.Add(category);
                        }
                    }
                }
                RegisterHelp("CollapseAll");
            }
            GUILayout.EndHorizontal();
            ScrollPositions[2] = GUILayout.BeginScrollView(ScrollPositions[2]);
            AvailablePart toMove = null;
            string lastCategory = null;
            foreach (AvailablePart part in PartCatalog.Instance.SortedPartList)
            {
                if (tagToList.IncludedParts.Contains(part))
                {
                    if (FilterPart(part))
                    {
                        bool draw = true;
                        string caption = part.title;
                        if (ConfigHandler.Instance.SortNumber == 0)
                        {
                            caption = part.name;
                        }
                        else
                        {
                            DrawPartCategory(ref lastCategory, part, ref draw, tagToList == selectedPartTag);
                        }

                        if (draw && GUILayout.Button(part.title))
                        {
                            toMove = part;
                        }
                        RegisterHelp("TransferPart");

                    }
                }
            }
            if (toMove != null)
            {
                if (!Input.GetKey(KeyCode.LeftControl))
                {
                    tagToList.RemovePart(toMove);
                }
                tagMoveTo.AddPart(toMove);
            }
            GUILayout.EndScrollView();
            RegisterHelp("TransferPartList");
            GUILayout.EndVertical();
        }

        private string GetPartCategory(AvailablePart part)
        {
            string category = null;
            if (ConfigHandler.Instance.SortNumber == 2)
            {
                category = part.manufacturer;
            }
            else if (ConfigHandler.Instance.SortNumber == 3)
            {
                category = PartCatalog.Instance.PartModIndex[part];
            }
            return category;
        }

        private void DrawPartCategory(ref string lastCategory, AvailablePart part, ref bool draw, bool DirectionAdd)
        {
            string category = null;
            bool newCategory = false;
            category = GetPartCategory(part);
            if (category != null)
            {
                if (lastCategory != category)
                {
                    lastCategory = category;
                    newCategory = true;
                }
                draw = !collapsedFilter.Contains(category);
            }


            if (newCategory)
            {
                GUILayout.BeginHorizontal();
                if (collapsedFilter.Contains(category))
                {
                    if (GUILayout.Button("+", GUILayout.Width(30)))
                    {
                        collapsedFilter.Remove(category);
                    }
                    RegisterHelp("ExpandCategory");
                }
                else
                {
                    if (GUILayout.Button("-", GUILayout.Width(30)))
                    {

                        collapsedFilter.Add(category);
                    }
                    RegisterHelp("CollapseCategory");
                }
                GUILayout.Label(category, GUILayout.ExpandWidth(true));
                RegisterHelp("CategoryLabel");
                if (DirectionAdd)
                {
                    if (GUILayout.Button("→", GUILayout.Width(30)))
                    {
                        AddPartsFromCategory(category);
                    }
                    RegisterHelp("AddCategory");
                }
                else
                {
                    if (GUILayout.Button("←", GUILayout.Width(30)))
                    {
                        RemovePartsFromCategory(category);
                    }
                    RegisterHelp("RemoveCategory");
                }
                GUILayout.EndHorizontal();
            }
        }

        private void RemovePartsFromCategory(string categoryName)
        {
            if (selectedPartTagTransfer == null)
            {
                HashSet<AvailablePart> toRemove = new HashSet<AvailablePart>();
                foreach (AvailablePart part in PartCatalog.Instance.SortedPartList)
                {

                    string category = GetPartCategory(part);
                    if (category != null && category == categoryName && FilterPart(part) && selectedPartTag.IncludedParts.Contains(part))
                    {
                        toRemove.Add(part);
                    }
                }
                selectedPartTag.RemoveParts(toRemove);
            }
            else
            {
                HashSet<AvailablePart> toAdd = new HashSet<AvailablePart>();
                HashSet<AvailablePart> toRemove = new HashSet<AvailablePart>();
                foreach (AvailablePart part in PartCatalog.Instance.SortedPartList)
                {
                    string category = GetPartCategory(part);
                    if (category != null && category == categoryName && part.name != "" && FilterPart(part) && selectedPartTagTransfer.IncludedParts.Contains(part))
                    {
                        toAdd.Add(part);
                        if (!Input.GetKey(KeyCode.LeftControl))
                        {
                            toRemove.Add(part);
                        }
                    }
                }
                selectedPartTagTransfer.RemoveParts(toRemove);
                selectedPartTag.AddParts(toAdd);
            }
        }

        private void AddPartsFromCategory(string categoryName)
        {
            if (selectedPartTagTransfer == null)
            {
                HashSet<AvailablePart> toAdd = new HashSet<AvailablePart>();
                foreach (AvailablePart part in PartCatalog.Instance.SortedPartList)
                {
                    string category = GetPartCategory(part);
                    if (category != null && category == categoryName && part.name != "" && FilterPart(part) && !selectedPartTag.IncludedParts.Contains(part))
                    {
                        toAdd.Add(part);
                    }
                }
                selectedPartTag.AddParts(toAdd);
            }
            else
            {
                HashSet<AvailablePart> toAdd = new HashSet<AvailablePart>();
                HashSet<AvailablePart> toRemove = new HashSet<AvailablePart>();
                foreach (AvailablePart part in PartCatalog.Instance.SortedPartList)
                {
                    string category = GetPartCategory(part);
                    if (category != null && category == categoryName && part.name != "" && FilterPart(part) && selectedPartTag.IncludedParts.Contains(part))
                    {
                        toAdd.Add(part);
                        if (!Input.GetKey(KeyCode.LeftControl))
                        {
                            toRemove.Add(part);
                        }
                    }
                }
                selectedPartTag.RemoveParts(toRemove);
                selectedPartTagTransfer.AddParts(toAdd);
            }
        }

        private bool FilterPart(AvailablePart part)
        {
            PartCategories cat = part.category;
            if (cat == PartCategories.none)
            {
                return false;
            }
            if (ConfigHandler.Instance.SynchronizePartCategories && EditorPartList.Instance.categorySelected != (int)cat)
            {
                return false;
            }

            bool add = manufacturerFilter.Count == 0 && modFilter.Count == 0;

            foreach (string manufacturer in manufacturerFilter)
            {
                if (PartCatalog.Instance.HashedManufacturerCatalog.ContainsKey(manufacturer) && PartCatalog.Instance.HashedManufacturerCatalog[manufacturer].Contains(part))
                {
                    add = true;
                    break;
                }
            }

            foreach (string mod in modFilter)
            {
                if (PartCatalog.Instance.HashedModCatalog.ContainsKey(mod) && PartCatalog.Instance.HashedModCatalog[mod].Contains(part))
                {
                    add = true;
                    break;
                }
            }

            return add;
        }

        private string GetCategoryString()
        {
            if (!ConfigHandler.Instance.SynchronizePartCategories)
            {
                return "";
            }
                    
            
            switch ((PartCategories)EditorPartList.Instance.categorySelected)
            {
                case PartCategories.Pods:
                    return "(Pods)";
                case PartCategories.Propulsion:
                    return "(Propulsion)";
                case PartCategories.Control:
                    return "(Control)";
                case PartCategories.Structural:
                    return "(Structural)";
                case PartCategories.Aero:
                    return "(Aerodynamics)";
                case PartCategories.Utility:
                    return "(Utilities)";
                case PartCategories.Science:
                    return "(Science)";
                default:
                    return "";
            }
        }

        private void DrawFilterButton()
        {
            if (GUILayout.Button(FilterCaption))
            {
                {
                    editFilter = true;
                    editTags = true;
                    editMods = true;
                }
            }
            RegisterHelp("FilterEditor");
        }

        private void DrawSortButton()
        {
            switch (ConfigHandler.Instance.SortNumber)
            {
                case 0:
                    if (GUILayout.Button("Sort: Name"))
                    {
                        ConfigHandler.Instance.SortNumber = 1;
                        PartCatalog.Instance.SortPartList();
                    }
                    RegisterHelp("SortName");
                    break;
                case 1:
                    if (GUILayout.Button("Sort: Title"))
                    {
                        ConfigHandler.Instance.SortNumber = 3;
                        PartCatalog.Instance.SortPartList();
                    }
                    RegisterHelp("SortTitle");
                    break;
                case 2:
                    if (GUILayout.Button("Sort: Manufacturer"))
                    {

                        ConfigHandler.Instance.SortNumber = 0;
                        PartCatalog.Instance.SortPartList();
                    }
                    RegisterHelp("SortManufacturer");
                    break;
                case 3:
                    if (GUILayout.Button("Sort: Mod"))
                    {
                        ConfigHandler.Instance.SortNumber = 2;
                        PartCatalog.Instance.SortPartList();
                    }
                    RegisterHelp("SortMod");
                    break;
            }
        }

        private void DrawIncludedPartColumn()
        {
            GUILayout.BeginVertical(GUILayout.Width((float)(windowPosition.width / 6 * 5 / 2)));
            DrawFilterButton();
            GUILayout.Label(String.Format("Included Parts {0}", GetCategoryString()), GUILayout.ExpandWidth(true));
            RegisterHelp("IncludedPartList");
            ScrollPositions[2] = GUILayout.BeginScrollView(ScrollPositions[2]);
            if (selectedPartTag != null)
            {
                AvailablePart toDelete = null;
                string lastCategory = null;
                foreach (AvailablePart part in PartCatalog.Instance.SortedPartList)
                {
                    if (selectedPartTag.IncludedParts.Contains(part))
                    {
                        if (FilterPart(part))
                        {
                            bool draw = true;
                            string caption = part.title;
                            if (ConfigHandler.Instance.SortNumber == 0)
                            {
                                caption = part.name;
                            }
                            else
                            {
                                DrawPartCategory(ref lastCategory, part, ref draw, false);
                            }

                            if (draw)
                            {
                                if (GUILayout.Button(part.title))
                                {
                                    toDelete = part;
                                }
                                RegisterHelp("RemovePart");
                            }
                        }
                    }
                }
                if (toDelete != null)
                {
                    selectedPartTag.RemovePart(toDelete);
                }
            }
            GUILayout.EndScrollView();
            RegisterHelp("IncludedPartList");
            GUILayout.EndVertical();

        }

        private void DrawAvailablePartColumn()
        {
            GUILayout.BeginVertical(GUILayout.Width((float)(windowPosition.width / 4 * 3 / 2)));
            DrawSortButton();
            GUILayout.BeginHorizontal();
            GUILayout.Label(String.Format("Available Parts {0}", GetCategoryString()), GUILayout.ExpandWidth(true));
            RegisterHelp("AvailablePartList");
            if (ConfigHandler.Instance.SynchronizePartCategories)
            {
                if (GUILayout.Button("S", GUILayout.Width(30)))
                {
                    ConfigHandler.Instance.SynchronizePartCategories = false;
                }
                RegisterHelp("SyncCategoriesOn");
            }
            else
            {
                if (GUILayout.Button("A", GUILayout.Width(30)))
                {
                    ConfigHandler.Instance.SynchronizePartCategories = true;
                }
                RegisterHelp("SyncCategoriesOff");
            }

            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                collapsedFilter.Clear();
            }
            RegisterHelp("ExpandAll");
            if (GUILayout.Button("-", GUILayout.Width(30)))
            {
                foreach (AvailablePart part in PartCatalog.Instance.SortedPartList)
                {
                    string category = GetPartCategory(part);
                    if (category != null)
                    {
                        collapsedFilter.Add(category);
                    }
                }
            }
            RegisterHelp("CollapseAll");

            GUILayout.EndHorizontal();
            ScrollPositions[1] = GUILayout.BeginScrollView(ScrollPositions[1]);
            string lastCategory = null;
            if (selectedPartTag != null)
            {
                foreach (AvailablePart part in PartCatalog.Instance.SortedPartList)
                {
                    if (part.name != "")
                    {
                        if (FilterPart(part))
                        {
                            if (PartInclusionFilter(selectedPartTag, part))
                            {
                                bool draw = true;

                                string caption = part.title;
                                if (ConfigHandler.Instance.SortNumber == 0)
                                {
                                    caption = part.name;
                                }
                                else
                                {
                                    DrawPartCategory(ref lastCategory, part, ref draw, true);
                                }
                                if (draw)
                                {
                                    if (GUILayout.Button(caption))
                                    {
                                        selectedPartTag.AddPart(part);
                                    }
                                    RegisterHelp("AddPart");
                                }
                            }
                        }
                    }
                }
            }

            GUILayout.EndScrollView();
            RegisterHelp("AvailablePartList");

            GUILayout.EndVertical();
        }

        private bool PartInclusionFilter(PartTag tag, AvailablePart part)
        {
            if (ConfigHandler.Instance.SearchPartInSubtags)
            {
                return !tag.VisibleParts.Contains(part);
            }
            else
            {
                return !tag.IncludedParts.Contains(part);
            }
        }

        private bool PartInclusionFilter(AvailablePart part)
        {
            return !selectedPartTag.IncludedParts.Contains(part);
        }

        private void DrawSettings()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(500));

            GUILayout.BeginHorizontal();
            ConfigHandler.Instance.EnableShortcuts = GUILayout.Toggle(ConfigHandler.Instance.EnableShortcuts, "Shortcuts");
            GUILayout.EndHorizontal();
            RegisterHelp("EnableShortcuts");
            GUILayout.BeginHorizontal();
            ConfigHandler.Instance.EnablePartListScrolling = GUILayout.Toggle(ConfigHandler.Instance.EnablePartListScrolling, "Editor Page Scrolling");
            GUILayout.EndHorizontal();
            RegisterHelp("EnablePartListScrolling");
            GUILayout.BeginHorizontal();
            ConfigHandler.Instance.EnableCategoryScrolling = GUILayout.Toggle(ConfigHandler.Instance.EnableCategoryScrolling, "Editor Category Scrolling");
            GUILayout.EndHorizontal();
            RegisterHelp("EnableCategoryScrolling");

            GUILayout.BeginHorizontal();
            ConfigHandler.Instance.DisplayAllOnEmptyFilter = GUILayout.Toggle(ConfigHandler.Instance.DisplayAllOnEmptyFilter, "Display all parts if filter is empty");
            GUILayout.EndHorizontal();
            RegisterHelp("DisplayAllPartsEmptyFilter");

            GUILayout.BeginHorizontal();
            ConfigHandler.Instance.HideUnresearchedTags = GUILayout.Toggle(ConfigHandler.Instance.HideUnresearchedTags, "Hide unavailable Parts");
            GUILayout.EndHorizontal();
            RegisterHelp("HideUnresearchedTags");
            /*
            GUILayout.BeginHorizontal();
            ConfigHandler.Instance.HideEmptyCategories = GUILayout.Toggle(ConfigHandler.Instance.HideEmptyCategories, "Hide empty categories");
            GUILayout.EndHorizontal();
            RegisterHelp("HideEmptyCategories");
            */
            if (ConfigHandler.Instance.UnionFilter)
            {
                if (GUILayout.Button("Union tags"))
                {
                    ConfigHandler.Instance.UnionFilter = false;
                }
                RegisterHelp("UnionFilter");
            }
            else
            {
                if (GUILayout.Button("Intersect tags"))
                {
                    ConfigHandler.Instance.UnionFilter = true;
                }
                RegisterHelp("IntersectFilter");
            }

            GUILayout.BeginHorizontal(GUILayout.Height(40));
            GUILayout.Label("Mouse Wheel Prescaler", GUILayout.Height(40), GUILayout.Width(100));
            GUILayout.Label(ConfigHandler.Instance.MouseWheelPrescaler.ToString(), GUILayout.Height(40));
            ConfigHandler.Instance.MouseWheelPrescaler = (int)GUILayout.HorizontalSlider((float)ConfigHandler.Instance.MouseWheelPrescaler, 0f, 20f, GUILayout.Height(40));
            GUILayout.EndHorizontal();
            RegisterHelp("MouseWheelPrescaler");

            GUILayout.BeginHorizontal(GUILayout.Height(40));
            GUILayout.Label("Tag Move Multiplier", GUILayout.Height(40), GUILayout.Width(100));
            GUILayout.Label(ConfigHandler.Instance.TagMoveMultiplier.ToString(), GUILayout.Height(40));
            ConfigHandler.Instance.TagMoveMultiplier = (int)GUILayout.HorizontalSlider((float)ConfigHandler.Instance.TagMoveMultiplier, 2f, 20f, GUILayout.Height(40));
            GUILayout.EndHorizontal();
            RegisterHelp("TagMoveMultiplier");

            GUILayout.BeginHorizontal(GUILayout.Height(40));
            GUILayout.Label("Small mod size", GUILayout.Height(40), GUILayout.Width(100));
            GUILayout.Label(ConfigHandler.Instance.SmallModTagPartCount.ToString(), GUILayout.Height(40));
            ConfigHandler.Instance.SmallModTagPartCount = (int)GUILayout.HorizontalSlider((float)ConfigHandler.Instance.SmallModTagPartCount, 0f, 100f, GUILayout.Height(40));
            GUILayout.EndHorizontal();
            RegisterHelp("SmallModSize");


            if (GUILayout.Button("Open Layout Options"))
            {
                Close();
                GUILayoutSettings.Instance.Open();
            }
            RegisterHelp("LayoutSettings");
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        #region HelpRelated
        private void RegisterHelp(string name)
        {
            if (ConfigHandler.Instance.HelpActive)
            {
                if (CurHelpField == null)
                {
                    if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    {
                        CurHelpField = name;
                    }
                }
            }
        }
        private void FillHelpTexts()
        {
            HelpTexts["FilterClose"] = "Click to return to the part list applying your filter changes";
            HelpTexts["TransferPartList"] = "List of parts to transfer";
            HelpTexts["SyncCategoriesOn"] = "Click to show all parts. Currently showing only parts based on your selected editor category";
            HelpTexts["SyncCategoriesOff"] = "Click to hide parts not included in the selected editor category. Currently showing all parts";
            HelpTexts["ExpandAll"] = "Expand all groups";
            HelpTexts["CollapseAll"] = "Collapse all groups";
            HelpTexts["ExpandAllTags"] = "Expand all tags";
            HelpTexts["CollapseAllTags"] = "Collapse all tags";
            HelpTexts["TransferPart"] = "Transfer part to other tag. Hold control while doing so to copy";
            HelpTexts["FilteredMods"] = "Applied mod filters";
            HelpTexts["RemoveModFilter"] = "Click to remove mod filter";
            HelpTexts["AddModFilter"] = "Click to add mod filter";
            HelpTexts["AvailableMods"] = "Available mods";
            HelpTexts["FilterEditor"] = "Click to enter the filter editor (filtering hides parts from the tag editor)";
            HelpTexts["IncludedSubTags"] = "All included subtags";
            HelpTexts["RemoveSubTag"] = "Click to exclude subtag";
            HelpTexts["AvailableSubTags"] = "All available subtags";
            HelpTexts["AddSubTag"] = "Click to include subtag";
            HelpTexts["IncludedPartList"] = "All parts included in selected tag";
            HelpTexts["RemovePart"] = "Click to remove part from selected tag";
            HelpTexts["AddAllParts"] = "Click to add all listed parts to selected tag";
            HelpTexts["RemoveAllParts"] = "Click to remove all listed parts to selected tag";
            HelpTexts["AvailablePartList"] = "All available parts not included in the selected tag";
            HelpTexts["AddPart"] = "Click to add part to selected tag";
            HelpTexts["ExpandCategory"] = "Click to expand this group";
            HelpTexts["CollapseCategory"] = "Click to collapse this group";
            HelpTexts["CategoryLabel"] = "A group making your part easier to navigate";
            HelpTexts["AddCategory"] = "Click to move all parts included in group";
            HelpTexts["RemoveCategory"] = "Click to move all parts included in group";
            HelpTexts["SortName"] = "Parts are sorted by part name";
            HelpTexts["SortTitle"] = "Parts are sorted by title";
            HelpTexts["SortManufacturer"] = "Parts are sorted by title and grouped by manufacturer";
            HelpTexts["SortMod"] = "Parts are sorted by title and grouped by mod";
            HelpTexts["AutoTag"] = "Click to automagically create tags based on mods and part class";
            HelpTexts["NewTag"] = "Click to create a new tag";
            HelpTexts["DeleteTag"] = "Click to delete selected tag, you've been warned";
            HelpTexts["RenameTag"] = "Click to rename tag";
            HelpTexts["InputText"] = "Enter new tag name here for creation and renaming. Double click a tag to copy its name here";
            HelpTexts["PageNumber"] = "The page this tag will appear on. * indicates new pages due to too many tags on toolbar";
            HelpTexts["ExpandTag"] = "Click to Expand this tag displaying all tags underneath. Hold control to expand all child tags aswell";
            HelpTexts["CollapseTag"] = "Click to Collapse this tag hiding all tags underneath. Hold control to collapse all child tags aswell";
            HelpTexts["SelectTag"] = "Click to select tag in order to edit it, or hold alt and select another one to transfer parts";
            HelpTexts["TagList"] = "A list of all your created tags";
            HelpTexts["UnindentTag"] = "Click to move this tag up in the hierachy";
            HelpTexts["MoveTagUp"] = "Click to move this tag further to the top of the list, hold shift for bigger increments";
            HelpTexts["MoveTagDown"] = "Click to move this tag further to the top of the list, hold shift for bigger increments";
            HelpTexts["IndentTag"] = "Click to move this tag deeper down the hierachy";
            HelpTexts["DisableNewPage"] = "Click to disable the pagebreak for this tag";
            HelpTexts["EnableNewPage"] = "Click to enable a pagebreak for this tag, displaying it on a new page in the toolbar";
            HelpTexts["VisibilityVS"] = "Click to switch toolbar visibility. The selected tag is currently visible in both VAB and SPH";
            HelpTexts["VisibilityS"] = "Click to switch toolbar visibility. The selected tag is currently visible in SPH";
            HelpTexts["VisibilityV"] = "Click to switch toolbar visibility. The selected tag is currently visible in both VAB";
            HelpTexts["VisibilityNone"] = "Click to switch toolbar visibility. The selected tag is currently not visible";
            HelpTexts["Settings"] = "Click to open the settings panel";
            HelpTexts["Help"] = "Turn off this help again";
            HelpTexts["EnableShortcuts"] = "Enable or disable using the number keys 1 to 0 to select tags. Hold down shift to select the next 10 tags";
            HelpTexts["EnablePartListScrolling"] = "Enables or disables using the mouse wheel to scroll through the editor part list while holding ctrl";
            HelpTexts["EnableCategoryScrolling"] = "Enables or disables using the mouse wheel and holding down ctrl and shift to change editor categries";
            HelpTexts["MouseWheelPrescaler"] = "Sets the mousewheel prescaler in order to slow down the mouse scrolling";
            HelpTexts["TagMoveMultiplier"] = "Sets the multiplier used when moving tags up and down the list while holding down shift";
            HelpTexts["AutoGroup"] = "Click to automagically subdivide a tag into a default set of groups";
            HelpTexts["LayoutSettings"] = "Click to open the layout settings window to reposition the toolbar and tag editor button";
            HelpTexts["IconSelect"] = "Click to select this icon for the selected tag";
            HelpTexts["SmallModSize"] = "Sets the maximum part count for a mod to be categorized under the small mods tag.";
            HelpTexts["DisplayAllPartsEmptyFilter"] = "If the selected combination of filters contains no parts, display all parts";
            HelpTexts["UnionFilter"] = "By selecting multiple tags with ctrl all parts contained in those tags are displayed";
            HelpTexts["IntersectFilter"] = "By selecting multiple tags with ctrl only parts contained in every selected tags are displayed";
            HelpTexts["HideUnresearchedTags"] = "Select this to hide tags not containing researched parts";
            HelpTexts["HideEmptyCategories"] = "Select this to hide empty categories";
            HelpTexts["OverlayText"] = "Set the initials displayed when no icon is set on the selected tag";
            HelpTexts["RenameTag"] = "Set the name of the selected tag";
            HelpTexts["ReloadIcons"] = "Reload all available icons";
            HelpTexts[""] = "";
        }
        #endregion

        private void DrawTagColumn()
        {
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.Width(windowPosition.width / 6));
            DrawCategoryButton();
            //GUILayout.BeginHorizontal();
            if (GUILayout.Button("Autotag"))
            {

                //PartCategoryRuleHandler.Instance.ReloadFiles();               
                //PartCatalog.Instance.AutoTagByMod();
                LuaRuleHandler.Instance.ParseParts();
            }

            RegisterHelp("AutoTag");   /*
            if (GUILayout.Button("Autogroup"))
            {
                if (selectedPartTag != null)
                {

                    //PartCategoryRuleHandler.Instance.AutoGroupPartTag(ref selectedPartTag);
                }
            }

            RegisterHelp("AutoGroup");  */
            //GUILayout.EndHorizontal();
            
            /* INDEV
            if (GUILayout.Button("Test"))
            {
                PartCategoryRuleHandler.Instance.ParseRulesFile("Default_rule.txt");
                if (selectedPartTag != null)
                {                         
                    foreach (var part in selectedPartTag.IncludedParts)
                    {
                        PartCategoryRuleHandler.Instance.GetCategoriesForPart(part);
                    }
                }
            }      
             */
            if (GUILayout.Button("New Tag"))
            {
                inputText = inputText.Trim();

                if (inputText != "")
                {
                    PartTag newTag = new PartTag();
                    newTag.Name = inputText;
                    newTag.VisibleInSPH = true;
                    newTag.VisibleInVAB = true;
                    if (selectedPartTag != null)
                    {
                        selectedPartTag.AddAfter(newTag);
                    }
                    else
                    {
                        PartCatalog.Instance.PartTags.AddLast(newTag);
                    }
                    selectedPartTag = newTag;
                    inputText = "";
                }

            }
            RegisterHelp("NewTag");

            if (GUILayout.Button("Delete Tag"))
            {
                if (selectedPartTag != null)
                {
                    PartTag nextTag = null;
                    LinkedListNode<PartTag> node = selectedPartTag.Parent.ChildTags.Find(selectedPartTag);
                    if (node != null)
                    {
                        if (node.Next != null)
                        {
                            nextTag = node.Next.Value;
                        }
                        else if (node.Previous != null)
                        {
                            nextTag = node.Previous.Value;
                        }
                    }

                    selectedPartTag.Delete();
                    selectedPartTag = nextTag;
                }
            }
            RegisterHelp("DeleteTag");            
            inputText = GUILayout.TextField(inputText);
            RegisterHelp("InputText");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Available Tags", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                PartCatalog.Instance.RootTag.setCollapsedRecursive(false);
            }
            RegisterHelp("ExpandAllTags");
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                PartCatalog.Instance.RootTag.setCollapsedRecursive(true);
            }
            RegisterHelp("CollapseAllTags");
            GUILayout.EndHorizontal();
            int ListPageNum = 0;
            int ListPageTags = 0;
            ScrollPositions[0] = GUILayout.BeginScrollView(ScrollPositions[0]); //List of Tags
            foreach (PartTag tag in PartCatalog.Instance.PartTags)
            {
                if (ListPageNum == 0 || tag.StartNewPage || ListPageTags == GUIEditorControls.Instance.MaxTagCount)
                {
                    ListPageNum++;
                    ListPageTags = 0;
                }
                if (ListPageTags == 0)
                {
                    DrawTagButton(ListPageNum, 0, tag);
                }
                else
                {
                    DrawTagButton(-1, 0, tag);
                }
                ListPageTags++;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndScrollView();
            RegisterHelp("TagList");

            if (selectedPartTag != null)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("←"))
                {
                    selectedPartTag.Unindent();
                }
                RegisterHelp("UnindentTag");
                if (GUILayout.Button("↑"))
                {
                    int num = 1;
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        num = ConfigHandler.Instance.TagMoveMultiplier;
                    }
                    for (int i = 0; i < num; i++)
                    {
                        selectedPartTag.MoveUp();
                    }
                }
                RegisterHelp("MoveTagUp");
                if (GUILayout.Button("↓"))
                {
                    int num = 1;
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        num = ConfigHandler.Instance.TagMoveMultiplier;
                    }
                    for (int i = 0; i < num; i++)
                    {
                        selectedPartTag.MoveDown();
                    }
                }
                RegisterHelp("MoveTagDown");
                if (GUILayout.Button("→"))
                {
                    selectedPartTag.Indent();
                    selectedPartTag.Parent.Collapsed = false;
                }
                RegisterHelp("IndentTag");

                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Label("Name");
                selectedPartTag.Name = GUILayout.TextField(selectedPartTag.Name);
                RegisterHelp("OverlayText");
                GUILayout.EndHorizontal();
                RegisterHelp("RenameTag");

                GUILayout.BeginHorizontal();
                GUILayout.Label("Initials");
                selectedPartTag.IconOverlay = GUILayout.TextField(selectedPartTag.IconOverlay);
                RegisterHelp("OverlayText");
                GUILayout.EndHorizontal();
                

                editIcon = GUILayout.Toggle(editIcon, "Edit Icon", HighLogic.Skin.button);

                if (selectedPartTag.Parent == PartCatalog.Instance.RootTag)
                {
                    if (selectedPartTag.StartNewPage)
                    {
                        if (GUILayout.Button("Disable New Page"))
                        {
                            selectedPartTag.StartNewPage = false;
                        }
                        RegisterHelp("DisableNewPage");
                    }
                    else
                    {
                        if (GUILayout.Button("Enable New Page"))
                        {
                            selectedPartTag.StartNewPage = true;
                        }
                        RegisterHelp("EnableNewPage");
                    }
                }
                if (selectedPartTag.VisibleInSPH && selectedPartTag.VisibleInVAB)
                {
                    if (GUILayout.Button("Visibility: VAB&SPH"))
                    {
                        selectedPartTag.VisibleInSPH = false;
                    }
                    RegisterHelp("VisibilityVS");
                }
                else if (selectedPartTag.VisibleInVAB)
                {
                    if (GUILayout.Button("Visibility: VAB"))
                    {
                        selectedPartTag.VisibleInSPH = true;
                        selectedPartTag.VisibleInVAB = false;
                    }
                    RegisterHelp("VisibilityV");
                }
                else if (selectedPartTag.VisibleInSPH)
                {
                    if (GUILayout.Button("Visibility: SPH"))
                    {
                        selectedPartTag.VisibleInSPH = false;
                    }
                    RegisterHelp("VisibilityS");
                }
                else
                {
                    if (GUILayout.Button("Visibility: None"))
                    {
                        selectedPartTag.VisibleInSPH = true;
                        selectedPartTag.VisibleInVAB = true;
                    }
                    RegisterHelp("VisibilityNone");
                }
            }
            GUILayout.BeginHorizontal();
            DisplaySettingsPanel = GUILayout.Toggle(DisplaySettingsPanel, "Settings", HighLogic.Skin.button);
            RegisterHelp("Settings");
            ConfigHandler.Instance.HelpActive = GUILayout.Toggle(ConfigHandler.Instance.HelpActive, "?", HighLogic.Skin.button, GUILayout.Width(30));
            RegisterHelp("Help");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawTagButton(int ListPageNum, int TagIndentation, PartTag tag)
        {
            int count = tag.VisibleParts.Count;
            string title = tag.Name;
            if (selectedPartTagTransfer == null)
            {
                if (selectedPartTag == tag)
                {
                    title = string.Format(">{0}<", title);
                }
            }
            else
            {
                if (selectedPartTag == tag)
                {
                    title = string.Format("←{0}", title);
                }
                if (selectedPartTagTransfer == tag)
                {
                    title = string.Format("→{0}", title);
                }
            }
            GUILayout.BeginHorizontal();
            if (ListPageNum != -1)
            {
                GUILayout.Label(tag.StartNewPage || ListPageNum == 1
                                    ? String.Format("{0}", ListPageNum)
                                    : String.Format("*{0}", ListPageNum), GUILayout.Width(17));
            }
            else
            {
                GUILayout.Space(20);
            }
            RegisterHelp("PageNumber");

            for (int i = 0; i < TagIndentation; i++)
            {
                GUILayout.Space(24);
            }
            if (tag.ChildTags.Count > 0)
            {
                if (tag.Collapsed)
                {
                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        if (Input.GetKey(KeyCode.LeftControl))
                        {
                            tag.setCollapsedRecursive(false);
                        }
                        else
                        {
                            tag.Collapsed = false;
                        }
                    }
                    RegisterHelp("ExpandTag");
                }
                else
                {
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        if (Input.GetKey(KeyCode.LeftControl))
                        {
                            tag.setCollapsedRecursive(true);
                        }
                        else
                        {
                            tag.Collapsed = true;
                        }
                    }
                    RegisterHelp("CollapseTag");
                }
            }
            bool buttonPress = false;
            if (tag.IconName != "")
            {
                buttonPress |= GUILayout.Button(ResourceProxy.Instance.GetIconTexture(tag.IconName, true), iconStyle, GUILayout.Width(ConfigHandler.Instance.ButtonSize.x), GUILayout.Height(ConfigHandler.Instance.ButtonSize.y));
            }
            TextAnchor oldAlignment = GUI.skin.button.alignment;
            GUI.skin.button.alignment = ConfigHandler.Instance.TagButtonTextAlignment;
            buttonPress |= GUILayout.Button(title);
            if (buttonPress)
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    if (selectedPartTag != null && selectedPartTag != tag)
                    {
                        selectedPartTagTransfer = tag;
                    }
                }
                else
                {
                    if (selectedPartTag == tag)
                    {
                        inputText = selectedPartTag.Name;
                    }
                    else
                    {
                        selectedPartTag = tag;
                    }
                    selectedPartTagTransfer = null;
                }
            }
            RegisterHelp("SelectTag");
            GUI.skin.button.alignment = oldAlignment;
            GUILayout.EndHorizontal();
            if (!tag.Collapsed)
            {
                foreach (PartTag subtag in tag.ChildTags)
                {
                    DrawTagButton(-1, TagIndentation + 1, subtag);
                }
            }
        }
        private void DrawCategoryButton()
        {
            if (editFilter)
            {
                if (editTags)
                {
                    if (editMods)
                    {
                        if (GUILayout.Button("Edit PartModules"))
                        {
                            editMods = false;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Edit Manufacturers"))
                        {
                            editTags = false;
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("Edit Mods"))
                    {
                        editTags = true;
                        editMods = true;
                    }
                }
            }
        }

        public bool isOpen { get { return display; } }

        public void Update()
        {
            if(display && Event.current.keyCode == KeyCode.Escape)
            {
                Close();
            }
        }
    }
}
