using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if !UNITY_SERVER
public partial class ObjectManager
{
    public GameObject Create(ObjectInfo info)
    {
        // 오브젝트 만들기
        GameObject go = CreateAndRegister(info.ObjectId, info.ObjectType);
        return go;
    }

    public List<GameObject> Create(List<ObjectInfo> infos)
    {
        List<GameObject> result = new List<GameObject>();
       
        foreach (var info in infos)
        {
            // 오브젝트 만들기
            GameObject go = CreateAndRegister(info.ObjectId, info.ObjectType);
            result.Add(go);
        }

        return result;
    }

    public List<GameObject> Create(RepeatedField<ObjectInfo> infos)
    {
        List<GameObject> result = new List<GameObject>();

        foreach (var info in infos)
        {
            // 오브젝트 만들기
            GameObject go = CreateAndRegister(info.ObjectId, info.ObjectType);
            result.Add(go);
        }

        return result;
    }

    private GameObject CreateAndRegister(int id, GameObjectType type)
    {
        if(_objects.ContainsKey(id))
        {
            Debug.Log($"Try create object but object Id {id} is already exist");
            return null;
        }

        GameObject gameObject = _factory[(int)type].Invoke();
        if(gameObject == null)
        {
            Debug.LogError("Failed to create game object");
            return null;
        }

        ObjectController controller = gameObject.GetComponent<ObjectController>();
        if (controller == null)
        {
            Debug.LogWarning("Object controller is not exist");
            return null;
        }

        Register(id, controller);

        Debug.Log($"Make {type} Object Id : {id}");
        return gameObject;
    }

    public void Register(int id, ObjectController oc)
    {
        if (oc == null)
            return;

        oc.Registered(id);
        _objects.Add(id, oc.gameObject);
    }

    public int Register(ObjectController oc)
    {
        Debug.LogError("This function must called on server");
        return 0;
    }

    public void Delete(List<int> ids)
    {
        foreach (int id in ids)
        {
            DeleteAndUnRegister(id);
        }
    }

    public void Delete(RepeatedField<int> ids)
    {
        foreach (int id in ids)
        {
            DeleteAndUnRegister(id);
        }
    }

    public bool Delete(int id)
    {
        return DeleteAndUnRegister(id);
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
