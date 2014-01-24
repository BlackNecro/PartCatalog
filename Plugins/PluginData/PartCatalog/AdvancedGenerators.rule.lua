for name,part in pairs(PARTS) do
	if containsNodeByType(part,"RESOURCE") then
		for resource in resources(part) do
			if( tonumber(resource.values.maxAmount) and tonumber(resource.values.maxAmount) > 0) then
				if resource.values.name == "IntakeAir" then
					addToModCategory(part,"Storage/IntakeAir","IntakeAir","Categories/Storage/IntakeAir")	
				end
			end
		end
	end
end