for name,part in pairs(PARTS) do
	if containsModule(part,"RealChuteModule") then
		addToModCategory(part,"Utility/Landing/Parachute")
	end	
end