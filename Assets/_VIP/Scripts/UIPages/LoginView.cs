using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

public partial class LoginPage
{
    public void OnStart()
    {
        KBEngine.Event.registerOut(KBEngine.EventOutTypes.onLoginBaseapp, this, "OnLoginBaseapp");
        genAccountSlider.onValueChanged.AddListener((value) =>
        {
            Debug.LogError(value);
            string s = new string('x', 6).Replace("x", value.ToString());
            accInput.text = s;
            pwdInput.text = s;
        });
        loginButton.onClick.AddListener(() =>
        {
            KBEngine.Event.fireIn(KBEngine.EventInTypes.login, accInput.text, pwdInput.text, Encoding.UTF8.GetBytes("xxl.com"));
        });
    }

    public void OnLoginBaseapp()
    {
        Addressables.LoadSceneAsync("Main");
    }
}
