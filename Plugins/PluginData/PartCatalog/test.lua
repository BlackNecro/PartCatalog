dofile("util.lua")
dofile("partString.lua")
dofile("default.rule.lua")
--dofile("default.fallback.lua")

for category,parts in pairs(CATEGORIES) do
	if(category:sub(1,4) == "Wing") then
	print(category)
	for name,v in pairs(parts) do
		print("",name)
	end
	end
end