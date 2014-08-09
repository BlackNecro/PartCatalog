using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSPAchievements;
using NLua;
using KSPLua;
using UnityEngine;

namespace PartCatalog
{
    class LuaRuleHandler
    {
        Lua luaInstance = new Lua();

        private Dictionary<string, HashSet<AvailablePart>> Categories = new Dictionary<string, HashSet<AvailablePart>>();
        public Dictionary<string, HashSet<AvailablePart>> GetCategoriesForTag(PartTag limitTo)
        {
            Dictionary<string, HashSet<AvailablePart>> toReturn = new Dictionary<string, HashSet<AvailablePart>>();
            foreach (var kv in Categories)
            {
                HashSet<AvailablePart> parts = new HashSet<AvailablePart>(kv.Value);
                parts.IntersectWith(limitTo.IncludedParts);
                toReturn[kv.Key] = parts;
            }
            return toReturn;
        }

        private LuaRuleHandler()
        {            
            SetupNamespace();
        }
        private static LuaRuleHandler instance = new LuaRuleHandler();
        public static LuaRuleHandler Instance
        {
            get
            {
                return instance;
            }
        }

        public void ParseParts()
        {
            SetupNamespace();
            foreach (string file in System.IO.Directory.GetFiles(GUIConstants.CatalogDataPath))
            {
                if (file.EndsWith(".rule.lua"))
                {
                    string input = KSP.IO.File.ReadAllText<PartCatalog>(file);
                    luaInstance.DoString(input);
                }
            }

            foreach (string file in System.IO.Directory.GetFiles(GUIConstants.CatalogDataPath))
            {
                if (file.EndsWith(".fallback.lua"))
                {
                    string input = KSP.IO.File.ReadAllText<PartCatalog>(file);
                    luaInstance.DoString(input);
                }
            }

            luaInstance.DoString(@"clean(CATEGORIES)
sortCat(CATEGORIES)");

            Categories.Clear();

            LuaTable category = (LuaTable)luaInstance["CATEGORIES"];
            LuaTable children = (LuaTable)category["children"];
            //PartCatalog.Instance.RootTag = new PartTag();
            HashSet<AvailablePart> leftOvers = new HashSet<AvailablePart>();
            PartCatalog.Instance.SortedPartList.ForEach((part) => { if (part.category != PartCategories.none) leftOvers.Add(part); });
            foreach (var value in children.Values)
            {
                if (value is LuaTable)
                {
                    ParseCategoryTable((LuaTable)value, PartCatalog.Instance.RootTag, leftOvers);
                }
            }

            PartTag smallMods = PartCatalog.Instance.RootTag.findChild("Small Mods");
            if(smallMods != null)
            {
                smallMods.Delete();
            }
            smallMods = new PartTag();
            smallMods.Name = "Small Mods";
            smallMods.IconName = "SmallMods"; 
            
            foreach(var tag in PartCatalog.Instance.RootTag.ChildTags)
            {
                if (tag.FullPartCount < ConfigHandler.Instance.SmallModTagPartCount)
                {
                    smallMods.ChildTags.AddLast(tag);
                }
            }
            if (smallMods.ChildTags.Count > 0)
            {
                foreach (var tag in smallMods.ChildTags)
                {
                    tag.Parent.RemoveChild(tag);
                    tag.Parent = smallMods;
                }
                smallMods.Rehash();
                PartCatalog.Instance.RootTag.AddChild(smallMods);
            }

            if (leftOvers.Count > 0)
            {
                PartTag leftOver = PartCatalog.Instance.RootTag.findChild("Uncategorized");
                if (leftOver == null)
                {
                    leftOver = new PartTag();
                    leftOver.Name = "Uncategorized";
                    leftOver.IconOverlay = "U";
                }
                foreach (var part in leftOvers)
                {
                    leftOver.AddPart(part);
                }

                PartCatalog.Instance.RootTag.AddChild(leftOver);
            }

            PartTag noConfigTag = PartCatalog.Instance.RootTag.findChild("NoConfigNode");
            if(noConfigTag != null)
            {
                noConfigTag.Delete();
            }
            if (PartCatalog.Instance.UnlistedParts.Count > 0)
            {
                noConfigTag = new PartTag();
                noConfigTag.Name = "ERROR: Parts without DataBase Config";
                noConfigTag.IconOverlay = "ERR";
                noConfigTag.AddParts(PartCatalog.Instance.UnlistedParts);
                PartCatalog.Instance.RootTag.AddChild(noConfigTag);
            }


            SearchManager.Instance.Refresh();

        }

        private static void ParseCategoryTable(LuaTable category, PartTag parent, HashSet<AvailablePart> leftOvers)
        {
            if (category != null)
            {
                PartTag newTag = parent.findChild((string)category["title"].ToString());
                if (newTag == null)
                {
                    newTag = new PartTag();
                    newTag.Name = (string)category["title"].ToString();
                    newTag.IconName = ((string)category["icon"].ToString());
                    if (newTag.IconName != "")
                    {
                        if (!ResourceProxy.Instance.IconExists(newTag.IconName))
                        {
                            newTag.IconName = "";
                        }
                    }
                    newTag.IconOverlay = (string)category["overlay"].ToString();
                }

                if (String.IsNullOrEmpty(newTag.IconName))
                {
                    newTag.IconName = ((string)category["icon"].ToString());
                    if (newTag.IconName != "")
                    {
                        if (!ResourceProxy.Instance.IconExists(newTag.IconName))
                        {
                            newTag.IconName = "";
                        }
                    }
                }

                parent.AddChild(newTag);
                LuaTable parts = (LuaTable)category["parts"];
                foreach (var value in parts.Keys)
                {
                    if (value is String)
                    {
                        if (!PartCatalog.Instance.PartIndex.ContainsKey((string)value.ToString()))
                        {
                            Debug.LogError("Part Index is missing part " + value);
                        }
                        else
                        {
                            AvailablePart part = PartCatalog.Instance.PartIndex[(string)value.ToString()];
                            leftOvers.Remove(part);
                            newTag.AddPart(part, false);
                        }
                    }
                }

                LuaTable children = (LuaTable)category["children"];
                foreach (var value in children.Values)
                {
                    if (value is LuaTable)
                    {
                        ParseCategoryTable((LuaTable)value, newTag, leftOvers);
                    }
                }

                if (ConfigHandler.Instance.MaxTagsPerPage > 0 && !parent.IsRoot)
                {
                    if (newTag.ChildTags.Count <= ConfigHandler.Instance.MaxTagsPerPage)
                    {
                        return;
                    }

                    int numLists = newTag.ChildTags.Count / ConfigHandler.Instance.MaxTagsPerPage + 1;
                    


                    var subPages = new List<PartTag>();

                    for (int i = 0; i < numLists; i++)
                    {
                            
                        var subPage = new PartTag();
                        for (int j = 0; j < ConfigHandler.Instance.MaxTagsPerPage; j++)
                        {
                            if (newTag.ChildTags.Count == 0)
                                break;

                            subPage.AddChild(newTag.ChildTags.First.Value);
                        }
                        subPage.Name = string.Format("Page {0}", i + 1);
                        subPages.Add(subPage);                       
                    }

                    foreach (var subPage in subPages)
                    {
                        newTag.AddChild(subPage);
                    }
                }
            }
        }

        private void SetupNamespace()
        {
            luaInstance.DoString(KSP.IO.File.ReadAllText<PartCatalog>("util.lua"));

            
            CreatePartTable();
            //luaInstance.DoString(PartString);
            luaInstance.DoString(KSP.IO.File.ReadAllText<PartCatalog>("default.category.lua"));
        }

        private void CreatePartTable()
        {
            using (LuaTable partTable = luaInstance.CreateTable())
            {
                luaInstance["PARTS"] = partTable;

                UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("PART");

                HashSet<AvailablePart> HandledParts = new HashSet<AvailablePart>();
                PartCatalog.Instance.UnlistedParts.Clear();
                foreach (var config in configs)
                {
                    string cleanedName = config.name.Replace('_', '.');
                    AvailablePart part;
                    if (!PartCatalog.Instance.PartIndex.TryGetValue(cleanedName, out part))
                    {
                        Debug.LogError("Could not find part in index: " + cleanedName);
                    }
                    else
                    {
                        if (part != null)
                        {
                            HandledParts.Add(part);
                            SerializeAvailablePart(partTable, part, config.config);
                        }
                    }

                }

                foreach (var part in PartCatalog.Instance.SortedPartList)
                {
                    if (part.category != PartCategories.none)
                    {
                        if (!HandledParts.Contains(part))
                        {
                            PartCatalog.Instance.UnlistedParts.Add(part);
                        }
                    }
                }

                using (LuaTable iconsTable = luaInstance.CreateTable())
                {
                    luaInstance["ICONS"] = iconsTable;
                    foreach (var texture in ResourceProxy.Instance.LoadedTextures.Keys)
                    {

                        iconsTable[texture.Replace(@"\", "/").Replace(".","\\.")] = true;
                    }
                }
            }
        }

        private void SerializeAvailablePart(LuaTable partsTable, AvailablePart part, ConfigNode node)
        {            
            using (LuaTable partTable = luaInstance.CreateTable())
            {
                partsTable[part.name.Replace(".","\\.")] = partTable;
                partTable["name"] = part.name;
                partTable["title"] = part.title;
                partTable["mod"] = PartCatalog.Instance.GetPartMod(part);
                partTable["manufacturer"] = part.manufacturer;
                partTable["author"] = part.author;
                partTable["category"] = part.category.ToString();
                partTable["cost"] = part.cost;
                partTable["description"] = part.description;
                partTable["entryCost"] = part.entryCost;
                partTable["techRequired"] = part.TechRequired;
                partTable["assigned"] = false;
                partTable["isPart"] = true;
                if (part.partPrefab != null)
                {
                    partTable["angularDrag"] = part.partPrefab.angularDrag;
                    partTable["breakingForce"] = part.partPrefab.breakingForce;
                    partTable["breakingTorque"] = part.partPrefab.breakingTorque;
                    partTable["buoyancy"] = part.partPrefab.buoyancy;
                    partTable["crashTolerance"] = part.partPrefab.crashTolerance;
                    partTable["crewCapacity"] = part.partPrefab.CrewCapacity;
                    partTable["dragModelType"] = part.partPrefab.dragModelType;
                    partTable["explosionPotential"] = part.partPrefab.explosionPotential;
                    partTable["fuelCrossFeed"] = part.partPrefab.fuelCrossFeed;
                    partTable["heatConductivity"] = part.partPrefab.heatConductivity;
                    partTable["heatDissipation"] = part.partPrefab.heatDissipation;
                    partTable["mass"] = part.partPrefab.mass;
                    partTable["minimum_drag"] = part.partPrefab.minimum_drag;
                    partTable["maximum_drag"] = part.partPrefab.maximum_drag;
                    partTable["maxTemp"] = part.partPrefab.maxTemp;
                    partTable["rescaleFactor"] = part.partPrefab.rescaleFactor;
                    partTable["scaleFactor"] = part.partPrefab.scaleFactor;
                    partTable["stagingIcon"] = part.partPrefab.stagingIcon;
                    partTable["stackSymmetry"] = part.partPrefab.stackSymmetry;

                    if (part.partPrefab.dragModelType == "override" && part.partPrefab is Winglet)
                    {
                        partTable["dragCoeff"] = ((Winglet) part.partPrefab).dragCoeff;
                        partTable["deflectionLiftCoeff"] = ((Winglet) part.partPrefab).deflectionLiftCoeff;
                    }

                    partTable["attachSurface"] = part.partPrefab.attachRules.srfAttach;
                    using (LuaTable nodesTable = luaInstance.CreateTable())
                    {
                        partTable["attachNodes"] = nodesTable;

                        double cnt = 1;
                        foreach (var attachnode in part.partPrefab.attachNodes)
                        {
                            using (LuaTable nodeTable = luaInstance.CreateTable())
                            {
                                nodesTable[cnt++] = nodeTable;

                                using (LuaTable offsetTable = luaInstance.CreateTable())
                                {
                                    nodeTable["offset"] = offsetTable;
                                    offsetTable["x"] = attachnode.offset.x;
                                    offsetTable["y"] = attachnode.offset.y;
                                    offsetTable["z"] = attachnode.offset.z;
                                }

                                using (LuaTable orientationTable = luaInstance.CreateTable())
                                {
                                    nodeTable["orientation"] = orientationTable;
                                    orientationTable["x"] = attachnode.orientation.x;
                                    orientationTable["y"] = attachnode.orientation.y;
                                    orientationTable["z"] = attachnode.orientation.z;
                                }

                                nodeTable["size"] = attachnode.size;
                                nodeTable["radius"] = attachnode.radius;
                            }
                        }
                    }
                }

                using (LuaTable nodesTable = SerializeConfigNode(node))
                {
                    partTable["nodes"] = nodesTable;
                }
            }
        }

        private LuaTable SerializeConfigNode(ConfigNode node)
        {
            LuaTable toReturn = luaInstance.CreateTable();
            
            double cnt = 1;
            foreach (ConfigNode childNode in node.nodes)
            {
                using (LuaTable childTable = luaInstance.CreateTable())
                {
                    toReturn[cnt++] = childTable;

                    childTable["name"] = childNode.name;

                    using (LuaTable childValueTable = luaInstance.CreateTable())
                    {
                        childTable["values"] = childValueTable;

                        foreach (ConfigNode.Value val in childNode.values)
                        {
                            childValueTable[val.name.Replace(".","\\.")] = val.value;
                        }
                    }

                    childTable["nodes"] = SerializeConfigNode(childNode);
                }
            }
            return toReturn;
            
        }

    }
}
