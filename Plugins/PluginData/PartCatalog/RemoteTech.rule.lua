for name,part in pairs(PARTS) do
	for antenna in modulesByName(part,"ModuleRTAntenna") do
		if antenna.values.DishAngle then
			addToModCategory(part,"Science/Antenna/Dish")
		else
			addToModCategory(part,"Science/Antenna/Omni")		
		end
	end
end