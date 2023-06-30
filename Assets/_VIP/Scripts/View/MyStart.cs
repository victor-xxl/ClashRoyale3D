using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 在这里填入页面类型（也即页面名称），但是首先我们要导出页面脚本对象
        // 这个报错先不管，先看看页面导出了没有
        UIRoot.SetInitParams(new Vector2(1080, 1920));
        UIPage.ShowPageAsync<LogoPage>();
    }
}
