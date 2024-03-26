using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_SERVER
public partial class ObjectManager
{
    int _curId = 0;
    List<int> _deletedId = new List<int>();

    public GameObject Create(ObjectInfo info)
    {
        // 만들 오브젝트 아이디 구하기
        int createId;
        if (_deletedId.Count == 0)
        {
            createId = _curId++;
        }
        else
        {
            int count = _deletedId.Count - 1;
            createId = _deletedId[count];
            _deletedId.RemoveAt(count);
        }

        // 오브젝트 만들기
        GameObject go = Create(createId, info.ObjectType);

        // 만든 오브젝트 모든 클라이언트에 전송
        S_SpawnObject spawnPacket = new S_SpawnObject();
        ObjectInfo objectInfo = new ObjectInfo();
        objectInfo.ObjectId = createId;
        objectInfo.ObjectType = info.ObjectType;
        spawnPacket.SpawnInfo = objectInfo;
        Managers.Network.SendMulticast(spawnPacket);

        return go;
    }

    public List<GameObject> Create(List<ObjectInfo> infos)
    {
        List<GameObject> result = new List<GameObject>();

        S_SpawnObjects spawnPacket = new S_SpawnObjects();
       
        foreach (var info in infos)
        {
            // 만들 오브젝트 아이디 구하기
            int createId;
            if (_deletedId.Count == 0)
            {
                createId = _curId++;
            }
            else
            {
                int count = _deletedId.Count - 1;
                createId = _deletedId[count];
                _deletedId.RemoveAt(count);
            }

            // 오브젝트 만들기
            GameObject go = Create(createId, info.ObjectType);

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

    public List<GameObject> Create(RepeatedField<ObjectInfo> infos)
    {
        List<GameObject> result = new List<GameObject>();

        S_SpawnObjects spawnPacket = new S_SpawnObjects();

        foreach (var info in infos)
        {
            // 만들 오브젝트 아이디 구하기
            int createId;
            if (_deletedId.Count == 0)
            {
                createId = _curId++;
            }
            else
            {
                int count = _deletedId.Count - 1;
                createId = _deletedId[count];
                _deletedId.RemoveAt(count);
            }

            // 오브젝트 만들기
            GameObject go = Create(createId, info.ObjectType);

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

    private GameObject Create(int id, GameObjectType type)
    {
        GameObject gameObject = _spawner[(int)type].Invoke();

        if (gameObject == null)
        {
            Debug.Log($"Failed to create object");
            return null;
        }


        ObjectController controller = gameObject.GetComponent<ObjectController>();
        if (controller != null)
            controller.Created(id);

        _objects.Add(id, gameObject);

        Debug.Log($"create {type} Object Id : {id}");
        return gameObject;
    }

    public void Delete(List<int> ids)
    {
        S_DespawnObjects despawnPacket = new S_DespawnObjects();

        foreach (int id in ids)
        {      

            // 아이디에 맞는 오브젝트가 있는지
            if (_objects.ContainsKey(id) == false)
                return;

            // 아이디에 맞는 오브젝트 찾기
            GameObject go = FindById(id);
            if (go == null)
                return;

            // 오브젝트 제거
            _objects.Remove(id);
            Managers.Resource.Destroy(go);

            despawnPacket.ObjectIds.Add(id);
        }

        // 오브젝트 제거 패킷 보내기
        if(despawnPacket.ObjectIds.Count > 0)
            Managers.Network.SendMulticast(despawnPacket);

    }

    public void Delete(RepeatedField<int> ids)
    {
        S_DespawnObjects despawnPacket = new S_DespawnObjects();

        foreach (int id in ids)
        {

            // 아이디에 맞는 오브젝트가 있는지
            if (_objects.ContainsKey(id) == false)
                return;

            // 아이디에 맞는 오브젝트 찾기
            GameObject go = FindById(id);
            if (go == null)
                return;

            // 오브젝트 제거
            _objects.Remove(id);
            Managers.Resource.Destroy(go);

            despawnPacket.ObjectIds.Add(id);
        }

        // 오브젝트 제거 패킷 보내기
        if(despawnPacket.ObjectIds.Count > 0)
            Managers.Network.SendMulticast(despawnPacket);
    }

    public bool Delete(int id)
    {
        // 아이디에 맞는 오브젝트가 있는지
        if (_objects.ContainsKey(id) == false)
            return false;

        // 아이디에 맞는 오브젝트 찾기
        GameObject go = FindById(id);
        if (go == null)
            return false;

        // 오브젝트 제거
        _objects.Remove(id);
        Managers.Resource.Destroy(go);

        // 오브젝트 제거 패킷 보내기
        S_DespawnObject despawnPacket = new S_DespawnObject();
        despawnPacket.ObjectId = id;

        return true;
    }
}
#endif
