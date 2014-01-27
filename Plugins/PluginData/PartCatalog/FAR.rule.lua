for name, part in pairs(PARTS) do
	if containsModule(part,"FARWingAerodynamicModel") then
		addToModCategory(part,"Aero/Wing")
	end
	if containsModule(part,"FARControllableSurface") then
		addToModCategory(part,"Aero/ControlSurface")
	end
end