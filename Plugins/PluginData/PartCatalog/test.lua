require "pl"

dofile("util.lua")
dofile("partString.lua")
dofile("default.category.lua")
dofile("default.rule.lua")
dofile("RemoteTech.rule.lua")
dofile("default.fallback.lua")

clean(CATEGORIES)
sortCat(CATEGORIES)
pretty.dump(CATEGORIES,"category.lua")
