using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace Class1.Server
{
    public class ServerMain : BaseScript
    {
        static string fileExtension=".prefab";
        List<int> spawnedEntityIds=new List<int>();
        public ServerMain()
        {
            RegisterCommand("adcreateobj", new Action<int, List<object>, string>(CreateObject), false);
            RegisterCommand("adobject", new Action<int, List<object>, string>(CreateObject), false);
            RegisterCommand("adobj", new Action<int, List<object>, string>(CreateObject), false);
            
            RegisterCommand("adeditobjs", new Action<int, List<object>, string>(OpenEditMenu), false);
            RegisterCommand("adeo", new Action<int, List<object>, string>(OpenEditMenu), false);
            
            RegisterCommand("addelallobjs", new Action<int, List<object>, string>(DeleteAllObjects), false);
            
            RegisterCommand("addeleteobjrange", new Action<int, List<object>, string>(DeleteObjectsInRange), false);
            
            RegisterCommand("admoveobjects", new Action<int, List<object>, string>(MoveAllObjects), false);
            
            RegisterCommand("adlistprefabs", new Action<int, List<object>, string>(ListPrefabsNew), false);
            RegisterCommand("adsaveprefab", new Action<int, List<object>, string>(SavePrefabNew), false);
            RegisterCommand("adspawnprefab", new Action<int, List<object>, string>(SpawnPrefabNew), false);
            RegisterCommand("addelspawnedprefabs", new Action<int, List<object>, string>(DeleteSpawnedPrefabsNew), false);
            RegisterCommand("adeditprefab", new Action<int, List<object>, string>(EditPrefabNew), false);


            

            EventHandlers["printSpawnedObject"] += new Action<Player, string, string>(TargetFunction);
            EventHandlers["saveSpawnedObjects"] += new Action<Player, string, string>(SavePrefab);
            EventHandlers["spawnSpawnedObjects"] += new Action<Player, string>(SpawnPrefab);
            //EventHandlers["listPrefabs"] += new Action<Player>(ListPrefabs);
            EventHandlers["deleteSpawnedPrefabs"] += new Action<Player>(DeleteSpawnedPrefabs);
            EventHandlers["getPrefabSerialized"] += new Action<Player,string>(GetPrefabSerialized);
            EventHandlers["editPrefab"] += new Action<Player,string>(EditPrefabEvent);
            //EventHandlers["Server:TrySpawn"] += new Action<Player,string,bool>(TrySpawn);
            EventHandlers["Server:TryDeleteAllObjects"] += new Action<Player>(TryDeleteAllObjects);
            EventHandlers["Server:TryDeleteInRange"] += new Action<Player,int>(TryDeleteInRange);
            EventHandlers["Server:TryMoveAll"] += new Action<Player,int,int,int>(TryMoveAll);
            EventHandlers["Server:Move"] += new Action<Player,int,Vector3,float>(MoveItem);
            EventHandlers["Server:Rotate"] += new Action<Player,int,Vector3,float>(RotateItem);
            EventHandlers["Server:ResetRotation"] += new Action<Player,int>(ResetRotation);
            EventHandlers["Server:DeleteObject"] += new Action<Player,int,int>(DeleteObject);
            EventHandlers["Server:DeleteAllObjects"] += new Action<Player,string>(DeleteAllObjectsFromDict);
            EventHandlers["Server:DeleteObjectsInRange"] += new Action<Player,string,int>(DeleteObjectsInRangeFromDict);
            EventHandlers["Server:MoveAllObjects"] += new Action<Player,string,int,int,int,float>(MoveAllObjectsFromDict);
            EventHandlers["Server:SavePrefab"] += new Action<Player,string,string>(SavePrefabFromDict);
            EventHandlers["Server:GetNetworkId"] += new Action<Player,int>(ReturnNetworkId);

        }
// Create a function to handle the event somewhere else in your code, or use a lambda.
        private void TargetFunctionDict([FromSource] Player source, Dictionary<int, ObjectSpawned> spawnedObjectsClassesDict)
        {
            // Code that gets executed once the event is triggered goes here.
            // The variable 'source' contains a reference to the player that triggered the event.
            foreach(KeyValuePair<int, ObjectSpawned> spawnedObject in spawnedObjectsClassesDict)
            {
                Debug.WriteLine(spawnedObject.Value.objectName+"ID: "+spawnedObject.Key);
            }
        }

        private void TargetFunctionObj([FromSource] Player source,  ObjectSpawned spawnedObject)
        {
            // Code that gets executed once the event is triggered goes here.
            // The variable 'source' contains a reference to the player that triggered the event.

                Debug.WriteLine(spawnedObject.objectName+"ID: "+spawnedObject.privateID);

        }
        private void TargetFunction([FromSource] Player source,  string param1,string param2)
        {
            // Code that gets executed once the event is triggered goes here.
            // The variable 'source' contains a reference to the player that triggered the event.

                Debug.WriteLine(param1+"ID: "+param2);

        }
        private void SavePrefab([FromSource] Player source,  string serializedObject,string fileName)
        {
                    Debug.WriteLine(serializedObject);
                    List <string> objectStrings=new List<string>(serializedObject.Split(';'));
                    //string fileName=objectStrings[objectStrings.Count-1]+fileExtension;
                    fileName+=fileExtension;
                    objectStrings.RemoveAt(objectStrings.Count - 1);

                    Dictionary<int,ObjectSpawned> spawnedObjectsClassesDict=GetDictFromSerializedString(serializedObject);
                    Debug.WriteLine("Printing loaded prefab");
                    String list="";
                    foreach(KeyValuePair<int, ObjectSpawned> spawnedObject in spawnedObjectsClassesDict)
                    {
                        ObjectSpawned obj=spawnedObject.Value;
                        int entityID=obj.entityID;
                        int privateID=obj.privateID;
                        string objectName=obj.objectName;
                        list+="\n"+"ID: "+privateID+" "+objectName;
                        
                        
                    } 
                    Debug.WriteLine(list);
                    Debug.WriteLine("Saving file: "+fileName);
                    SaveResourceFile(GetCurrentResourceName(),fileName,serializedObject,-1);
                    Debug.WriteLine("Saving completed");
                    Debug.WriteLine("Loading file:"+fileName);
                    string loadedPrefabString=LoadResourceFile(GetCurrentResourceName(),fileName);
                    Debug.WriteLine("Loading completed");
                    Debug.WriteLine(loadedPrefabString);



        }
        private void ListPrefabs([FromSource] Player source)
        {
            string path=GetResourcePath(GetCurrentResourceName());
            Debug.WriteLine("Path of resource:"+path);
            string[] files = Directory.GetFiles(path);
            List<string> prefabFiles=new List<string>();
            string prefabFilesConcat="";
            foreach(string file in files)
            {
                string fileName=Path.GetFileName(file);
                Debug.WriteLine(fileName);
                if(fileName.Contains(fileExtension)){
                    fileName=fileName.Split('.')[0];
                    prefabFiles.Add(fileName);
                    prefabFilesConcat+=fileName+";";
                }
            }
            Debug.WriteLine("Sending prefabList: "+prefabFilesConcat);
            string authorName=source.Name;

            TriggerClientEvent(source, "listPrefabs", prefabFilesConcat,authorName);  


        }
        private void ListPrefabsNew(int playerId, List<object> args, string raw)
        {
            string path=GetResourcePath(GetCurrentResourceName());
            Debug.WriteLine("Path of resource:"+path);
            string[] files = Directory.GetFiles(path);
            List<string> prefabFiles=new List<string>();
            string prefabFilesConcat="";

            foreach(string file in files)
            {
                string fileName=Path.GetFileName(file);
                Debug.WriteLine(fileName);
                if(fileName.Contains(fileExtension)){
                    fileName=fileName.Split('.')[0];
                    prefabFiles.Add(fileName);
                    prefabFilesConcat+=fileName+";";
                }
            }
            Debug.WriteLine("Sending prefabList: "+prefabFilesConcat);
            string authorName=Players[playerId].Name;

            TriggerClientEvent(Players[playerId], "listPrefabs", prefabFilesConcat,authorName);  


        }
        private void SavePrefabNew(int playerId, List<object> args, string raw)
        {
            string fileName=args.Count > 0 ? $"{args[0]}" : "test";
            Debug.WriteLine("Saving prefab with filename:"+fileName);

            TriggerClientEvent(Players[playerId], "Client:GetSerializedDictSavePrefab", fileName);  

        }
        private void SpawnPrefabNew(int playerId, List<object> args, string raw)
        {
            string fileName=args.Count > 0 ? $"{args[0]}" : "test";

            SpawnPrefabFromFilename(fileName);

        }
        private void DeleteSpawnedPrefabsNew(int playerId, List<object> args, string raw)
        {
            DeleteSpawnedPrefabsNoPlayer();

        }
        private void EditPrefabNew(int playerId, List<object> args, string raw)
        {
            string fileName=args.Count > 0 ? $"{args[0]}" : "test";
            Dictionary<int,ObjectSpawned> dict= SpawnPrefabFromFilename(fileName);
            string serializedString=GetDictSerializedTest(dict);
            TriggerClientEvent(Players[playerId], "loadPrefabInMenu", serializedString);  


        }
        private void EditPrefabEvent([FromSource] Player source,  string fileName){
            Dictionary<int,ObjectSpawned> dict= SpawnPrefabFromFilename(fileName);
            string serializedString=GetDictSerializedTest(dict);
            TriggerClientEvent(source, "loadPrefabInMenu", serializedString);  
        }
        private void GetPrefabSerialized([FromSource] Player source,  string fileName)
        {
            fileName+=fileExtension;
            Debug.WriteLine("Loading file:"+fileName);
            string loadedPrefabString=LoadResourceFile(GetCurrentResourceName(),fileName);
            TriggerClientEvent(source, "loadPrefab", loadedPrefabString);  
                    
        }
        private void SpawnPrefab([FromSource] Player source,  string fileName)
        {
                    SpawnPrefabFromFilename(fileName);


        }
        private Dictionary<int,ObjectSpawned> SpawnPrefabFromFilename(string fileName)
        {
                    fileName+=fileExtension;
                    Debug.WriteLine("Loading file:"+fileName);
                    string loadedPrefabString=LoadResourceFile(GetCurrentResourceName(),fileName);
                    Debug.WriteLine("Loading completed");
                    Debug.WriteLine(loadedPrefabString);

                    Dictionary<int,ObjectSpawned> spawnedObjectsClassesDict=GetDictFromSerializedString(loadedPrefabString);
                    List <Vector3> coords=new List<Vector3>();

                    foreach(KeyValuePair<int, ObjectSpawned> spawnedObject in spawnedObjectsClassesDict)
                    {
                        ObjectSpawned obj=spawnedObject.Value;
                        int hash=GetHashKey(obj.objectName);
                        int entityID= CitizenFX.Core.Native.API.CreateObject(hash, obj.Coord.X, obj.Coord.Y, obj.Coord.Z, true, false, false);
                        FreezeEntityPosition(entityID, spawnedObject.Value.frozen);
                        SetEntityRotation(entityID,obj.Rotation.X,obj.Rotation.Y,obj.Rotation.Z,2,false);
                        while(!DoesEntityExist(entityID)){
                            System.Threading.Thread.Sleep(100);  
                        }
                        entityID=NetworkGetNetworkIdFromEntity(entityID);
                        spawnedObject.Value.entityID=entityID;
                        Debug.WriteLine("entityID"+entityID);
                        spawnedEntityIds.Add(entityID);
                        coords.Add(obj.Coord);
                    }
                    int count=spawnedObjectsClassesDict.Count;
                    float sumX=0,sumY=0,sumZ=0;
                    foreach (var item in coords)
                    {
                        sumX+=item.X;
                        sumY+=item.Y;
                        sumZ+=item.Z;
                    }
                    Vector3 avgCoord=new Vector3(sumX/count,sumY/count,sumZ/count);
                    TriggerClientEvent("chat:addMessage", new
                    {
                        color = new[] {255, 0, 0},
                        args = new[] {"Prefab "+ fileName+ " spawned. Average Prefab location: X= "+avgCoord.X+", Y= "+avgCoord.Y+", Z= "+avgCoord.Z}
                    });
                    return spawnedObjectsClassesDict;


        }
        private void DeleteSpawnedPrefabs([FromSource] Player source)
        {
            foreach (var entityID in spawnedEntityIds)
            {
                DeleteEntity(entityID);
            }
            spawnedEntityIds.Clear();
        }
        private void DeleteSpawnedPrefabsNoPlayer()
        {
            foreach (var entityID in spawnedEntityIds)
            {
                int newtworkId=NetworkGetEntityFromNetworkId(entityID);

                DeleteEntity(newtworkId);
            }
            spawnedEntityIds.Clear();
        }
        Dictionary<int,ObjectSpawned> GetDictFromSerializedString(string serializedObject)
        {
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
                        Vector3 Coord=new Vector3(CoordX,CoordY,CoordZ);
                        Vector3 Rotation=new Vector3(RotationX,RotationY,RotationZ);
                        bool frozen=bool.Parse(objectParams[9]);
                        
                        ObjectSpawned obj=new ObjectSpawned(objName,entityID,privateID,Coord,Rotation,frozen);
                        spawnedObjectsClassesDict.Add(privateID,obj);
                    }
                    return spawnedObjectsClassesDict;
        }
        
        private void OpenEditMenu(int playerId, List<object> args, string raw)
        {

            TriggerClientEvent(Players[playerId], "Client:OpenObjectEditMenu", "true");

        }
        private void CreateObject(int playerId, List<object> args, string raw)
        {
       
            string objectName=args.Count > 0 ? $"{args[0]}" : "prop_generator_03b";
                
            bool freeze=args.Count > 1 ? Convert.ToBoolean(int.Parse($"{args[1]}")): true;
            int hash=GetHashKey(objectName);
            
            Vector3 plyPos = Players[playerId].Character.Position;
            Vector3 position=new Vector3(plyPos.X, plyPos.Y+3, plyPos. Z);
            int objectID= CitizenFX.Core.Native.API.CreateObject(hash, position.X, position.Y, position. Z, true, true, false);
            FreezeEntityPosition(objectID, freeze);
            while(!DoesEntityExist(objectID)){
              System.Threading.Thread.Sleep(100);  
            }
            Debug.WriteLine("EntityExists:"+DoesEntityExist(objectID).ToString());
            //var obj = Entity.FromHandle(objectID);
            objectID=NetworkGetNetworkIdFromEntity(objectID);
            Debug.WriteLine("Created model with ID="+objectID); 
            TriggerClientEvent(Players[playerId], "Client:SpawnClient",objectName, objectID,freeze,position);


        }
        private void MoveItem([FromSource] Player source,int networkID,Vector3 direction, float multiplier){
            int entityID=NetworkGetEntityFromNetworkId(networkID);
            Vector3 Coord=GetEntityCoords(entityID);
            Vector3 Offset=direction;
            Vector3 OffsetedCoord=Coord+Offset*multiplier;
            SetEntityCoords(entityID, OffsetedCoord.X, OffsetedCoord.Y, OffsetedCoord.Z, true, false, false, false);
        }
        private void RotateItem([FromSource] Player source,int networkID,Vector3 rotationAxis, float multiplier){

            int entityID=NetworkGetEntityFromNetworkId(networkID);
            Vector3 Rotation=GetEntityRotation(entityID);
            
            Vector3 OffsetedRotation=Rotation+rotationAxis*multiplier;
            SetEntityRotation(entityID, OffsetedRotation.X, OffsetedRotation.Y, OffsetedRotation.Z, 2, false);

        }
        private void ResetRotation([FromSource] Player source,int networkID){
            int entityID=NetworkGetEntityFromNetworkId(networkID);

            SetEntityRotation(entityID, 0,0,0, 2, false);
        }
        private void DeleteObject([FromSource] Player source,int networkID,int privateID){
            int entityID=NetworkGetEntityFromNetworkId(networkID);

            DeleteEntity(entityID);
            TriggerClientEvent(source, "Client:DeleteClient", privateID);

        }
        private void DeleteAllObjects(int playerId, List<object> args, string raw){

            
            TriggerClientEvent(Players[playerId], "Client:GetSerializedDictDeleteAll");

        }
        private void MoveAllObjects(int playerId, List<object> args, string raw){

            int dx=args.Count > 0 ? int.Parse($"{args[0]}"): 0;
            int dy=args.Count > 1 ? int.Parse($"{args[1]}"): 0;
            int dz=args.Count > 2 ? int.Parse($"{args[2]}"): 0; 

            TriggerClientEvent(Players[playerId], "Client:GetSerializedDictMoveAll",dx,dy,dz);

        }
        private void DeleteObjectsInRange(int playerId, List<object> args, string raw){

            int range=args.Count > 0 ? int.Parse($"{args[0]}"): 10;               
            TriggerClientEvent(Players[playerId], "Client:GetSerializedDictDeleteInRange",range);

        }
        private void TryDeleteAllObjects([FromSource] Player source)
        {
        
            TriggerClientEvent(source, "Client:DeleteAll", "true");


        }
        private void TryDeleteInRange([FromSource] Player source,int range)
        {
       
            TriggerClientEvent(source, "Client:DeleteInRange", range);

        }
        private void DeleteAllObjectsFromDict([FromSource] Player source,string serializedDict)
        {
           Dictionary<int,ObjectSpawned> dict=GetDictFromSerializedString(serializedDict);
            foreach (var obj in dict)
            {
                int entityID=NetworkGetEntityFromNetworkId(obj.Value.entityID);

                DeleteEntity(entityID);
            }
            TriggerClientEvent(source, "Client:DeleteAllClient");

        }
        private void SavePrefabFromDict([FromSource] Player source,string serializedDict, string fileName)
        {
            Dictionary<int,ObjectSpawned> spawnedObjectsClassesDict=GetDictFromSerializedString(serializedDict);
            string serializedDictionary=GetDictSerialized(spawnedObjectsClassesDict);
            spawnedObjectsClassesDict=GetDictFromSerializedString(serializedDictionary);
            fileName+=fileExtension;

            Debug.WriteLine("Printing loaded prefab");
                    String list="";
                    foreach(KeyValuePair<int, ObjectSpawned> spawnedObject in spawnedObjectsClassesDict)
                    {
                        ObjectSpawned obj=spawnedObject.Value;
                        int entityID=obj.entityID;
                        int privateID=obj.privateID;
                        //spawnedObject.Value.Coord=GetEntityCoords(entityID);
                        //spawnedObject.Value.Rotation=GetEntityRotation(entityID);
                        string objectName=obj.objectName;
                        list+="\n"+"ID: "+privateID+" "+objectName;
                        
                        
                    } 
                    Debug.WriteLine(list);
                    Debug.WriteLine("Saving file: "+fileName);
                    SaveResourceFile(GetCurrentResourceName(),fileName,serializedDictionary,-1);
                    Debug.WriteLine("Saving completed");
                    Debug.WriteLine("Loading file:"+fileName);
                    string loadedPrefabString=LoadResourceFile(GetCurrentResourceName(),fileName);
                    Debug.WriteLine("Loading completed");
                    Debug.WriteLine(loadedPrefabString);

        }
        
        private void DeleteObjectsInRangeFromDict([FromSource] Player source,string serializedDict, int range)
        {
            Dictionary<int,ObjectSpawned> dict=GetDictFromSerializedString(serializedDict);
            range*=range;
            Vector3 plyPos = source.Character.Position;
            List <int> IDsToDelete=new List<int>();
            List <int> privateIDsToDelete=new List<int>();
            foreach(KeyValuePair<int, ObjectSpawned> spawnedObject in dict)
            {
                int entityID=NetworkGetEntityFromNetworkId(spawnedObject.Value.entityID);

                Vector3 objPos=GetEntityCoords(entityID);

                if(Vector3.DistanceSquared(plyPos,objPos)<=range)
                {
                    IDsToDelete.Add(spawnedObject.Value.entityID);
                    privateIDsToDelete.Add(spawnedObject.Value.privateID);
                }
            }
            foreach (var item in IDsToDelete)
            {
                int entityID=NetworkGetEntityFromNetworkId(item);

                DeleteEntity(entityID);

            }
            foreach (var item in privateIDsToDelete)
            {
                TriggerClientEvent(source, "Client:DeleteClient", item);
            }

        }
        private void MoveAllObjectsFromDict([FromSource] Player source,string serializedDict, int dx,int dy, int dz,float multiplier)
        {
            Dictionary<int,ObjectSpawned> dict=GetDictFromSerializedString(serializedDict);
            foreach(KeyValuePair<int, ObjectSpawned> spawnedObject in dict)
            {
                MoveItem(spawnedObject.Value.entityID,new Vector3(dx,dy,dz),1);
            }
        }
        private void MoveItem(int networkId,Vector3 direction, float multiplier){
            int entityID=NetworkGetEntityFromNetworkId(networkId);

            Vector3 Coord=GetEntityCoords(entityID);
            Vector3 Offset=direction;
            Vector3 OffsetedCoord=Coord+Offset*multiplier;
            
            SetEntityCoords(entityID, OffsetedCoord.X, OffsetedCoord.Y, OffsetedCoord.Z, true, false, false, false);
        }
        private void TryMoveAll([FromSource] Player source,int dx,int dy,int dz)
        {
        
            TriggerClientEvent(source, "Client:MoveAll", dx,dy,dz);

        }
        private void ReturnNetworkId([FromSource] Player source,int entityID)
        {
            Debug.WriteLine("EntityExists:"+DoesEntityExist(entityID).ToString());
            int networkId=NetworkGetNetworkIdFromEntity(entityID);
            Debug.WriteLine("EntityID:"+entityID+"networkId"+networkId);
            TriggerClientEvent(source, "Client:SetCurrentNetworkId", networkId);

        }
        
        private string GetDictSerialized(Dictionary<int,ObjectSpawned> dict){
            string prefabSerialized = "";
            foreach(KeyValuePair<int, ObjectSpawned> spawnedObject in dict)
            {
                prefabSerialized+=spawnedObject.Value.GetSerialized()+";";
            }
            return prefabSerialized;
        }
        private string GetDictSerializedTest(Dictionary<int,ObjectSpawned> dict){
            string prefabSerialized = "";
            foreach(KeyValuePair<int, ObjectSpawned> spawnedObject in dict)
            {
                prefabSerialized+=spawnedObject.Value.GetSerializedTest()+";";
            }
            return prefabSerialized;
        }
    }
}
public class ObjectSpawned
{
        public string objectName;
        public int entityID;
        public int privateID;
        public Vector3 Coord;
        public Vector3 Rotation;
        public bool frozen;
        public ObjectSpawned(string _objectName, int _entityID,int _privateID, Vector3 _Coord, Vector3 _Rotation, bool _frozen){
            objectName=_objectName;
            entityID=_entityID;
            privateID=_privateID;
            Coord=_Coord;
            Rotation=_Rotation;
            frozen=_frozen;

        }
        //TODO deserialize object
        public ObjectSpawned(string serialized){
            objectName="";
            entityID=0;
            privateID=0;
            Coord=new Vector3(0,0,0);
        }
        public string GetSerialized(){
            entityID=NetworkGetEntityFromNetworkId(entityID);
            Coord=GetEntityCoords(entityID);
            Rotation=GetEntityRotation(entityID);
            return objectName+","+entityID+","+privateID+","+Coord.X+","+Coord.Y+","+Coord.Z+","+Rotation.X+","+Rotation.Y+","+Rotation.Z+","+frozen;
        }
        public string GetSerializedTest(){
            //Coord=GetEntityCoords(entityID);
            //Rotation=GetEntityRotation(entityID);
            return objectName+","+entityID+","+privateID+","+Coord.X+","+Coord.Y+","+Coord.Z+","+Rotation.X+","+Rotation.Y+","+Rotation.Z+","+frozen;
        }
}