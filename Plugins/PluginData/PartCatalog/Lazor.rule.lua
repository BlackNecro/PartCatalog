for name,part in pairs(PARTS) do
	local lowerName = part.name:lower()
	if lowerName:find("lazor") then
		addToModCategory(part,"Science/Misc")
	end
end