for name,part in pairs(PARTS) do
	if containsModule(part,"MechJebCore") then
		addToModCategory(part,"Storage/Echarge/Misc")
	end	
end