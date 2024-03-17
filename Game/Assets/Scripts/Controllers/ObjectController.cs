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
    // �÷��̾ �����ϰ� �ִٸ� �� ƽ���� ȣ��
    public virtual void ControllerUpdate()
    {

    }

    public virtual void OnPossess()
    {

    }

    public virtual void OnUnpossess()
    {

    }

    // return : �÷��̾ �����ϰ� �ִ���
    public virtual bool IsLocallyControlled()
    {
        return this == Managers.Controller.MyController;
    }
    #endregion

}
