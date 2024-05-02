using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Util
{
    #region Object
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }
    #endregion

    #region Network
    public static bool IsServer()
    {
        return Managers.Network.IsServer;
    }

    public static bool IsClient()
    {
        return Managers.Network.IsClient;
    }

    public static bool CheckFuncCalledOnServer()
    {
        if (IsServer())
            return true;

        Debug.Log("This function must be called on server");
        return false;
    }

    public static bool CheckFuncCalledOnClient()
    {
        if (IsClient())
            return true;

        Debug.Log("This function must be called on client");
        return false;
    }
    #endregion

    #region Math
    public static float GetAngle(Vector2 vec)
    {
        return Mathf.Atan2(vec.x, vec.y) * Mathf.Rad2Deg;
    }

    public static float GetAngleY(Vector3 vec)
    {
        return Mathf.Atan2(vec.x, vec.z) * Mathf.Rad2Deg;
    }
    #endregion

    #region Bytes
    public static int GetObjectBytesSize<T>()
    {
        return Marshal.SizeOf(default(T));
    }

    public static byte[] ObjectToBytes<T>(T obj)
    {
        int datasize = Marshal.SizeOf<T>(obj);
        IntPtr buff = Marshal.AllocHGlobal(datasize);
        Marshal.StructureToPtr(obj, buff, true);
        byte[] data = new byte[datasize];
        Marshal.Copy(buff, data, 0, datasize);
        Marshal.FreeHGlobal(buff);

        return data;

    }

    public static T BytesToObject<T>(byte[] data)
    {
        IntPtr ptr = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, ptr, data.Length);
        var obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
        Marshal.FreeHGlobal(ptr);

        if (Marshal.SizeOf(obj) != data.Length)
            return default(T);

        return obj;
    }
    #endregion
}
