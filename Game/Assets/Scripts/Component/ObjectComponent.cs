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
    // 다른 클라이언트로 패킷을 받으면 FunctionId에 맞는 함수 호출
    // ByteString : 받은 패킷의 바이트 배열
    public virtual void RpcFunction_ReceivePacket(byte[] packet)
    {

    }

    // 클라이언트에서 받은 패킷이 악성 패킷인지 확인
    // ByteString : 받은 패킷의 바이트 배열
    // return : 받은 패킷이 악성 패킷이 아닌지
    public virtual bool RpcFunction_Validate(byte[] packet)
    {
        Debug.Log("Receive wrong function id");
        return false;
    }
    #endregion
}
