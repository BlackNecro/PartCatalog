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

            if (ExcludeTags.Contains(toAdd))
            {
                RemoveExcludeFilter(toAdd);
            } else if(toAdd.ExcludedFromFilter)
            {
                return;
            }
            if (!IncludeTags.Contains(toAdd))
            {
                IncludeTags.Add(toAdd);
            }

        }
        public void RemoveIncludeFilter(PartTag toRemove)
        {
            if (IncludeTags.Contains(toRemove))
            {
                IncludeTags.Remove(toRemove);
            }
        }
        public void AddExcludeFilter(PartTag toAdd)
        {
            if (IncludeTags.Contains(toAdd))
            {
                RemoveIncludeFilter(toAdd);
            }
            if (!ExcludeTags.Contains(toAdd))
            {
                ExcludeTags.Add(toAdd);
            }

        }
        public void RemoveExcludeFilter(PartTag toRemove)
        {
            if (ExcludeTags.Contains(toRemove))
            {
                ExcludeTags.Remove(toRemove);
            }
        }

        private PartTag lastToggledFilter = null;
        private bool lastToggleAdded = false;

        public void PartTagToggleClick(PartTag toToggle)
        {
            var modifiedTags = new HashSet<PartTag>();

            if (!toToggle.Parent.IsRoot && Input.GetKey(KeyCode.LeftShift))
            {
                if (lastToggledFilter != null && lastToggledFilter != toToggle && lastToggledFilter.Parent == toToggle.Parent)
                {
                    PartTag parent = lastToggledFilter.Parent;
                    bool found = false;
                    bool forward = false;
                    foreach (var node in parent.ChildTags)
                    {
                        if (node == lastToggledFilter)
                        {
                            if (found)
                            {
                                forward = false;
                                break;
                            }
                            found = true;
                        }
                        else if (node == toToggle)
                        {
                            if (found)
                            {
                                forward = true;
                                break;
                            }
                            found = true;
                        }
                    }
                    LinkedListNode<PartTag> listNode, endNode;
                    if (forward)
                    {
                        listNode = parent.ChildTags.Find(lastToggledFilter);
                        endNode = parent.ChildTags.Find(toToggle);
                    }
                    else
                    {
                        listNode = parent.ChildTags.Find(toToggle);
                        endNode = parent.ChildTags.Find(lastToggledFilter);
                    }

                    while (listNode != null)
                    {
                        if (lastToggleAdded)
                        {
                            AddTag(listNode.Value);
                        }
                        else
                        {
                            RemoveTag(listNode.Value);
                        }
                        modifiedTags.Add(listNode.Value);
                        if (listNode == endNode)
                        {
                            break;
                        }
                        listNode = listNode.Next;
                    }
                }
            }
            else
            {
                ToggleTag(toToggle);
                modifiedTags.Add(toToggle);
            }

            if (Input.GetKey(KeyCode.LeftAlt))
            {

                if (toToggle.Parent != null)
                {
                    HashSet<PartTag> oldModified = new HashSet<PartTag>(modifiedTags);
                    modifiedTags.Clear();

                    bool addRemove = lastToggleAdded;
                    bool first = true;

                    foreach (var tag in toToggle.Parent.ChildTags)
                    {
                        if (first)
                        {
                            first = false;
                            ToggleTag(tag);
                            addRemove = lastToggleAdded;
                        }
                        else
                        {
                            if (addRemove)
                            {
                                AddTag(tag);
                            }
                            else
                            {
                                RemoveTag(tag);
                            }
                        }

                    }
                }

            }
            else
            {

                if (!Input.GetKey(KeyCode.LeftControl))
                {
                    IncludeTags.IntersectWith(modifiedTags);
                    ExcludeTags.IntersectWith(modifiedTags);
                }
            }

            Rehash();
            SearchManager.Instance.Refresh();
            lastToggledFilter = toToggle;
        }

        private void ToggleTag(PartTag toToggle)
        {
            if (Event.current.button == 0) //Add Enabled filter
            {
                ToggleIncludeTag(toToggle);
            }
            else if (Event.current.button == 1) //Add Disabled filter
            {
                ToggleExcludeTag(toToggle);
            }
        }

        private void AddTag(PartTag toToggle)
        {
            if (Event.current.button == 0) //Add Enabled filter
            {
                AddIncludeFilter(toToggle);
            }
            else if (Event.current.button == 1) //Add Disabled filter
            {
                AddExcludeFilter(toToggle);
            }
        }

        private void RemoveTag(PartTag toToggle)
        {
            if (Event.current.button == 0) //Add Enabled filter
            {
                RemoveIncludeFilter(toToggle);
            }
            else if (Event.current.button == 1) //Add Disabled filter
            {
                RemoveExcludeFilter(toToggle);
            }
        }

        private void ToggleExcludeTag(PartTag toToggle)
        {
            if (ExcludeTags.Contains(toToggle))
            {
                RemoveExcludeFilter(toToggle);
                lastToggleAdded = false;
            }
            else
            {
                AddExcludeFilter(toToggle);
                lastToggleAdded = true;
            }
        }

        private void ToggleIncludeTag(PartTag toToggle)
        {
            if (ExcludeTags.Contains(toToggle))
            {
                RemoveExcludeFilter(toToggle);
            }
            if (IncludeTags.Contains(toToggle))
            {
                RemoveIncludeFilter(toToggle);
                lastToggleAdded = false;
            }
            else
            {
                AddIncludeFilter(toToggle);
                lastToggleAdded = true;
            }
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
