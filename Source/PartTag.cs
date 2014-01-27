﻿using System;
using System.Collections.Generic;
using KSP.IO;
using UnityEngine;

namespace PartCatalog
{
    public class PartTag
    {

        #region Members
        #region Basic Members
        public string Name = "";
        private bool visibleInSPH = true;

        public bool VisibleInSPH
        {
            get { return visibleInSPH; }
            set
            {
                visibleInSPH = value;
                GUIEditorControls.Instance.UpdateDisplayedTags();
            }
        }
        private bool visibleInVAB = true;

        public bool VisibleInVAB
        {
            get { return visibleInVAB; }
            set
            {
                visibleInVAB = value;
                GUIEditorControls.Instance.UpdateDisplayedTags();
            }
        }
        private bool startNewPage = false;

        public bool StartNewPage
        {
            get
            {
                if (Parent == PartCatalog.Instance.RootTag)
                {
                    return startNewPage;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                startNewPage = value;
                GUIEditorControls.Instance.UpdateDisplayedTags();
            }
        }
        public string IconName = "";
        public string IconOverlay = "";
        public int OverlayStyle = 0;
        public bool Collapsed = true;
        #endregion
        #region Parts
        public HashSet<AvailablePart> IncludedParts = new HashSet<AvailablePart>();
        public HashSet<AvailablePart> FilteredParts = new HashSet<AvailablePart>();
        public HashSet<AvailablePart> VisibleParts = new HashSet<AvailablePart>();
        #endregion
        #region PartData
        public HashSet<PartCategories> PartCategories = new HashSet<PartCategories>();
        public HashSet<PartCategories> FilteredCategories = new HashSet<PartCategories>();
        public HashSet<PartCategories> VisiblePartCategories = new HashSet<PartCategories>();
        #endregion
        private bool isResearched = false;
        public bool Researched
        {
            get
            {
                return isResearched;
            }
        }
        #region Child Parent Relation
        public LinkedList<PartTag> ChildTags = new LinkedList<PartTag>();
        public PartTag Parent = null;
        #endregion
        #endregion

        #region PartManipulation
        public void AddParts(HashSet<AvailablePart> toAdd)
        {
            toAdd.ExceptWith(IncludedParts);
            if (toAdd.Count > 0)
            {
                IncludedParts.UnionWith(toAdd);
                Rehash();
            }            
        }

        public void RemoveParts(HashSet<AvailablePart> toRemove)
        {
            if (IncludedParts.Overlaps(toRemove))
            {
                IncludedParts.ExceptWith(toRemove);
                Rehash();
            }
        }

        public void AddPart(AvailablePart toAdd, bool rehash = true)
        {
            if (!IncludedParts.Contains(toAdd))
            {
                IncludedParts.Add(toAdd);
                if (rehash)
                {
                    Rehash();
                }
            }
        }
        public void RemovePart(AvailablePart toRemove)
        {
            if (IncludedParts.Contains(toRemove))
            {
                IncludedParts.Remove(toRemove);
                Rehash();
            }
        }

        public void Rehash()
        {
            //Debug.Log("Rehash Tag " + Name);
            //Debug.Log("Rehashing Tag " + this.Name);            
            PartCategories.Clear();
            VisibleParts.Clear();
            VisiblePartCategories.Clear();

            bool newResearched = RehashParts();
          

            foreach (PartTag child in ChildTags)
            {
                VisibleParts.UnionWith(child.VisibleParts);
                VisiblePartCategories.UnionWith(child.VisiblePartCategories);

                newResearched |= child.Researched;
            }

            isResearched = newResearched;   
            if (Parent != null)
            {
                Parent.Rehash();
            }
            UpdateTagList();                                       
        }

        private bool RehashParts()
        {
            //Debug.Log(" Rehasing Parts");
            bool newResearched = false;
            foreach (AvailablePart part in IncludedParts)
            {
                //Debug.Log("  Checking Part "+ part.name);
                PartCategories.Add(part.category);
                if (!ConfigHandler.Instance.HideUnresearchedTags || (ResearchAndDevelopment.PartModelPurchased(part) && ResearchAndDevelopment.PartTechAvailable(part)))
                {
                    //Debug.Log("   Is Researched");
                    newResearched = true;
                    if (SearchManager.Instance.InFilter(part,this))
                    {
                        //Debug.Log("   Is in Filter");
                        FilteredParts.Add(part);
                        FilteredCategories.Add(part.category);
                    }
                }
            }
            if (Enabled/* && SearchManager.Instance.DisplayTag(this)*/ && SearchManager.Instance.InFilterRefresh(this))
            {
                //Debug.Log("Tag Is Enabled and filtered moving visibile stuff");
                VisiblePartCategories.UnionWith(FilteredCategories);
                VisibleParts.UnionWith(FilteredParts);                
            }

            return newResearched;
        }

        public void RehashDown()
        {
            //Debug.Log("Rehash Down Tag " + Name);
            PartCategories.Clear();            
            VisibleParts.Clear();
            VisiblePartCategories.Clear();
            FilteredParts.Clear();
            FilteredCategories.Clear();

            bool newResearched = RehashParts();
            


            foreach (PartTag child in ChildTags)
            {
                child.RehashDown();
                FilteredParts.UnionWith(child.FilteredParts);
                FilteredCategories.UnionWith(child.FilteredCategories);
                VisibleParts.UnionWith(child.VisibleParts);
                VisiblePartCategories.UnionWith(child.VisiblePartCategories);
                newResearched |= child.Researched;
            }

            isResearched = newResearched;

        }

        private static void UpdateTagList()
        {
            GUIEditorControls.Instance.UpdateDisplayedTags();
        }
        #endregion
        #region Child Parent Manipulation
        public bool ContainsChild(PartTag toCheck)
        {
            foreach (PartTag child in ChildTags)
            {
                if (child == toCheck)
                {
                    return true;
                }
                if (child.ContainsChild(toCheck))
                {
                    return true;
                }
            }
            return false;
        }

        public void AddChild(PartTag toAdd, PartTag addAfter = null)
        {            
            if (!ContainsChild(toAdd))
            {
                if (toAdd.Parent != null)
                {
                    toAdd.Parent.RemoveChild(toAdd);
                }
                toAdd.Parent = this;

                if (addAfter == null)
                {
                    ChildTags.AddLast(toAdd);
                }
                else
                {
                    LinkedListNode<PartTag> node = ChildTags.Find(addAfter);
                    if (node != null)
                    {
                        ChildTags.AddAfter(node, toAdd);
                    }
                    else
                    {
                        ChildTags.AddLast(toAdd);    
                    }
                }
                Rehash();
            }
        }

        public void RemoveChild(PartTag toRemove)
        {
            if (ChildTags.Contains(toRemove))
            {
                ChildTags.Remove(toRemove);
                toRemove.Parent = null;
                Rehash();
            }
        }

        public void MoveUp()
        {
            if (Parent != null)
            {
                LinkedListNode<PartTag> thisNode = Parent.ChildTags.Find(this);
                LinkedListNode<PartTag> prevNode = thisNode.Previous;
                if (prevNode != null)
                {
                    PartTag temp = thisNode.Value;
                    thisNode.Value = prevNode.Value;
                    prevNode.Value = temp;
                    UpdateTagList();
                }
            }
        }
        public void MoveDown()
        {
            if (Parent != null)
            {
                LinkedListNode<PartTag> thisNode = Parent.ChildTags.Find(this);
                LinkedListNode<PartTag> nextNode = thisNode.Next;
                if (nextNode != null)
                {
                    PartTag temp = thisNode.Value;
                    thisNode.Value = nextNode.Value;
                    nextNode.Value = temp;
                    UpdateTagList();
                }
            }
        }
        public void Indent()
        {
            if (Parent != null)
            {
                LinkedListNode<PartTag> thisNode = Parent.ChildTags.Find(this);
                if (thisNode != null && thisNode.Previous != null)
                {
                    thisNode.Previous.Value.AddChild(this);
                }                                   
            }
        }

        public void Unindent()
        {
            PartTag partent = Parent;
            if (partent != null)
            {
                PartTag parent2 = Parent.Parent;
                if (parent2 != null)
                {
                    partent.RemoveChild(this);
                    parent2.AddChild(this, partent);
                }
            }
        }

        public void Delete()
        {
            if (Parent != null)
            {
                Parent.RemoveChild(this);
            }
        }

        public void setCollapsedRecursive(bool collapsed)
        {
            Collapsed = collapsed;
            foreach (PartTag child in ChildTags)
            {
                child.setCollapsedRecursive(collapsed);        
            }
        }

        public PartTag findChild(string name)
        {
            foreach(var tag in ChildTags)
            {
                if(tag.Name == name)
                {
                    return tag;
                }
            }
            return null;
        }
        #endregion
        #region Persistance
        public void writeToFile(TextWriter file)
        {
            file.WriteLine("TAG " + Name);
            if (IconName != "")
            {
                file.WriteLine("ICON " + IconName);
            }
            if(IconOverlay != "")
            {
                file.WriteLine("OVERLAY " + IconOverlay);
            }
            file.WriteLine("SHOW SPH " + visibleInSPH.ToString());
            file.WriteLine("SHOW VAB " + visibleInVAB.ToString());
            if (startNewPage)
            {
                file.WriteLine("NEW PAGE");
            }
            foreach (AvailablePart part in IncludedParts)
            {
                file.WriteLine("ADD PART " + part.name);
            }
            foreach (PartTag child in ChildTags)
            {
                child.writeToFile(file);
            }
            file.WriteLine("END TAG");
        }
        #endregion

        public bool Enabled
        {
            get                                                                                                              
            {
                if (PartFilterManager.Instance.EnabledTags.Contains(this))
                {
                    return true;
                }
                if (Parent != null)
                {
                    return Parent.Enabled;
                }
                return PartFilterManager.Instance.EnabledTags.Count == 0;
            }
        }

        internal void AddAfter(PartTag newTag)
        {
            if (Parent != null)
            {
                Parent.AddChild(newTag, this);
            }
        }
    }
}
