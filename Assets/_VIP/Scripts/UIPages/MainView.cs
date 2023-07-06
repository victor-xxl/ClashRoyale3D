using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public partial class MainPage
{
	public void OnStart()
	{
		//KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");

		this.battleButton.onClick.AddListener(() =>
		{
			KBEngine.Event.fireIn("EnterRoom");
			//Addressables.LoadSceneAsync("Battle").Completed += MainPage_Completed;
		});
	}

	private void MainPage_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> obj)
	{
		UIPage.CloseAllPages();
	}

	protected override void OnActive()
	{
		UIPage.ShowPageAsync<TopFixPage>();
		UIPage.ShowPageAsync<BottomFixPage>();

	}

	//public void MyEventHandler()
	//{
	//}
}
