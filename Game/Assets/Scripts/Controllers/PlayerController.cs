using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : ObjectController
{
    void Start()
    {
        
    }

    void Update()
    {
    }

    #region Controller
    public override void ControllerUpdate()
    {
        base.ControllerUpdate();

        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W))
            moveDir.z += 1.0f;
        if (Input.GetKey(KeyCode.S))
            moveDir.z -= 1.0f;

        if (Input.GetKey(KeyCode.A))
            moveDir.x -= 1.0f;
        if (Input.GetKey(KeyCode.D))
            moveDir.x += 1.0f;

        transform.Translate(moveDir * Time.deltaTime);
    }

    public override void OnPossess()
    {
        base.OnPossess();

        transform.position = new UnityEngine.Vector3(1.0f, 3.0f, 1.0f);
    }
    #endregion
}
