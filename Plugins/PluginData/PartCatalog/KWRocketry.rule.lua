for name,part in pairs(PARTS) do
	local lowerName = part.name:lower()
	if lowerName:find("fairing",1,true) then
		if lowerName:find("base",1,true) then
			addToModCategory(part,"Aero/Fairing/Base","Base","Categories/Aero/Fairing_Base")
		else	
			addToModCategory(part,"Aero/Fairing/Side","Side","Categories/Aero/Fairing")
		end
	end
end