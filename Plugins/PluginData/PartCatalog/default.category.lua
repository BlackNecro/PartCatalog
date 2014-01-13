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

        local maxOverlayLength = 3
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
	
	-- Pods
	setCategory(mod .. "/Pod" , "Pod", "Categories/Pod")
		setCategory(mod .. "/Pod/Manned","Manned","Categories/MannedPod")
		setCategory(mod .. "/Pod/Unmanned","Unmanned","Categories/UnmannedPod")
		setCategory(mod .. "/Pod/Seat","Seat","Categories/Seat")
	
	-- Engines
	setCategory(mod .. "/Engine","Engine","Categories/Engine")
		setCategory(mod .. "/Engine/LFOX","Rocket","Categories/Engine_LFOX")  
		setCategory(mod .. "/Engine/SRB","SRB","Categories/Engine_SRB") -- EDIT
		setCategory(mod .. "/Engine/Jet","Jet","Categories/Engine_Jet")
		setCategory(mod .. "/Engine/Ion","Ion","Categories/Engine_Ion")
		setCategory(mod .. "/Engine/MonoProp","MonoPropellant","Categories/Engine_MonoPropellant") -- EDIT
		setCategory(mod .. "/Engine/MultiMode","MultiMode","Categories/Engine_MultiMode")
	
	--Storage
	setCategory(mod .. "/Storage","Storage","Categories/Storage_LFOX")
		setCategory(mod .. "/Storage/Transfer","Transfer","Categories/FuelTransfer")
		setCategory(mod .. "/Storage/LFOX","Rocket Fuel","Categories/Storage_LFOX")
		setCategory(mod .. "/Storage/LF","Jet Fuel","Categories/Storage_LF")
		setCategory(mod .. "/Storage/OX","Oxidizer","Categories/Storage_OX")
		setCategory(mod .. "/Storage/_MonoPropellant","Mono Propellant","Categories/Storage_MonoPropellant")
		setCategory(mod .. "/Storage/_ElectricCharge","Electric Charge","Categories/Storage_EC_Battery")
		setCategory(mod .. "/Storage/XenonGas","XenonGas","Categories/Storage_Xenon") -- EDIT
		setCategory(mod .. "/Storage/ServiceModule","ServiceModule","Categories/Storage_Storage_Xenon_Off")
		--[[
		setCategory(mod .. "/Storage/Echarge","ElectricCharge","Categories/Electricity") -- EDIT
			setCategory(mod .. "/Storage/Echarge/BatteryPack","BatteryPack","Categories/Storage_EC_Battery") -- EDIT
			setCategory(mod .. "/Storage/Echarge/Misc","Misc","Categories/Electricity") -- EDIT]]
	
	-- Control
	setCategory(mod .. "/Control","Control","Categories/Control")
		setCategory(mod .. "/Control/SAS","SAS","Categories/SAS")
		setCategory(mod .. "/Control/RCS","RCS","Categories/RCS",true)
		setCategory(mod .. "/Control/Misc","Misc","Categories/Control") -- EDIT
	
	-- Structural
	setCategory(mod .. "/Structural","Structural", "Categories/Structural")
		setCategory(mod .. "/Structural/Misc","Main", "Categories/Structural")
		setCategory(mod .. "/Structural/Decoupler","Decoupler", "Categories/Decoupler")
			setCategory(mod .. "/Structural/Decoupler/Separator","Separator", "Categories/Separator")
			setCategory(mod .. "/Structural/Decoupler/Stack","Stack", "Categories/Decoupler")
			setCategory(mod .. "/Structural/Decoupler/Radial","Radial", "Categories/DecouplerRadial")
		setCategory(mod .. "/Structural/GroundSupport","Ground Support", "Categories/GroundSupport")
		setCategory(mod .. "/Structural/StrutConnector","Strut Connector", "Categories/StrutConnector")

	-- Aero
	setCategory(mod .. "/Aero","Aero","Categories/Aero")
		setCategory(mod .. "/Aero/ControlSurface","Control Surface","Categories/Aero_ControlSurface")
		setCategory(mod .. "/Aero/Winglet","Winglet", "Categories/Aero_Winglet")
		setCategory(mod .. "/Aero/Wing","Wing", "Categories/Aero_Wing")
		setCategory(mod .. "/Aero/Intake","AirIntake","Categories/AirIntake") -- EDIT
		setCategory(mod .. "/Aero/NoseCone","Nose Cone","Categories/Aero_NoseCone")
		setCategory(mod .. "/Aero/Misc","Misc", "Categories/Aero")
		

	-- Utility
	setCategory(mod .. "/Utility","Utility", "Categories/Utility")
		setCategory(mod .. "/Utility/Docking","Docking", "Categories/Docking",true)
		setCategory(mod .. "/Utility/Docking/Misc","Misc", "Categories/Docking")
		setCategory(mod .. "/Utility/Landing","Landing","Categories/Landing") -- Icon fehlt
			setCategory(mod .. "/Utility/Landing/LandingLeg","Landing Leg","Categories/LandingLeg")
			setCategory(mod .. "/Utility/Landing/Wheel","Wheel","Categories/Wheel")
			setCategory(mod .. "/Utility/Landing/LandingGear","Landing Gear","Categories/LandingGear")
			setCategory(mod .. "/Utility/Landing/Parachute","Parachute","Categories/Parachute")
		setCategory(mod .. "/Utility/Ladder/Retractable","Retractable","Categories/RetractableLadder") -- Icon fehlt
		setCategory(mod .. "/Utility/Ladder/Static","Static","Categories/StaticLadder")
	
	-- Utility_Electricity
	setCategory(mod .. "/Utility/Generator","Generator","Categories/Generator",true)
		setCategory(mod .. "/Utility/SolarPanel","Solar Panel","Categories/SolarPanel")
			setCategory(mod .. "/Utility/SolarPanel/Static","Static", "Categories/SolarPanelStatic") -- Icon fehlt
			setCategory(mod .. "/Utility/SolarPanel/Deployable","Deployable", "Categories/SolarPanelDeployable") -- Icon fehlt
		setCategory(mod .. "/Utility/Light","Light","Categories/Light")
		setCategory(mod .. "/Utility/Misc","Misc", "Categories/Utility_Misc") -- Icon fehlt

	-- Utility_Science
	setCategory(mod .. "/Science","Science","Categories/Science")
	setCategory(mod .. "/Science/Sensor","Sensor","Categories/Sensor")
	setCategory(mod .. "/Science/Antenna","Antenna","Categories/Antenna")
	-- Omni, Dish, Com fehlt
	setCategory(mod .. "/Science/Experiment","Experiment","Categories/Experiment")
	setCategory(mod .. "/Science/Lab","Lab","Categories/Lab")
	setCategory(mod .. "/Science/Misc","Misc","Categories/ScienceMisc") -- Icon fehlt

end

DefaultTags("All")

DefaultTags("Squad")
for _,mod in pairs(modsSorted) do
	DefaultTags(mod)
end
