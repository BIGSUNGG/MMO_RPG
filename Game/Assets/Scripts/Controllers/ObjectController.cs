using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectController : MonoBehaviour
{
    void Start()
    {
        
    }

    private void Update()
    {

    }

    #region Object
    public int ObjectId { get; private set; }

    public virtual void Spawn(int id)
    {
        ObjectId = id;
    }
    #endregion

    #region Controller
    // 플레이어가 빙의하고 있다면 매 틱마다 호출
    public virtual void ControllerUpdate()
    {

    }

    public virtual void OnPossess()
    {

    }

    public virtual void OnUnpossess()
    {

    }

    // return : 플레이어가 빙의하고 있는지
    public virtual bool IsLocallyControlled()
    {
        return this == Managers.Controller.MyController;
    }
    #endregion

}
