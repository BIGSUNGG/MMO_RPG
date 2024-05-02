using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class ObjectManager
{
    public ObjectManager()
    {
        _spawner = new List<Func<GameObject>>(new Func<GameObject>[10]);
        _spawner[(int)GameObjectType.Character] = () => { return Managers.Resource.Instantiate("Object/Character");  }; 
    }
    public Dictionary<int, GameObject> _objects { get; private set; } = new Dictionary<int, GameObject>();
    List<Func<GameObject>> _spawner;

    public GameObject FindById(int id)
	{
		GameObject go = null;
		_objects.TryGetValue(id, out go);

        if (go == null)
            Debug.Log($"Find GameObject failed Id : {id}");

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
