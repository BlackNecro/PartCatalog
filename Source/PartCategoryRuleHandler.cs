﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using KSP.IO;

namespace PartCatalog
{

    /*
    public static class Debug
    {
        static public bool Enabled = true;
        public static void Log(string toLog)
        {
            if (Enabled)
            {
                UnityEngine.Debug.Log(toLog);
                File.AppendAllText<PartCatalog>(toLog + "\n", "timeLog.log");
            }
        }
        public static void LogError(string toLog)
        {
            UnityEngine.Debug.LogError(toLog);
        }
        public static void LogWarning(string toLog)
        {
            UnityEngine.Debug.LogWarning(toLog);
        }
    }
         */
    public class ParseFile
    {
        LinkedList<string> lines = new LinkedList<string>();
        LinkedListNode<string> current = null;
        bool finished = false;

        public ParseFile(string name)
        {
            //TextReader file = File.OpenText(name);
            TextReader file = TextReader.CreateForType<PartCatalog>(name);
            while (!file.EndOfStream)
            {
                string line = file.ReadLine().Trim().Replace('\t',' ');
                if (line.Length > 0)
                {
                    lines.AddLast(line);
                }
            }
            file.Close();
            current = lines.First;
        }

        public string CurrentLine
        {
            get
            {
                if (current != null)
                {

                    return current.Value;
                }
                else
                {
                    return null;
                }
            }
        }
        public bool EndOfFile
        {
            get
            {
                return finished;
            }
        }
        public void Advance()
        {
            if (current != null)
            {
                if (current.Next != null)
                {
                    current = current.Next;
                    return;
                }
            }
            finished = true;
        }
    }
    public class PartCategoryRuleHandler
    {
        static readonly PartCategoryRuleHandler instance = new PartCategoryRuleHandler();

        private PartCategoryRuleHandler()
        {
            ReloadFiles();
        }
        public static PartCategoryRuleHandler Instance
        {
            get
            {
                return instance;
            }
        }

        Dictionary<string, LinkedList<CategoryRule>> CategorizationRules = new Dictionary<string, LinkedList<CategoryRule>>();
        Dictionary<string, LinkedList<CategoryRule>> FallbackCategorizationRules = new Dictionary<string, LinkedList<CategoryRule>>();

        private Dictionary<string, LinkedList<CategoryStructure>> CategoryStructures = new Dictionary<string, LinkedList<CategoryStructure>>();

        public HashSet<string> GetCategoriesForPart(AvailablePart part)
        {
            HashSet<string> toReturn = new HashSet<string>();

            UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("PART");

            UrlDir.UrlConfig config = Array.Find<UrlDir.UrlConfig>(configs,
                (c => part.name == c.name.Replace('_', '.')));
            if (config != null && config.config != null)
            {
                CategoryRuleContext context = new CategoryRuleContext();
                context.currentPart = part;
                context.categories = toReturn;
                context.currentNode = config.config;
                context.variables = new Dictionary<string, string>();
                foreach (var ruleList in CategorizationRules.Values)
                {
                    foreach (CategoryRule rule in ruleList)
                    {                                               
                        rule.Execute(context);
                    }

                }
                
                if (toReturn.Count == 0)
                {
                    foreach (var ruleList in FallbackCategorizationRules.Values)
                    {
                        foreach (CategoryRule rule in ruleList)
                        {
                            rule.Execute(context);
                        }
                    }
                }
            }




            return toReturn;
        }

        public void ReloadFiles()
        {
            CategorizationRules.Clear();
            FallbackCategorizationRules.Clear();
            CategoryStructures.Clear();

            foreach (string file in System.IO.Directory.GetFiles(GUIConstants.CatalogDataPath))
            {
                if (file.EndsWith(".category.txt", StringComparison.OrdinalIgnoreCase))
                {                    
                    ParseCategoryFile(file);
                }
                /*else if (file.EndsWith(".rules.txt", StringComparison.OrdinalIgnoreCase))
                {
                    ParseRulesFile(file);
                } */
            }
        }
        public static string GetLegacyPartModule(AvailablePart Part)
        {
            if (Part.partPrefab is Winglet)
            {
                return "Winglet";
            }
            else if (Part.partPrefab is FuelLine)
            {
                return "FuelLine";
            }
            else if (Part.partPrefab is HLandingLeg)
            {
                return "HLandingLeg";
            }
            else if (Part.partPrefab is ControlSurface)
            {
                return "ControlSurface";
            }
            else if (Part.partPrefab is Strut)
            {
                return "Strut";
            }
            else if (Part.partPrefab is Decoupler)
            {
                return "Decoupler";
            }
            else if (Part.partPrefab is StrutConnector)
            {
                return "StrutConnector";
            }
            return "Part";
        }

        public Dictionary<string, HashSet<AvailablePart>> GetCategoriesForParts(HashSet<AvailablePart> toCheck)
        {

            Dictionary<string, HashSet<AvailablePart>> toReturn = new Dictionary<string, HashSet<AvailablePart>>();

            foreach (var part in toCheck)
            {

                HashSet<string> categories = GetCategoriesForPart(part);

                foreach (var cat in categories)
                {
                    if (!toReturn.ContainsKey(cat))
                    {
                        toReturn[cat] = new HashSet<AvailablePart>();
                    }
                    toReturn[cat].Add(part);
                }

            }

            return toReturn;
        }

        static DateTime startTime;
        public void AutoGroupPartTag(ref PartTag toGroup)
        {
            startTime = DateTime.Now;           
            foreach (var kv in CategoryStructures)
            {

                foreach (var structure in kv.Value)
                {
                    structure.Execute(ref toGroup, toGroup, LuaRuleHandler.Instance.Categories);
                }
            }
        }
        public void ParseRulesFile(string fileName)
        {
            ParseFile file = new ParseFile(fileName);

            FallbackCategorizationRules[fileName] = new LinkedList<CategoryRule>();
            CategorizationRules[fileName] = new LinkedList<CategoryRule>();
            while (!file.EndOfFile)
            {

                CategoryRule rule = CategoryRule.ParseLine(file);                
                if (rule != null && rule.Loaded)
                {                    
                    if (rule.Fallback)
                    {
                        FallbackCategorizationRules[fileName].AddLast(rule);
                    }
                    else
                    {
                        CategorizationRules[fileName].AddLast(rule);
                    }
                }
            }

        }

        public void ParseCategoryFile(string fileName)
        {
            ParseFile file = new ParseFile(fileName);
            CategoryStructures[fileName] = new LinkedList<CategoryStructure>();

            while (!file.EndOfFile)
            {
                CategoryStructure structure = CategoryStructure.ParseLine(file);

                if (structure != null && structure.Loaded)
                {
                    CategoryStructures[fileName].AddLast(structure);
                }
            }
        }
    }

    #region TagStructures
    class CategoryStructure
    {
        protected string Name;
        public bool Loaded = false;
        public string Line = "";
        public string IconName = "";

        protected LinkedList<CategoryStructure> ChildStructures = new LinkedList<CategoryStructure>();

        public CategoryStructure(ParseFile file)
        {

            Line = file.CurrentLine;

            int index = Line.IndexOf(" WITH ICON ", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                IconName = "Category_" + Line.Substring(index + " WITH ICON ".Length).Trim().Replace(@"*", @"{0}");
                if(!ResourceProxy.Instance.IconExists(IconName))
                {
                    IconName = "";
                }
                Line = Line.Substring(0, index);
            }

            Name = Line.Substring("TAG".Length);
            file.Advance();

            while (!file.EndOfFile)
            {
                string line = file.CurrentLine;

                if (line.StartsWith("END TAG", StringComparison.OrdinalIgnoreCase))
                {

                    file.Advance();
                    Loaded = true;
                    return;
                }

                CategoryStructure childStructure = ParseLine(file);

                if (childStructure != null)
                {
                    ChildStructures.AddLast(childStructure);
                }
            }
        }

        protected CategoryStructure()
        {
        }

        public virtual void Execute(ref PartTag toCategorize, PartTag rootNode, Dictionary<string, HashSet<AvailablePart>> rootCategories)
        {
            PartTag childTag = new PartTag();
            childTag.Name = Name;
            childTag.IconName = IconName;
            foreach (var childStructure in ChildStructures)
            {
                childStructure.Execute(ref childTag, rootNode, rootCategories);
            }
            if(childTag.ChildTags.Count > 0 || childTag.IncludedParts.Count > 0)
            {
                toCategorize.AddChild(childTag);
            }
        }

        public static CategoryStructure ParseLine(ParseFile file)
        {
            string line = file.CurrentLine;
            if (line.StartsWith("TAG ", StringComparison.OrdinalIgnoreCase))
            {
                CategoryStructure structure = new CategoryStructure(file);
                if (structure.Loaded)
                {
                    return structure;
                }
            }
            else if (line.StartsWith("INCLUDE ", StringComparison.OrdinalIgnoreCase))
            {
                CategoryStructure structure = new CategoryStructureInclude(file);
                if (structure.Loaded)
                {
                    return structure;
                }
            }
            else if (line.StartsWith("REORDER", StringComparison.OrdinalIgnoreCase))
            {
                CategoryStructure structure = new CategoryStructureReorder(file);
                if (structure.Loaded)
                {
                    return structure;
                }
            }
            Debug.LogError("Unknown Line " + line);
            file.Advance();
            return null;
        }
    }

    internal class CategoryStructureInclude : CategoryStructure
    {
        public string CategoryName;
        public string TagName;

        public CategoryStructureInclude(ParseFile file)
        {
            Line = file.CurrentLine;
            file.Advance();

            int index = Line.IndexOf(" WITH ICON ",StringComparison.OrdinalIgnoreCase);
            if(index >= 0)
            {
                IconName = "Category_" + Line.Substring(index + " WITH ICON ".Length).Trim().Replace(@"*", @"{0}");
                Line = Line.Substring(0, index);               
            }

            index = Line.IndexOf(" AS ", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                CategoryName = Line.Substring("INCLUDE ".Length, index - "Include ".Length + 1).Trim();
                TagName = Line.Substring(index + " AS ".Length).Trim().Replace(@"*", @"{0}");                
            }
            else
            {
                CategoryName = Line.Substring("INCLUDE".Length).Trim();
            }
            CategoryName = String.Format("^{0}$", CategoryName.Replace(@"*", @"(.*)"));
            Loaded = !String.IsNullOrEmpty(CategoryName);
        }

        public override void Execute(ref PartTag toCategorize, PartTag rootNode, Dictionary<string, HashSet<AvailablePart>> rootCategories)
        {
            foreach (var kv in rootCategories)
            {
                if (kv.Value.Count > 0)
                {
                    Match match = Regex.Match(kv.Key, CategoryName);
                    if (match.Success)//key matches category name
                    {

                        if (TagName != null)
                        {
                            PartTag childTag = new PartTag();
                            string icon = null;
                            if (match.Groups.Count > 1)
                            {
                                childTag.Name = String.Format(TagName, match.Groups[1].Value);
                                if(IconName != null)
                                {
                                    string iconFormatted = String.Format(IconName, match.Groups[1].Value);
                                    if(ResourceProxy.Instance.IconExists(iconFormatted))
                                    {
                                        icon = iconFormatted;
                                    } else {
                                        icon = String.Format(IconName, "");
                                    }
                                }
                            }
                            else
                            {
                                childTag.Name = String.Format(TagName, "");
                                icon = IconName;
                            }
                            if(icon != null && ResourceProxy.Instance.IconExists(icon))
                            {
                                childTag.IconName = icon;
                            }
                            childTag.AddParts(kv.Value);
                            toCategorize.AddChild(childTag);
                        }
                        else
                        {
                            toCategorize.AddParts(kv.Value);
                        }
                        rootNode.RemoveParts(kv.Value);
                    }
                }
            }
        }

    }

    class CategoryStructureReorder : CategoryStructure
    {
        public CategoryStructureReorder(ParseFile file)
        {
            Line = file.CurrentLine;
            file.Advance();

            Loaded = true;
        }

        public override void Execute(ref PartTag toCategorize, PartTag rootNode, Dictionary<string, HashSet<AvailablePart>> rootCategories)
        {
            toCategorize.ChildTags.OrderBy(tag => tag.Name);
        }

    }
    class CategoryStructureIcon : CategoryStructure
    {
        public CategoryStructureIcon(ParseFile file)
        {
            Line = file.CurrentLine;
            Name = "Category_"+Line.Substring("ICON".Length);
            file.Advance();

            Loaded = true;
        }

        public override void Execute(ref PartTag toCategorize, PartTag rootNode, Dictionary<string, HashSet<AvailablePart>> rootCategories)
        {
            toCategorize.IconName = Name;
        }

    }

    #endregion

    #region CategoryRule
    class CategoryRule
    {
        public bool Loaded = false;
        public bool Fallback = false;
        public string Line = "";
        protected LinkedList<CategoryRule> ChildRules = new LinkedList<CategoryRule>();

        public virtual void Execute(CategoryRuleContext context)
        {
            foreach (CategoryRule childRule in ChildRules)
            {
                childRule.Execute(context);
            }
        }

        static public CategoryRule ParseLine(ParseFile file)
        {
            string line = file.CurrentLine;
            if (line.StartsWith("RULE", StringComparison.OrdinalIgnoreCase))
            {

                CategoryRule rule = new CategoryRule(file);

                if (rule.Loaded)
                {
                    return rule;
                }
            }
            else if (line.StartsWith("FALLBACK RULE", StringComparison.OrdinalIgnoreCase))
            {

                CategoryRule rule = new CategoryRule(file);
                rule.Fallback = true;

                if (rule.Loaded)
                {

                    return rule;
                }
            }
            else if (line.StartsWith("NODE", StringComparison.OrdinalIgnoreCase))
            {
                CategoryRule rule = new CategoryRuleNode(file, line.Substring("NODE".Length).Trim());

                if (rule.Loaded)
                {

                    return rule;
                }
            }
            else if (line.StartsWith("MODULE", StringComparison.OrdinalIgnoreCase))
            {

                CategoryRule rule = new CategoryRuleModule(file, line.Substring("MODULE".Length).Trim());

                if (rule.Loaded)
                {

                    return rule;
                }
            }
            else if (line.StartsWith("IF", StringComparison.OrdinalIgnoreCase))
            {

                CategoryRule rule = new CategoryRuleIf(file, line.Substring("IF".Length).Trim());

                if (rule.Loaded)
                {
                    return rule;
                }
            }
            else if (line.StartsWith("CATEGORY", StringComparison.OrdinalIgnoreCase))
            {


                CategoryRule rule = new CategoryRuleCategory(file, line.Substring("CATEGORY".Length).Trim());

                if (rule.Loaded)
                {

                    return rule;
                }
            } else if(line.StartsWith("SET VARIABLE",StringComparison.OrdinalIgnoreCase))
            {
                CategoryRule rule = new CategoryRuleSetVariable(file, line.Substring("SET VARIABLE".Length).Trim());
                if(rule.Loaded)
                {
                    return rule;
                }
            }


            Debug.LogError("Unknown Line " + line);
            file.Advance();
            return null;
        }

        public CategoryRule()
        {
        }

        public CategoryRule(ParseFile file)
        {

            Line = file.CurrentLine;
            file.Advance();

            while (!file.EndOfFile)
            {

                string line = file.CurrentLine;

                if (line.StartsWith("END RULE", StringComparison.OrdinalIgnoreCase))
                {

                    file.Advance();
                    Loaded = true;
                    return;
                }

                CategoryRule childRule = ParseLine(file);

                if (childRule != null)
                {

                    ChildRules.AddLast(childRule);
                }
            }
        }
    }

    struct CategoryRuleContext
    {
        public HashSet<string> categories;
        public ConfigNode currentNode;
        public AvailablePart currentPart;
        public Dictionary<string, string> variables;
    }

    class CategoryRuleNode : CategoryRule
    {
        protected string Name;
        public override void Execute(CategoryRuleContext context)
        {
            ConfigNode[] childNodes = context.currentNode.GetNodes(Name);            
            foreach (ConfigNode childNode in childNodes)
            {
                CategoryRuleContext newContext = context;
                newContext.currentNode = childNode;
                foreach (CategoryRule childRule in ChildRules)
                {
                    childRule.Execute(newContext);
                }
            }
        }
        public CategoryRuleNode()
        {
        }
        public CategoryRuleNode(ParseFile file, string name)
        {

            Line = file.CurrentLine;
            file.Advance();

            Name = name;
            while (!file.EndOfFile)
            {
                string line = file.CurrentLine;

                if (line.StartsWith("END NODE", StringComparison.OrdinalIgnoreCase))
                {
                    file.Advance();
                    Loaded = true;
                    return;
                }
                CategoryRule newRule = ParseLine(file);
                if (newRule != null)
                {
                    ChildRules.AddLast(newRule);
                }
            }
        }
    }

    class CategoryRuleModule : CategoryRuleNode
    {
        public override void Execute(CategoryRuleContext context)
        {

            ConfigNode[] childNodes = context.currentNode.GetNodes("MODULE");
            foreach (ConfigNode childNode in childNodes)
            {
                if (childNode.GetValue("name") == Name)
                {
                    CategoryRuleContext newContext = context;
                    newContext.currentNode = childNode;
                    foreach (CategoryRule childRule in ChildRules)
                    {
                        childRule.Execute(newContext);
                    }
                }
            }

            if (PartCategoryRuleHandler.GetLegacyPartModule(context.currentPart) == Name)
            {
                foreach (CategoryRule childRule in ChildRules)
                {
                    childRule.Execute(context);
                }
            }

        }
        public CategoryRuleModule(ParseFile file, string name)
        {

            Line = file.CurrentLine;
            file.Advance();

            Name = name;
            string line;
            while (!file.EndOfFile)
            {
                line = file.CurrentLine;

                if (line.StartsWith("END MODULE", StringComparison.OrdinalIgnoreCase))
                {
                    file.Advance();
                    Loaded = true;
                    return;
                }
                CategoryRule newRule = ParseLine(file);
                if (newRule != null)
                {
                    ChildRules.AddLast(newRule);
                }
            }
        }
     
    }

    abstract class CategoryRuleCondition
    {
        public abstract bool Evaluate(CategoryRuleContext context);

        public static CategoryRuleCondition Parse(string conditionString)
        {
            if (conditionString.IndexOf(" OR ", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string[] splitParts = Regex.Split(conditionString, " OR ", RegexOptions.IgnoreCase);
                CategoryRuleConditionLogic logicContainer = new CategoryRuleConditionLogic();
                logicContainer.Type = CategoryRuleConditionLogic.LogicType.Or;
                for (int i = 0; i < splitParts.Length; i++)
                {
                    logicContainer.Conditions.Add(Parse(splitParts[i].Trim()));
                }
                return logicContainer;
            }
            else if (conditionString.IndexOf(" AND ", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string[] splitParts = Regex.Split(conditionString, " AND ", RegexOptions.IgnoreCase);
                CategoryRuleConditionLogic logicContainer = new CategoryRuleConditionLogic();
                logicContainer.Type = CategoryRuleConditionLogic.LogicType.And;
                for (int i = 0; i < splitParts.Length; i++)
                {
                    logicContainer.Conditions.Add(Parse(splitParts[i].Trim()));
                }
                return logicContainer;
            }
            else if (conditionString.IndexOf("==", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string[] splitParts = conditionString.Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries);
                if (splitParts.Length == 2)
                {
                    return new CategoryRuleConditionCompare(CategoryRuleValue.ParseValue(splitParts[0].Trim()), CategoryRuleValue.ParseValue(splitParts[1].Trim()),CategoryRuleConditionCompare.CompareType.Equal);                    
                }
            }
            else if (conditionString.IndexOf("!=", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string[] splitParts = conditionString.Split(new string[] { "<=" }, StringSplitOptions.RemoveEmptyEntries);
                if (splitParts.Length == 2)
                {
                    return new CategoryRuleConditionCompare(CategoryRuleValue.ParseValue(splitParts[0].Trim()), CategoryRuleValue.ParseValue(splitParts[1].Trim()), CategoryRuleConditionCompare.CompareType.Unequal);                    
                }
            }
            else if (conditionString.IndexOf("<", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string[] splitParts = conditionString.Split(new string[] { "<" }, StringSplitOptions.RemoveEmptyEntries);
                if (splitParts.Length == 2)
                {
                    return new CategoryRuleConditionCompare(CategoryRuleValue.ParseValue(splitParts[0].Trim()), CategoryRuleValue.ParseValue(splitParts[1].Trim()), CategoryRuleConditionCompare.CompareType.Less);
                }
            }
            else if (conditionString.IndexOf("<=", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string[] splitParts = conditionString.Split(new string[] { "<=" }, StringSplitOptions.RemoveEmptyEntries);
                if (splitParts.Length == 2)
                {
                    return new CategoryRuleConditionCompare(CategoryRuleValue.ParseValue(splitParts[0].Trim()), CategoryRuleValue.ParseValue(splitParts[1].Trim()), CategoryRuleConditionCompare.CompareType.LessOrEqual);
                    
                }
            }

            else if (conditionString.IndexOf(">", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string[] splitParts = conditionString.Split(new string[] { ">" }, StringSplitOptions.RemoveEmptyEntries);
                if (splitParts.Length == 2)
                {
                    return new CategoryRuleConditionCompare(CategoryRuleValue.ParseValue(splitParts[0].Trim()), CategoryRuleValue.ParseValue(splitParts[1].Trim()), CategoryRuleConditionCompare.CompareType.Greater);
                }
            }

            else if (conditionString.IndexOf(">=", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string[] splitParts = conditionString.Split(new string[] { ">=" }, StringSplitOptions.RemoveEmptyEntries);
                if (splitParts.Length == 2)
                {
                    return new CategoryRuleConditionCompare(CategoryRuleValue.ParseValue(splitParts[0].Trim()), CategoryRuleValue.ParseValue(splitParts[1].Trim()), CategoryRuleConditionCompare.CompareType.GreaterOrEqual);
                }
            }

            return new CategoryRuleConditionFalse();
        }
    }

    class CategoryRuleConditionFalse : CategoryRuleCondition
    {
        public override bool Evaluate(CategoryRuleContext context)
        {

            return false;
        }
    }

    class CategoryRuleConditionLogic : CategoryRuleCondition
    {
        public enum LogicType
        {
            None,
            And,
            Or
        }
        public List<CategoryRuleCondition> Conditions = new List<CategoryRuleCondition>();
        public LogicType Type = LogicType.None;
        public override bool Evaluate(CategoryRuleContext context)
        {
            if (Conditions.Count > 0)
            {
                switch (Type)
                {
                    case LogicType.None:
                        return false;
                    case LogicType.And:
                        return (Conditions.TrueForAll(condition => condition.Evaluate(context)));
                    case LogicType.Or:
                        return (Conditions.Exists(condition => condition.Evaluate(context)));
                }
            }
            return false;
        }
    }

    abstract class CategoryRuleValue
    {
        public abstract string GetValue(CategoryRuleContext context);



        public static CategoryRuleValue ParseValue(string value)
        {
            if (value.StartsWith(@"%"))
            {
                return new CategoryRuleNodeValue(value.Substring(1));
            }
            else if(value.StartsWith(@"$")) 
            {
                return new CategoryRuleVariableValue(value.Substring(1));
            }
            else
            {
                return new CategoryRuleConstantValue(value);
            }
        }
    }

    class CategoryRuleConstantValue : CategoryRuleValue
    {
        string Value;
        public override string GetValue(CategoryRuleContext context)
        {
            return Value;
        }
        public CategoryRuleConstantValue(string value)
        {
            Value = value.Replace("*","(.*)");
        }
    }
    class CategoryRuleNodeValue : CategoryRuleValue
    {
        string Name;
        public override string GetValue(CategoryRuleContext context)
        {
            if (context.currentNode.name.Equals("part", StringComparison.OrdinalIgnoreCase))
            {
                switch (Name.ToLower()) //Fuck you Squad!
                {
                    case "name":
                        return context.currentPart.name;
                    case "author":
                        return context.currentPart.author;
                    case "techrequired":
                        return context.currentPart.TechRequired;
                    case "entrycost":
                        return context.currentPart.entryCost.ToString();
                    case "category":
                        return context.currentPart.category.ToString();
                    case "cost":
                        return context.currentPart.cost.ToString();
                    case "description":
                        return context.currentPart.description;
                    case "manufacturer":
                        return context.currentPart.manufacturer;
                    case "title":
                        return context.currentPart.title;
                    case "module":
                        return PartCategoryRuleHandler.GetLegacyPartModule(context.currentPart);
                }
            }
            return context.currentNode.GetValue(Name);
        }
        public CategoryRuleNodeValue(string name)
        {
            Name = name;
        }
    }

    class CategoryRuleVariableValue : CategoryRuleValue
    {
                  string Name;
        public override string GetValue(CategoryRuleContext context)
        {
            if(context.variables.ContainsKey(Name))
            {
                return context.variables[Name];
            } else
            {
                return null;
            }
        }
        public CategoryRuleVariableValue(string name)
        {
            Name = name;
        }
    }

    class CategoryRuleConditionCompare : CategoryRuleCondition
    {
        public enum CompareType
        {
            Equal = 0x00,
            Unequal = 0x01,
            Greater = 0x02,
            Less = 0x03,
            GreaterOrEqual = 0x04,            
            LessOrEqual = 0x05
        }

        CompareType Compare;
        bool Invert;
        CategoryRuleValue First;
        CategoryRuleValue Second;
        public override bool Evaluate(CategoryRuleContext context)
        {
            if (First != null && Second != null)
            {
                string firstVal = First.GetValue(context);
                string secondVal = Second.GetValue(context);
                if (firstVal != null && secondVal != null)
                {
                    if(Compare == CompareType.Equal)
                    {
                            Match match = Regex.Match(firstVal, secondVal, RegexOptions.IgnoreCase);
                            return match.Success ^ Invert;
                    }
                    double firstDouble = 0;
                    double secondDouble = 0;
                    try
                    {
                        firstDouble = Convert.ToDouble(firstVal);
                        secondDouble = Convert.ToDouble(secondVal);
                    }
                    catch (System.InvalidCastException e)
                    {
                        return false;
                    }
                    switch (Compare)
                    {
                        
                        case CompareType.Greater:
                            return (firstDouble > secondDouble) ^ Invert;
                        case CompareType.GreaterOrEqual:
                            return (firstDouble >= secondDouble) ^ Invert;
                    }
                }
            }
            return false;
        }
        public CategoryRuleConditionCompare(CategoryRuleValue first, CategoryRuleValue second, CompareType compare)
        {
            First = first;
            Second = second;
            Compare = (CompareType)(((int)compare) & (~0x01));   //Limit to half the types
            if(compare == CompareType.Unequal || compare == CompareType.Less || compare == CompareType.LessOrEqual)
            {
                Invert = true;
            }
        }
    }

    class CategoryRuleIf : CategoryRule
    {
        CategoryRuleCondition Condition;
        LinkedList<CategoryRule> ElseChildRules = new LinkedList<CategoryRule>();
        public CategoryRuleIf(ParseFile file, string conditionString)
        {

            Line = file.CurrentLine;
            file.Advance();

            Condition = CategoryRuleCondition.Parse(conditionString);

            bool elsePath = false;
            string line;
            while (!file.EndOfFile)
            {
                line = file.CurrentLine;

                if (line.StartsWith("ELSE", StringComparison.OrdinalIgnoreCase))
                {
                    file.Advance();
                    elsePath = true;
                    continue;
                }
                else if (line.StartsWith("END IF", StringComparison.OrdinalIgnoreCase))
                {
                    file.Advance();
                    Loaded = true;
                    return;
                }
                CategoryRule newRule = ParseLine(file);
                if (newRule != null)
                {
                    if (elsePath)
                    {
                        ElseChildRules.AddLast(newRule);
                    }
                    else
                    {
                        ChildRules.AddLast(newRule);
                    }
                }
            }
        }
        public override void Execute(CategoryRuleContext context)
        {

            if (Condition != null)
            {
                if (Condition.Evaluate(context))
                {
                    foreach (CategoryRule childRule in ChildRules)
                    {
                        childRule.Execute(context);
                    }
                }
                else
                {
                    foreach (CategoryRule childRule in ElseChildRules)
                    {
                        childRule.Execute(context);
                    }
                }
            }

        }
    }

    class CategoryRuleCategory : CategoryRule
    {
        string Name;
        List<CategoryRuleNodeValue> Keys = new List<CategoryRuleNodeValue>();

        public CategoryRuleCategory(ParseFile file, string category)
        {
            Line = file.CurrentLine;
            file.Advance();

            if (category.Length > 0)
            {
                string[] parts = category.Split('$');
                if (parts.Length > 0)
                {
                    Name = parts[0];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        if (i % 2 == 1)
                        {
                            Keys.Add(new CategoryRuleNodeValue(parts[i]));
                            Name += "{" + ((i - 1) / 2).ToString() + "}";
                        }
                        else
                        {
                            Name += parts[i];
                        }
                    }
                    Loaded = true;
                }
            }

        }
        public override void Execute(CategoryRuleContext context)
        {

            if (Keys.Count > 0)
            {
                List<string> Values = new List<string>();
                foreach (var key in Keys)
                {
                    string value = key.GetValue(context);
                    if (value == null)
                    {
                        return;
                    }
                    Values.Add(value);
                }
                context.categories.Add(String.Format(Name, Values.ToArray()));
                return;
            }
            if (Name != null)
            {
                context.categories.Add(Name);
            }

        }
    }

    class CategoryRuleSetVariable : CategoryRule
    {
        string Name;
        List<CategoryRuleNodeValue> Keys = new List<CategoryRuleNodeValue>();

        public CategoryRuleSetVariable(ParseFile file, string line)
        {
            Line = file.CurrentLine;
            file.Advance();


            //Todo Finish Set variable parsing
                       /*
            if (category.Length > 0)
            {
                string[] parts = category.Split('$');
                if (parts.Length > 0)
                {
                    Name = parts[0];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        if (i % 2 == 1)
                        {
                            Keys.Add(new CategoryRuleNodeValue(parts[i]));
                            Name += "{" + ((i - 1) / 2).ToString() + "}";
                        }
                        else
                        {
                            Name += parts[i];
                        }
                    }
                    Loaded = true;            a
                } 
            }
                 */
        }
        public override void Execute(CategoryRuleContext context)
        {

            if (Keys.Count > 0)
            {
                List<string> Values = new List<string>();
                foreach (var key in Keys)
                {
                    string value = key.GetValue(context);
                    if (value == null)
                    {
                        return;
                    }
                    Values.Add(value);
                }
                context.categories.Add(String.Format(Name, Values.ToArray()));
                return;
            }
            if (Name != null)
            {
                context.categories.Add(Name);
            }

        }
    }
#endregion
}
