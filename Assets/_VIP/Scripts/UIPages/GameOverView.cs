using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static UnityRoyale.Placeable;
using DG.Tweening;

public partial class GameOverPage
{
	public void OnStart()
	{
		//KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");

		// 点击ok跳到main场景
		this.oKButton.onClick.AddListener(() =>
		{
			UIPage.CloseAllPages();
			Addressables.LoadSceneAsync("Main");
		});
	}

	protected override void OnActive()
	{
		// 每次显示该ui，都要显示获胜方动画
		Debug.Log($"OnActive: {data}");
		var faction = (Faction)data;
		var winner = faction == Faction.Player ? kingRed : kingRed; // 拿到获胜方的皇冠父节点

		// 做动画将皇冠震动、淡入显示
		var cg = winner.GetComponent<CanvasGroup>();
		cg.alpha = 0;
		cg.DOFade(1, 1.5f); // 淡入

		winner.transform.DOShakeScale(1.5f); // 震动
		
		winImage.transform.localPosition = winner.localPosition; // 胜利文字显示在胜利方
	}

	//public void MyEventHandler()
	//{
	//}
}
