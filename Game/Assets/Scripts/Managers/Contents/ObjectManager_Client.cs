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
    #region Find
    // 클라이언트에서 오브젝트를 찾을때 호출 (오브젝트가 없다면 클라이언트가 서버에게 오브젝트를 요청함)
    // id : 찾을 오브젝트 아이디
    // findAction : 오브젝트를 찾는다면 오브젝트에게 할 행동 (오브젝트가 없다면 해당 id에 오브젝트를 생성할 때 호출)
    // return : 찾은 오브젝트
    public GameObject FindOrRequest(int id, Action<GameObject> findAction = null)
    {
        if (Util.CheckFuncCalledOnClient() == false)
            return null;

        GameObject result = FindById(id);
        if (result != null)
        {
            if (findAction != null)
            {
                findAction.Invoke(result);
            }
        }
        else
        {
            if (findAction != null)
            {
                _onCreateEvent.Add(id, findAction);
            }

            // 서버에게 오브젝트 정보 요청하기
            C_RequestObjectInfo requestPacket = new C_RequestObjectInfo();
            requestPacket.RequestObjectId = id;

            Managers.Network.SendServer(requestPacket);
        }

        return result;
    }
    #endregion

    #region Create
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
            GameObject go = Create(info);
            result.Add(go);
        }

        return result;
    }

    public List<GameObject> Create(RepeatedField<ObjectInfo> infos)
    {
        return Create(infos.ToList());
    }

    // ObjectType에 맞는 오브젝트를 만들고 등록
    private GameObject CreateAndRegister(int id, GameObjectType type)
    {
        if (_objects.TryGetValue(id, out var go) && go != null)
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

    // 오브젝트 리스트에 오브젝트 등록
    public void Register(int id, ObjectController oc)
    {
        if (oc == null)
            return;

        oc.Registered(id);
        _objects[id] = oc.gameObject;
        
        _onCreateEvent.TryGetValue(id, out var action);
        if(action != null)
        {
            action.Invoke(oc.gameObject);
            _onCreateEvent.Remove(id);
        }
    }

    public int Register(ObjectController oc)
    {
        Debug.LogError("This function must called on server");
        return 0;
    }
    #endregion

    #region Delete
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

    // 오브젝트 제거 및 등록 해제
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

    // 오브젝트 리스트에서 오브젝트 제거
    public int UnRegister(ObjectController oc)
    {
        int deleteId = oc.ObjectId;

        // 오브젝트 제거
        _objects.Remove(deleteId);
        Managers.Resource.Destroy(oc.gameObject);

        return deleteId;
    }
    #endregion
}
#endif
