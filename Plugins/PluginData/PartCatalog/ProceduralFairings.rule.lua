for name,part in pairs(PARTS) do
	if containsModule(part,"ProceduralFairingSide") then
		addToModCategory(part,"Aero/Fairing/Side","Side","Categories/Aero/Fairing")
	end
	if containsModule(part,"ProceduralFairingBase")  then
		addToModCategory(part,"Aero/Fairing/Base","Base","Categories/Aero/Fairing_Base")
	end
	
end