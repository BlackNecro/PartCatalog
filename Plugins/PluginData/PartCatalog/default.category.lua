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
	setCategory(mod .. "/Pod" , "Pod", "Categories/Pods/Pod")
		setCategory(mod .. "/Pod/Manned","Manned","Categories/Pods/MannedPod")
		setCategory(mod .. "/Pod/Unmanned","Unmanned","Categories/Pods/UnmannedPod")
		setCategory(mod .. "/Pod/Seat","Seat","Categories/Pods/Seat")
		setCategory(mod .. "/Pod/Misc","Misc","Categories/Pods/Misc")							-- EDIT added Pods/Misc Cat
	
	-- Engines
	setCategory(mod .. "/Engine","Engine","Categories/Engine/Engine")
		setCategory(mod .. "/Engine/LFOX","Rocket","Categories/Engine/LFOX")  
		setCategory(mod .. "/Engine/SRB","SRB","Categories/Engine/SRB") 
		setCategory(mod .. "/Engine/Jet","Jet","Categories/Engine/Jet")
		setCategory(mod .. "/Engine/Ion","Ion","Categories/Engine/Ion")
		setCategory(mod .. "/Engine/MonoProp","MonoPropellant","Categories/Engine/MonoPropellant") 
		setCategory(mod .. "/Engine/MultiMode","MultiMode","Categories/Engine/RamJet")					-- EDIT RamJet Icon
	
	--Storage
	setCategory(mod .. "/Storage","Storage","Categories/Storage/LFOX")
		setCategory(mod .. "/Storage/LFOX","Rocket Fuel","Categories/Storage/LFOX")
		setCategory(mod .. "/Storage/LF","Jet Fuel","Categories/Storage/LF")
		setCategory(mod .. "/Storage/OX","Oxidizer","Categories/Storage/OX")
		setCategory(mod .. "/Storage/_MonoPropellant","MonoPropellant","Categories/Storage/MonoPropellant")
		setCategory(mod .. "/Storage/_XenonGas","XenonGas","Categories/Storage/Xenon")
		setCategory(mod .. "/Storage/ServiceModule","ServiceModule","Categories/Storage/ServiceModule")
		setCategory(mod .. "/Storage/_ElectricCharge","Electric Charge","Categories/Storage/EC_Battery")
		setCategory(mod .. "/Storage/Transfer","Transfer","Categories/Storage/FuelTransfer")
		--[[
		setCategory(mod .. "/Storage/Echarge","ElectricCharge","Categories/Utility/Electricity") -- EDIT
			setCategory(mod .. "/Storage/Echarge/BatteryPack","BatteryPack","Categories/Storage/EC_Battery") -- EDIT
			setCategory(mod .. "/Storage/Echarge/Misc","Misc","Categories/Utility/Electricity") -- EDIT
		]]
		
	
	-- Control
	setCategory(mod .. "/Control","Control","Categories/Control/Control")
		setCategory(mod .. "/Control/SAS","SAS","Categories/Control/SAS")
		setCategory(mod .. "/Control/RCS","RCS","Categories/Control/RCS")
			setCategory(mod .. "/Control/RCS/MonoPropellant","MonoPropellant","Categories/Control/RCS_MonoProp")	 -- EDIT added RCS-MonoProp
		setCategory(mod .. "/Control/Misc","Misc","Categories/Control/Control")
	
	-- Structural
	setCategory(mod .. "/Structural","Structural", "Categories/Structural/Structural")
		setCategory(mod .. "/Structural/Misc","Main", "Categories/Structural/Structural")
		setCategory(mod .. "/Structural/Decoupler","Decoupler", "Categories/Structural/Decoupler")
			setCategory(mod .. "/Structural/Decoupler/Separator","Separator", "Categories/Structural/Separator")
			setCategory(mod .. "/Structural/Decoupler/Stack","Stack", "Categories/Structural/Decoupler")
			setCategory(mod .. "/Structural/Decoupler/Radial","Radial", "Categories/Structural/DecouplerRadial")
		setCategory(mod .. "/Structural/Adapter","Adapter","Categories/Structural/Adapter")				-- EDIT added Adapter Cat
		setCategory(mod .. "/Structural/Coupler","Coupler","Categories/Structural/Coupler")				-- EDIT added Coupler Cat
		setCategory(mod .. "/Structural/Compartment","Compartment","Categories/Structural/CargoBay")			-- EDIT added Cargo-Bay & -Compartment Cat
		setCategory(mod .. "/Structural/GroundSupport","Ground Support", "Categories/Structural/GroundSupport")
		setCategory(mod .. "/Structural/StrutConnector","Strut Connector", "Categories/Structural/StrutConnector")

	-- Aero
	setCategory(mod .. "/Aero","Aero","Categories/Aero/Aero")
		setCategory(mod .. "/Aero/ControlSurface","Control Surface","Categories/Aero/ControlSurface")
		setCategory(mod .. "/Aero/Winglet","Winglet", "Categories/Aero/Winglet")
		setCategory(mod .. "/Aero/Wing","Wing", "Categories/Aero/Wing")
		setCategory(mod .. "/Aero/Intake","AirIntake","Categories/Aero/AirIntake")
		setCategory(mod .. "/Aero/NoseCone","Nose Cone","Categories/Aero/NoseCone")
		setCategory(mod .. "/Aero/Fairing","Fairing","Categories/Aero/Fairing")						 -- EDIT added Fairing Cat
		setCategory(mod .. "/Aero/Misc","Misc", "Categories/Aero/Aero")
		


	-- Utility
	setCategory(mod .. "/Utility","Utility", "Categories/Utility/Utility")
		setCategory(mod .. "/Utility/Docking","Docking", "Categories/Utility/Docking")
		setCategory(mod .. "/Utility/Docking/Misc","Misc", "Categories/Utility/Docking")
		setCategory(mod .. "/Utility/Landing","Landing","Categories/Utility/Parachute")					 -- EDIT set Parachute as Icon
			setCategory(mod .. "/Utility/Landing/LandingLeg","Landing Leg","Categories/Utility/LandingLeg")
			setCategory(mod .. "/Utility/Landing/Wheel","Wheel","Categories/Utility/Wheel")
			setCategory(mod .. "/Utility/Landing/LandingGear","Landing Gear","Categories/Utility/LandingGear")
			setCategory(mod .. "/Utility/Landing/Parachute","Parachute","Categories/Utility/Parachute")
		setCategory(mod .. "/Utility/Ladder","Ladder","Categories/Utility/LadderStatic") 
		setCategory(mod .. "/Utility/Ladder/Retractable","Retractable","Categories/Utility/LadderRetract") 
		setCategory(mod .. "/Utility/Ladder/Static","Static","Categories/Utility/LadderStatic")
	
	-- Utility_Electricity
		setCategory(mod .. "/Utility/Generator","Generator","Categories/Utility/Generator")
			setCategory(mod .. "/Utility/Generator/RTG","RTG","Categories/Utility/Generator_Nuc")
		setCategory(mod .. "/Utility/Converter","Converter","Categories/Utility/Converter/Converter")
		setCategory(mod .. "/Utility/SolarPanel","Solar Panel","Categories/Utility/SolarPanel")
			setCategory(mod .. "/Utility/SolarPanel/Static","Static", "Categories/Utility/SolarPanel")
			setCategory(mod .. "/Utility/SolarPanel/Deployable","Tracking", "Categories/Utility/SolarPanel_Track")
		setCategory(mod .. "/Utility/Light","Light","Categories/Utility/Light")
		setCategory(mod .. "/Utility/Misc","Misc", "Categories/Utility/Utility")

	-- Utility_Science
	setCategory(mod .. "/Science","Science","Categories/Science/Science")
	setCategory(mod .. "/Science/Sensor","Sensor","Categories/Science/Sensor")
	setCategory(mod .. "/Science/Antenna","Antenna","Categories/Science/Antenna")
	-- Omni, Dish, Com fehlt
	setCategory(mod .. "/Science/Experiment","Experiment","Categories/Science/Experiment")
	setCategory(mod .. "/Science/Lab","Lab","Categories/Science/Lab")
	setCategory(mod .. "/Science/Misc","Misc","Categories/Science/Science")

end

DefaultTags("All")

DefaultTags("Squad")
for _,mod in pairs(modsSorted) do
	DefaultTags(mod)
end
