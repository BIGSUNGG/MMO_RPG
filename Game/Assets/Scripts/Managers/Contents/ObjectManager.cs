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
        _factory = new List<Func<GameObject>>(new Func<GameObject>[10]);
        _factory[(int)GameObjectType.KnightPlayer] = () => { return Managers.Resource.Instantiate("Object/Knight");  }; 
        _factory[(int)GameObjectType.KnightMonster] = () => { return Managers.Resource.Instantiate("Object/Monster");  };
        _factory[(int)GameObjectType.Npc] = () => { return Managers.Resource.Instantiate("Object/Npc");  };
    }
    List<Func<GameObject>> _factory;
    Dictionary<int, Action<GameObject>> _onCreateEvent = new Dictionary<int, Action<GameObject>>(); // Key : object id, Value : 오브젝트를 찾았았을 때 이벤트
    public Dictionary<int, GameObject> _objects { get; private set; } = new Dictionary<int, GameObject>();

    public GameObject FindById(int id)
	{
		GameObject go = null;
		_objects.TryGetValue(id, out go);

        if (go == null)
            Debug.LogWarning($"Find GameObject failed Id : {id}");

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

		_objects = new Dictionary<int, GameObject>();
        _onCreateEvent = new Dictionary<int, Action<GameObject>>();
	}
}
