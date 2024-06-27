using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawnerController : ObjectController
{
    public GameObject _spawnObject;
    protected GameObject _spawnedObject;

    protected override void Start()
    {
        base.Start();

        if(Managers.Network.IsServer)
        {
            _spawnedObject = GameObject.Instantiate(_spawnObject);
            _spawnedObject.transform.position = this.transform.position;
            _spawnedObject.transform.rotation = this.transform.rotation;

            CharacterController cc = _spawnedObject.GetComponent<CharacterController>();
            if(cc)
                cc._spawnPosition = this.transform.position;
        }
    }

    protected override void Update()
    {
        base.Update();

    }
}
