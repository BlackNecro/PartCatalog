local counter = 0

for name,part in pairs(PARTS) do
	--Modular Fuel
	if not part.assigned and part.category ~= "none" then
		if containsModule(part,"ModuleFuelTanks") then
			addToModCategory(part,"Storage/Modular")
		end

		--Storage
		if containsNodeByType(part,"RESOURCE") then
		
			local gotLF = false;
			local gotOx = false;
			local gotMonoProp = false;
			local gotOther = false;
		
			for resource in resources(part) do
				if( tonumber(resource.values.maxAmount) and tonumber(resource.values.maxAmount) > 0) then
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
							addToModCategory(part,"Storage/ServiceModule")
						else
							addToModCategory(part,"Storage/LFOX")
						end
					else
						addToModCategory(part,"Storage/LF")
					end
				elseif gotOx then
					addToModCategory(part,"Storage/OX")
				elseif gotMonoProp then
					addToModCategory(part,"Storage/_MonoPropellant")
				end	
			elseif gotLF and gotOx and gotMonoProp and gotOther then
				addToModCategory(part,"Storage/ServiceModule","ServiceModule","StorageServiceModule")
			else
				for resource in resources(part) do
					addToModCategory(part,"Storage/_"..resource.values.name,resource.values.name,"Categories/Storage_"..resource.values.name)
				end
			end		
		end
		
	end
	
	if not part.assigned  and part.category ~= "none" then
		addToModCategory(part,part.category .. "/Misc")
	end
	
	
end