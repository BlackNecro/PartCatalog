for name,part in pairs(PARTS) do
	
	--Command Modules
	local hasCommand = false
	for module in modulesByName(part,"ModuleCommand") do		
		hasCommand = true
		if module.values.minimumCrew ~= "0" then			
			hasCommand = false
			addCategory(part,"MannedPod")
			break
		end		
	end
	if hasCommand then
		addCategory(part,"UnmannedPod")
	end
	
	if containsModule(part,"KerbalSeat") then
		addCategory(part,"Seat")
	end
	
	
	--Engines
	for engine in modulesByName(part,"ModuleEngines") do	
		
		local special = false
		if containsNodeTypeName(engine,"PROPELLANT","LiquidFuel") then
			if containsNodeTypeName(engine,"PROPELLANT","Oxidizer") then
				addCategory(part,"LiquidEngine")				
			elseif containsNodeTypeName(engine,"PROPELLANT","IntakeAir") then
				addCategory(part,"JetEngine")
			else
				special = true
			end
		elseif containsNodeTypeName(engine,"PROPELLANT","XenonGas") and containsNodeTypeName(engine,"PROPELLANT","ElectricCharge") then
			addCategory(part,"IonEngine")			
		else
			special = true
		end
		if special then
			for propellant in nodesByName(engine,"PROPELLANT") do
				addCategory(part,"Engine_" .. propellant.values.name)			
			end
		end		
	end
	for engine in modulesByName(part,"MultiModeEngine") do
		addCategory(part,"EngineMultiMode")
	end
	
	--Intakes	
	for intake in modulesByName(part,"ModuleResourceIntake") do
		addCategory(part,"Intake_" .. intake.values.resourceName)
	end
	
	--Control Surfaces
	for controlSurface in modulesByName(part,"ModuleControlSurface") do
		addCategory(part,"ControlSurface")
	end
	
	--Winglet
	if part.dragModelType == "override" and part.deflectionLiftCoeff then
		local coeff = tonumber(part.deflectionLiftCoeff) 
		if coeff then
			if coeff < 1 then
				addCategory(part,"Winglet")
			else
				addCategory(part,"Wing")
			end
		end
	end
	
	
	
	--Decoupler
	for decoupler in modulesByName(part,"ModuleDecouple") do
		if decoupler.values.isOmniDecoupler == "true" then
			addCategory(part,"Seperator")			
		else
			addCategory(part,"DecouplerStack")						
		end
	end
	if containsModule(part,"ModuleAnchoredDecoupler") then	
		addCategory(part,"DecouplerRadial")
	end
	
	--Docking
	for docking in modulesByName(part,"ModuleDockingNode") do
		if docking.values.nodeType and string.sub(docking.values.nodeType,1,4) == "size" then
			addCategory(part,"Docking_" .. docking.values.nodeType)
		else
			addCategory(part,"Docking")
		end
	end
	
	--RCS
	for rcs in modulesByName(part,"ModuleRCS") do
		addCategory(part,"RCS_" .. rcs.values.resourceName)		
	end
	
	--Clamp
	if containsModule(part,"LaunchClamp") then	
		addCategory(part,"GroundSupport")
	end	
	
	--Strut
	if containsModule(part,"StrutConnector") then	
		addCategory(part,"StrutConnector")
	end
	
	--Solar Panel
	for panel in modulesByName(part,"ModuleDeployableSolarPanel") do
		if panel.values.sunTracking == "false" then
			addCategory(part,"SolarPanelStatic")
		else			
			addCategory(part,"SolarPanelDeployable")
		end
	end
	
	if part.category == "Aero" then
		if part.name:lower():find("nose") or part.description:lower():find("nose") then
			addCategory(part,"NoseCone")
		end
	end
	
	--Landing Legs
	if containsModule(part,"ModuleLandingLeg") or containsModule(part,"HLandingLeg") then	
		addCategory(part,"LandingLeg")
	end

	--Wheel
	if containsModule(part,"ModuleWheel") then	
		addCategory(part,"Wheel")
	end	

	--Landing Gear
	if containsModule(part,"ModuleLandingGear") then	
		addCategory(part,"LandingGear")
	end	

	--Parachute	
	if containsModule(part,"ModuleParachute") then	
		addCategory(part,"Parachute")
	end	

	--Lights
	if containsModule(part,"ModuleLight") then	
		addCategory(part,"Light")
	end	
	--Sensor
	if containsModule(part,"ModuleEnviroSensor") then	
		addCategory(part,"Sensor")
	end	
	--Antenna
	if containsModule(part,"ModuleDataTransmitter") then	
		addCategory(part,"ScienceAntenna")
	end	
	--Fuel Duct
	if part.name == "fuelLine" then	
		addCategory(part,"FuelTransfer")
	end	
	--SAS
	if containsModule(part,"ModuleReactionWheel") or containsModule(part,"ModuleSAS") then	
		addCategory(part,"SAS")
	end	
	--ScienceExperiment
	if containsModule(part,"ModuleScienceExperiment") then	
		addCategory(part,"ScienceExperiment")
	end	
	--ScienceLab
	if containsModule(part,"ModuleScienceLab") then	
		addCategory(part,"ScienceLab")
	end	
	--Retractable Ladder
	if containsModule(part,"RetractableLadder") then	
		addCategory(part,"RetractableLadder")
	elseif part.name:lower():find("ladder") then
		addCategory(part,"StaticLadder")		
	end	
	
	--Generators
	for panel in modulesByName(part,"ModuleGenerator") do
		for output in nodesByName(part,"OUTPUT_RESOURCE") do
			addCategory(part,"Generator_" .. output.values.name)
		end
	end
	
	--Storage
	local gotLF = false;
	local gotOx = false;
	local gotMonoProp = false;
	local gotOther = false;
	for resource in resources(part) do
		if( tonumber(resource.values.maxAmount) > 0) then
			if resource.values.name == "LiquidFuel" then
				gotLF = true
			elseif resource.values.name == "Oxidizer" then
				gotOx = true;
			elseif resource.values.name == "MonoPropellant" then
				gotMonoProp = true;
			else
				gotOther = true;
			end 
		end
	end
	
	if not gotOther then
		if gotLF then
			if gotOx then
				if gotMonoProp then
					addCategory(part,"StorageServiceModule");
				else
					addCategory(part,"StorageLFOX");
				end
			else
				addCategory(part,"StorageLF");
			end
		elseif gotOx then
			addCategory(part,"StorageOX");
		elseif gotMonoProp then
			addCategory(part,"Storage_MonoPropellant");			
		end
	elseif gotLF and gotOx and gotMonoProp and gotOther then
		addCategory(part,"StorageServiceModule");		
	else
		for resource in resources(part) do
			if( tonumber(resource.values.maxAmount) > 0) then
				addCategory(part,"Storage_"..resource.values.name);
			end
		end
	end
end

