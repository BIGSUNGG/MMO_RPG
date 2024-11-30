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
        GameObject go = Create(info.ObjectId, info.ObjectType);
        return go;
    }

    public List<GameObject> Create(List<ObjectInfo> infos)
    {
        List<GameObject> result = new List<GameObject>();
       
        foreach (var info in infos)
        {
            // 오브젝트 만들기
            GameObject go = Create(info.ObjectId, info.ObjectType);
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
            GameObject go = Create(info.ObjectId, info.ObjectType);
            result.Add(go);
        }

        return result;
    }

    private GameObject Create(int id, GameObjectType type)
    {
        if(_objects.ContainsKey(id))
        {
            Debug.Log($"Try create object but object Id {id} is already exist");
            return null;
        }

        GameObject gameObject = _spawner[(int)type].Invoke();

        ObjectController controller = gameObject.GetComponent<ObjectController>();
        if (controller != null)
            controller.Created(id);

        _objects.Add(id, gameObject);

        Debug.Log($"Make {type} Object Id : {id}");
        return gameObject;
    }

    public void Delete(List<int> ids)
    {
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
        }
    }

    public void Delete(RepeatedField<int> ids)
    {
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
        }
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

        return true;
    }

}
#endif
