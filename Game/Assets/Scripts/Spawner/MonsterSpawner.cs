using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject _spawnObject;
    protected GameObject _spawnedObject;

    protected virtual void Start()
    {
        if(Managers.Network.IsServer)
        {
            _spawnedObject = GameObject.Instantiate(_spawnObject);
            _spawnedObject.transform.position = this.transform.position;
            _spawnedObject.transform.rotation = this.transform.rotation;
        }
    }

    protected virtual void Update()
    {
        
    }
}
