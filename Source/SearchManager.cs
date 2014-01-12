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
                return searchText != "";
            }
        }

        public void Open()
        {
            Debug.Log("open " + Event.current.type.ToString());
            open = true;
            setFocus = true;
            //GUI.FocusWindow(ConfigHandler.Instance.SearchWindow);            
        }

        public void Close()
        {
            Debug.Log("close " + Event.current.type.ToString());
            open = false;
        }

        public void Toggle()
        {
            if (open)
            {
                Close();
            }
            else
            {
                Open();
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

        Dictionary<string, bool> FilteredParts = new Dictionary<string, bool>();
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
                UpdateSearchText(searchText);
            }


            GUILayout.EndVertical();
        }

        private void UpdateSearchText(string newSearchText)
        {
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
                if (open && Event.current.keyCode == KeyCode.Escape)
                {
                    Close();
                }
                if (open && Event.current.keyCode == KeyCode.Space && (Event.current.modifiers == EventModifiers.Control))
                {
                    Event.current.Use();
                    Open();
                }
            }
        }

        public void Update()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.LeftControl))
                {
                    Open();
                }
            }
        }

        public bool InFilter(AvailablePart part)
        {
            bool toReturn = false;

            if (FilteredParts.ContainsKey(part.name))
            {
                return true;
            }
            else if (searchText == "")
            {
                toReturn = true;
            }
            else if (searchNames && FilterString(part.name, searchText))
            {
                toReturn = true;
            }
            else if (searchTitles && FilterString(part.title, searchText))
            {
                toReturn = true;
            }

            else if (searchDescription && FilterString(part.description, searchText))
            {
                toReturn = true;
            }

            return FilteredParts[part.name] = toReturn;
        }
        public bool InFilter(PartTag tag)
        {

            if (searchText == "")
            {
                return true;
            }

            if (FilteredTags.ContainsKey(tag))
            {
                return FilteredTags[tag];
            }

            if (searchTags)
            {
                if (FilterString(tag.Name, searchText))
                {
                    return FilteredTags[tag] = true;
                }
                else
                {
                    return FilteredTags[tag] = false;
                }

            }
            if (searchNames | searchTitles | searchDescription)
            {
                foreach (var child in tag.ChildTags)
                {
                    if (InFilter(child))
                    {
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

            if (DisplayedTags.ContainsKey(tag))
            {
                return DisplayedTags[tag];
            }

            if (InFilter(tag))
            {
                return DisplayedTags[tag] = true;
            }

            if(tag.VisibleParts.Count > 0)
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
