using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcSpawnerController : ObjectController
{
    public GameObject _spawnObject;
    protected GameObject _spawnedObject;

    protected override void Start()
    {
        base.Start();

        if (Managers.Network.IsServer)
        {
            _spawnedObject = GameObject.Instantiate(_spawnObject);
            _spawnedObject.transform.position = this.transform.position;
            _spawnedObject.transform.rotation = this.transform.rotation;

            NpcController npc = _spawnedObject.GetComponent<NpcController>();
            if (npc)
            {
                Managers.Object.Register(npc);
            }
            else
            {
                Debug.LogError("NpcController is not exist");
            }                
        }
    }

    protected override void Update()
    {
        base.Update();

    }
}
