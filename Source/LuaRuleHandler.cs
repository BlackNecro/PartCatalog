using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using NLua;

namespace PartCatalog
{
    class LuaRuleHandler
    {

        Lua luaInstance = new Lua();

        String PartString = String.Empty;
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
            PartString = CreatePartString();
            SetupNamespace();
            KSP.IO.File.WriteAllText<PartCatalog>(PartString, "partString.lua");

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
                    Debug.Log("Executing " + file);
                    string input = KSP.IO.File.ReadAllText<PartCatalog>(file);
                    luaInstance.DoString(input);
                }
            }

            foreach (string file in System.IO.Directory.GetFiles(GUIConstants.CatalogDataPath))
            {
                if (file.EndsWith(".fallback.lua"))
                {
                    Debug.Log("Fallback " + file);
                    string input = KSP.IO.File.ReadAllText<PartCatalog>(file);
                    luaInstance.DoString(input);
                }
            }

            luaInstance.DoString(@"clean(CATEGORIES)
sortCat(CATEGORIES)");

            Categories.Clear();

            LuaTable category = luaInstance.GetTable("CATEGORIES");
            LuaTable children = (LuaTable)category["children"];
            foreach (var value in children.Values)
            {
                if (value is LuaTable)
                {
                    ParseCategoryTable((LuaTable)value, PartCatalog.Instance.RootTag);
                }
            }
        }

        private static void ParseCategoryTable(LuaTable category, PartTag parent)
        {
            if (category != null)
            {
                PartTag newTag = new PartTag();                               
                newTag.Name = (string)category["title"];
                newTag.IconName = (string)category["icon"];
                if(!ResourceProxy.Instance.BufferTexture(newTag.IconName))
                {
                    newTag.IconName = "";
                }
                parent.AddChild(newTag);
                LuaTable parts = (LuaTable)category["parts"];
                foreach (var value in parts.Keys)
                {
                    if (value is string)
                    {
                        if (!PartCatalog.Instance.PartIndex.ContainsKey((string)value))
                        {
                            Debug.LogError("Part Index is missing part " + value);
                        }
                        else
                        {
                            newTag.AddPart(PartCatalog.Instance.PartIndex[(string)value]);
                        }
                    }
                }

                LuaTable children = (LuaTable)category["children"];
                foreach (var value in children.Values)
                {
                    if (value is LuaTable)
                    {
                        ParseCategoryTable((LuaTable)value, newTag);
                    }
                }                
            }
        }

        private void SetupNamespace()
        {
            luaInstance.DoString(KSP.IO.File.ReadAllText<PartCatalog>("util.lua"));
            luaInstance.DoString(PartString);
            luaInstance.DoString(KSP.IO.File.ReadAllText<PartCatalog>("default.category.lua"));
        }

        private static string CreatePartString()
        {
            string toRun = "PARTS = {";
            UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("PART");

            bool first = true;
            foreach (var config in configs)
            {
                if (!PartCatalog.Instance.PartIndex.ContainsKey(config.name.Replace('_', '.')))
                {
                    Debug.LogError("Could not find part in index: " + config.name.Replace('_', '.'));
                }
                else
                {
                    AvailablePart part = PartCatalog.Instance.PartIndex[config.name.Replace('_', '.')];
                    if (part != null)
                    {
                        if (!first)
                        {
                            toRun += ",";
                        }
                        first = false;
                        toRun = SerializeAvailablePart(toRun, part, config.config);
                    }
                }

            }
            toRun += "}";
            return toRun;
        }

        private static string SerializeAvailablePart(string toRun, AvailablePart part, ConfigNode node)
        {
            toRun += "[ [[" + part.name + "]] ] = {";
            toRun += "name = [[" + part.name + "]],";
            toRun += "title = [[" + part.title + "]],";
            toRun += "mod = [[" + PartCatalog.Instance.GetPartMod(part) + "]],";
            toRun += "manufacturer = [[" + part.manufacturer + "]],";
            toRun += "author = [[" + part.author + "]],";
            toRun += "category = [[" + part.category + "]],";
            toRun += "cost = [[" + part.cost + "]],";
            toRun += "description = [[" + part.description + "]],";
            toRun += "entryCost = [[" + part.entryCost + "]],";
            toRun += "techRequired = [[" + part.TechRequired + "]],";
            toRun += "angularDrag = [[" + part.partPrefab.angularDrag + "]],";
            toRun += "breakingForce = [[" + part.partPrefab.breakingForce + "]],";
            toRun += "breakingTorque = [[" + part.partPrefab.breakingTorque + "]],";
            toRun += "buoyancy = [[" + part.partPrefab.buoyancy + "]],";
            toRun += "crashTolerance = [[" + part.partPrefab.crashTolerance + "]],";
            toRun += "crewCapacity = [[" + part.partPrefab.CrewCapacity + "]],";
            toRun += "dragModelType = [[" + part.partPrefab.dragModelType + "]],"; //Determines whether we got stock aero or not
            toRun += "explosionPotential = [[" + part.partPrefab.explosionPotential + "]],";
            toRun += "fuelCrossFeed = [[" + part.partPrefab.fuelCrossFeed + "]],";
            toRun += "heatConductivity = [[" + part.partPrefab.heatConductivity + "]],";
            toRun += "heatDissipation = [[" + part.partPrefab.heatDissipation + "]],";
            toRun += "mass = [[" + part.partPrefab.mass + "]],";
            toRun += "minimum_drag = [[" + part.partPrefab.minimum_drag + "]],";
            toRun += "maximum_drag = [[" + part.partPrefab.maximum_drag + "]],";
            toRun += "maxTemp = [[" + part.partPrefab.maxTemp + "]],";
            toRun += "rescaleFactor = [[" + part.partPrefab.rescaleFactor + "]],";
            toRun += "scaleFactor = [[" + part.partPrefab.scaleFactor + "]],";
            toRun += "stagingIcon = [[" + part.partPrefab.stagingIcon + "]],";
            if (part.partPrefab.dragModelType == "override" && part.partPrefab is Winglet)
            {
                toRun += "dragCoeff = [[" + ((Winglet)part.partPrefab).dragCoeff + "]],";
                toRun += "deflectionLiftCoeff = [[" + ((Winglet)part.partPrefab).deflectionLiftCoeff + "]],";
            }
            toRun += "assigned = false,";
            toRun += "isPart = true,";


            toRun = SerializeConfigNode(toRun, node);

            toRun += "}";

            return toRun;
        }

        private static string SerializeConfigNode(string toRun, ConfigNode node)
        {
            toRun += "nodes = {";
            bool firstNode = true;
            foreach (ConfigNode childNode in node.nodes)
            {
                if (!firstNode)
                {
                    toRun += ",";
                }
                firstNode = false;
                toRun += "{";

                toRun += "name = [[" + childNode.name + "]],";
                toRun += "values = {";
                bool first = true;
                foreach (ConfigNode.Value val in childNode.values)
                {
                    if (!first)
                    {
                        toRun += ",";
                    }
                    first = false;
                    toRun += "[ [[" + val.name + "]] ] = [[" + val.value + "]]";
                }
                toRun += "},";

                toRun = SerializeConfigNode(toRun, childNode);

                toRun += "}";


            }
            toRun += "}";
            return toRun;
        }

        public void Test()
        {
            Debug.Log(luaInstance.DoString("return 1+1")[0]);
        }

    }
}
