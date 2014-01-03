using System;
using System.Collections.Generic;
using System.Text;
using KSP.IO;

namespace PartCatalog
{
    class PartCategorizer
    {
        static readonly PartCategorizer instance = new PartCategorizer();

        private PartCategorizer()
        {

        }

        public PartCategorizer Instance
        {
            get
            {
                return instance;
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
