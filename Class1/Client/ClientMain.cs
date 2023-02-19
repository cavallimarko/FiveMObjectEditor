using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using MenuAPI;
using static MenuAPI.MenuItem;

namespace Class1.Client
{
    public class ClientMain : BaseScript
    {
        // privateID
        private int currentSelectedID;
        private String currentSelectedPrefabName;
        private Menu menu;
        private Menu prefabListMenu;
        private Menu submenu;
        private Menu prefabListSubMenu;
        public static int spawnedItemsCounter;
        private MenuListItem editMultiplier;
       
        public static Dictionary<int, ObjectSpawned> spawnedObjectsClassesDict =
    new Dictionary<int, ObjectSpawned>();
        public static List<MenuItem> spawnedMenuItems=new List<MenuItem>();
        private bool OutlineON=false;
        private int CurrentSelectedNetworkId;
        public ClientMain()
        {
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["listPrefabs"] += new Action<string,string>(ListPrefabs);
            EventHandlers["loadPrefab"] += new Action<string>(LoadPrefab);
            EventHandlers["loadPrefabInMenu"] += new Action<string>(LoadPrefabInMenu);
            
            EventHandlers["Client:OpenObjectEditMenu"] += new Action<string>(OpenEditMenu);
            EventHandlers["Client:Spawn"] += new Action<string,bool>(Spawn);
            EventHandlers["Client:SpawnClient"] += new Action<string,int,bool,Vector3>(SpawnClient);
            EventHandlers["Client:DeleteClient"] += new Action<int>(DeleteClient);
            EventHandlers["Client:DeleteAll"] += new Action(DeleteAll);
            EventHandlers["Client:GetSerializedDictDeleteAll"] += new Action(SendSerializedDictDeleteAll);
            EventHandlers["Client:GetSerializedDictDeleteInRange"] += new Action<int>(SendSerializedDictDeleteInRange);
            EventHandlers["Client:GetSerializedDictSavePrefab"] += new Action<string>(SendSerializedDictSavePrefab);
            EventHandlers["Client:GetSerializedDictMoveAll"] += new Action<int,int,int>(SendSerializedDictMoveAll);
            EventHandlers["Client:DeleteAllClient"] += new Action(DeleteAllClient);
            EventHandlers["Client:DeleteInRange"] += new Action<int>(DeleteInRange);
            EventHandlers["Client:MoveAll"] += new Action<int,int,int>(MoveAll);
            EventHandlers["Client:SetCurrentNetworkId"] += new Action<int>(SetCurrentNetworkId);
            
            CreateMenu();
        }
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
        private void CreateListPrefabMenu()
        {

            prefabListMenu = new Menu("Prefab list", "List of prefabs available.");
           
            prefabListMenu.Visible=true;
            
            /*
            //disable hotkey
            menu.Visible=false;
            MenuController.MenuToggleKey = (Control) (-1);
            */
            MenuController.AddMenu(prefabListMenu);
            prefabListMenu.OnItemSelect += (menu, item1, index) =>
                {
                    currentSelectedPrefabName=item1.Text;
                    Debug.WriteLine("Selected"+item1.Text+", currentSelectedPrefabName: "+currentSelectedPrefabName); 
                };

            prefabListSubMenu = new Menu("Prefab edit/spawn", "Choose edit (client) or spawn (server).");
            MenuController.AddSubmenu(prefabListMenu, prefabListSubMenu);
            MenuItem editPrefabItem = new MenuItem("Edit Prefab", "Edit Prefab");
            MenuItem spawnPrefabItem = new MenuItem("Spawn Prefab", "Spawn Prefab");
            prefabListSubMenu.OnItemSelect += (_menu, _item, _index) =>
            {
                if (_item == editPrefabItem)
                {
                    TriggerServerEvent("editPrefab",currentSelectedPrefabName);
                }
                else if (_item == spawnPrefabItem)
                {
                    TriggerServerEvent("spawnSpawnedObjects",currentSelectedPrefabName);
                }
            };
            prefabListSubMenu.AddMenuItem(editPrefabItem);

            prefabListSubMenu.AddMenuItem(spawnPrefabItem);

        }
        private void CreateMenu()
        {

            menu = new Menu("Object Editor", "List of spawned objects, select one to edit.");
           
            menu.Visible=true;
            
            
            //disable hotkey
            menu.Visible=false;
            MenuController.MenuToggleKey = (Control) (-1);
            
            MenuController.AddMenu(menu);
            CreateSubMenu();
        }
        private void CreateSubMenu(){
            // Creating a submenu, adding it to the menus list, and creating and binding a button for it.
            submenu = new Menu("Object Editor", "Here you can edit object properties.");
            MenuController.AddSubmenu(menu, submenu);
/*
            MenuItem menuButton = new MenuItem(
                "Submenu",
                "This button is bound to a submenu. Clicking it will take you to the submenu."
            )
            {
                Label = "→→→"
            };
            menu.AddMenuItem(menuButton);
            MenuController.BindMenuItem(menu, submenu, menuButton);
*/
            List<string> values = new List<string>() {
            "0.1",
            "1",
            "5",
            "10"
            };

            // Specify the index which will be used when the item is visible by default.
            // Value must be a valid index value for the given values list.
            int currentIndex = 1; // ("Option 1")

            editMultiplier = new MenuListItem("editMultiplier", values, currentIndex, "Item description");

            MenuSliderItem sliderX = new MenuSliderItem("MoveX", -1000000, 1000000, 0, false);
            MenuSliderItem sliderY = new MenuSliderItem("MoveY", -1000000, 1000000, 0, false);
            MenuSliderItem sliderZ = new MenuSliderItem("MoveZ", -1000000, 1000000, 0, false);
            MenuSliderItem sliderRotationX = new MenuSliderItem("RotationX", -1000000, 1000000, 0, false);
            MenuSliderItem sliderRotationY = new MenuSliderItem("RotationY", -1000000, 1000000, 0, false);
            MenuSliderItem sliderRotationZ = new MenuSliderItem("RotationZ", -1000000, 1000000, 0, false);
            
            submenu.OnSliderPositionChange += (_menu, _sliderItem, _oldPosition, _newPosition, _itemIndex) =>
                {
                    int entityId=spawnedObjectsClassesDict[currentSelectedID].entityID;
                    float multiplier=float. Parse(editMultiplier.GetCurrentSelection());
                    if (_sliderItem == sliderX)
                    {
                        if(_newPosition>_oldPosition){
                            TriggerServerEvent("Server:Move",entityId,new Vector3(1,0,0),multiplier);

                        }else{
                            TriggerServerEvent("Server:Move",entityId,new Vector3(1,0,0),-multiplier);
                        }
                       Debug.WriteLine(_sliderItem.Label+"oldPosition: "+_oldPosition+", newPosition: "+_newPosition); 
                    }
                    else if (_sliderItem == sliderY)
                    {
                       if(_newPosition>_oldPosition){
                            TriggerServerEvent("Server:Move",entityId,new Vector3(0,1,0),multiplier);

                        }else{
                            TriggerServerEvent("Server:Move",entityId,new Vector3(0,1,0),-multiplier);
                        }
                       Debug.WriteLine(_sliderItem.Label+"oldPosition: "+_oldPosition+", newPosition: "+_newPosition); 
                    }
                    else if (_sliderItem == sliderZ)
                    {
                        if(_newPosition>_oldPosition){
                            TriggerServerEvent("Server:Move",entityId,new Vector3(0,0,1),multiplier);

                        }else{
                            TriggerServerEvent("Server:Move",entityId,new Vector3(0,0,1),-multiplier);
                        }
                       Debug.WriteLine(_sliderItem.Label+"oldPosition: "+_oldPosition+", newPosition: "+_newPosition); 
                    }
                    else if (_sliderItem == sliderRotationX)
                    {
                        if(_newPosition>_oldPosition){
                            TriggerServerEvent("Server:Rotate",entityId,new Vector3(1,0,0),multiplier);


                        }else{
                            TriggerServerEvent("Server:Rotate",entityId,new Vector3(1,0,0),-multiplier);

                        }

                       Debug.WriteLine(_sliderItem.Label+"oldRotation: "+_oldPosition+", newRotation: "+_newPosition); 
                    }
                     else if (_sliderItem == sliderRotationY)
                    {
                        if(_newPosition>_oldPosition){
                            TriggerServerEvent("Server:Rotate",entityId,new Vector3(0,1,0),multiplier);


                        }else{
                            TriggerServerEvent("Server:Rotate",entityId,new Vector3(0,1,0),-multiplier);

                        }
                       Debug.WriteLine(_sliderItem.Label+"oldRotation: "+_oldPosition+", newRotation: "+_newPosition); 
                    }
                     else if (_sliderItem == sliderRotationZ)
                    {
                        if(_newPosition>_oldPosition){
                            TriggerServerEvent("Server:Rotate",entityId,new Vector3(0,0,1),multiplier);

                        }else{
                            TriggerServerEvent("Server:Rotate",entityId,new Vector3(0,0,1),-multiplier);

                        }
                       Debug.WriteLine(_sliderItem.Label+"oldRotation: "+_oldPosition+", newRotation: "+_newPosition); 
                    }
                };
            menu.OnItemSelect += (menu, item1, index) =>
                {
                    currentSelectedID=int.Parse(item1.Label.Split(' ')[1]);
                    Debug.WriteLine("Selected"+item1.Text+", currentSelectedID: "+currentSelectedID); 
                };
            MenuItem resetRotationItem = new MenuItem("Reset Rotation", "Resets Rotation");

            MenuItem deleteItem = new MenuItem("~r~Delete Object", "Delete Object");
            submenu.OnItemSelect += (_menu, _item, _index) =>
            {
                if (_item == deleteItem)
                {
                    TriggerServerEvent("Server:DeleteObject",spawnedObjectsClassesDict[currentSelectedID].entityID,currentSelectedID);

                    //DeleteItem(currentSelectedID);
                }
                else if (_item == resetRotationItem)
                {
                    TriggerServerEvent("Server:ResetRotation",spawnedObjectsClassesDict[currentSelectedID].entityID);
                    //SetEntityRotation(spawnedObjectsClassesDict[currentSelectedID].entityID, 0, 0, 0, 2, false);
                }
            };
            submenu.OnMenuOpen+=(_submenu)=>
            {
                if(spawnedObjectsClassesDict.Count>0){
                    int id=NetworkGetEntityFromNetworkId(spawnedObjectsClassesDict[currentSelectedID].entityID);

                    SetEntityDrawOutline(id,true);
                }
            };
            submenu.OnMenuClose+=(_submenu)=>
            {
                if(spawnedObjectsClassesDict.Count>0){
                    int id=NetworkGetEntityFromNetworkId(spawnedObjectsClassesDict[currentSelectedID].entityID);
                    SetEntityDrawOutline(id,false);
                }
            };
            // Add a menu item to a menu:
            submenu.AddMenuItem(editMultiplier);
            // adding the sliders to the menu.
            submenu.AddMenuItem(sliderX);
            submenu.AddMenuItem(sliderY);
            submenu.AddMenuItem(sliderZ);
            submenu.AddMenuItem(sliderRotationX);
            submenu.AddMenuItem(sliderRotationY);
            submenu.AddMenuItem(sliderRotationZ);
            submenu.AddMenuItem(resetRotationItem);

            submenu.AddMenuItem(deleteItem);

        }
    private void ListPrefabs(string prefabStringsConcat,string author)
        {
            CreateListPrefabMenu();
            Debug.WriteLine("ListingPrefabs");
            List <string> prefabsStrings=new List<string>(prefabStringsConcat.Split(';'));
            prefabsStrings.RemoveAt(prefabsStrings.Count-1);
            foreach (var item in prefabsStrings)
            {
                Debug.WriteLine(item);
                MenuItem menuItem = new MenuItem(item);
                menuItem.Label="Author: "+author;

                prefabListMenu.AddMenuItem(menuItem);
                MenuController.BindMenuItem(prefabListMenu, prefabListSubMenu, menuItem);
            }
            
            
        }
        private void LoadPrefab(string loadedPrefabString)
        {
            
            Debug.WriteLine("Loaded prefab string"+loadedPrefabString);
            Dictionary<int, ObjectSpawned> spawnedObjectsClassesDictLocal=GetDictFromSerializedString(loadedPrefabString);
            foreach(KeyValuePair<int, ObjectSpawned> spawnedObject in spawnedObjectsClassesDictLocal)
            {
                int hash=GetHashKey(spawnedObject.Value.objectName);
                int plyPed = GetPlayerPed(-1);
                Vector3 plyPos = GetEntityCoords(plyPed,true);
                Debug.WriteLine("LoadingModel");
                RequestModel((uint) hash);

                Vector3 position=spawnedObject.Value.Coord;
                int entityID=CreateObject(hash,position.X, position.Y, position. Z, true, false, false);
                SetEntityRotation(entityID,spawnedObject.Value.Rotation.X,spawnedObject.Value.Rotation.Y,spawnedObject.Value.Rotation.Z,2,false);
                
                Debug.WriteLine("Created model with ID="+entityID); 
                FreezeEntityPosition(entityID, spawnedObject.Value.frozen);
                
                MenuItem item = new MenuItem(spawnedObject.Value.objectName);
                item.Label="ID: "+spawnedItemsCounter;

                menu.AddMenuItem(item);

                spawnedObjectsClassesDict.Add(spawnedItemsCounter,new ObjectSpawned(spawnedObject.Value.objectName,entityID,spawnedItemsCounter,position,spawnedObject.Value.frozen,item));
                 spawnedMenuItems.Add(item);
                MenuController.BindMenuItem(menu, submenu, item);
                currentSelectedID=spawnedItemsCounter;
                spawnedItemsCounter++;
           
            }
            
            
        }
        private void LoadPrefabInMenu(string loadedPrefabString)
        {
            
            Debug.WriteLine("Loaded prefab string"+loadedPrefabString);
            Dictionary<int, ObjectSpawned> spawnedObjectsClassesDictLocal=GetDictFromSerializedString(loadedPrefabString);
            foreach(KeyValuePair<int, ObjectSpawned> spawnedObject in spawnedObjectsClassesDictLocal)
            {
                
                MenuItem item = new MenuItem(spawnedObject.Value.objectName);
                item.Label="ID: "+spawnedItemsCounter;

                menu.AddMenuItem(item);
                menu.Visible=true;
                spawnedObjectsClassesDict.Add(spawnedItemsCounter,new ObjectSpawned(spawnedObject.Value.objectName,spawnedObject.Value.entityID,spawnedItemsCounter,spawnedObject.Value.Coord,spawnedObject.Value.frozen,item));
                 spawnedMenuItems.Add(item);
                MenuController.BindMenuItem(menu, submenu, item);
                currentSelectedID=spawnedItemsCounter;
                spawnedItemsCounter++;
           
            }
            
            
        }
        private void OnClientResourceStart(string resourceName)
        {
            
            if (GetCurrentResourceName() != resourceName) return;
        }
        private void MoveItem(int privateID){
            int entityID=spawnedObjectsClassesDict[privateID].entityID;
            Vector3 Coord=GetEntityCoords(entityID, true);
            Vector3 Offset=new Vector3(0,2,0);
            Vector3 OffsetedCoord=Coord+Offset;
            SetEntityCoords(entityID, OffsetedCoord.X, OffsetedCoord.Y, OffsetedCoord.Z, true, false, false, false);
        }
        private void MoveItem(int privateID,Vector3 direction, float multiplier){
            multiplier*=float. Parse(editMultiplier.GetCurrentSelection());
            int entityID=spawnedObjectsClassesDict[privateID].entityID;

            Vector3 Coord=GetEntityCoords(entityID, true);
            Vector3 Offset=direction;
            Vector3 OffsetedCoord=Coord+Offset*multiplier;
            SetEntityCoords(entityID, OffsetedCoord.X, OffsetedCoord.Y, OffsetedCoord.Z, true, false, false, false);
        }
        private void RotateItem(int privateID,Vector3 rotationAxis,float angle, float multiplier){
            multiplier*=float. Parse(editMultiplier.GetCurrentSelection());
            
            int entityID=spawnedObjectsClassesDict[privateID].entityID;

            Vector3 Rotation=GetEntityRotation(entityID, 2);
            
            Vector3 OffsetedRotation=Rotation+rotationAxis*angle*multiplier;
            SetEntityRotation(entityID, OffsetedRotation.X, OffsetedRotation.Y, OffsetedRotation.Z, 2, false);
                    
        }
        private void DeleteItem(int privateID){
            ObjectSpawned obj=spawnedObjectsClassesDict[privateID];
            int entityID=spawnedObjectsClassesDict[privateID].entityID;

            SetEntityAsMissionEntity(entityID, true,true);
            DeleteEntity(ref entityID);
            MenuItem item=obj.MenuItem;
            menu.RemoveMenuItem(item);
            spawnedMenuItems.Remove(item);
            spawnedObjectsClassesDict.Remove(privateID);
        }
        private void DeleteAll(){
            foreach(KeyValuePair<int, ObjectSpawned> spawnedObject in spawnedObjectsClassesDict)
                {
                    int entityID=spawnedObject.Value.entityID;
                    SetEntityAsMissionEntity(entityID, true,true);
                    DeleteEntity(ref entityID);
                }     
                for (int i = 0; i < spawnedMenuItems.Count; i++)
                {
                    MenuItem item=spawnedMenuItems[i];
                    menu.RemoveMenuItem(item);
                }      
                spawnedMenuItems.Clear();
                spawnedObjectsClassesDict=new Dictionary<int,ObjectSpawned>();
                spawnedItemsCounter=0;
        }
        Dictionary<int,ObjectSpawned> GetDictFromSerializedString(string serializedObject){
            Dictionary<int, ObjectSpawned> spawnedObjectsClassesDict =
                new Dictionary<int, ObjectSpawned>();

            List <string> objectStrings=new List<string>(serializedObject.Split(';'));
            objectStrings.RemoveAt(objectStrings.Count - 1);

            foreach (var item in objectStrings)
            {
                string [] l=item.Split(',');
                List <string> objectParams=new List<string>(item.Split(','));
                for (int i = 0; i < l.Length; i++)
                {
                    Debug.WriteLine(l[i]);
                }
                string objName=objectParams[0];
                int entityID=int.Parse(objectParams[1]);
                int privateID=int.Parse(objectParams[2]);
                float CoordX=float.Parse(objectParams[3]);
                float CoordY=float.Parse(objectParams[4]);
                float CoordZ=float.Parse(objectParams[5]);
                float RotationX=float.Parse(objectParams[6]);
                float RotationY=float.Parse(objectParams[7]);
                float RotationZ=float.Parse(objectParams[8]);
                bool frozen=bool.Parse(objectParams[9]);
                Vector3 Coord=new Vector3(CoordX,CoordY,CoordZ);
                Vector3 Rotation=new Vector3(RotationX,RotationY,RotationZ);
                ObjectSpawned obj=new ObjectSpawned(objName,entityID,privateID,Coord,Rotation,frozen);
                spawnedObjectsClassesDict.Add(privateID,obj);
            }
            return spawnedObjectsClassesDict;
        }
        
        //[Command("adcreateobj")]
        private void TrySpawn(int playerId, List<object> args, string raw)
        {
            string objectName=args.Count > 0 ? $"{args[0]}" : "prop_generator_03b";
                
            bool freeze=args.Count > 1 ? Convert.ToBoolean(int.Parse($"{args[1]}")): true;
            
            TriggerServerEvent("Server:TrySpawn",objectName,freeze);
        }
        //[Command("addelallobjs")]
        private void TryDeleteAllObjects(int playerId, List<object> args, string raw)
        {
            Debug.WriteLine("starting addelallobjs");
            TriggerServerEvent("Server:TryDeleteAllObjects",playerId);
        }
        [Command("addeleteobjects")]
        private void TryDeleteInRange(int playerId, List<object> args, string raw)
        {
            int range=args.Count > 0 ? int.Parse($"{args[0]}"): 10;               
            
            TriggerServerEvent("Server:TryDeleteInRange",range);
        }
        //[Command("admoveobjects")]
        private void TryMoveAll(int playerId, List<object> args, string raw)
        {
            int dx=args.Count > 0 ? int.Parse($"{args[0]}"): 0;
            int dy=args.Count > 1 ? int.Parse($"{args[1]}"): 0;
            int dz=args.Count > 2 ? int.Parse($"{args[2]}"): 0;               
            
            TriggerServerEvent("Server:TryMoveAll",dx,dy,dz);
        }
        private void OpenEditMenu(string a)
        {
            menu.Visible=true;
        }

        private void Spawn(string objectName,bool freeze)
        {
            
            TriggerEvent("chat:addMessage", new
            {
                color = new[] {255, 0, 0},
                args = new[] {"Spawning: "+objectName}
            });
            int hash=GetHashKey(objectName);
            int plyPed = GetPlayerPed(-1);
            Vector3 plyPos = GetEntityCoords(plyPed,true);
            Debug.WriteLine("LoadingModel");
            RequestModel((uint) hash);
            Debug.WriteLine("ModelLoaded");
            Vector3 position=new Vector3(plyPos.X, plyPos.Y+3, plyPos. Z);
            int objectID=CreateObject(hash,position.X, position.Y, position. Z, true, false, false);
            Debug.WriteLine("Created model with ID="+objectID); 
            FreezeEntityPosition(objectID, freeze);
            
            MenuItem item = new MenuItem(objectName);
            item.Label="ID: "+spawnedItemsCounter;

            menu.AddMenuItem(item);

            spawnedObjectsClassesDict.Add(spawnedItemsCounter,new ObjectSpawned(objectName,objectID,spawnedItemsCounter,position,freeze,item));
            spawnedMenuItems.Add(item);
            MenuController.BindMenuItem(menu, submenu, item);
            currentSelectedID=spawnedItemsCounter;
            spawnedItemsCounter++;
        }
        private void SpawnClient(string objectName,int objectID,bool freeze,Vector3 position)
        {
            
           
            Debug.WriteLine("entityID"+objectID);
            
            MenuItem item = new MenuItem(objectName);
            item.Label="ID: "+spawnedItemsCounter;

            menu.AddMenuItem(item);

            spawnedObjectsClassesDict.Add(spawnedItemsCounter,new ObjectSpawned(objectName,objectID,spawnedItemsCounter,position,freeze,item));
            spawnedMenuItems.Add(item);
            MenuController.BindMenuItem(menu, submenu, item);
            currentSelectedID=spawnedItemsCounter;
            spawnedItemsCounter++;
        }
        private void DeleteClient(int privateID){
            ObjectSpawned obj=spawnedObjectsClassesDict[privateID];
            //int entityID=spawnedObjectsClassesDict[privateID].entityID;

            //SetEntityAsMissionEntity(entityID, true,true);
            //DeleteEntity(ref entityID);

            MenuItem item=obj.MenuItem;
            menu.RemoveMenuItem(item);
            spawnedMenuItems.Remove(item);
            spawnedObjectsClassesDict.Remove(privateID);
        }
        private void SendSerializedDictDeleteAll(){
            string serializedDict=GetDictSerialized();
            TriggerServerEvent("Server:DeleteAllObjects",serializedDict);

        }
        private void SendSerializedDictDeleteInRange(int range){
            string serializedDict=GetDictSerialized();
            TriggerServerEvent("Server:DeleteObjectsInRange",serializedDict,range);

        }
        private void SendSerializedDictSavePrefab(string filename){
            string serializedDict=GetDictSerialized();
            TriggerServerEvent("Server:SavePrefab",serializedDict,filename);

        }
        
         private void SendSerializedDictMoveAll(int dx,int dy,int dz){
            string serializedDict=GetDictSerialized();
            float multiplier=float. Parse(editMultiplier.GetCurrentSelection());
            TriggerServerEvent("Server:MoveAllObjects",serializedDict,dx,dy,dz,multiplier);

        }
        private void DeleteAllClient(){
           for (int i = 0; i < spawnedMenuItems.Count; i++)
            {
                MenuItem item=spawnedMenuItems[i];
                menu.RemoveMenuItem(item);
            }      
            spawnedMenuItems.Clear();
            spawnedObjectsClassesDict=new Dictionary<int,ObjectSpawned>();
            spawnedItemsCounter=0;
        }
        private void DeleteInRange(int range)
        {
            range*=range;
            int plyPed = GetPlayerPed(-1);
            Vector3 plyPos = GetEntityCoords(plyPed,true);
            List <int> IDsToDelete=new List<int>();
            foreach(KeyValuePair<int, ObjectSpawned> spawnedObject in spawnedObjectsClassesDict)
            {
                Vector3 objPos=GetEntityCoords(spawnedObject.Value.entityID,true);

                if(Vector3.DistanceSquared(plyPos,objPos)<=range)
                {
                    IDsToDelete.Add(spawnedObject.Value.privateID);
                }
            }
            foreach (var item in IDsToDelete)
            {
                    DeleteItem(item);

            }
        }
        private void MoveAll(int dx,int dy, int dz)
        {
            foreach(KeyValuePair<int, ObjectSpawned> spawnedObject in spawnedObjectsClassesDict)
            {
                MoveItem(spawnedObject.Value.privateID,new Vector3(dx,dy,dz),1);
            }
        }
        private void SetCurrentNetworkId(int currentNetworkId)
        {
            CurrentSelectedNetworkId=currentNetworkId;
            SetEntityDrawOutlineColor(255,125,0,255);
            SetEntityDrawOutlineShader(1);
            int entityId=spawnedObjectsClassesDict[currentSelectedID].entityID;
            int id=NetworkGetEntityFromNetworkId(CurrentSelectedNetworkId);
               
            SetEntityDrawOutline(id,true);
        }
        
        private string GetDictSerialized(){
            string prefabSerialized = "";
            foreach(KeyValuePair<int, ObjectSpawned> spawnedObject in spawnedObjectsClassesDict)
            {
                prefabSerialized+=spawnedObject.Value.GetSerialized()+";";
            }
            TriggerServerEvent("saveSpawnedObjects",prefabSerialized);
            return prefabSerialized;
        }
        [Tick]
        public Task OnTick()
        {
            
            int plyPed = GetPlayerPed(-1);
            Vector3 plyPos = GetEntityCoords(plyPed,true);
            foreach(KeyValuePair<int, ObjectSpawned> spawnedObject in spawnedObjectsClassesDict)
                {
                //Vector3 objPos=spawnedObject.Value.Coord;
                int networkId=spawnedObject.Value.entityID;
                int entityId=NetworkGetEntityFromNetworkId(networkId);

                Vector3 objPos = GetEntityCoords(entityId,true);

                long time=GetGameTimer();
                int dist =(int)GetDistanceBetweenCoords(plyPos.X,plyPos.Y,plyPos.Z,objPos.X,objPos.Y,objPos.Z,true);
                
                spawnedObject.Value.MenuItem.Text=spawnedObject.Value.objectName+" ("+dist.ToString()+"m)";
        
            }
           
            return Task.FromResult(0);
        }
        
    }
    public class ObjectSpawned{
        public string objectName;
        public int entityID;
        public int privateID;
        public Vector3 Coord;
        public Vector3 Rotation;
        public bool frozen;
        public MenuItem MenuItem;
        public ObjectSpawned(string _objectName, int _entityID,int _privateID, Vector3 _Coord, bool _frozen, MenuItem _menuItem){
            objectName=_objectName;
            entityID=_entityID;
            privateID=_privateID;
            Coord=_Coord;
            frozen=_frozen;
            MenuItem=_menuItem;
        }
        public ObjectSpawned(string _objectName, int _entityID,int _privateID, Vector3 _Coord, Vector3 _Rotation, bool _frozen){
            objectName=_objectName;
            entityID=_entityID;
            privateID=_privateID;
            Coord=_Coord;
            Rotation=_Rotation;
            frozen=_frozen;
        }
        public string GetSerialized(){
            Coord=GetEntityCoords(entityID,true);
            Rotation=GetEntityRotation(entityID,2);
            return objectName+","+entityID+","+privateID+","+Coord.X+","+Coord.Y+","+Coord.Z+","+Rotation.X+","+Rotation.Y+","+Rotation.Z+","+frozen;
        }
    }
}
