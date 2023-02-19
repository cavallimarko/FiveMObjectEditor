# FiveMObjectEditor


Features:
Object editor list with object name, distance to player and ID info, when object is selected with Enter key submenu is shown with following options:
edit multiplier: controls movement/rotation multiplier when manipulating with object. MoveX, MoveY and MoveZ sliders move object with left and right arrow keys and RotationX, RotationY and RotationZ rotate object. Reset rotation and delete object buttons are at the bottom. Pink outline marks currently selected object for editing.

https://user-images.githubusercontent.com/37544557/219975997-238cafe5-98bb-4d3f-a912-7c49a180afb1.mp4

Objects can be spawned with freeze argument equal to zero, which enables physics interaction.

https://user-images.githubusercontent.com/37544557/219976005-99164cf1-8fb5-47c7-804a-d173eab5f26f.mp4

Objects can be edited in bulk: delete in range command deletes all object in specified range around the player, move all moves all objects by the same amount. 

https://user-images.githubusercontent.com/37544557/219976134-c72b7e3c-3899-488b-9875-db9369576be4.mp4

Objects can be saved together in a prefab file that can be loaded, edited and saved. Prefab list with prefab author names is also available.

https://user-images.githubusercontent.com/37544557/219976206-a3311a93-899a-4ca2-9dd1-31a584a2b19a.mp4

Full command list:

Basic commands:
/adeo -Open object editor menu: contains all spawned objects for editing
/adobj [name] [freeze bool]- spawns object with specified name and freeze status, if no name is given object "prop_generator_03b" is spawned, if no freeze status is entered freeze status is set to true. (full list of hashes:https://gtahash.ru/) 

Bulk edit:
/addelallobjs- deletes all spawned object currently in object editor menu
/addeleteobjrange [range]- deletes spawned objects with specified radius from the player, if no radius is given defualt radius of 10 meters is used.
/admoveobjects [x][y][z]- moves all spawned object simultaneusly by specified x,y and z offset, if for some coord is missing its value will be 0(e.g /admoveobjets 1 1  will move object only on x and y, z offset will be 0)

Prefabs: file containing objects and their positions and rotations.
/adlistprefabs -lists all available prefabs on the server with the name of their author
/adsaveprefab [filename]- saves currently spawned objects in object editor on the server with the specified name. If no name is present test.prefab filename is used. 
/adspawnprefab [filename] -  spawns prefab with the specified name. If no name is present test.prefab filename is used. 
/adeditprefab [filename]  -  spawns prefab with the specified name in object editor for editing. If no name is present test.prefab filename is used. 

/addelspawnedprefabs - deletes all spawned prefab objects that are not available for editing.
