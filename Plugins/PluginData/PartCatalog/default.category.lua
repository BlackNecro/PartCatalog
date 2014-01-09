local mods = {}

for name, part in pairs(PARTS) do
	mods[part.mod] = true
end
mods["Squad"] = nil
local modsSorted = {}

for mod,_ in pairs(mods) do
	modsSorted[#modsSorted+1] = mod
end

table.sort(modsSorted)


function DefaultTags(mod)	
	print(mod)
	setCategory(mod, mod, mod)
	
	setCategory(mod .. "/Pod" , "Pod", "Pod")
	setCategory(mod .. "/Pod/Manned","Manned","MannedPod")
	setCategory(mod .. "/Pod/Unmanned","Unmanned","UnmannedPod")
	setCategory(mod .. "/Pod/Seat","Seat","Seat")
	
	setCategory(mod .. "/Engine","Engine","Engine",true)
	setCategory(mod .. "/Engine/LFOX","Rocket","EngineLFOX")
	setCategory(mod .. "/Engine/Jet","Jet","EngineJet")	
	setCategory(mod .. "/Engine/Ion","Ion","EngineIon")
	setCategory(mod .. "/Engine/MultiMode","MultiMode","EngineMultiMode")
	
	setCategory(mod .. "/Storage","Storage","Storage",true)
	setCategory(mod .. "/Storage/Transfer","Transfer","FuelTransfer")
	setCategory(mod .. "/Storage/LFOX","Rocket Fuel","StorageLFOX")
	setCategory(mod .. "/Storage/LF","Jet Fuel","StorageLiquidFuel")
	setCategory(mod .. "/Storage/OX","Oxidizer","StorageOxidizier")
	setCategory(mod .. "/Storage/_MonoPropellant","MonoPropellant","StorageMonoPropellant")	
	setCategory(mod .. "/Storage/ServiceModule","ServiceModule","StorageServiceModule")
	
	setCategory(mod .. "/Control","Control","Control")
	setCategory(mod .. "/Control/SAS","SAS","SAS")
	setCategory(mod .. "/Control/RCS","RCS","RCS",true)
	setCategory(mod .. "/Structural","Structural", "Structural")
	setCategory(mod .. "/Structural/Misc","Main", "Main")
	setCategory(mod .. "/Structural/Decoupler","Decoupler", "Decoupler")
	setCategory(mod .. "/Structural/Decoupler/Separator","Separator", "Separator")
	setCategory(mod .. "/Structural/Decoupler/Stack","Stack", "DecouplerStack")
	setCategory(mod .. "/Structural/Decoupler/Radial","Radial", "DecouplerRadial")
	setCategory(mod .. "/Structural/GroundSupport","Ground Support", "GroundSupport")		
	setCategory(mod .. "/Structural/StrutConnector","Strut Connector", "StrutConnector")
	
	setCategory(mod .. "/Aero","Aero","Aero")
	setCategory(mod .. "/Aero/ControlSurface","Control Surface","ControlSurface")
	setCategory(mod .. "/Aero/Winglet","Winglet", "Winglet")
	setCategory(mod .. "/Aero/Wing","Wing", "Wing")
	setCategory(mod .. "/Aero/NoseCone","Nose Cone","NoseCone")
	setCategory(mod .. "/Aero/Misc","Misc", "AeroMisc")
	
	setCategory(mod .. "/Utility","Utility", "Utility")	
	setCategory(mod .. "/Utility/Docking","Docking", "Docking",true)	
	setCategory(mod .. "/Utility/Docking/Misc","Misc", "Docking")	
	setCategory(mod .. "/Utility/Landing","Landing","Landing")
	setCategory(mod .. "/Utility/Landing/LandingLeg","Landing Leg","LandingLeg")
	setCategory(mod .. "/Utility/Landing/Wheel","Wheel","Wheel")
	setCategory(mod .. "/Utility/Landing/LandingGear","Landing Gear","LandingGear")
	setCategory(mod .. "/Utility/Landing/Parachute","Parachute","Parachute")
	setCategory(mod .. "/Utility/Ladder/Retractable","Retractable","RetractableLadder")
	setCategory(mod .. "/Utility/Ladder/Static","Static","StaticLadder")
	setCategory(mod .. "/Utility/Generator","Generator","Generator",true)
	setCategory(mod .. "/Utility/SolarPanel","Solar Panel","SolarPanel")
	setCategory(mod .. "/Utility/SolarPanel/Static","Static", "SolarPanelStatic")
	setCategory(mod .. "/Utility/SolarPanel/Deployable","Deployable", "SolarPanelDeployable")
	setCategory(mod .. "/Utility/Light","Light","Light")
	setCategory(mod .. "/Utility/Misc","Misc", "UtilityMisc")	
	
	setCategory(mod .. "/Science","Science","Science")
	setCategory(mod .. "/Science/Sensor","Sensor","Sensor")
	setCategory(mod .. "/Science/Antenna","Antenna","Antenna")
	setCategory(mod .. "/Science/Experiment","Experiment","Experiment")
	setCategory(mod .. "/Science/Lab","Lab","Lab")
	setCategory(mod .. "/Science/Misc","Misc","ScienceMisc")
	
end

DefaultTags("All")

DefaultTags("Squad")
for _,mod in pairs(modsSorted) do
	DefaultTags(mod)
end
