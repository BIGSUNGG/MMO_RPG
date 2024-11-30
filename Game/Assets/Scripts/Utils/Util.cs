using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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
    // 바이트 배열을 인티저 리스트로 변환
    // bytes : 변환할 바이트 배열
    // offset : 바이트 시작 인덱스
    // count : 리스트 사이즈
    // return : 변환한 인티저 리스트
    public static List<int> BytesToIntList(byte[] bytes, int offset, int count)
    {
        List<int> list = new List<int>(count);
        for(int i = 0; i < count; i++)
        {
            int value = BitConverter.ToInt32(bytes, offset + i * sizeof(int));
            list.Add(value);
        }

        return list;
    }

    // 바이트 배열을 실수 리스트로 변환
    // bytes : 변환할 바이트 배열
    // offset : 바이트 시작 인덱스
    // count : 리스트 사이즈
    // return : 변환한 실수 리스트
    public static List<float> BytesToFloatList(byte[] bytes, int offset, int count)
    {
        List<float> list = new List<float>(count);
        for (int i = 0; i < count; i++)
        {
            float value = BitConverter.ToSingle(bytes, offset + i * sizeof(float));
            list.Add(value);
        }

        return list;
    }

    // 인티저 리스트를 바이트 배열로 변환
    // list : 변환할 인티저 리스트
    // return : 변환한 바이트 배열
    public static byte[] IntListToBytes(List<int> list)
    {
        byte[] bytes = new byte[list.Count * sizeof(int)];
        for (int i = 0; i < list.Count; i++)
            Array.Copy(BitConverter.GetBytes((int)list[i]), 0, bytes, sizeof(int) * i, sizeof(int));

        return bytes;
    }

    // 실수 리스트를 바이트 배열로 변환
    // list : 변환할 실수 리스트
    // return : 변환한 바이트 배열
    public static byte[] FloatListToBytes(List<float> list)
    {
        byte[] bytes = new byte[list.Count * sizeof(float)];
        for (int i = 0; i < list.Count; i++)
            Array.Copy(BitConverter.GetBytes((float)list[i]), 0, bytes, sizeof(float) * i, sizeof(float));

        return bytes;
    }

    // T의 자료형의 사이트를 구함
    public static int GetObjectBytesSize<T>()
    {
        return Marshal.SizeOf(default(T));
    }

    // 구조체를 바이트 배열로 변환
    // obj : 변환할 구조체
    // return : 변환한 바이트 배열
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

    // 바이트 배열을 T 구조체로 변환
    // data : 변환할 바이트 배열
    // return : 변환한 구조체
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
