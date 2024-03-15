using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    public ObjectManager()
    {
        _spawner = new List<Func<GameObject>>(new Func<GameObject>[10]);
        _spawner[(int)GameObjectType.Character] = () => { return Managers.Resource.Instantiate("Object/Character");  }; 
    }
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
    List<Func<GameObject>> _spawner;

    public GameObject Add(ObjectInfo info)
    {
        GameObject gameObject = _spawner[(int)info.ObjectType].Invoke();
        _objects.Add(info.ObjectId, gameObject);
        return gameObject;
    }

	public void Remove(int id)
	{
        if (_objects.ContainsKey(id) == false)
            return;

        GameObject go = FindById(id);
        if (go == null)
            return;

        _objects.Remove(id);
        Managers.Resource.Destroy(go);
    }

    public GameObject FindById(int id)
	{
		GameObject go = null;
		_objects.TryGetValue(id, out go);
		return go;
	}

	public GameObject Find(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects.Values)
		{
			if (condition.Invoke(obj))
				return obj;
		}

		return null;
	}

	public void Clear()
	{
		foreach (GameObject obj in _objects.Values)
			Managers.Resource.Destroy(obj);
		_objects.Clear();
	}
}
