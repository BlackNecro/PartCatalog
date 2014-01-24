for name,part in pairs(PARTS) do	
	for engine in modulesByName(part,"HydraEngineController") do
		addToModCategory(part,"Engine/MultiMode")				
	end
		
	if part.mod == "B9_Aerospace" then	
	
		if part.title:lower():find("crew") then
				addToModCategory(part,"Pod/Misc")
		else if part.title:lower():find("cargo") then
				addToModCategory(part,"Structural/Compartment")
			end
		end
	
	
		if part.category == "Structural" then
			if part.title:lower():find("mount") then
				addToModCategory(part,"Structural/Adapter")
			end
		end	
		--Coupler
		if part.category == "Structural" then
			if part.name:lower():find("mount") and part.stackSymmetry then
				local stsym = tonumber(part.stackSymmetry) 
				if stsym then
					if stsym > 0 then
						addToModCategory(part,"Structural/Coupler")
					end
				end
			end
		end							
	end
end