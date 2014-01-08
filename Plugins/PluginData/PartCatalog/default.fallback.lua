for name,part in pairs(PARTS) do
	if not part.assigned then
		addCategory(part,"Misc_"..part.category)
	end
end