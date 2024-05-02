using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : ObjectComponent
{
    protected enum RpcFunctionId
    {

    }

    protected override void Start()
    {
        base.Start();

        _curHp = _maxHp;
    }

    protected override void Update()
    {
        base.Update();
    }

    #region Health
    public bool _bDead = false; // ������Ʈ�� �׾��ִ���
    public int _curHp { get; protected set; } // ���� ü��
    public int _maxHp { get; protected set; } = 100; // �ִ� ü��

    UnityEvent _onTakeDamageEvent; // ������� �ް� ���� ȣ��Ǵ� �̺�Ʈ
    UnityEvent _onDeathEvent; // ������� �ް� ���� ȣ��Ǵ� �̺�Ʈ

    // �������� ������Ʈ�� ������� ���� �� ȣ��
    // damage : ���� �����, damageCauser : ������� �ִ� ������Ʈ
    // return : ���������� ���� �����
    public float OnServer_TakeDamage(float damage, ObjectController damageCauser) 
    {
        if (Util.CheckFuncCalledOnServer())
            return 0.0f;

        if (_bDead) // �̹� �׾��ִٸ�
            return 0.0f;

        _curHp -= (int)damage;
        _onTakeDamageEvent.Invoke();

        if(_curHp <= 0) // ü���� 0���Ϸ� �������ٸ�
        {
            _curHp = 0;
            OnServer_Death(damage, damageCauser);
        }

        return damage;
    }

    // �������� ������Ʈ�� ü���� 0���Ϸ� �������� �� ȣ��
    // damage : ���� �����, damageCauser : ������� �ִ� ������Ʈ
    public void OnServer_Death(float damage, ObjectController damageCauser)
    {
        if (Util.CheckFuncCalledOnServer())
            return;

        _bDead = true;
        _onDeathEvent.Invoke();
    }

    #endregion

    #region Sync
    #endregion
}
