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

        #endregion
        #region Catalog Members
        public List<AvailablePart> SortedPartList = new List<AvailablePart>();

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
            SortedPartList.Clear();
            HashedManufacturerCatalog.Clear();
            HashedModCatalog.Clear();
            PartModIndex.Clear();
            foreach (AvailablePart part in PartLoader.Instance.parts)
            {
                SortedPartList.Add(part);

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
            }


            HashSet<string> modFolders = new HashSet<string>(DirectoryLister.Instance.ListDirectories(GUIConstants.ModFolderPath));
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
                if (!ConfigHandler.Instance.HideUnresearchedTags || tag.Researched)
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


        internal void AutoTagByMod()
        {
            CreateAllPartTag();

            HashSet<PartTag> smallTags = new HashSet<PartTag>();
            

            foreach (KeyValuePair<string, HashSet<AvailablePart>> kv in HashedModCatalog)
            {
                if (kv.Key != "")
                {
                    HashSet<AvailablePart> toAdd = new HashSet<AvailablePart>();
                    foreach (AvailablePart modPart in kv.Value)
                    {
                        if (!ConfigHandler.Instance.AutotagOnlyUntagged || !RootTag.VisibleParts.Contains(modPart.title))
                        {
                            toAdd.Add(modPart);
                        }
                    }
                    if (toAdd.Count > 0)
                    {

                        PartTag existingTag = RootTag.ChildTags.FirstOrDefault(stag => stag.Name == kv.Key);
                        LinkedListNode<PartTag> node = null;
                        if (existingTag != null)
                        {
                            node = RootTag.ChildTags.Find(existingTag);
                        }
                        PartTag tag = new PartTag();
                        tag.Name = kv.Key;
                        if (ResourceProxy.Instance.IconExists(kv.Key))
                        {
                            tag.IconName = kv.Key;
                        }
                        tag.AddParts(toAdd);
                        if (tag.VisibleParts.Count < ConfigHandler.Instance.SmallModTagPartCount)
                        {
                            smallTags.Add(tag);
                        }
                        RootTag.AddChild(tag);
                        AutoGroupTag(tag);
                        if (existingTag != null && node != null)
                        {
                            RootTag.ChildTags.Remove(tag);
                            node.Value.Parent = null;
                            node.Value = tag;
                            if (tag.IconName == "")
                            {
                                tag.IconName = existingTag.IconName;
                            }
                        }
                    }
                }
            }
            List<PartTag> toDelete = new List<PartTag>();
            foreach (PartTag tag in RootTag.ChildTags)
            {
                if (tag.Name == "Small Mods")
                {
                    toDelete.Add(tag);
                }
            }
            toDelete.ForEach(tag => tag.Delete());
            if (smallTags.Count > 1)
            {
                PartTag smallTag = new PartTag();
                smallTag.Name = "Small Mods";
                smallTag.IconName = "SmallMods";
                foreach (var tag in smallTags)
                {
                    smallTag.AddChild(tag);
                }
                RootTag.AddChild(smallTag);     
            }

            RootTag.Rehash();
        }

        private void CreateAllPartTag()
        {
            List<PartTag> toDelete = new List<PartTag>();
            foreach (PartTag tag in RootTag.ChildTags)
            {
                if (tag.Name == "All")
                {
                    toDelete.Add(tag);
                }
            }
            toDelete.ForEach(tag => tag.Delete());
            PartTag allTag = new PartTag();
            allTag.Name = "All";
            allTag.IconName = "All";
            HashSet<AvailablePart> allParts = new HashSet<AvailablePart>();
            SortedPartList.ForEach(part => { if (part.category != PartCategories.none) allParts.Add(part); });
            allTag.AddParts(allParts);
            RootTag.AddChild(allTag);
            RootTag.ChildTags.Remove(allTag);
            RootTag.ChildTags.AddFirst(allTag);
            AutoGroupTag(allTag);
        }

        internal void AutoGroupTag(PartTag tag)
        {
            Dictionary<string, HashSet<AvailablePart>> categories = PartCategorizer.GetCategories(tag.IncludedParts);
            if (categories.Count > 0)
            {
                if (categories.ContainsKey("Pod"))
                {
                    PartTag pod = new PartTag();
                    pod.Name = "Pods";
                    if (categories.ContainsKey("MannedPod"))
                    {
                        PartTag mpod = new PartTag();
                        mpod.Name = "Manned";
                        mpod.AddParts(categories["MannedPod"]);
                        tag.RemoveParts(categories["MannedPod"]);
                        pod.AddChild(mpod);
                    }
                    if (categories.ContainsKey("UnmannedPod"))
                    {
                        PartTag upod = new PartTag();
                        upod.Name = "Unmanned";
                        upod.AddParts(categories["UnmannedPod"]);
                        tag.RemoveParts(categories["UnmannedPod"]);
                        pod.AddChild(upod);
                    }
                    if (categories.ContainsKey("Seat"))
                    {
                        PartTag seat = new PartTag();
                        seat.Name = "Seats";
                        seat.AddParts(categories["Seat"]);
                        tag.RemoveParts(categories["Seat"]);
                        pod.AddChild(seat);
                    }
                    tag.AddChild(pod);
                }
                if (categories.ContainsKey("Engine"))
                {
                    PartTag engines = new PartTag();
                    engines.Name = "Propulsion";
                    engines.AddParts(categories["Engine"]);
                    tag.RemoveParts(categories["Engine"]);
                    foreach (string cat in categories.Keys)
                    {
                        if (cat.StartsWith("EngineProp_"))
                        {
                            PartTag catEngine = new PartTag();
                            catEngine.Name = cat.Substring("EngineProp_".Length);
                            catEngine.AddParts(categories[cat]);
                            engines.RemoveParts(categories[cat]);
                            engines.AddChild(catEngine);
                        }
                    }
                    if (categories.ContainsKey("RCS"))
                    {
                        PartTag rcs = new PartTag();
                        rcs.Name = "RCS";
                        rcs.AddParts(categories["RCS"]);
                        tag.RemoveParts(categories["RCS"]);
                        engines.AddChild(rcs);
                    }
                    if (engines.IncludedParts.Count > 0)
                    {
                        PartTag other = new PartTag();
                        other.Name = "Other";
                        other.AddParts(engines.IncludedParts);
                        engines.RemoveParts(other.IncludedParts);
                        engines.AddChild(other);
                    }
                    tag.AddChild(engines);

                    engines.ChildTags = new LinkedList<PartTag>(engines.ChildTags.OrderBy(sorttag => sorttag.Name));
                }
                if (categories.ContainsKey("Storage"))
                {
                    PartTag storage = new PartTag();
                    storage.Name = "Storage";
                    storage.AddParts(categories["Storage"]);
                    tag.RemoveParts(categories["Storage"]);
                    foreach (string cat in categories.Keys)
                    {
                        if (cat.StartsWith("Storage_"))
                        {
                            PartTag catStore = new PartTag();
                            catStore.Name = cat.Substring("Storage_".Length);
                            catStore.AddParts(categories[cat]);
                            storage.RemoveParts(categories[cat]);
                            storage.AddChild(catStore);
                        }
                    }
                    storage.ChildTags = new LinkedList<PartTag>(storage.ChildTags.OrderBy(sorttag => sorttag.Name));
                    if (categories.ContainsKey("Transfer"))
                    {
                        PartTag transfer = new PartTag();
                        transfer.Name = "Transfer";
                        transfer.AddParts(categories["Transfer"]);
                        storage.RemoveParts(categories["Transfer"]);
                        storage.AddChild(transfer);
                    }
                    tag.AddChild(storage);
                    if (storage.IncludedParts.Count > 0)
                    {
                        PartTag other = new PartTag();
                        other.Name = "Other";
                        other.AddParts(storage.IncludedParts);
                        storage.RemoveParts(other.IncludedParts);
                        storage.AddChild(other);
                    }
                }
                if (categories.ContainsKey("Structural"))
                {
                    PartTag structural = new PartTag();
                    structural.Name = "Structural";
                    structural.AddParts(categories["Structural"]);
                    tag.RemoveParts(categories["Structural"]);
                    tag.AddChild(structural);
                    if (categories.ContainsKey("Structure"))
                    {
                        PartTag structure = new PartTag();
                        structure.Name = "Main";
                        structure.AddParts(categories["Structure"]);
                        structural.RemoveParts(categories["Structure"]);
                        structural.AddChild(structure);
                    }
                    if (categories.ContainsKey("Decoupler"))
                    {
                        PartTag decoupler = new PartTag();
                        decoupler.Name = "Decoupler";
                        decoupler.AddParts(categories["Decoupler"]);
                        structural.RemoveParts(categories["Decoupler"]);
                        structural.AddChild(decoupler);
                    }

                    if (structural.IncludedParts.Count > 0)
                    {
                        PartTag other = new PartTag();
                        other.Name = "Other";
                        other.AddParts(structural.IncludedParts);
                        structural.RemoveParts(other.IncludedParts);
                        structural.AddChild(other);
                    }
                }
                if (categories.ContainsKey("Aero"))
                {
                    PartTag aero = new PartTag();
                    aero.Name = "Aero";
                    aero.AddParts(categories["Aero"]);
                    tag.RemoveParts(categories["Aero"]);
                    if (categories.ContainsKey("ControlSurface"))
                    {
                        PartTag controlsurface = new PartTag();
                        controlsurface.Name = "Control Surfaces";
                        controlsurface.AddParts(categories["ControlSurface"]);
                        aero.RemoveParts(categories["ControlSurface"]);
                        aero.AddChild(controlsurface);
                    }
                    if (categories.ContainsKey("Winglet"))
                    {
                        PartTag winglet = new PartTag();
                        winglet.Name = "Winglets";
                        winglet.AddParts(categories["Winglet"]);
                        aero.RemoveParts(categories["Winglet"]);
                        aero.AddChild(winglet);
                    }
                    if (categories.ContainsKey("Intake"))
                    {
                        PartTag intake = new PartTag();
                        intake.Name = "Intakes";
                        intake.AddParts(categories["Intake"]);
                        aero.RemoveParts(categories["Intake"]);
                        aero.AddChild(intake);
                    }
                    if (aero.IncludedParts.Count > 0)
                    {
                        PartTag other = new PartTag();
                        other.Name = "Other";
                        other.AddParts(aero.IncludedParts);
                        aero.RemoveParts(other.IncludedParts);
                        aero.AddChild(other);
                    }
                    tag.AddChild(aero);
                }
                if (categories.ContainsKey("Utility"))
                {
                    PartTag utils = new PartTag();
                    utils.Name = "Utility";
                    utils.AddParts(categories["Utility"]);
                    tag.RemoveParts(categories["Utility"]);
                    if (categories.ContainsKey("Docking"))
                    {
                        PartTag docking = new PartTag();
                        docking.Name = "Docking";
                        docking.AddParts(categories["Docking"]);
                        utils.RemoveParts(categories["Docking"]);
                        utils.AddChild(docking);
                    }
                    if (categories.ContainsKey("Generator"))
                    {
                        PartTag generator = new PartTag();
                        generator.Name = "Generators";
                        generator.AddParts(categories["Generator"]);
                        utils.RemoveParts(categories["Generator"]);
                        if (categories.ContainsKey("Solar Panel"))
                        {
                            PartTag solarpanels = new PartTag();
                            solarpanels.Name = "Solar Panels";
                            solarpanels.AddParts(categories["Solar Panel"]);
                            generator.RemoveParts(categories["Solar Panel"]);
                            generator.AddChild(solarpanels);
                        }
                        if (generator.IncludedParts.Count > 0)
                        {
                            PartTag other = new PartTag();
                            other.Name = "Other";
                            other.AddParts(generator.IncludedParts);
                            generator.RemoveParts(other.IncludedParts);
                            generator.AddChild(other);
                        }
                        utils.AddChild(generator);
                    }
                    if (categories.ContainsKey("LandingAparatus"))
                    {
                        PartTag landing = new PartTag();
                        landing.Name = "Landing";
                        landing.AddParts(categories["LandingAparatus"]);
                        utils.RemoveParts(categories["LandingAparatus"]);

                        if (categories.ContainsKey("Parachute"))
                        {
                            PartTag parachutes = new PartTag();
                            parachutes.Name = "Parachutes";
                            parachutes.AddParts(categories["Parachute"]);
                            landing.RemoveParts(categories["Parachute"]);
                            landing.AddChild(parachutes);
                        }
                        if (categories.ContainsKey("Wheel"))
                        {
                            PartTag wheels = new PartTag();
                            wheels.Name = "Wheels";
                            wheels.AddParts(categories["Wheel"]);
                            landing.RemoveParts(categories["Wheel"]);
                            landing.AddChild(wheels);
                        }
                        if (categories.ContainsKey("Landing Gear"))
                        {
                            PartTag gear = new PartTag();
                            gear.Name = "Landing Gears";
                            gear.AddParts(categories["Landing Gear"]);
                            landing.RemoveParts(categories["Landing Gear"]);
                            landing.AddChild(gear);
                        }
                        if (categories.ContainsKey("Landing Leg"))
                        {
                            PartTag leg = new PartTag();
                            leg.Name = "Landing Legs";
                            leg.AddParts(categories["Landing Leg"]);
                            landing.RemoveParts(categories["Landing Leg"]);
                            landing.AddChild(leg);
                        }
                        utils.AddChild(landing);
                    }
                    if (categories.ContainsKey("Light"))
                    {
                        PartTag light = new PartTag();
                        light.Name = "Light";
                        light.AddParts(categories["Light"]);
                        utils.RemoveParts(categories["Light"]);
                        utils.AddChild(light);
                    }
                    if (categories.ContainsKey("SAS"))
                    {
                        PartTag sas = new PartTag();
                        sas.Name = "SAS";
                        sas.AddParts(categories["SAS"]);
                        utils.RemoveParts(categories["SAS"]);
                        utils.AddChild(sas);
                    }
                    if (categories.ContainsKey("Control"))
                    {
                        PartTag control = new PartTag();
                        control.Name = "Control";
                        control.AddParts(categories["Control"]);
                        utils.RemoveParts(categories["Control"]);
                        utils.AddChild(control);
                    }
                    if (categories.ContainsKey("Ladder"))
                    {
                        PartTag ladders = new PartTag();
                        ladders.Name = "Ladders";
                        ladders.AddParts(categories["Ladder"]);
                        utils.RemoveParts(categories["Ladder"]);
                        utils.AddChild(ladders);
                    }
                    if (utils.IncludedParts.Count > 0)
                    {
                        PartTag other = new PartTag();
                        other.Name = "Other";
                        other.AddParts(utils.IncludedParts);
                        utils.RemoveParts(other.IncludedParts);
                        utils.AddChild(other);
                    }
                    tag.AddChild(utils);
                }
                if (categories.ContainsKey("Science"))
                {
                    PartTag science = new PartTag();
                    science.Name = "Science";
                    science.AddParts(categories["Science"]);
                    tag.RemoveParts(categories["Science"]);

                    if (categories.ContainsKey("Sensor"))
                    {
                        PartTag sensors = new PartTag();
                        sensors.Name = "Sensors";
                        sensors.AddParts(categories["Sensor"]);
                        science.RemoveParts(categories["Sensor"]);
                        science.AddChild(sensors);
                    }
                    if (categories.ContainsKey("Antenna"))
                    {
                        PartTag antenna = new PartTag();
                        antenna.Name = "Antennas";
                        antenna.AddParts(categories["Antenna"]);
                        science.RemoveParts(categories["Antenna"]);
                        science.AddChild(antenna);
                    }
                    if (science.IncludedParts.Count > 0)
                    {
                        PartTag other = new PartTag();
                        other.Name = "Other";
                        other.AddParts(science.IncludedParts);
                        science.RemoveParts(other.IncludedParts);
                        science.AddChild(other);
                    }
                    tag.AddChild(science);
                }
                if (tag.IncludedParts.Count > 0)
                {
                    PartTag ungrouped = new PartTag();
                    ungrouped.Name = "Other";
                    ungrouped.AddParts(tag.IncludedParts);
                    tag.RemoveParts(ungrouped.IncludedParts);
                    tag.AddChild(ungrouped);
                }

            }
        }

        public void Dispose()
        {
            SavePartTags();
        }
    }
}
