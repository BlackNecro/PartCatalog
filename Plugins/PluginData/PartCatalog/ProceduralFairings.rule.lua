for name,part in pairs(PARTS) do
	if containsModule(part,"ProceduralFairingSide") then
		addToModCategory(part,"Aero/Fairing/Side")
	end
	if containsModule(part,"ProceduralFairingBase")  then
		addToModCategory(part,"Aero/Fairing/Base")
	end
	
end