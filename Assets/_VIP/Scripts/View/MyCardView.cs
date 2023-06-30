using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityRoyale;

public class MyCardView : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
	public MyCard data;

	public int index;

	private Transform previewHolder;

	private Camera mainCam;

	private void Start()
	{
		mainCam = Camera.main;

		previewHolder = GameObject.Find("PreviewHolder").transform;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		// 把该张卡牌放到所有卡牌的所在的节点的最后一个，使其在绘制时叠加在其他卡牌的上面
		transform.SetAsLastSibling();

		// 将敌方区域渲染为禁放区
		MyCardMgr.instance.forbiddenAreaRenderer.enabled = true;
	}

	private bool isDragging = false; // 是否已经卡牌变兵
	public async void OnDrag(PointerEventData eventData)
	{
		// 移动卡牌到鼠标位置
		RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent as RectTransform, eventData.position, null, out Vector3 posWorld);

		transform.position = posWorld;

		// 从鼠标位置发射一条射线
		Ray ray = mainCam.ScreenPointToRay(eventData.position);

		// 判断该射线碰到场景什么位置
		bool hitGround = Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer("PlayingField"));

		// 如果碰到场景物体
		if (hitGround)
		{
			previewHolder.transform.position = hit.point;

			if (isDragging == false) // 如果卡牌之前没有被拖拽出来（没有变成小兵）
			{
				print("命中地面 & 卡牌没有变兵");

				isDragging = true; // 防止重入

				// 1.隐藏该张卡牌
				GetComponent<CanvasGroup>().alpha = 0f;

				// 2.创建预览卡牌
				// NOTE: 这里暂不能用await，因为在CreatePlacable还没有完成之前，if这个代码段可能已经被重入了无数次
				// 就会创建多个重叠的角色，所以要么不用await，要么isDragging = true前移
				await CreatePlacable(data, hit.point, previewHolder.transform, Placeable.Faction.Player);
				//CreatePlacable(data, hit.point, previewHolder.transform, Placeable.Faction.Player);
			}
			else
			{
				print("命中地面 & 卡牌已经变兵");
			}
		}
		else // 鼠标没有命中地面（正放回出牌位置）
		{
			if (isDragging) // 如果卡牌曾经激活（曾经放到场景中了）
			{
				print("鼠标没有命中地面（正放回出牌位置）");
				// 1.标记卡牌为未激活（未显示预览小兵）
				isDragging = false;

				// 2.显示卡牌
				GetComponent<CanvasGroup>().alpha = 1f;

				// 3.销毁预览用的小兵
				foreach (Transform trUnit in previewHolder)
				{
					Addressables.ReleaseInstance(trUnit.gameObject);
				}
			}
		}
	}

	/// <summary>
	/// 根据兵种数据，创建一个兵种到场地中
	/// </summary>
	/// <param name="cardData"></param>
	/// <param name="pos"></param>
	/// <param name="parent"></param>
	/// <param name="faction"></param>
	//public static async List<MyPlaceableView> CreatePlacable(MyCard cardData, Vector3 pos, Transform parent, Placeable.Faction faction)
	public static async Task<List<MyPlaceableView>> CreatePlacable(MyCard cardData, Vector3 pos, Transform parent, Placeable.Faction faction)
	{
		List<MyPlaceableView> viewList = new List<MyPlaceableView>();

		// 2.从卡牌数据数组找出该张卡牌的数据
		for (int i = 0; i < cardData.placeablesIndices.Length; i++)
		{
			// 2.1 取出小兵数据
			int unitId = cardData.placeablesIndices[i]; // 10000

			MyPlaceable p = null;
			for (int j = 0; j < MyPlaceableModel.instance.list.Count; j++)
			{
				if (MyPlaceableModel.instance.list[j].id == unitId)
				{
					p = MyPlaceableModel.instance.list[j];
					break;
				}
			}

			// 2.2 取出小兵之间的相对偏移
			Vector3 offset = cardData.relativeOffsets[i];

			// 2.3.生成该卡牌对应的小兵数组，并且将其设置为预览用的卡牌（将其放置到一个统一的节点下（previewHolder）
			//Profiler.BeginSample("Create unit by Resources");
			//GameObject unitPrefab = Resources.Load<GameObject>(faction == Placeable.Faction.Player ? p.associatedPrefab : p.alternatePrefab);

			////GameObject unit = GameObject.Instantiate(unitPrefab, previewHolder, false);
			////unit.transform.localPosition = offset;

			////parent.position = pos;
			//GameObject unit = GameObject.Instantiate(unitPrefab, parent, false);
			//Profiler.EndSample();

			// NOTE: 由于InstantiateAsync是异步的，会造成性能分析器在前一个EndSample还没执行到的时候，就执行了下一个BeginSample
			// Unity不允许性能分析器的Begin/End数量不匹配，所以报错了，这里我们不用Profiler分析
			//Profiler.BeginSample("Create unit by Addressables");
			string prefabName = faction == Placeable.Faction.Player ? p.associatedPrefab : p.alternatePrefab;
			GameObject unit = await Addressables.InstantiateAsync(prefabName, parent, false).Task;
			//Profiler.EndSample();

			unit.transform.localPosition = offset;
			unit.transform.position = pos + offset;

			if (faction == Placeable.Faction.Opponent)
			{
				unit.transform.Rotate(0, 180, 0);
			}

			MyPlaceable p2 = p.Clone();
			p2.faction = faction;
			MyPlaceableView view = unit.GetComponent<MyPlaceableView>();
			view.data = p2;

			viewList.Add(view);
		}
		return viewList;
	}

	public async void OnPointerUp(PointerEventData eventData)
	{
		// 从鼠标位置发射一条射线
		Ray ray = mainCam.ScreenPointToRay(eventData.position);

		// 判断该射线碰到场景什么位置
		bool hitGround = Physics.Raycast(ray, float.PositiveInfinity, 1 << LayerMask.NameToLayer("PlayingField"));

		if (hitGround)
		{
			OnCardUsed();

			// 销毁打出去的卡牌
			Addressables.ReleaseInstance(this.gameObject);

			// 从预览区取一张卡牌放到出牌区
			// NOTE: 这里的await只是异步等待，没有new一个Task对象，所以本方法的返回值类型可以为void
			await MyCardMgr.instance.预览区到出牌区(index, 0.5f);

			// 生成一张卡牌放到预览区
			await MyCardMgr.instance.创建卡牌到预览区(0.5f);
		}
		else
		{
			// 卡牌放回出牌区
			transform.DOMove(MyCardMgr.instance.cards[index].position, .2f);
		}

		MyCardMgr.instance.forbiddenAreaRenderer.enabled = false;
	}

	private void OnCardUsed()
	{
		// 游戏单位放到游戏单位管理器（MyPlaceableView）下
		for (int i = previewHolder.childCount - 1; i >= 0; i--)
		{
			Transform trUnit = previewHolder.GetChild(i);

			trUnit.SetParent(MyPlaceableMgr.instance.transform, true);

			MyPlaceableMgr.instance.mine.Add(trUnit.GetComponent<MyPlaceableView>());
		}
	}
}
