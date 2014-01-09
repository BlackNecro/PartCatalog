local counter = 0

for name,part in pairs(PARTS) do
	if not part.assigned  and part.category ~= "none" then
		addToModCategory(part,part.category .. "/Misc")
	end
end