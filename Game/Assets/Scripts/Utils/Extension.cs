﻿using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Extension
{
    public static int GiveDamage(this GameObject go, ObjectController victim, int damage)
    {
        HealthComponent health = go.GetComponentInChildren<HealthComponent>();
        if (health != null)
        {
            return health.OnServer_GiveDamage(victim, damage);
        }
        else
        {
            Debug.LogWarning("HealthComponent is not exist");
            return 0;
        }
    }

    public static int TakeDamage(this GameObject go, ObjectController attacker, int damage)
    {
        HealthComponent health = go.GetComponentInChildren<HealthComponent>();
        if (health != null)
        {
            return health.OnServer_TakeDamage(attacker, damage);
        }
        else
        {
            Debug.LogWarning("HealthComponent is not exist");
            return 0;
        }
    }


    public static ObjectComponent GetComponent(this GameObject go, GameComponentType compType)
    {
        switch (compType)
        {
            case GameComponentType.ObjectComponent:
                return go.GetComponent<ObjectComponent>();
            case GameComponentType.CharacterMovementComponent:
                return go.GetComponent<CharacterMovementComponent>();
            case GameComponentType.PlayerMovementComponent:
                return go.GetComponent<PlayerMovementComponent>();
            case GameComponentType.HealthComponent:
                return go.GetComponent<HealthComponent>();
            default:
                return null;
        }
    }

    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
	{
		return Util.GetOrAddComponent<T>(go);
	}

	public static void BindEvent(this GameObject go, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
	{
		UI_Base.BindEvent(go, action, type);
	}

	public static bool IsValid(this GameObject go)
	{
		return go != null && go.activeSelf;
	}
}
