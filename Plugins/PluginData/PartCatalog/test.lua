require "pl"

dofile("util.lua")
dofile("partString.lua")
dofile("default.category.lua")
dofile("default.rule.lua")
dofile("RemoteTech.rule.lua")
dofile("default.fallback.lua")
dofile("default.nodeSizes.lua")

clean(CATEGORIES)
sortCat(CATEGORIES)
pretty.dump(CATEGORIES,"category.lua")
pretty.dump(PARTS,"partStringPretty.lua")
for k,v in pairs(CATEGORIES.children) do
	print(v.name, v.overlay)
end
--[[
for name,part in pairs(PARTS) do
	local sizes = {}
	local first = true
	local toPrint = false
	for _,node in pairs(part.attachNodes) do		
		if not sizes[node.size] then
			sizes[node.size] = true
			if first then
				first = false
			else
				toPrint = true
			end
		end
	end
	if toPrint then
		print(name)
		for k,v in pairs(sizes) do 
			print(" ",k)
		end
	end
end]]
