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

            LuaTable category = luaInstance.GetTable("CATEGORIES");
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
                    Debug.Log("Adding " + part.name);
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

                PartTag newTag = parent.findChild((string)category["title"]);
                if (newTag == null)
                {
                    newTag = new PartTag();
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
                }

                if (String.IsNullOrEmpty(newTag.IconName))
                {
                    newTag.IconName = ((string)category["icon"]);
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
                    if (value is string)
                    {
                        if (!PartCatalog.Instance.PartIndex.ContainsKey((string)value))
                        {
                            Debug.LogError("Part Index is missing part " + value);
                        }
                        else
                        {
                            AvailablePart part = PartCatalog.Instance.PartIndex[(string)value];
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
            toRun.EnsureCapacity((configs.Length + ResourceProxy.Instance.LoadedTextures.Count) * ConfigHandler.Instance.PartSerializationBufferSize);
            bool first = true;

            HashSet<AvailablePart> HandledParts = new HashSet<AvailablePart>();
            PartCatalog.Instance.UnlistedParts.Clear();
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
                        HandledParts.Add(part);
                        if (first)
                        {
                            first = false;
                        }   
                        else
                        {
                            toRun.AppendLine(",");
                        }
                        first = false;
                        SerializeAvailablePart(toRun, part, config.config);
                    }
                }

            }

            foreach(var part in PartCatalog.Instance.SortedPartList)
            {
                if(part.category != PartCategories.none)
                { 
                if(!HandledParts.Contains(part))
                {
                    PartCatalog.Instance.UnlistedParts.Add(part);
                }
            }
            }

            toRun.AppendLine("}");

            toRun.Append("ICONS = {");
            first = true;
            foreach(var texture in ResourceProxy.Instance.LoadedTextures.Keys)
            {
                
                if(first)
                {
                    first = false;
                }
                else
                {
                    toRun.Append(",");
                }
                toRun.Append("[ \"").Append(escapeString(texture.Replace(@"\", "/"))).Append("\" ]").Append(" = true");
            }
            toRun.Append("}");


            return toRun.ToString();
        }

        private static string escapeString(object toEscape)
        {
            return toEscape.ToString().Replace("\"", "\\\"");
        }

        private static void SerializeAvailablePart(StringBuilder toRun, AvailablePart part, ConfigNode node)
        {
            toRun.Append("[ \"").Append(escapeString(part.name)).Append("\" ] = {")
                    .Append("name = \"").Append(escapeString(part.name)).Append("\",")
                    .Append("title = \"").Append(escapeString(part.title)).Append("\",")
                    .Append("mod = \"").Append(escapeString(PartCatalog.Instance.GetPartMod(part))).Append("\",")
                    .Append("manufacturer = \"").Append(escapeString(part.manufacturer)).Append("\",")
                    .Append("author = \"").Append(escapeString(part.author)).Append("\",")
                    .Append("category = \"").Append(escapeString(part.category)).Append("\",")
                    .Append("cost = \"").Append(escapeString(part.cost)).Append("\",")
                    .Append("description = \"").Append(escapeString(part.description)).Append("\",")
                    .Append("entryCost = \"").Append(escapeString(part.entryCost)).Append("\",")
                    .Append("techRequired = \"").Append(escapeString(part.TechRequired)).Append("\",")
                    .Append("angularDrag = \"").Append(escapeString(part.partPrefab.angularDrag)).Append("\",")
                    .Append("breakingForce = \"").Append(escapeString(part.partPrefab.breakingForce)).Append("\",")
                    .Append("breakingTorque = \"").Append(escapeString(part.partPrefab.breakingTorque)).Append("\",")
                    .Append("buoyancy = \"").Append(escapeString(part.partPrefab.buoyancy)).Append("\",")
                    .Append("crashTolerance = \"").Append(escapeString(part.partPrefab.crashTolerance)).Append("\",")
                    .Append("crewCapacity = \"").Append(escapeString(part.partPrefab.CrewCapacity)).Append("\",")
                    .Append("dragModelType = \"").Append(escapeString(part.partPrefab.dragModelType)).Append("\",") //Determines whether we got stock aero or not
                    .Append("explosionPotential = \"").Append(escapeString(part.partPrefab.explosionPotential)).Append("\",")
                    .Append("fuelCrossFeed = \"").Append(escapeString(part.partPrefab.fuelCrossFeed)).Append("\",")
                    .Append("heatConductivity = \"").Append(escapeString(part.partPrefab.heatConductivity)).Append("\",")
                    .Append("heatDissipation = \"").Append(escapeString(part.partPrefab.heatDissipation)).Append("\",")
                    .Append("mass = \"").Append(escapeString(part.partPrefab.mass)).Append("\",")
                    .Append("minimum_drag = \"").Append(escapeString(part.partPrefab.minimum_drag)).Append("\",")
                    .Append("maximum_drag = \"").Append(escapeString(part.partPrefab.maximum_drag)).Append("\",")
                    .Append("maxTemp = \"").Append(escapeString(part.partPrefab.maxTemp)).Append("\",")
                    .Append("rescaleFactor = \"").Append(escapeString(part.partPrefab.rescaleFactor)).Append("\",")
                    .Append("scaleFactor = \"").Append(escapeString(part.partPrefab.scaleFactor)).Append("\",")
                    .Append("stagingIcon = \"").Append(escapeString(part.partPrefab.stagingIcon)).Append("\",")
                    .Append("stackSymmetry = \"").Append(escapeString(part.partPrefab.stackSymmetry)).Append("\",")
                    .Append("assigned = false,")
                    .Append("isPart = true,");

            if (part.partPrefab != null)
            {
                if (part.partPrefab.dragModelType == "override" && part.partPrefab is Winglet)
                {
                    toRun.Append("dragCoeff = \"").Append(escapeString(((Winglet)part.partPrefab).dragCoeff)).Append("\",")
                         .Append("deflectionLiftCoeff = \"").Append(escapeString(((Winglet)part.partPrefab).deflectionLiftCoeff)).Append("\",");
                }

                toRun.Append("attachSurface = ").Append(part.partPrefab.attachRules.allowSrfAttach).Append(",")

                .Append("attachNodes = {");
                    bool firstNode = true;
                    foreach (var attachnode in part.partPrefab.attachNodes)
                    {
                        if(firstNode)
                        {
                            firstNode = false;
                        }
                        else
                        {
                            toRun.Append(",");
                        }
                        toRun.Append("{ ")
                            .Append("offset = { x = ").Append(attachnode.offset.x).Append(", y = ").Append(attachnode.offset.y).Append(", z = ").Append(attachnode.offset.z).Append(" } ")
                            .Append(",orientation = { x = ").Append(attachnode.orientation.x).Append(", y = ").Append(attachnode.orientation.y).Append(", z = ").Append(attachnode.orientation.z).Append(" } ")
                            .Append(",size = ").Append(attachnode.size)
                            .Append(",radius= ").Append(attachnode.radius)
                            .Append("}");
                    }
                   toRun.Append("},");
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
                toRun.Append("{")
                        .Append("name = \"").Append(escapeString(childNode.name)).Append("\",")
                        .Append("values = {");
                bool first = true;
                foreach (ConfigNode.Value val in childNode.values)
                {
                    if (!first)
                    {
                        toRun.Append(",");
                    }
                    first = false;
                    toRun.Append("[ \"").Append(escapeString(val.name)).Append("\" ] = \"").Append(escapeString(val.value)).Append("\"");
                }
                toRun.Append("},");

                SerializeConfigNode(toRun, childNode);

                toRun.Append("}");


            }
            toRun.Append("}");
        }

    }
}
