using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class CopyDlls
{
	public static string src = "Library/ScriptAssemblies"; // 拷贝源
	public static string dest = "Assets/GameRes/MyDlls"; // 拷贝目标

	public static string[] files = new []{"HelloDll.dll", "HelloDll.pdb"}; // 要拷贝的文件列表

	[MenuItem("Tools/Copy Dlls")]
	public static void DoCopyDlls()
	{
		// 创建一个目标目录（如果不存在）
		Directory.CreateDirectory(dest);

		foreach (var f in files)
		{
			// 源文件逐个拷贝到目标位置，并改名加上.bytes后缀（只有.bytes后缀的文件会被认为是二进制数据，dll无法被打包）
			Debug.Log($"{Path.Combine(src, f)} => {Path.Combine(dest, f + ".bytes")}");
			File.Copy(Path.Combine(src, f), Path.Combine(dest, f + ".bytes"), true);
		}

		AssetDatabase.Refresh(); // 拷贝资源后自动刷新项目目录
	}
}
