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
        public HashSet<PartTag> EnabledTags = new HashSet<PartTag>();
        HashSet<PartCategories> EnabledCategories = new HashSet<PartCategories>();
        public bool InvertFilter = false;
        #endregion
        #region Performance Hashed
        public HashSet<string> HashedEnabledPartNames = new HashSet<string>();
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
            
            if (ConfigHandler.Instance.DisplayAllOnEmptyFilter && HashedEnabledPartNames.Count == 0)
            {
                return true;
            }

            return HashedEnabledPartNames.Contains(toFilter.name) ^ ConfigHandler.Instance.InvertFilter;
            
        }
        #endregion

        #region FilterManipulation
        #region Enabled Manipulation
        public void AddFilter(PartTag toAdd)
        {
            if (!EnabledTags.Contains(toAdd))
            {
                EnabledTags.Add(toAdd);
                Rehash();
            }

        }
        public void RemoveFilter(PartTag toRemove)
        {
            if (EnabledTags.Contains(toRemove))
            {
                EnabledTags.Remove(toRemove);
                Rehash();
            }
        }

        public void ToggleFilter(PartTag toToggle)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (EnabledTags.Contains(toToggle))
                {
                    RemoveFilter(toToggle);
                }
                else
                {
                    AddFilter(toToggle);
                }
            }
            else
            {
                bool add = !EnabledTags.Contains(toToggle);
                EnabledTags.Clear();
                if (add)
                {
                    EnabledTags.Add(toToggle);
                }
                Rehash();
            }
        }

        public bool CategoryEnabled(PartCategories cat)
        {
            if (EnabledCategories.Count == 0)
            {
                return true;
            }
            return EnabledCategories.Contains(cat);
        }
        #endregion
        #region Rehashing

        public void Rehash()
        {

            HashedEnabledPartNames.Clear();
            EnabledCategories.Clear();            
            
            if (EnabledTags.Count == 0)
            {
                EnabledCategories.UnionWith(PartCatalog.Instance.RootTag.VisiblePartCategories);
                HashedEnabledPartNames.UnionWith(PartCatalog.Instance.RootTag.VisibleParts);
            }
            else
            {
                bool firstRound = true;
                foreach (PartTag tag in EnabledTags)
                {
                    if (firstRound || ConfigHandler.Instance.UnionFilter)
                    {
                        firstRound = false;
                        HashedEnabledPartNames.UnionWith(tag.VisibleParts);
                        EnabledCategories.UnionWith(tag.VisiblePartCategories);
                    }
                    else
                    {
                        HashedEnabledPartNames.IntersectWith(tag.VisibleParts);
                        EnabledCategories.IntersectWith(tag.VisiblePartCategories);
                    }
                }
            }
            if (EnabledCategories.Count == 0)
            {
                EditorPartList.Instance.ShowTabs();
            }
            else
            {
                PartCategories selectedCategory = (PartCategories)EditorPartList.Instance.categorySelected;
                EditorPartList.Instance.HideTabs();
                EditorPartList.Instance.ShowTab(EditorPartList.Instance.tabs.Length - 1);
                foreach (PartCategories category in EnabledCategories)
                {

                    if (category != PartCategories.none)
                    {
                        if (!EnabledCategories.Contains((PartCategories)EditorPartList.Instance.categorySelected))
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
        public void RehashFrom(PartTag tag)
        {
            if (EnabledTags.Contains(tag))
            {
                Rehash();
            }
        }
        #endregion
        #endregion
    }
}
