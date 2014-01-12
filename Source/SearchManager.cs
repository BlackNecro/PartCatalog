using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PartCatalog
{
    class SearchManager
    {
        private SearchManager()
        {
        }

        private static SearchManager instance = new SearchManager();
        public static SearchManager Instance
        {
            get
            {
                return instance;
            }
        }

        private bool open = false;
        public bool IsFiltered
        {
            get
            {
                return searchText != "" && (IsFilteringParts || IsFilteringTags);
            }
        }

        public bool IsFilteringParts
        {
            get
            {
                return (searchNames || searchTitles || searchDescription);
            }
        }

        public bool IsFilteringTags
        {
            get
            {
                return searchTags;
            }
        }

        public void Open()
        {
            //Debug.Log("open " + Event.current.type.ToString());
            open = true;
            setFocus = true;
            GUIEditorControls.Instance.KillMouseOver();
            //GUI.FocusWindow(ConfigHandler.Instance.SearchWindow);            
        }

        public void Close()
        {
            //Debug.Log("close " + Event.current.type.ToString());
            open = false;
        }

        public void Toggle()
        {
            if (!open || GUIEditorControls.Instance.MouseOverVisible)
            {
                Open();
            }
            else
            {
                Close();
            }
        }

        private Rect windowPos = new Rect(0, 0, 0, 0);
        private string searchText = "";
        private string lastSearchText = "";
        private bool searchNames = true;
        private bool searchTitles = true;
        private bool searchDescription = true;
        private bool searchTags = true;
        private bool setFocus = false;

        Dictionary<AvailablePart, bool> FilteredParts = new Dictionary<AvailablePart, bool>();
        Dictionary<PartTag, bool> FilteredTags = new Dictionary<PartTag, bool>();
        Dictionary<PartTag, bool> DisplayedTags = new Dictionary<PartTag, bool>();

        public void Draw()
        {
            if (open && !GUIEditorControls.Instance.MouseOverVisible && !GUITagEditor.Instance.isOpen)
            {
                windowPos.x = GUIConstants.EditorPartListWidth;
                if (ConfigHandler.Instance.ToolBarPreset == ToolBarPositions.VerticalLeft)
                {
                    windowPos.x += ConfigHandler.Instance.ButtonSize.x;
                }
                windowPos.y = GUIConstants.EditorToolbarTop + GUIConstants.EditorToolbarHeight;
                windowPos = GUILayout.Window(ConfigHandler.Instance.SearchWindow, windowPos, DrawWindow, "Search");
            }
        }

        public void DrawWindow(int id)
        {
            GUILayout.BeginVertical();
            //GUILayout.Label(GUI.GetNameOfFocusedControl());

            UpdateWindow();
            GUI.SetNextControlName("PartCatalogSearchField");
            searchText = GUILayout.TextField(searchText);
            if (setFocus)
            {
                GUI.FocusControl("PartCatalogSearchField");
                setFocus = false;
            }
            // isFocussed = GUI.GetNameOfFocusedControl() == "PartCatalogSearchField";



            bool lastSearchNames = searchNames;
            bool lastSearchTitles = searchTitles;
            bool lastSearchTags = searchTags;
            bool lastSearchDescription = searchDescription;
            GUILayout.BeginHorizontal(GUILayout.MinWidth(125));

            searchNames = GUILayout.Toggle(searchNames, "", HighLogic.Skin.toggle);
            GUILayout.Label("ID");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            searchTitles = GUILayout.Toggle(searchTitles, "", HighLogic.Skin.toggle);
            GUILayout.Label("Name");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            searchDescription = GUILayout.Toggle(searchDescription, "", HighLogic.Skin.toggle);
            GUILayout.Label("Description");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            searchTags = GUILayout.Toggle(searchTags, "", HighLogic.Skin.toggle);
            GUILayout.Label("Tags");
            GUILayout.EndHorizontal();

            if (searchNames != lastSearchNames || searchTitles != lastSearchTitles || searchTags != lastSearchTags || searchDescription != lastSearchDescription || searchText != lastSearchText) //We fucking touched anything
            {
                if ((DateTime.Now - lastSearchTime).TotalMilliseconds > 250)
                {
                    lastSearchTime = DateTime.Now;
                    UpdateSearchText(searchText);
                }
            }


            GUILayout.EndVertical();
        }

        DateTime lastSearchTime = DateTime.Now;


        public void Refresh()
        {
            UpdateSearchText(searchText);
        }
        public void UpdateSearchText(string newSearchText)
        {
            ////Debug.LogError("Updating Search to " + newSearchText);
            searchText = lastSearchText = newSearchText;
            FilteredParts.Clear();
            FilteredTags.Clear();
            DisplayedTags.Clear();
            PartCatalog.Instance.RootTag.RehashDown(); //Updates visibility lists for all parts

            PartFilterManager.Instance.Rehash();
            GUIEditorControls.Instance.UpdateDisplayedTags(); //Updates the Tags in the toolbar
        }


        public void UpdateWindow()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (open)
                {
                    if (Event.current.keyCode == KeyCode.Escape)
                    {
                        Close();
                    }
                    if (Event.current.modifiers == EventModifiers.Control)
                    {
                        if (Event.current.keyCode == KeyCode.Space)
                        {
                            Event.current.Use();
                            Open();
                        }
                        else if (Event.current.keyCode == KeyCode.Alpha1)
                        {
                            searchNames = !searchNames;
                            Refresh();
                        }
                        else if (Event.current.keyCode == KeyCode.Alpha2)
                        {
                            searchTitles = !searchTitles;
                            Refresh();
                        }
                        else if (Event.current.keyCode == KeyCode.Alpha3)
                        {
                            searchDescription = !searchDescription;
                            Refresh();
                        }
                        else if (Event.current.keyCode == KeyCode.Alpha4)
                        {
                            searchTags = !searchTags;
                            Refresh();
                        }
                    }
                }
            }
        }

        public void OnUpdate()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.LeftControl))
                {
                    Open();
                }
            }
        }

        public bool InFilter(AvailablePart part, PartTag fromTag = null)
        {
            bool toReturn = false;

            if (FilteredParts.ContainsKey(part))
            {
                return FilteredParts[part];
            }
            ////Debug.LogWarning("Checking Part " + part.name);
            if (!IsFiltered)
            {
                toReturn = true;
            }
            else if (searchNames && FilterString(part.name, searchText))
            {
                //Debug.Log(" Name Matched");
                toReturn = true;
            }
            else if (searchTitles && FilterString(part.title, searchText))
            {
                //Debug.Log(" Title Matched");
                toReturn = true;
            }

            else if (searchDescription && FilterString(part.description, searchText))
            {
                //Debug.Log(" Description Matched");
                toReturn = true;
            }
            else if (fromTag != null)
            {
                toReturn = InFilterNoParts(fromTag);
            }

            //Debug.Log(" Returning "+ toReturn);
            return FilteredParts[part] = toReturn;
        }

        public bool InFilterRefresh(PartTag tag)
        {
            //Debug.LogWarning("Refreshing in filter for " + tag.Name);
            FilteredTags.Remove(tag);
            DisplayedTags.Remove(tag);

            bool toReturn = InFilter(tag);
            //Debug.Log("Was " + toReturn);
            return toReturn;
        }

        public bool InFilterNoParts(PartTag tag)
        {
            if (!IsFiltered)
            {
                return true;
            }

            if (FilteredTags.ContainsKey(tag))
            {
                return FilteredTags[tag];
            }

            //Debug.LogWarning("InFilter Tag " + tag.Name);

            if (searchTags)
            {
                if (FilterString(tag.Name, searchText))
                {
                    //Debug.Log(" Tag name match");
                    return FilteredTags[tag] = true;
                }
            }
            return false;
        }
        public bool InFilter(PartTag tag)
        {
            if (InFilterNoParts(tag))
            {
                return true;
            }

            if (IsFilteringParts)
            {
                foreach (var child in tag.IncludedParts)
                {
                    //Debug.Log(" checking childpart "+ child.name);
                    if (InFilter(child))
                    {
                        //Debug.Log("  in Filter");
                        return FilteredTags[tag] = true;
                    }
                }
            }

            return FilteredTags[tag] = false;
        }


        public bool DisplayTagUp(PartTag tag)
        {
            if (tag != null)
            {
                if (InFilter(tag))
                {
                    return true;
                }
                return DisplayTagUp(tag.Parent);
            }
            return false;
        }

        public bool DisplayTagDown(PartTag tag)
        {
            if (InFilter(tag))
            {
                return true;
            }
            foreach (var child in tag.ChildTags)
            {
                if (DisplayTagDown(child))
                {
                    return true;
                }
            }
            return false;
        }

        public bool DisplayTag(PartTag tag)
        {
            if (!IsFiltered)
            {
                return true;
            }

            if (DisplayedTags.ContainsKey(tag))
            {
                return DisplayedTags[tag];
            }

            if (InFilter(tag))
            {
                return DisplayedTags[tag] = true;
            }

            if ((searchDescription || searchTitles || searchNames) && tag.FilteredParts.Count > 0)
            {
                return true;
            }

            if (DisplayTagUp(tag.Parent))
            {
                return DisplayedTags[tag] = true;
            }

            foreach (var child in tag.ChildTags)
            {
                if (DisplayTagDown(child))
                {
                    return DisplayedTags[tag] = true;
                }
            }
            return DisplayedTags[tag] = false;
        }

        private bool FilterString(string property, string pattern)
        {
            return property.IndexOf(pattern, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}
