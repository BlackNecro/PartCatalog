using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using KSP.IO;

namespace PartCatalog
{
    class PartCategorizer
    {
        static readonly PartCategorizer instance = new PartCategorizer();

        private PartCategorizer()
        {

        }

        public static PartCategorizer Instance
        {
            get
            {
                return instance;
            }
        }

        public void CreatePartTags(PartTag toCategorize)
        {
            Dictionary<string, HashSet<AvailablePart>> categories = PartCategorizer.GetCategories(toCategorize.IncludedParts);
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
                        toCategorize.RemoveParts(categories["MannedPod"]);
                        pod.AddChild(mpod);
                    }
                    if (categories.ContainsKey("UnmannedPod"))
                    {
                        PartTag upod = new PartTag();
                        upod.Name = "Unmanned";
                        upod.AddParts(categories["UnmannedPod"]);
                        toCategorize.RemoveParts(categories["UnmannedPod"]);
                        pod.AddChild(upod);
                    }
                    if (categories.ContainsKey("Seat"))
                    {
                        PartTag seat = new PartTag();
                        seat.Name = "Seats";
                        seat.AddParts(categories["Seat"]);
                        toCategorize.RemoveParts(categories["Seat"]);
                        pod.AddChild(seat);
                    }
                    toCategorize.AddChild(pod);
                }
                if (categories.ContainsKey("Engine"))
                {
                    PartTag engines = new PartTag();
                    engines.Name = "Propulsion";
                    engines.AddParts(categories["Engine"]);
                    toCategorize.RemoveParts(categories["Engine"]);
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
                        toCategorize.RemoveParts(categories["RCS"]);
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
                    toCategorize.AddChild(engines);

                    engines.ChildTags = new LinkedList<PartTag>(engines.ChildTags.OrderBy(sorttag => sorttag.Name));
                }
                if (categories.ContainsKey("Storage"))
                {
                    PartTag storage = new PartTag();
                    storage.Name = "Storage";
                    storage.AddParts(categories["Storage"]);
                    toCategorize.RemoveParts(categories["Storage"]);
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
                    toCategorize.AddChild(storage);
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
                    toCategorize.RemoveParts(categories["Structural"]);
                    toCategorize.AddChild(structural);
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
                    toCategorize.RemoveParts(categories["Aero"]);
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
                    toCategorize.AddChild(aero);
                }
                if (categories.ContainsKey("Utility"))
                {
                    PartTag utils = new PartTag();
                    utils.Name = "Utility";
                    utils.AddParts(categories["Utility"]);
                    toCategorize.RemoveParts(categories["Utility"]);
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
                    toCategorize.AddChild(utils);
                }
                if (categories.ContainsKey("Science"))
                {
                    PartTag science = new PartTag();
                    science.Name = "Science";
                    science.AddParts(categories["Science"]);
                    toCategorize.RemoveParts(categories["Science"]);

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
                    toCategorize.AddChild(science);
                }
                if (toCategorize.IncludedParts.Count > 0)
                {
                    PartTag ungrouped = new PartTag();
                    ungrouped.Name = "Other";
                    ungrouped.AddParts(toCategorize.IncludedParts);
                    toCategorize.RemoveParts(ungrouped.IncludedParts);
                    toCategorize.AddChild(ungrouped);
                }

            }
        }

        public static HashSet<string> GetPartCategories(AvailablePart part)
        {
            HashSet<string> toReturn = new HashSet<string>();

            if (part.partPrefab.Modules != null)
            {
                foreach (PartModule partModule in part.partPrefab.Modules)
                {

                    if (partModule.ClassName == "ModuleCommand" || partModule.ClassName == "ModuleRemoteTechSPU")
                    {

                        if (((ModuleCommand)partModule).minimumCrew == 0)
                        {
                            toReturn.Add("UnmannedPod");
                        }
                        else
                        {
                            toReturn.Add("MannedPod");
                        }
                        toReturn.Add("Pod");
                    }
                    else if (partModule.ClassName == "KerbalSeat")
                    {
                        toReturn.Add("Pod");
                        toReturn.Add("Seat");
                    }
                    else if (partModule.ClassName == "ModuleEngines" || partModule.ClassName == "HydraEngineController")
                    {
                        toReturn.Add("Engine");
                        if (partModule is ModuleEngines)
                        {
                            ((ModuleEngines)partModule).propellants.ForEach(prop => toReturn.Add("EngineProp_" + prop.name));
                        }

                    }
                    else if (partModule.ClassName == "ModuleResourceIntake")
                    {
                        toReturn.Add("Intake");
                        toReturn.Add("Aero");
                    }
                    else if (partModule.ClassName == "ControlSurface" || partModule.ClassName == "FARControllableSurface")
                    {
                        toReturn.Add("ControlSurface");
                        toReturn.Add("Aero");
                    }
                    else if (partModule.ClassName == "Winglet" || partModule.ClassName == "FARWingAerodynamicModel")
                    {
                        toReturn.Add("Winglet");
                        toReturn.Add("Aero");
                    }
                    else if (partModule.ClassName == "ModuleDecouple" || partModule.ClassName == "ModuleAnchoredDecoupler")
                    {
                        toReturn.Add("Decoupler");
                        toReturn.Add("Structural");
                    }
                    else if (partModule.ClassName == "ModuleDockingNode")
                    {
                        toReturn.Add("Docking");
                        toReturn.Add("Utility");
                    }
                    else if (partModule.ClassName == "ModuleRCS")
                    {
                        toReturn.Add("RCS");
                        toReturn.Add("Engines");
                    }
                    else if (partModule.ClassName == "LaunchClamp")
                    {
                        toReturn.Add("Structural");
                        toReturn.Add("Decoupler");
                    }
                    else if (partModule.ClassName == "ModuleDeployableSolarPanel")
                    {
                        toReturn.Add("Solar Panel");
                        toReturn.Add("Generator");
                        toReturn.Add("Utility");
                    }
                    else if (partModule.ClassName == "Strut")
                    {
                        if (part.category == PartCategories.Aero)
                        {
                            toReturn.Add("Aerodynamic");
                            toReturn.Add("Aero");
                        }
                        else
                        {
                            toReturn.Add("Structure");
                            toReturn.Add("Structural");
                        }
                    }
                    else if (partModule.ClassName == "HLandingLeg")
                    {
                        toReturn.Add("Landing Leg");
                        toReturn.Add("Utility");
                        toReturn.Add("LandingAparatus");
                    }
                    else if (partModule.ClassName == "ModuleWheel")
                    {
                        toReturn.Add("Wheel");
                        toReturn.Add("Utility");
                        toReturn.Add("LandingAparatus");
                    }
                    else if (partModule.ClassName == "ModuleLandingGear")
                    {
                        toReturn.Add("Landing Gear");
                        toReturn.Add("Utility");
                        toReturn.Add("LandingAparatus");
                    }
                    else if (partModule.ClassName == "ModuleParachute")
                    {
                        toReturn.Add("Parachute");
                        toReturn.Add("LandingAparatus");
                        toReturn.Add("Utility");
                    }
                    else if (partModule.ClassName == "RetractableLadder")
                    {
                        toReturn.Add("Ladder");
                        toReturn.Add("Utility");
                    }
                    else if (partModule.ClassName == "ModuleGenerator")
                    {
                        toReturn.Add("Generator");
                        toReturn.Add("Utility");
                    }
                    else if (partModule.ClassName == "ModuleLight")
                    {
                        toReturn.Add("Light");
                        toReturn.Add("Utility");
                    }
                    else if (partModule.ClassName == "ModuleEnviroSensor")
                    {
                        toReturn.Add("Sensor");
                        toReturn.Add("Science");
                    }
                    else if (partModule.ClassName == "ProceduralFairingSide" || partModule.ClassName == "ProceduralFairingBase")
                    {
                        toReturn.Add("Decoupler");
                        toReturn.Add("Structural");
                    }
                    else if (partModule.ClassName == "ModuleAnimateGeneric")
                    {
                        if (partModule is ModuleAnimateGeneric)
                        {
                            if (((ModuleAnimateGeneric)partModule).animationName == "antenna" || ((ModuleAnimateGeneric)partModule).animationName == "dish")
                            {
                                toReturn.Add("Antenna");
                                toReturn.Add("Science");
                            }

                        }
                    }
                    else if (partModule.ClassName == "ModuleRTAnimatedAntenna" || partModule.ClassName == "ModuleRTModalAntenna")
                    {
                        toReturn.Add("Antenna");
                        toReturn.Add("Science");
                    }
                    else if (partModule.ClassName == "AdvSASModule" || partModule.ClassName == "SASModule")
                    {
                        toReturn.Add("SAS");
                        toReturn.Add("Utility");
                    }
                }
            }
            if (part.partPrefab is HLandingLeg)
            {
                toReturn.Add("Landing Leg");
                toReturn.Add("Utility");
                toReturn.Add("LandingAparatus");
            }
            if (part.partPrefab is ControlSurface)
            {
                toReturn.Add("ControlSurface");
                toReturn.Add("Aero");
            }
            if (part.partPrefab is Winglet)
            {
                toReturn.Add("Winglet");
                toReturn.Add("Aero");
            }
            if (part.partPrefab is FuelLine)
            {
                toReturn.Add("Storage");
                toReturn.Add("Transfer");
            }
            if (toReturn.Count == 0)
            {
                if (part.partPrefab is Strut)
                {
                    if (part.category == PartCategories.Aero)
                    {
                        toReturn.Add("Aerodynamic");
                        toReturn.Add("Aero");
                    }
                    else if (part.category == PartCategories.Structural)
                    {
                        toReturn.Add("Structure");
                        toReturn.Add("Structural");
                    }
                    else if (part.category == PartCategories.Utility)
                    {
                        toReturn.Add("Utility");
                        toReturn.Add("Ladder");
                    }
                }
                else if (part.partPrefab.Resources.Count > 0)
                {
                    toReturn.Add("Storage");
                    part.partPrefab.Resources.list.ForEach(res => toReturn.Add("Storage_" + res.resourceName));
                }
                else if (part.category == PartCategories.Structural)
                {
                    toReturn.Add("Structure");
                    toReturn.Add("Structural");
                }
                else if (part.category == PartCategories.Aero)
                {
                    toReturn.Add("Aerodynamic");
                    toReturn.Add("Aero");
                }
                else if (part.category == PartCategories.Science)
                {
                    toReturn.Add("MiscScience");
                    toReturn.Add("Science");
                }
                else if (part.category == PartCategories.Utility)
                {
                    toReturn.Add("MiscUtils");
                    toReturn.Add("Utility");
                }
                else if (part.category == PartCategories.Control)
                {
                    toReturn.Add("Utility");
                    toReturn.Add("Control");
                }
                else
                {
                    toReturn.Add("Ungrouped");
                }
            }
            return toReturn;
        }

        public static Dictionary<string, HashSet<AvailablePart>> GetCategories(HashSet<AvailablePart> parts)
        {
            Dictionary<string, HashSet<AvailablePart>> toReturn = new Dictionary<string, HashSet<AvailablePart>>();
            foreach (AvailablePart part in parts)
            {
                HashSet<string> categories = GetPartCategories(part);

                foreach (string category in categories)
                {
                    if (!toReturn.ContainsKey(category))
                    {
                        toReturn[category] = new HashSet<AvailablePart>();
                    }
                    toReturn[category].Add(part);
                }
            }
            return toReturn;
        }

        public static void printConfigNode(ConfigNode node, string indent, TextWriter file)
        {

            file.WriteLine(indent + "ID: " + node.id + " Name: " + node.name);

            file.WriteLine(indent + "Values: ");
            foreach (ConfigNode.Value val in node.values)
            {
                file.WriteLine(indent + "val name: " + val.name + " value " + val.value);
            }
            file.WriteLine(indent + "Sub Nodes: ");
            foreach (ConfigNode nod in node.nodes)
            {
                file.WriteLine(indent + "Sub Node");
                printConfigNode(nod, indent + " ", file);
            }
            file.WriteLine(indent + "End");
        }

        public static void printPartInfo(AvailablePart part)
        {
            TextWriter file = TextWriter.CreateForType<PartCatalog>("catalogConfig.txt");
            //UnityEngine.Debug.Log(part.partPrefab.);    
            //printConfigNode(GameDatabase.Instance.GetConfigNode(part.partUrl),"",file);   
            UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("PART");
            UrlDir.UrlConfig config = Array.Find<UrlDir.UrlConfig>(configs, (c => part.name == c.name.Replace('_', '.')));

            file.WriteLine("New Config: " + config.name);
            file.WriteLine("URL : " + config.url);
            printConfigNode(config.config, " ", file);
            file.Flush();
            file.Close();
        }
    }
}
