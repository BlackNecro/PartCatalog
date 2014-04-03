using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PartCatalog
{
    class PartFilterManager
    {

        #region Members
        #region Instance
        static readonly PartFilterManager instance = new PartFilterManager();
        public static PartFilterManager Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion
        #region Basic Members
        public HashSet<PartTag> IncludeTags = new HashSet<PartTag>();
        public HashSet<PartTag> ExcludeTags = new HashSet<PartTag>();
        public bool InvertFilter = false;
        #endregion
        #region Performance Hashed
        public HashSet<AvailablePart> DisplayedParts = new HashSet<AvailablePart>();
        HashSet<PartCategories> DisplayedCategories = new HashSet<PartCategories>();
        #endregion
        #endregion

        #region Constructor
        private PartFilterManager()
        {
        }

        public void EnablePartFilter()
        {
            EditorPartList.Instance.ExcludeFilters.RemoveFilter("PartCatalogFilter");
            EditorPartListFilter PartFilter = new EditorPartListFilter("PartCatalogFilter", FilterPart);
            EditorPartList.Instance.ExcludeFilters.AddFilter(PartFilter);
        }
        #endregion

        #region FilterFunction
        private bool FilterPart(AvailablePart toFilter)
        {

            if (ConfigHandler.Instance.DisplayAllOnEmptyFilter && DisplayedParts.Count == 0)
            {
                return true;
            }

            return DisplayedParts.Contains(toFilter) ^ ConfigHandler.Instance.InvertFilter;

        }
        #endregion

        #region FilterManipulation
        #region Enabled Manipulation
        public void AddIncludeFilter(PartTag toAdd)
        {
            if (!IncludeTags.Contains(toAdd))
            {
                IncludeTags.Add(toAdd);
                Rehash();
            }

        }
        public void RemoveIncludeFilter(PartTag toRemove)
        {
            if (IncludeTags.Contains(toRemove))
            {
                IncludeTags.Remove(toRemove);
                Rehash();
            }
        }
        public void AddExcludeFilter(PartTag toAdd)
        {
            if (!ExcludeTags.Contains(toAdd))
            {
                ExcludeTags.Add(toAdd);
                Rehash();
            }

        }
        public void RemoveExcludeFilter(PartTag toRemove)
        {
            if (ExcludeTags.Contains(toRemove))
            {
                ExcludeTags.Remove(toRemove);
                Rehash();
            }
        }

        public void ToggleFilter(PartTag toToggle)
        {
            if (Event.current.button == 0) //Add Enabled filter
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    if (IncludeTags.Contains(toToggle))
                    {
                        RemoveIncludeFilter(toToggle);
                    }
                    else
                    {
                        AddIncludeFilter(toToggle);
                    }
                }
                else
                {
                    bool add = !IncludeTags.Contains(toToggle);
                    IncludeTags.Clear();
                    ExcludeTags.Clear();
                    if (add)
                    {
                        IncludeTags.Add(toToggle);
                    }
                    Rehash();
                }
            }
            else if (Event.current.button == 1) //Add Disabled filter
            {
                if (ExcludeTags.Contains(toToggle))
                {
                    RemoveExcludeFilter(toToggle);
                }
                else
                {
                    AddExcludeFilter(toToggle);
                }
            }
            SearchManager.Instance.Refresh();
        }

        public bool CategoryEnabled(PartCategories cat)
        {
            if (DisplayedCategories.Count == 0)
            {
                return true;
            }
            return DisplayedCategories.Contains(cat);
        }
        #endregion
        #region Rehashing

        public void Rehash()
        {

            DisplayedParts.Clear();
            DisplayedCategories.Clear();

            if (IncludeTags.Count == 0)
            {
                DisplayedParts.UnionWith(PartCatalog.Instance.RootTag.VisibleParts);
            }
            else
            {
                foreach (PartTag tag in IncludeTags)
                {
                    DisplayedParts.UnionWith(tag.VisibleParts);
                }
            }
            foreach (PartTag tag in ExcludeTags)
            {
                DisplayedParts.ExceptWith(tag.VisibleParts);
            }
            foreach (var part in DisplayedParts)
            {
                DisplayedCategories.Add(part.category);
            }

            if (DisplayedCategories.Count == 0 && ConfigHandler.Instance.DisplayAllOnEmptyFilter)
            {
                EditorPartList.Instance.ShowTabs();
            }

            else
            {
                PartCategories selectedCategory = (PartCategories)EditorPartList.Instance.categorySelected;
                EditorPartList.Instance.HideTabs();
                EditorPartList.Instance.ShowTab(EditorPartList.Instance.tabs.Length - 1);
                foreach (PartCategories category in DisplayedCategories)
                {

                    if (category != PartCategories.none)
                    {
                        if (!DisplayedCategories.Contains((PartCategories)EditorPartList.Instance.categorySelected))
                        {
                            EditorPartList.Instance.ForceSelectTab(category);
                            selectedCategory = category;
                        }
                        EditorPartList.Instance.ShowTab(category);
                    }
                }
            }
            EditorPartList.Instance.Refresh();
        }
        #endregion
        #endregion

        public void Update()
        {
            EditorPartList.Instance.ShowTabs();

            foreach (PartCategories enumVal in Enum.GetValues(typeof(PartCategories)))
            {
                if (enumVal != PartCategories.none)
                {
                    if (!DisplayedCategories.Contains(enumVal))
                    {
                        EditorPartList.Instance.HideTab((int)enumVal);
                    }
                }
            }
        }
    }
}
