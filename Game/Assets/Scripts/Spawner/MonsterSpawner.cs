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

            CharacterController cc = _spawnedObject.GetComponent<CharacterController>();
            if(cc)
            {
                cc._spawnPosition = this.transform.position;
                Debug.Log($"{this.transform.position}");
            }
        }
    }

    protected virtual void Update()
    {
        
    }
}
