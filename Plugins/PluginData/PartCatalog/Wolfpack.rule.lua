for name,part in pairs(PARTS) do
	if part.manufacturer:lower():find("lsi") then
		if part.name:lower():find("bay") then
			addToModCategory(part,"Structural/Cargo")
		end
	end
end