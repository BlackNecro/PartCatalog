CATEGORIES = { name = "root", sort = false,  title = "", overlay = "", icon = "", parts = {}, children = {}}
PARTS = {}


function compareCategory(a,b)
	if a and b then
		return a.title < b.title
	end
end

function sortCat(category)
	if(category.sort) then
		table.sort(category.children,compareCategory)
	end
	for k,v in pairs(category.children) do
		sortCat(v)
	end
end

function count(tab)
	local cnt = 0
	for k,v in pairs(tab) do
		cnt = cnt + 1
	end
	return cnt
end

function clean(category)	
	local toRemove = {}
	for k,v in pairs(category.children) do
		clean(v)		
		if(count(v.children)== 0 and count(v.parts) == 0) then
			toRemove[k] = true
		end
	end
	for k,v in pairs(toRemove) do
		category.children[k] = nil
	end
end

function findOrCreateCategory(name,curcategory)
	if name == curcategory.name then
		return curcategory
	end
	for k,childcat in pairs(curcategory.children) do
		if childcat.name == name then
			return childcat
		end
	end			
	local new = { name = name, sort = false, title = name, overlay = "", icon = "", parts = {}, children = {}}
	curcategory.children[#curcategory.children+1] = new
	return new
end

function findCategory(path,curcategory)
	local index = path:find("/")
	if index then
		local first = path:sub(1,index-1)
		local child = findOrCreateCategory(first,curcategory)
		return findCategory(path:sub(index+1),child)
	end
	return findOrCreateCategory(path,curcategory)
end

function isInModCategory(part,path)
	local cat = findCategory(part.mod.."/"..path,CATEGORIES)
	return cat.parts[part.name] == true
end

function setOverlay(path, overlay)
	local cat = findCategory(path,CATEGORIES)
	cat.overlay = overlay
end

function setCategory(path, title, icon,sort,overrideIcon)
	local cat = findCategory(path,CATEGORIES)
	if sort ~= nil then
		cat.sort = sort
	end

	cat.title = title or cat.title
	if icon then
		if overrideIcon or cat.icon == "" then
			cat.icon = icon
		end
	end
	return cat
end

function addToCategory(part,path,title,icon,sort)
	local cat = setCategory(path,title,icon,sort)	
	cat.parts[part.name] = true
	part.assigned = true
	return cat
end

function addToModCategory(part,path,title,icon,sort,overrideIcon)
	setCategory(part.mod, part.mod, "Mods/"..part.mod,sort,overrideIcon)	
	addToCategory(part,string.format("%s/%s",part.mod,path),title,icon,sort)
	addToCategory(part,string.format("All/%s",path),title,icon,sort)
	part.assigned = true
end

function containsName(toCheck,names)
	if(type(names) == "table") then
		for k,v in pairs(names) do
			if(v == toCheck) then
				return true
			end
		end
	end
	return names == toCheck
end

function nodesByTypeName(tab,typ,name)
	local nodeTable = tab.nodes
	
	local curNode = 0
	local maxNode = #nodeTable
	return function()
		local node = nodeTable[curNode]
		repeat
			curNode = curNode + 1		
			node = nodeTable[curNode]
		until curNode > maxNode or (containsName(node.name,typ) and containsName(node.values.name,name))
		
		if curNode <= maxNode then
			return node
		end
	end
end

function nodesByName(tab,name)
	local nodeTable = tab.nodes
	
	local curNode = 0
	local maxNode = #nodeTable
	return function()
		repeat
			curNode = curNode + 1
		until curNode > maxNode or containsName(nodeTable[curNode].name,name)
		
		if curNode <= maxNode then
			return nodeTable[curNode]
		end
	end
end

function containsNodeByType(tab, typ)
	local nodeTable = tab.nodes
	
	for k,node in pairs(nodeTable) do
		if containsName(node.name,typ) then
			return true
		end
	end
end


function containsNodeTypeName(tab,typ,name)
	local nodeTable = tab.nodes

	for k,node in pairs(nodeTable) do
		if containsName(node.name,typ) and containsName(node.values.name, name) then
			return true
		end
	end
	return false
end

function containsModule(tab, name)
	return containsNodeTypeName(tab,"MODULE",name)
end

function containsResource(tab, name)
	return containsNodeTypeName(tab,"RESOURCE",name)
end



function modules(tab)
	return nodesByName(tab,"MODULE")
end

function resources(tab)
	return nodesByName(tab,"RESOURCE")
end

function modulesByName(tab,name)
	local nodeTable = tab.nodes
	
	local curNode = 0
	local maxNode = #nodeTable
	return function()
		local node = nodeTable[curNode]
		repeat
			curNode = curNode + 1		
			node = nodeTable[curNode]
		until curNode > maxNode or ((node.name == "MODULE") and containsName(node.values.name,name))
		
		if curNode <= maxNode then
			return node
		end
	end
end


function resourcesByName(tab,name)
	local nodeTable = tab.nodes
	
	local curNode = 0
	local maxNode = #nodeTable
	return function()
		local node = nodeTable[curNode]
		repeat
			curNode = curNode + 1		
			node = nodeTable[curNode]
		until curNode > maxNode or ((node.name == "RESOURCE") and containsName(node.values.name, name))
		
		if curNode <= maxNode then
			return node
		end
	end
end

function tableEqual(tab1,tab2)
	for k,v in pairs(tab1) do
		if tab2[k] ~= v then
			return false
		end
	end
	for k,v in pairs(tab2) do
		if tab1[k] == nil then
			return false
		end
	end
	return true
end

function iconExists(path)
	return ICONS and ICONS[path] or false
end