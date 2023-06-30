using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityRoyale;
using static UnityRoyale.Placeable;

public class CPU : MonoBehaviour
{
	public float interval = 5; // 出牌时间间隔

	public Transform[] range = new Transform[2];

	private bool isGameOver = false;

    // Start is called before the first frame update
    async void Start()
    //void Start()
    {
		KBEngine.Event.registerOut("OnGameOver", this, "OnGameOver");

	    await CardOut();
    }

	// NOTE: 必须public，否则kb反射不到此方法
	public void OnGameOver(Faction faction)
	{
		Debug.Log($"OnGameOver({faction})");
		isGameOver = true;
	}

    //IEnumerator CardOut()
    async Task CardOut()
    {
		// 这里出兵的协程中，每一个异步操作前，都要加isGameOver判定
	    while (true)
	    {
		    var cardList = MyCardModel.instance.list;
		    var cardData = cardList[Random.Range(0, cardList.Count)];

			if (isGameOver)
				break;

            var viewList = await MyCardView.CreatePlacable(
	            cardData,
	            new Vector3(Random.Range(range[0].position.x,range[1].position.x), 0, Random.Range(range[0].position.z,range[1].position.z)),
	            MyPlaceableMgr.instance.transform,
	            Placeable.Faction.Opponent
	            );

			foreach (var view in viewList)
			{
				MyPlaceableMgr.instance.his.Add(view);
			}

			if (isGameOver)
				break;

		    //yield return new WaitForSeconds(interval); // 采用设定的时间间隔出兵
		    await new WaitForSeconds(interval); // 采用设定的时间间隔出兵
	    }
    }
}
