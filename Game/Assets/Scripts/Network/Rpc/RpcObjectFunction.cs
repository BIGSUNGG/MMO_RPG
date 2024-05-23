using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface RpcObjectFunction
{
    // �ٸ� Ŭ���̾�Ʈ�� ��Ŷ�� ������ FunctionId�� �´� �Լ� ȣ��
    // ByteString : ���� ��Ŷ�� ����Ʈ �迭
    public void RpcFunction_ReceivePacket(RpcObjectFunctionId functionId, byte[] packet);

    // Ŭ���̾�Ʈ���� ���� ��Ŷ�� �Ǽ� ��Ŷ���� Ȯ��
    // ByteString : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
    public bool RpcFunction_Validate(RpcObjectFunctionId functionId, byte[] packet);
}
