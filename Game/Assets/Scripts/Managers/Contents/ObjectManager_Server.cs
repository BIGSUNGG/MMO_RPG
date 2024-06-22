using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_SERVER
public partial class ObjectManager
{
    object _lock = new object();

    int _curId = 0;
    int GetRegisterId()
    {
        return _curId++;
    }

    public GameObject Create(ObjectInfo info)
    {
        lock (_lock)
        {
            // 만들 오브젝트 아이디 구하기
            int createId = GetRegisterId();
	
	        // 오브젝트 만들기
	        GameObject go = CreateAndRegister(createId, info.ObjectType);
            if (go == null)
                return null;

            // 만든 오브젝트 모든 클라이언트에 전송
            S_SpawnObject spawnPacket = new S_SpawnObject();
	        ObjectInfo objectInfo = new ObjectInfo();
	        objectInfo.ObjectId = createId;
	        objectInfo.ObjectType = info.ObjectType;
	        spawnPacket.SpawnInfo = objectInfo;
	        Managers.Network.SendMulticast(spawnPacket);
	
	        return go;
        }
    }

    public List<GameObject> Create(List<ObjectInfo> infos)
    {
        lock (_lock)
        {
	        List<GameObject> result = new List<GameObject>();
	
	        S_SpawnObjects spawnPacket = new S_SpawnObjects();
	       
	        foreach (var info in infos)
	        {
                // 만들 오브젝트 아이디 구하기
                int createId = GetRegisterId();

	            // 오브젝트 만들기
	            GameObject go = CreateAndRegister(createId, info.ObjectType);
                if (go == null)
                    continue;

                // 만든 오브젝트 모든 클라이언트에 전송
                ObjectInfo objectInfo = new ObjectInfo();
	            objectInfo.ObjectId = createId;
	            objectInfo.ObjectType = info.ObjectType;
	            spawnPacket.SpawnInfos.Add(objectInfo);
	
	            result.Add(go);
	        }
	
	        if(spawnPacket.SpawnInfos.Count > 0)
	            Managers.Network.SendMulticast(spawnPacket);
	
	        return result;
        }
    }

    public List<GameObject> Create(RepeatedField<ObjectInfo> infos)
    {
        lock (_lock)
        {
	        List<GameObject> result = new List<GameObject>();
	
	        S_SpawnObjects spawnPacket = new S_SpawnObjects();
	
	        foreach (var info in infos)
	        {
                // 만들 오브젝트 아이디 구하기
                int createId = GetRegisterId();
	
	            // 오브젝트 만들기
	            GameObject go = CreateAndRegister(createId, info.ObjectType);
                if (go == null)
                    continue;
	
	            // 만든 오브젝트 모든 클라이언트에 전송
	            ObjectInfo objectInfo = new ObjectInfo();
	            objectInfo.ObjectId = createId;
	            objectInfo.ObjectType = info.ObjectType;
	            spawnPacket.SpawnInfos.Add(objectInfo);
	
	            result.Add(go);
	        }
	
	        if(spawnPacket.SpawnInfos.Count > 0)
	            Managers.Network.SendMulticast(spawnPacket);
	
	        return result;
        }
    }

    // 오브젝트 타입에 맞는 오브젝트를 만들고 id에 맞춰 추가
    private GameObject CreateAndRegister(int id, GameObjectType type)
    {
        lock (_lock)
        {
            if(_objects.ContainsKey(id))
            {
                Debug.LogWarning($"Try create object but object Id {id} is already exist");
                return null;
            }

	        GameObject gameObject = _factory[(int)type].Invoke();	
	        if (gameObject == null)
	        {
	            Debug.LogError($"Failed to create object");
	            return null;
	        }	
	
	        ObjectController oc = gameObject.GetComponent<ObjectController>();
	        if (oc == null)
            {
                Debug.LogError("Object controller is not exist");
                return null;
            }

            Register(id, oc);
	
	        Debug.Log($"create {type} Object Id : {id}");
	        return gameObject;
        }
    }

    public int Register(ObjectController oc)
    {
        // 만들 오브젝트 아이디 구하기
        int registerId = GetRegisterId();
        Register(registerId, oc);
	    return registerId;
    }

    public void Register(int id, ObjectController oc)
    {
        if (oc == null)
            return;

        oc.Registered(id);
        _objects.Add(id, oc.gameObject);
    }

    public void Delete(List<int> ids)
    {
        lock (_lock)
        {
	        S_DespawnObjects despawnPacket = new S_DespawnObjects();
	
	        foreach (int id in ids)
	        {
                if (DeleteAndUnRegister(id) == false)
                    continue;

                despawnPacket.ObjectIds.Add(id);
	        }
	
	        // 오브젝트 제거 패킷 보내기
	        if(despawnPacket.ObjectIds.Count > 0)
	            Managers.Network.SendMulticast(despawnPacket);
        }
    }

    public void Delete(RepeatedField<int> ids)
    {
        lock (_lock)
        {
	       S_DespawnObjects despawnPacket = new S_DespawnObjects();
	
	        foreach (int id in ids)
	        {
                if (DeleteAndUnRegister(id) == false)
                    continue;

                despawnPacket.ObjectIds.Add(id);
	        }
	
	        // 오브젝트 제거 패킷 보내기
	        if(despawnPacket.ObjectIds.Count > 0)
	            Managers.Network.SendMulticast(despawnPacket);
        }
    }

    public bool Delete(int id)
    {
        lock (_lock)
        {
            if (DeleteAndUnRegister(id) == false)
                return false;

            // 오브젝트 제거 패킷 보내기
            S_DespawnObject despawnPacket = new S_DespawnObject();
	        despawnPacket.ObjectId = id;
            Managers.Network.SendMulticast(despawnPacket);
	
	        return true;
        }
    }

    public bool DeleteAndUnRegister(int id)
    {
        // 아이디에 맞는 오브젝트가 있는지
        if (_objects.ContainsKey(id) == false)
            return false;

        // 아이디에 맞는 오브젝트 찾기
        GameObject go = FindById(id);
        if (go == null)
            return false;

        ObjectController oc = go.GetComponent<ObjectController>();
        if (oc == null)
            return false;

        // 오브젝트 제거
        UnRegister(oc);

        return true;
    }

    public int UnRegister(ObjectController oc)
    {
        int deleteId = oc.ObjectId;

        // 오브젝트 제거
        _objects.Remove(deleteId);
        Managers.Resource.Destroy(oc.gameObject);

        return deleteId;
    }
}
#endif
