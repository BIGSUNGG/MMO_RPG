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
    public bool _bDead = false; // 오브젝트가 죽어있는지
    public int _curHp { get; protected set; } // 현재 체력
    public int _maxHp { get; protected set; } = 100; // 최대 체력

    UnityEvent _onTakeDamageEvent; // 대미지를 받고 나서 호출되는 이벤트
    UnityEvent _onDeathEvent; // 대미지를 받고 나서 호출되는 이벤트

    // 서버에서 오브젝트가 대미지를 받을 때 호출
    // damage : 받은 대미지, damageCauser : 대미지를 주는 오브젝트
    // return : 최종적으로 받은 대미지
    public float OnServer_TakeDamage(float damage, ObjectController damageCauser) 
    {
        if (Util.CheckFuncCalledOnServer())
            return 0.0f;

        if (_bDead) // 이미 죽어있다면
            return 0.0f;

        _curHp -= (int)damage;
        _onTakeDamageEvent.Invoke();

        if(_curHp <= 0) // 체력이 0이하로 떨어졌다면
        {
            _curHp = 0;
            OnServer_Death(damage, damageCauser);
        }

        return damage;
    }

    // 서버에서 오브젝트의 체력이 0이하로 떨어졌을 때 호출
    // damage : 받은 대미지, damageCauser : 대미지를 주는 오브젝트
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
