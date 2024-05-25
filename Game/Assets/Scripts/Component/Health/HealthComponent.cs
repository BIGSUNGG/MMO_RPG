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

    UnityEvent _onTakeDamageEvent = new UnityEvent(); // ������� �ް� ���� ȣ��Ǵ� �̺�Ʈ
    UnityEvent _onDeathEvent = new UnityEvent(); // ������� �ް� ���� ȣ��Ǵ� �̺�Ʈ

    // �������� ������Ʈ�� ������� �� �� ȣ��
    // damage : �� �����
    // victim : ������� ���� ������Ʈ
    // return : ���������� �� �����
    public float OnServer_GiveDamage(GameObject victim, float damage)
    {
        if (Util.CheckFuncCalledOnServer() == false) // �������� ȣ��������� ���
            return 0.0f;

        HealthComponent health = victim.GetComponentInChildren<HealthComponent>();
        if (health == null)
            return 0.0f;

        return health.OnServer_TakeDamage(gameObject, damage);
    }

    // �������� ������Ʈ�� ������� ���� �� ȣ��
    // damage : ���� �����
    // damageCauser : ������� �ִ� ������Ʈ
    // return : ���������� ���� �����
    public float OnServer_TakeDamage(GameObject attacker, float damage) 
    {
        if (Util.CheckFuncCalledOnServer() == false) // �������� ȣ��������� ���
            return 0.0f;

        if (_bDead) // �̹� �׾��ִٸ�
            return 0.0f;

        _curHp -= (int)damage;
        _onTakeDamageEvent.Invoke();

        if(_curHp <= 0) // ü���� 0���Ϸ� �������ٸ�
        {
            _curHp = 0;
            OnServer_Death(damage, attacker);
        }

        Debug.Log($"{_curHp}");

        return damage;
    }

    // �������� ������Ʈ�� ü���� 0���Ϸ� �������� �� ȣ��
    // damage : ���� �����, damageCauser : ������� �ִ� ������Ʈ
    public void OnServer_Death(float damage, GameObject attacker)
    {
        if (Util.CheckFuncCalledOnServer() == false) // �������� ȣ��������� ���
            return;

        _bDead = true;
        _onDeathEvent.Invoke();
        Debug.Log("Dead");
    }

    #endregion

}
