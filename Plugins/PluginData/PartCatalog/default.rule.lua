for name,part in pairs(PARTS) do	
	
	--Command Modules
	local hasCommand = false
	for module in modulesByName(part,"ModuleCommand") do		
		hasCommand = true
		if module.values.minimumCrew ~= "0" then			
			hasCommand = false
			addToModCategory(part,"Pod/Manned")
			break
		end		
	end
	if hasCommand then
		addToModCategory(part,"Pod/Unmanned")
	end
	
	if containsModule(part,"KerbalSeat") then
		addToModCategory(part,"Pod/Seat")
	end
	
	
	--Engines
	for engine in modulesByName(part,"ModuleEngines") do	
		
		local special = false
		if containsNodeTypeName(engine,"PROPELLANT","LiquidFuel") then
			if containsNodeTypeName(engine,"PROPELLANT","Oxidizer") then
				addToModCategory(part,"Engine/LFOX")		
			elseif containsNodeTypeName(engine,"PROPELLANT","IntakeAir") then
				addToModCategory(part,"Engine/Jet")
			else
				special = true
			end
		elseif containsNodeTypeName(engine,"PROPELLANT","XenonGas") and containsNodeTypeName(engine,"PROPELLANT","ElectricCharge") then
			addToModCategory(part,"Engine/Ion")
		elseif containsNodeTypeName(engine,"PROPELLANT","SolidFuel") then 	-- EDIT START
				addToModCategory(part,"Engine/SRB")
		elseif containsNodeTypeName(engine,"PROPELLANT","MonoPropellant") then 
				addToModCategory(part,"Engine/MonoProp")		-- EDIT END
		else
			special = true
		end
		if special then
			for propellant in nodesByName(engine,"PROPELLANT") do
				addToModCategory(part,"Engine/_"..propellant.values.name,propellant.values.name,"Categories/Engine"..propellant.values.name)					
			end
		end		
	end
	for engine in modulesByName(part,"MultiModeEngine") do
		addToModCategory(part,"Engine/MultiMode")				
	end
	
	--Intakes	
	for intake in modulesByName(part,"ModuleResourceIntake") do
		addToModCategory(part,"Aero/Intake/_"..intake.values.resourceName,intake.values.resourceName,"Categories/Intake"..intake.values.resourceName)					
	end
	
	--Control Surfaces
	for controlSurface in modulesByName(part,"ModuleControlSurface") do
		addToModCategory(part,"Aero/ControlSurface")					
	end
	
	--Wing/let
	if part.dragModelType == "override" and part.deflectionLiftCoeff then
		local coeff = tonumber(part.deflectionLiftCoeff) 
		if coeff then
			if coeff < 1 then
				addToModCategory(part,"Aero/Winglet")
			else
				addToModCategory(part,"Aero/Wing")									
			end
		end
	end
	
	
	
	--Decoupler
	for decoupler in modulesByName(part,"ModuleDecouple") do
		if decoupler.values.isOmniDecoupler == "true" then
			addToModCategory(part,"Structural/Decoupler/Separator")
		else
			addToModCategory(part,"Structural/Decoupler/Stack")				
		end
	end
	if containsModule(part,"ModuleAnchoredDecoupler") then	
		addToModCategory(part,"Structural/Decoupler/Radial")				
	end
	
	--Docking
	for docking in modulesByName(part,"ModuleDockingNode") do
		if docking.values.nodeType and string.sub(docking.values.nodeType,1,4) == "size" then
			local size = string.sub(docking.values.nodeType,5)
			addToModCategory(part,"Utility/Docking/Size"..size,"Size "..size, "Categories/Docking"..size)							
		else
			addToModCategory(part,"Utility/Docking/Misc")		
		end
	end
	
	--RCS
	for rcs in modulesByName(part,"ModuleRCS") do
		addToModCategory(part,"Control/RCS/"..rcs.values.resourceName,rcs.values.resourceName, "Categories/RCS"..rcs.values.resourceName)						
	end
	
	--Clamp
	if containsModule(part,"LaunchClamp") then	
		addToModCategory(part,"Structural/GroundSupport")				
	end	
	
	--Strut
	if containsModule(part,"StrutConnector") then	
		addToModCategory(part,"Structural/StrutConnector")
	end
	
	--Solar Panel
	for panel in modulesByName(part,"ModuleDeployableSolarPanel") do
		if panel.values.sunTracking == "false" then
			addToModCategory(part,"Utility/SolarPanel/Static")
		else			
			addToModCategory(part,"Utility/SolarPanel/Deployable")
		end
	end
	
	if part.category == "Aero" then
		if part.name:lower():find("nose") or part.description:lower():find("nose") then
			addToModCategory(part,"Aero/NoseCone")
		end
	end
	
	--Landing Leg
	if containsModule(part,"ModuleLandingLeg") or containsModule(part,"HLandingLeg") then	
		addToModCategory(part,"Utility/Landing/LandingLeg")
	end

	--Wheel
	if containsModule(part,"ModuleWheel") then	
		addToModCategory(part,"Utility/Landing/Wheel")
	end	

	--Landing Gear
	if containsModule(part,"ModuleLandingGear") then	
		addToModCategory(part,"Utility/Landing/LandingGear")
	end	

	--Parachute	
	if containsModule(part,"ModuleParachute") then	
		addToModCategory(part,"Utility/Landing/Parachute")
	end	

	--Lights
	if containsModule(part,"ModuleLight") then	
		addToModCategory(part,"Utility/Light")
	end	
	--Sensor
	if containsModule(part,"ModuleEnviroSensor") then	
		addToModCategory(part,"Science/Sensor")
	end	
	--Antenna
	if containsModule(part,"ModuleDataTransmitter") then	
		addToModCategory(part,"Science/Antenna")
	end	
	--Fuel Duct
	if part.name == "fuelLine" then	
		addToModCategory(part,"Storage/Transfer")
	end	
	--SAS
	if containsModule(part,"ModuleReactionWheel") or containsModule(part,"ModuleSAS") then	
		addToModCategory(part,"Control/SAS")
	end	
	--ScienceExperiment
	if containsModule(part,"ModuleScienceExperiment") then	
		addToModCategory(part,"Science/Experiment")
	end	
	--ScienceLab
	if containsModule(part,"ModuleScienceLab") then	
		addToModCategory(part,"Science/Lab")
	end	
	--Retractable Ladder
	if containsModule(part,"RetractableLadder") then	
		addToModCategory(part,"Utility/Ladder/Retractable")
	elseif part.name:lower():find("ladder") then
		addToModCategory(part,"Utility/Ladder/Static")
	end	
	
	--Generators
	for generator in modulesByName(part,"ModuleGenerator") do		
		
		local inputs = {}
		local outputs = {}
		for output in nodesByName(generator,"OUTPUT_RESOURCE") do	
			if output.values.name == "ElectricCharge" then				
				local hasInput = false
				for input in nodesByName(part,"INPUT_RESOURCE") do
					hasInput = true
					addToModCategory(part,"Utility/Generator/_"..input.values.name,input.values.name,"Categories/Generator"..input.values.name)
				end				
				if not hasInput then
					if not isInModCategory(part,"Structural/GroundSupport") then
						addToModCategory(part,"Utility/Generator/RTG")
					end
				end
			else	
				outputs[output.values.name] = true
				for input in nodesByName(part,"INPUT_RESOURCE") do
					inputs[input.values.name] = true
				end
			end
		end		
	end
	
end