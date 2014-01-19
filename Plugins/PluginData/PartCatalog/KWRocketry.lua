for name,part in pairs(PARTS) do
	local lowerName = name:lower()
	if lowerName:find("fairing",1,true) then
		if lowerName:find("base",1,true) then
			addToModCategory(part,"Aero/Fairing/Base")
		else	
			addToModCategory(part,"Aero/Fairing/Side")
		end
	end
end