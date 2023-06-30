using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MyDllLoader : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        // NOTE: TextAsset不仅可用于承载文本数据，也可用于承载二进制数据
        // 加载二进制数据（dll & pdb）到内存
        TextAsset dll = await Addressables.LoadAssetAsync<TextAsset>("HelloDll.dll").Task;
        TextAsset pdb = await Addressables.LoadAssetAsync<TextAsset>("HelloDll.pdb").Task;

        // 载入到mono虚拟机来
        var ass = Assembly.Load(dll.bytes, pdb.bytes);

        //// 打印所有该dll中的数据类型（结果应该是HelloDll这个类名被打印）=》 OK!
        //foreach (var t in ass.GetTypes())
        //{
        //    print(t);
        //}

        // 执行SayHello方法
        Type t = ass.GetType("HelloDll");
        t.GetMethod("SayHello").Invoke(null, null);

        Addressables.Release<TextAsset>(dll);
        Addressables.Release<TextAsset>(pdb);
    }
}
