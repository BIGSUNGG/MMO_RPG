using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class ObjectComponent : MonoBehaviour, RpcComponentFunction
{
    public ObjectController Owner
    {
        get
        {
            if (_owner == null)
                _owner = GetComponent<ObjectController>();

            return _owner;
        }
        set { _owner = value; }
    }
    private ObjectController _owner;

    protected virtual void Start()
    {
        Owner = gameObject.GetComponent<ObjectController>();
        if (Owner == null)
        {
            Debug.LogWarning("Failed to find ObjectController");
            Debug.Assert(false);
        }
    }

    protected virtual void Update()
    {
        
    }

    #region RpcFunction
    // �ٸ� Ŭ���̾�Ʈ�� ��Ŷ�� ������ FunctionId�� �´� �Լ� ȣ��
    // functionId : ���� ��Ŷ�� �Լ� ���̵�
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    public virtual void RpcFunction_ReceivePacket(RpcComponentFunctionId functionId, byte[] packet)
    {

    }

    // Ŭ���̾�Ʈ���� ���� ��Ŷ�� �Ǽ� ��Ŷ���� Ȯ��
    // functionId : ���� ��Ŷ�� �Լ� ���̵�
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
    public virtual bool RpcFunction_Validate(RpcComponentFunctionId functionId, byte[] packet)
    {
        Debug.Log("Receive wrong function id");
        return false;
    }
    #endregion
}
