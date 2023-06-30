using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AddressableAssets;

public partial class LogoPage
{
	private float showSeconds = 4f;

	public void OnStart()
	{
		//KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");

		//this.progressSlider.DOValue(1, showSeconds).OnComplete(()=>{ 
		//	//UIPage.ShowPageAsync<MainPage>();
		//	Addressables.LoadSceneAsync("Main");
		//});
	}
    protected override void OnActive()
    {
        base.OnActive();
		UIPage.ShowPageAsync<LoginPage>();
    }
    //public void MyEventHandler()
    //{
    //}
}
