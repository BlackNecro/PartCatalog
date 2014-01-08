CATEGORIES = {}
PARTS = {}

function addCategory(part,category)
   CATEGORIES[category] = CATEGORIES[category] or {}
   CATEGORIES[category][part.name] = true
   part.assigned = true
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
		until curNode > maxNode or ((node.name == typ) and node.values.name == name)
		
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
		until curNode > maxNode or nodeTable[curNode].name == name
		
		if curNode <= maxNode then
			return nodeTable[curNode]
		end
	end
end


function containsNodeTypeName(tab,typ,name)
	local nodeTable = tab.nodes

	for k,node in pairs(nodeTable) do
		if (node.name == typ) and (node.values.name == name) then
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
		until curNode > maxNode or ((node.name == "MODULE") and node.values.name == name)
		
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
		until curNode > maxNode or ((node.name == "RESOURCE") and node.values.name == name)
		
		if curNode <= maxNode then
			return node
		end
	end
end