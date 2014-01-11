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
            KSP.IO.File.WriteAllText<PartCatalog>(PartString, "partString.lua");
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
                newTag.IconName = ((string)category["icon"]);
                if (newTag.IconName != "")
                {
                    if (!ResourceProxy.Instance.IconExists(newTag.IconName))
                    {
                        newTag.IconName = "";
                    }
                }
                newTag.IconOverlay = (string)category["overlay"];
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
            StringBuilder toRun = new StringBuilder();


            toRun.Append("PARTS = {");
            UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("PART");
            toRun.EnsureCapacity(configs.Length * ConfigHandler.Instance.PartSerializationBufferSize);
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
                            toRun.Append(",");
                        }
                        first = false;
                        SerializeAvailablePart(toRun, part, config.config);
                    }
                }

            }
            toRun.Append("}");
            return toRun.ToString();
        }

        private static void SerializeAvailablePart(StringBuilder toRun, AvailablePart part, ConfigNode node)
        {
            toRun   .Append("[ [[").Append(part.name).Append("]] ] = {")
                    .Append("name = [[").Append(part.name).Append("]],")
                    .Append("title = [[" ).Append(part.title ).Append("]],")
                    .Append("mod = [[" ).Append(PartCatalog.Instance.GetPartMod(part) ).Append("]],")
                    .Append("manufacturer = [[" ).Append(part.manufacturer ).Append("]],")
                    .Append("author = [[" ).Append(part.author ).Append("]],")
                    .Append("category = [[" ).Append(part.category ).Append("]],")
                    .Append("cost = [[" ).Append(part.cost ).Append("]],")
                    .Append("description = [[" ).Append(part.description ).Append("]],")
                    .Append("entryCost = [[" ).Append(part.entryCost ).Append("]],")
                    .Append("techRequired = [[" ).Append(part.TechRequired ).Append("]],")
                    .Append("angularDrag = [[" ).Append(part.partPrefab.angularDrag ).Append("]],")
                    .Append("breakingForce = [[" ).Append(part.partPrefab.breakingForce ).Append("]],")
                    .Append("breakingTorque = [[" ).Append(part.partPrefab.breakingTorque ).Append("]],")
                    .Append("buoyancy = [[" ).Append(part.partPrefab.buoyancy ).Append("]],")
                    .Append("crashTolerance = [[" ).Append(part.partPrefab.crashTolerance ).Append("]],")
                    .Append("crewCapacity = [[" ).Append(part.partPrefab.CrewCapacity ).Append("]],")
                    .Append("dragModelType = [[" ).Append(part.partPrefab.dragModelType ).Append("]],") //Determines whether we got stock aero or not
                    .Append("explosionPotential = [[" ).Append(part.partPrefab.explosionPotential ).Append("]],")
                    .Append("fuelCrossFeed = [[" ).Append(part.partPrefab.fuelCrossFeed ).Append("]],")
                    .Append("heatConductivity = [[" ).Append(part.partPrefab.heatConductivity ).Append("]],")
                    .Append("heatDissipation = [[" ).Append(part.partPrefab.heatDissipation ).Append("]],")
                    .Append("mass = [[" ).Append(part.partPrefab.mass ).Append("]],")
                    .Append("minimum_drag = [[" ).Append(part.partPrefab.minimum_drag ).Append("]],")
                    .Append("maximum_drag = [[" ).Append(part.partPrefab.maximum_drag ).Append("]],")
                    .Append("maxTemp = [[" ).Append(part.partPrefab.maxTemp ).Append("]],")
                    .Append("rescaleFactor = [[" ).Append(part.partPrefab.rescaleFactor ).Append("]],")
                    .Append("scaleFactor = [[" ).Append(part.partPrefab.scaleFactor ).Append("]],")
                    .Append("stagingIcon = [[" ).Append(part.partPrefab.stagingIcon ).Append("]],")
                    .Append("assigned = false,")
                    .Append("isPart = true,");
            if (part.partPrefab.dragModelType == "override" && part.partPrefab is Winglet)
            {
                toRun.Append("dragCoeff = [[").Append(((Winglet)part.partPrefab).dragCoeff).Append("]],")
                     .Append("deflectionLiftCoeff = [[").Append(((Winglet)part.partPrefab).deflectionLiftCoeff).Append("]],");
            }

            SerializeConfigNode(toRun, node);

            toRun.Append("}");
        }

        private static void SerializeConfigNode(StringBuilder toRun, ConfigNode node)
        {
            toRun.Append("nodes = {");
            bool firstNode = true;
            foreach (ConfigNode childNode in node.nodes)
            {
                if (!firstNode)
                {
                    toRun.Append(",");
                }
                firstNode = false;
                toRun   .Append("{")
                        .Append("name = [[").Append(childNode.name).Append("]],")
                        .Append("values = {");
                bool first = true;
                foreach (ConfigNode.Value val in childNode.values)
                {
                    if (!first)
                    {
                        toRun.Append(",");
                    }
                    first = false;
                    toRun.Append("[ [[" ).Append(val.name).Append("]] ] = [[").Append(val.value).Append("]]");
                }
                toRun.Append("},");

                SerializeConfigNode(toRun, childNode);

                toRun.Append("}");


            }
            toRun.Append("}");
        }

        public void Test()
        {
            Debug.Log(luaInstance.DoString("return 1+1")[0]);
        }

    }
}
