using System;
using System.Collections.Generic;
using KSP.IO;
using System.Linq;
using UnityEngine;

namespace PartCatalog
{
    public class PartCatalog : IDisposable
    {
        #region Members
        #region Instance
        static readonly PartCatalog instance = new PartCatalog();
        public static PartCatalog Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion
        #region Basic Members
        public PartTag RootTag = new PartTag();
        public LinkedList<PartTag> PartTags
        {
            get
            {
                return RootTag.ChildTags;
            }
        }

        public bool NewPartsAvailable = false;

        #endregion
        #region Catalog Members
        public List<AvailablePart> SortedPartList = new List<AvailablePart>();
        public Dictionary<string, AvailablePart> PartIndex = new Dictionary<string, AvailablePart>();

        public HashSet<AvailablePart> UnlistedParts = new HashSet<AvailablePart>();


        public SortedDictionary<string, HashSet<AvailablePart>> HashedManufacturerCatalog = new SortedDictionary<string, HashSet<AvailablePart>>();
        public SortedDictionary<string, HashSet<AvailablePart>> HashedModCatalog = new SortedDictionary<string, HashSet<AvailablePart>>();
        public Dictionary<AvailablePart, string> PartModIndex = new Dictionary<AvailablePart, string>();
        #endregion
        #endregion

        #region Constructors
        private PartCatalog()
        {
            BuildCatalogs();            
        }

        #endregion

        #region Catalog
        #region Building
        public void BuildCatalogs()
        {
            PartIndex.Clear();
            SortedPartList.Clear();
            HashedManufacturerCatalog.Clear();
            HashedModCatalog.Clear();
            PartModIndex.Clear();

            HashSet<string> lastParts = new HashSet<string>();
            if(KSP.IO.File.Exists<PartCatalog>("lastParts.txt"))
            {
                foreach(var part in KSP.IO.File.ReadAllLines<PartCatalog>("lastParts.txt"))
                {
                    lastParts.Add(part);
                }
            }
            KSP.IO.File.Delete<PartCatalog>("lastParts.txt");

            var FileWriter = KSP.IO.File.AppendText<PartCatalog>("lastParts.txt");

            foreach (AvailablePart part in PartLoader.Instance.parts)
            {
                SortedPartList.Add(part);
                PartIndex[part.name] = part;

                if (!HashedManufacturerCatalog.ContainsKey(part.manufacturer))
                {
                    HashedManufacturerCatalog[part.manufacturer] = new HashSet<AvailablePart>();
                }
                HashedManufacturerCatalog[part.manufacturer].Add(part);

                string mod = GetPartMod(part);
                if (!HashedModCatalog.ContainsKey(mod))
                {
                    HashedModCatalog[mod] = new HashSet<AvailablePart>();
                }
                HashedModCatalog[mod].Add(part);

                if(!lastParts.Contains(part.name))
                {
                    NewPartsAvailable = true;
                    lastParts.Add(part.name);
                }
                FileWriter.WriteLine(part.name);
            }
            FileWriter.Close();



            HashSet<string> modFolders = new HashSet<string>(System.IO.Directory.GetDirectories(GUIConstants.ModFolderPath));
            HashSet<string> detectedMods = new HashSet<string>(HashedModCatalog.Keys);
            HashSet<string> perfectMatches = new HashSet<string>(modFolders);

            perfectMatches.IntersectWith(detectedMods);
            modFolders.ExceptWith(perfectMatches);
            detectedMods.ExceptWith(perfectMatches);

            foreach (string folder in modFolders)
            {
                string match = null;
                foreach (string mod in detectedMods)
                {
                    if (mod != "" && folder.StartsWith(mod) && (match == null || match.Length < mod.Length))
                    {
                        match = mod;
                    }
                }
                if (match != null)
                {
                    detectedMods.Remove(match);
                    HashedModCatalog[folder] = HashedModCatalog[match];
                    HashedModCatalog.Remove(match);
                }
            }
            SortPartList();
        }
        public void SortPartList()
        {
            switch (ConfigHandler.Instance.SortNumber)
            {
                case 0:
                    SortedPartList = SortedPartList.OrderBy(x => x.name).Distinct().ToList();
                    break;
                case 1:
                    SortedPartList = SortedPartList.OrderBy(x => x.title).ThenBy(x => x.name).Distinct().ToList();
                    break;
                case 2:
                    SortedPartList = SortedPartList.OrderBy(x => x.manufacturer).ThenBy(x => x.title).ThenBy(x => x.name).Distinct().ToList();
                    break;
                case 3:
                    SortedPartList = SortedPartList.OrderBy(x => GetPartMod(x)).ThenBy(x => x.title).ThenBy(x => x.name).Distinct().ToList();
                    break;
            }
        }
        #endregion
        #region Accessing
        public string GetPartMod(AvailablePart part)
        {
            if (PartModIndex.ContainsKey(part))
            {
                return PartModIndex[part];
            }
            UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("PART");
            string mod = "";
            UrlDir.UrlConfig config = Array.Find<UrlDir.UrlConfig>(configs, (c => part.name == c.name.Replace('_', '.')));
            if (config != null)
            {
                var id = new UrlDir.UrlIdentifier(config.url);
                mod = id[0];
            }
            PartModIndex[part] = mod;
            return mod;
        }
        #endregion
        #endregion
        #region Persistance

        public void LoadPartTags()
        {          
            if (File.Exists<PartCatalog>("catalog.txt"))
            {
                RootTag = new PartTag();
                string[] lines = File.ReadAllLines<PartCatalog>("catalog.txt");
                uint lineNum = 0;
                PartTag curTag = RootTag;
                bool tagError = false;
                foreach (string line in lines)
                {
                    lineNum++;
                    string trimmed = line.Trim();
                    if (trimmed.Length > 0)
                    {
                        if (trimmed.StartsWith("TAG", StringComparison.OrdinalIgnoreCase))
                        {
                            string trimmedName = trimmed.Substring("TAG".Length).Trim();
                            if (trimmedName.Length > 0)
                            {
                                PartTag newTag = new PartTag();
                                newTag.Parent = curTag;
                                curTag = newTag;
                                curTag.Name = trimmedName;
                                if (curTag.Parent == null)
                                {
                                    RootTag.AddChild(curTag);
                                }
                                else
                                {
                                    curTag.Parent.AddChild(curTag);
                                }
                                tagError = false;
                            }
                            else
                            {
                                Debug.LogWarning(String.Format("PartCatalog WARNING Line {0}: TAG without proper name", lineNum));
                            }
                        }
                        else
                        {
                            if (curTag == null)
                            {
                                if (!tagError)
                                {
                                    tagError = true;
                                    Debug.LogError(String.Format("PartCatalog ERROR Line {0} parameters without started TAG", lineNum));
                                }
                            }
                            else
                            {
                                if (trimmed.StartsWith("NEW PAGE", StringComparison.OrdinalIgnoreCase))
                                {
                                    curTag.StartNewPage = true;
                                }
                                else if (trimmed.StartsWith("ICON", StringComparison.OrdinalIgnoreCase))
                                {
                                    string toParse = trimmed.Substring("ICON".Length).Trim();
                                    curTag.IconName = toParse;
                                    if (curTag.IconName.Length == 0)
                                    {
                                        Debug.LogError(String.Format("PartCatalog Error Line {0} malformed Icon Name \"{1}\"", lineNum, toParse));
                                    }
                                }
                                else if (trimmed.StartsWith("OVERLAY", StringComparison.OrdinalIgnoreCase))
                                {
                                    string toParse = trimmed.Substring("OVERLAY".Length).Trim();
                                    curTag.IconOverlay = toParse;
                                    if (curTag.IconOverlay.Length == 0)
                                    {
                                        Debug.LogError(String.Format("PartCatalog Error Line {0} malformed Overlay \"{1}\"", lineNum, toParse));
                                    }
                                }
                                else if (trimmed.StartsWith("SHOW SPH", StringComparison.OrdinalIgnoreCase))
                                {
                                    string toParse = trimmed.Substring("SHOW SPH".Length).Trim();
                                    bool newState = false;
                                    if (!bool.TryParse(toParse, out newState))
                                    {
                                        Debug.LogError(String.Format("PartCatalog Error Line {0} malformed boolean \"{1}\"", lineNum, toParse));
                                    }
                                    else
                                    {
                                        curTag.VisibleInSPH = newState;
                                    }
                                }
                                else if (trimmed.StartsWith("SHOW VAB", StringComparison.OrdinalIgnoreCase))
                                {
                                    string toParse = trimmed.Substring("SHOW VAB".Length).Trim();
                                    bool newState = false;
                                    if (!bool.TryParse(toParse, out newState))
                                    {
                                        Debug.LogError(String.Format("PartCatalog Error Line {0} malformed boolean \"{1}\"", lineNum, toParse));
                                    }
                                    else
                                    {
                                        curTag.VisibleInVAB = newState;
                                    }
                                }
                                else if (trimmed.StartsWith("ADD PART", StringComparison.OrdinalIgnoreCase))
                                {
                                    string partName = trimmed.Substring("ADD PART".Length).Trim();
                                    AvailablePart part = PartLoader.getPartInfoByName(partName);
                                    if (part == null)
                                    {
                                        Debug.LogError(String.Format("PartCatalog ERROR Line {0} Unknown Part {1}", lineNum, partName));
                                    }
                                    else
                                    {
                                        curTag.AddPart(part);
                                    }
                                }
                                else if (trimmed.StartsWith("END TAG", StringComparison.OrdinalIgnoreCase))
                                {
                                    curTag = curTag.Parent;
                                }
                                else
                                {
                                    Debug.LogError(string.Format("PartCatalog ERROR: Malformed Line {0}: {1}", lineNum, trimmed));
                                }
                            }
                        }
                    }
                }
            }
            Debug.Log("****Finished Loading Parts****");
        }

        public void SavePartTags()
        {
            TextWriter file = TextWriter.CreateForType<PartCatalog>("catalog.txt");
            foreach (PartTag tag in RootTag.ChildTags)
            {
                tag.writeToFile(file);
            }
            file.Flush();
            file.Close();
        }

        #endregion

        #region Page Management

        public LinkedList<PartTag> GetTagsForPage(int page, int maxNumPerPage, out bool nextPage)
        {
            LinkedList<PartTag> toReturn = new LinkedList<PartTag>();

            int curCount = 0;

            foreach (PartTag tag in RootTag.ChildTags)
            {
                if (tag.VisibleInVAB && HighLogic.LoadedScene == GameScenes.EDITOR || tag.VisibleInSPH && HighLogic.LoadedScene == GameScenes.SPH)
                {
                    if (!ConfigHandler.Instance.HideUnresearchedTags || tag.Researched)
                    {
                        if (SearchManager.Instance.DisplayTag(tag))
                        {
                            curCount++;
                            if (curCount == maxNumPerPage || tag.StartNewPage)
                            {
                                if (page == 0)
                                {
                                    break;
                                }
                                page--;
                                curCount = 0;
                            }
                            if (page == 0)
                            {
                                toReturn.AddLast(tag);
                            }
                        }
                    }
                }
            }

            if (toReturn.Count == 0)
            {
                nextPage = false;
                return toReturn;
            }

            LinkedListNode<PartTag> last = RootTag.ChildTags.FindLast(toReturn.Last.Value);
            nextPage = last != null && last.Next != null;
            return toReturn;
        }

        #endregion
        public void Dispose()
        {
            SavePartTags();
        }
    }
}
