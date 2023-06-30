using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MyCardMgr : MonoBehaviour
{
	public static MyCardMgr instance;

	#region DeckPage
	// 这部分字段需要在Deck加载完毕后动态赋值
	public Transform[] cards; // 出牌区的牌的位置

	//public GameObject[] cardPrefabs; // 卡牌预制体（弓箭手/战士/法师/。。。）

	public Transform canvas; // 创建出来的卡牌必须放在Canvas下，否则显示不出来

	public Transform startPos, endPos; // 发牌动画的起始位置和终止位置
	#endregion

	public MeshRenderer forbiddenAreaRenderer;

	private Transform previewCard; // 预览卡牌

	private void Awake()
	{
		instance = this;
	}

	// Start is called before the first frame update
	//void Start()
	async void Start()
	{
		//StartCoroutine(创建卡牌到预览区(0.5f));
		//StartCoroutine(预览区到出牌区(0, 1f));

		//StartCoroutine(创建卡牌到预览区(1.5f));
		//StartCoroutine(预览区到出牌区(1, 2f));

		//StartCoroutine(创建卡牌到预览区(2.5f));
		//StartCoroutine(预览区到出牌区(2, 3f));

		//StartCoroutine(创建卡牌到预览区(3.5f));

		// 加载出牌区UI，创建卡牌必须在出牌区创建完毕再执行
		// 由于await是异步等待，被放在ShowPageAsync的lambda表达式写的回调函数里，所以要给该lambda表达式加上async标记
		UIPage.ShowPageAsync<DeckPage>(async () =>
		{
			await 创建卡牌到预览区(0.5f);
			await 预览区到出牌区(0, 0.5f);

			await 创建卡牌到预览区(0.5f);
			await 预览区到出牌区(1, 0.5f);

			await 创建卡牌到预览区(0.5f);
			await 预览区到出牌区(2, 0.5f);

			await 创建卡牌到预览区(0.5f);
		});

	}

	//public IEnumerator 创建卡牌到预览区(float 延迟值)
	public async Task 创建卡牌到预览区(float 延迟值)
	{
		//yield return new WaitForSeconds(延迟值);
		await new WaitForSeconds(延迟值); // 这里会创建一个Task，在await时C#会返回这个Task对象，所以返回值类型不能写void

		int iCard = Random.Range(0, MyCardModel.instance.list.Count);
		MyCard card = MyCardModel.instance.list[iCard];

		Debug.Log($"准备实例化卡牌[{card.cardPrefab}]");
		//GameObject cardPrefab = Resources.Load<GameObject>(card.cardPrefab);
		////GameObject cardPrefab = cardPrefabs[Random.Range(0, cardPrefabs.Length)];
		//previewCard = Instantiate(cardPrefab).transform;

		// 由于是异步实例化，所以我们不能通过InstantiateAsync的返回值直接获取到创建的卡牌对象
		// 我们需要等待异步实例化完毕，同时又不能阻塞Unity程序的执行（会造成卡顿），
		// 所以我们要用C#的异步等待语法
		// NOTE: 在Addressable系统中，InstantiateAsync == Resources.Load + Instantiate
		// NOTE: 这里的报错是因为await异步等待必须写在支持异步的方法里----必须声明该方法为异步方法
		// NOTE: 用了异步就可以不再使用协程了，前提是我们要引入支持协程所有功能（WaitForSeconds/WaitForEndOfFrame）的一个库
		// 关于这个库的使用，可以参考git：https://github.com/svermeulen/Unity3dAsyncAwaitUtil
		GameObject cardPrefab = await Addressables.InstantiateAsync(card.cardPrefab).Task; // 异步等待实例化预制体完毕
		previewCard = cardPrefab.transform;

		previewCard.SetParent(canvas, false); // 位于父节点下的（0，0，0）偏移处
		previewCard.localScale = Vector3.one * 0.7f;
		previewCard.position = startPos.position;
		previewCard.DOMove(endPos.position, .5f);

		previewCard.GetComponent<MyCardView>().data = card;
	}

	//public IEnumerator 预览区到出牌区(int i, float 延迟值)
	public async Task 预览区到出牌区(int i, float 延迟值)
	{
		//yield return new WaitForSeconds(延迟值);
		await new WaitForSeconds(延迟值);

		previewCard.localScale = Vector3.one;
		previewCard.DOMove(cards[i].position, .5f);

		previewCard.GetComponent<MyCardView>().index = i;
	}
}
