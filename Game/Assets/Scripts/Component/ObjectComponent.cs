using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class ObjectComponent : MonoBehaviour
{
    protected ObjectController _owner = null;

    protected virtual void Start()
    {
        _owner = gameObject.GetComponent<ObjectController>();
        if (_owner == null)
        {
            Debug.Log("Failed to find ObjectController");
            Debug.Assert(false);
        }
    }

    protected virtual void Update()
    {
        
    }

    #region RpcFunction
    // �ٸ� Ŭ���̾�Ʈ�� ��Ŷ�� ������ FunctionId�� �´� �Լ� ȣ��
    // ByteString : ���� ��Ŷ�� ����Ʈ �迭
    public virtual void RpcFunction_ReceivePacket(byte[] packet)
    {

    }

    // Ŭ���̾�Ʈ���� ���� ��Ŷ�� �Ǽ� ��Ŷ���� Ȯ��
    // ByteString : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
    public virtual bool RpcFunction_Validate(byte[] packet)
    {
        Debug.Log("Receive wrong function id");
        return false;
    }
    #endregion
}
