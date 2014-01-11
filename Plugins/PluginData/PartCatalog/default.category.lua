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

	local maxOverlayLength = 4
	local overlay = ""

	local curPos = 1

	while overlay:len() < maxOverlayLength do
		local capitalIndex = mod:find("%u",curPos)
		local lowerIndex = mod:find("%l",curPos)
		local numIndex = mod:find("%d",curPos)

		if capitalIndex and ((not numIndex) or capitalIndex < numIndex) then
			curPos = capitalIndex + 1
			overlay = overlay .. mod:sub(capitalIndex,capitalIndex)
		elseif overlay:len() > 0 and numIndex and ((not capitalindex) or capitalIndex > numIndex) then
			curPos = numIndex + 1
			overlay = overlay .. mod:sub(numIndex,numIndex)
		elseif lowerIndex and overlay:len() == 0 then
			overlay = overlay .. mod:sub(lowerIndex,lowerIndex)
			break
		else
			break
		end
	end


	setOverlay(mod,overlay)
	setCategory(mod, mod, "Mods/"..mod)

	setCategory(mod .. "/Pod" , "Pod", "Categories/Pod")
	setCategory(mod .. "/Pod/Manned","Manned","Categories/MannedPod")
	setCategory(mod .. "/Pod/Unmanned","Unmanned","Categories/UnmannedPod")
	setCategory(mod .. "/Pod/Seat","Seat","Categories/Seat")

	setCategory(mod .. "/Engine","Engine","Categories/Engine",true)
	setCategory(mod .. "/Engine/LFOX","Rocket","Categories/EngineLFOX")
	setCategory(mod .. "/Engine/Jet","Jet","Categories/EngineJet")
	setCategory(mod .. "/Engine/Ion","Ion","Categories/EngineIon")
	setCategory(mod .. "/Engine/MultiMode","MultiMode","Categories/EngineMultiMode")

	setCategory(mod .. "/Storage","Storage","Categories/Storage",true)
	setCategory(mod .. "/Storage/Transfer","Transfer","Categories/FuelTransfer")
	setCategory(mod .. "/Storage/LFOX","Rocket Fuel","Categories/StorageLFOX")
	setCategory(mod .. "/Storage/LF","Jet Fuel","Categories/StorageLiquidFuel")
	setCategory(mod .. "/Storage/OX","Oxidizer","Categories/StorageOxidizier")
	setCategory(mod .. "/Storage/_MonoPropellant","MonoPropellant","Categories/StorageMonoPropellant")
	setCategory(mod .. "/Storage/ServiceModule","ServiceModule","Categories/StorageServiceModule")

	setCategory(mod .. "/Control","Control","Categories/Control")
	setCategory(mod .. "/Control/SAS","SAS","Categories/SAS")
	setCategory(mod .. "/Control/RCS","RCS","Categories/RCS",true)
	setCategory(mod .. "/Structural","Structural", "Categories/Structural")
	setCategory(mod .. "/Structural/Misc","Main", "Categories/Main")
	setCategory(mod .. "/Structural/Decoupler","Decoupler", "Categories/Decoupler")
	setCategory(mod .. "/Structural/Decoupler/Separator","Separator", "Categories/Separator")
	setCategory(mod .. "/Structural/Decoupler/Stack","Stack", "Categories/DecouplerStack")
	setCategory(mod .. "/Structural/Decoupler/Radial","Radial", "Categories/DecouplerRadial")
	setCategory(mod .. "/Structural/GroundSupport","Ground Support", "Categories/GroundSupport")
	setCategory(mod .. "/Structural/StrutConnector","Strut Connector", "Categories/StrutConnector")

	setCategory(mod .. "/Aero","Aero","Categories/Aero")
	setCategory(mod .. "/Aero/ControlSurface","Control Surface","Categories/ControlSurface")
	setCategory(mod .. "/Aero/Winglet","Winglet", "Categories/Winglet")
	setCategory(mod .. "/Aero/Wing","Wing", "Categories/Wing")
	setCategory(mod .. "/Aero/NoseCone","Nose Cone","Categories/NoseCone")
	setCategory(mod .. "/Aero/Misc","Misc", "Categories/AeroMisc")

	setCategory(mod .. "/Utility","Utility", "Categories/Utility")
	setCategory(mod .. "/Utility/Docking","Docking", "Categories/Docking",true)
	setCategory(mod .. "/Utility/Docking/Misc","Misc", "Categories/Docking")
	setCategory(mod .. "/Utility/Landing","Landing","Categories/Landing")
	setCategory(mod .. "/Utility/Landing/LandingLeg","Landing Leg","Categories/LandingLeg")
	setCategory(mod .. "/Utility/Landing/Wheel","Wheel","Categories/Wheel")
	setCategory(mod .. "/Utility/Landing/LandingGear","Landing Gear","Categories/LandingGear")
	setCategory(mod .. "/Utility/Landing/Parachute","Parachute","Categories/Parachute")
	setCategory(mod .. "/Utility/Ladder/Retractable","Retractable","Categories/RetractableLadder")
	setCategory(mod .. "/Utility/Ladder/Static","Static","Categories/StaticLadder")
	setCategory(mod .. "/Utility/Generator","Generator","Categories/Generator",true)
	setCategory(mod .. "/Utility/SolarPanel","Solar Panel","Categories/SolarPanel")
	setCategory(mod .. "/Utility/SolarPanel/Static","Static", "Categories/SolarPanelStatic")
	setCategory(mod .. "/Utility/SolarPanel/Deployable","Deployable", "Categories/SolarPanelDeployable")
	setCategory(mod .. "/Utility/Light","Light","Categories/Light")
	setCategory(mod .. "/Utility/Misc","Misc", "Categories/UtilityMisc")

	setCategory(mod .. "/Science","Science","Categories/Science")
	setCategory(mod .. "/Science/Sensor","Sensor","Categories/Sensor")
	setCategory(mod .. "/Science/Antenna","Antenna","Categories/Antenna")
	setCategory(mod .. "/Science/Experiment","Experiment","Categories/Experiment")
	setCategory(mod .. "/Science/Lab","Lab","Categories/Lab")
	setCategory(mod .. "/Science/Misc","Misc","Categories/ScienceMisc")

end

DefaultTags("All")

DefaultTags("Squad")
for _,mod in pairs(modsSorted) do
	DefaultTags(mod)
end
