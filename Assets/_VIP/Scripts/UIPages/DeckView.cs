using System.Collections.Generic;
using UnityEngine;

public partial class DeckPage
{
	public void OnStart()
	{
		//KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");

	}

	protected override void OnActive()
	{
		MyCardMgr.instance.canvas = this.transform;
		MyCardMgr.instance.startPos = this.startPos;
		MyCardMgr.instance.endPos = this.endPos;

		// 获取cardPanel下三张卡牌的位置
		for (int i = 0; i < this.cardPanel.transform.childCount; i++)
		{
			MyCardMgr.instance.cards[i] = this.cardPanel.transform.GetChild(i);
		}
	}

	//public void MyEventHandler()
	//{
	//}
}
