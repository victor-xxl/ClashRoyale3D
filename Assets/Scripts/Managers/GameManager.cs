using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace UnityRoyale
{
    public class GameManager : MonoBehaviour
    {
	    private List<ThinkingPlaceable> 所有己方可移动单位, 所有敌方可移动单位;
	    private List<ThinkingPlaceable> 所有己方建筑, 所有敌方建筑;
	    private List<ThinkingPlaceable> 所有己方游戏单位, 所有敌方游戏单位; //contains both Buildings and Units
	    private List<ThinkingPlaceable> 所有带AI游戏单位;
	    private List<Projectile> 所有投掷物;
		
	    [Header("Settings")]
		public bool autoStart = false;

		[Header("Public References")]
        public NavMeshSurface navMesh;
		public GameObject playersCastle, opponentCastle;
		public GameObject introTimeline;
        public PlaceableData castlePData;
		public ParticlePool appearEffectPool;

        private CardManager cardManager;
        private CPUOpponent CPUOpponent;
        private InputManager inputManager;
		private AudioManager audioManager;
		private UIManager UIManager;
		private CinematicsManager cinematicsManager;

        private bool gameOver = false;
        private bool updateAllPlaceables = false; //used to force an update of all AIBrains in the Update loop
        private const float THINKING_DELAY = 2f;

        private void Awake()
        {
            cardManager = GetComponent<CardManager>();
            CPUOpponent = GetComponent<CPUOpponent>();
            inputManager = GetComponent<InputManager>();
			audioManager = GetComponentInChildren<AudioManager>();
			cinematicsManager = GetComponentInChildren<CinematicsManager>();
			UIManager = GetComponent<UIManager>();

			if(autoStart)
				introTimeline.SetActive(false);

			//listeners on other managers
			cardManager.OnCardUsed += UseCard;
			CPUOpponent.OnCardUsed += UseCard;

			//initialise Placeable lists, for the AIs to pick up and find a target
			所有己方可移动单位 = new List<ThinkingPlaceable>();
            所有己方建筑 = new List<ThinkingPlaceable>();
            所有敌方可移动单位 = new List<ThinkingPlaceable>();
            所有敌方建筑 = new List<ThinkingPlaceable>();
            所有己方游戏单位 = new List<ThinkingPlaceable>();
            所有敌方游戏单位 = new List<ThinkingPlaceable>();
			所有带AI游戏单位 = new List<ThinkingPlaceable>();
			所有投掷物 = new List<Projectile>();
        }

        private void Start()
        {
			//Insert castles into lists
			SetupPlaceable(playersCastle, castlePData, Placeable.Faction.Player);
            SetupPlaceable(opponentCastle, castlePData, Placeable.Faction.Opponent);

			cardManager.LoadDeck();
            CPUOpponent.LoadDeck();

			audioManager.GoToDefaultSnapshot();

			if(autoStart)
				StartMatch();
        }

		//called by the intro cutscene
		public void StartMatch()
		{
			CPUOpponent.StartActing();
		}

        //the Update loop pings all the ThinkingPlaceables in the scene, and makes them act
        private void Update()
        {
            if(gameOver)
                return;

			for(int pN=0; pN<所有带AI游戏单位.Count; pN++)
            {
	            ThinkingPlaceable p = 所有带AI游戏单位[pN];

                if(updateAllPlaceables)
                    p.状态 = ThinkingPlaceable.角色状态.空闲; //forces the assignment of a target in the switch below

                switch(p.状态)
                {
                    case ThinkingPlaceable.角色状态.空闲:
                        //this if is for innocuous testing Units
                        if(p.targetType == Placeable.PlaceableTarget.None)
                            break;

                        ThinkingPlaceable targetToPass;
                        bool targetFound = 找靠玩家最近的列表中的单位(p.transform.position,
	                        按照阵营和攻击目标类型返回可攻击单位列表(p.faction, p.targetType),
                            out targetToPass
	                        );
                        if(!targetFound) Debug.LogError("No more targets!"); //this should only happen on Game Over
                        p.设置攻击目标V(targetToPass);
						p.寻路V();
                        break;


                    case ThinkingPlaceable.角色状态.寻路中:
						if(p.目标是否在攻击范围内())
                    	{
							p.开始攻击V();
						}
                        break;
                        

					case ThinkingPlaceable.角色状态.攻击中:
						if(p.目标是否在攻击范围内())
						{
							if(Time.time >= p.lastBlowTime + p.attackRatio)
							{
								p.处理打击效果V();
								// 通过调用动画事件OnDealDamage和OnProjectileFired，动画会产生伤害，参见ThinkingPlaceable
							}
						}
						break;

					case ThinkingPlaceable.角色状态.死亡:
						Debug.LogError("A dead ThinkingPlaceable shouldn't be in this loop");
						break;
                }
            }

			for(int prjN=0; prjN<所有投掷物.Count; prjN++)
            {
	            Projectile currProjectile = 所有投掷物[prjN];
	            float progressToTarget = currProjectile.Move(); // [0,1]
				if(progressToTarget >= 1f) // 如果子弹飞行到目标位置了
				{
					if(currProjectile.target.状态 != ThinkingPlaceable.角色状态.死亡) // 目标死亡后投掷物仍然在飞行
					{
						float newHP = currProjectile.target.受击处理(currProjectile.damage);
						currProjectile.target.healthBar.SetHealth(newHP); // 根据血值设置血条长度
					}
					Destroy(currProjectile.gameObject); // 销毁子弹
					所有投掷物.RemoveAt(prjN); // 从子弹数组销毁（不删除已销毁对象会引起内存访问异常！）
				}
			}

            updateAllPlaceables = false; //is set to true by UseCard()
        }

        private List<ThinkingPlaceable> 按照阵营和攻击目标类型返回可攻击单位列表(Placeable.Faction f, Placeable.PlaceableTarget t)
        {
            switch(t)
            {
                case Placeable.PlaceableTarget.Both:
                    return (f == Placeable.Faction.Player) ? 所有敌方游戏单位 : 所有己方游戏单位;
				case Placeable.PlaceableTarget.OnlyBuildings:
                    return (f == Placeable.Faction.Player) ? 所有敌方建筑 : 所有己方建筑;
				default:
					Debug.LogError("What faction is this?? Not Player nor Opponent.");
					return null;
            }
        }

        private bool 找靠玩家最近的列表中的单位(Vector3 p, List<ThinkingPlaceable> list, out ThinkingPlaceable t)
        {
            t = null;
            bool targetFound = false;
            float closestDistanceSqr = Mathf.Infinity; //anything closer than here becomes the new designated target

            for(int i=0; i<list.Count; i++)
            {                
				float sqrDistance = (p - list[i].transform.position).sqrMagnitude;
                if(sqrDistance < closestDistanceSqr)
                {
                    t = list[i];
                    closestDistanceSqr = sqrDistance;
                    targetFound = true;
                }
            }

            return targetFound;
        }

        public void UseCard(CardData cardData, Vector3 position, Placeable.Faction pFaction)
        {
            for(int pNum=0; pNum<cardData.placeablesData.Length; pNum++)
            {
                PlaceableData pDataRef = cardData.placeablesData[pNum];
                Quaternion rot = (pFaction == Placeable.Faction.Player) ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
                //Prefab to spawn is the associatedPrefab if it's the Player faction, otherwise it's alternatePrefab. But if alternatePrefab is null, then first one is taken
                GameObject prefabToSpawn = (pFaction == Placeable.Faction.Player) ? pDataRef.associatedPrefab : ((pDataRef.alternatePrefab == null) ? pDataRef.associatedPrefab : pDataRef.alternatePrefab);
                GameObject newPlaceableGO = Instantiate<GameObject>(prefabToSpawn, position + cardData.relativeOffsets[pNum], rot);

				if (SceneManager.GetActiveScene().buildIndex >= 8)
				{
					SetupPlaceable(newPlaceableGO, pDataRef, pFaction);
				}

				if (SceneManager.GetActiveScene().buildIndex >= 7)
				{
					appearEffectPool.UseParticles(position + cardData.relativeOffsets[pNum]);
				}
            }
			audioManager.PlayAppearSFX(position);

            updateAllPlaceables = true; //will force all AIBrains to update next time the Update loop is run
        }


        //setups all scripts and listeners on a Placeable GameObject
        private void SetupPlaceable(GameObject go, PlaceableData pDataRef, Placeable.Faction pFaction)
        {
            //Add the appropriate script
                switch(pDataRef.pType)
                {
                    case Placeable.PlaceableType.Unit:
                        Unit uScript = go.GetComponent<Unit>();
                        uScript.Activate(pFaction, pDataRef); //enables NavMeshAgent
						uScript.OnDealDamage += OnPlaceableDealtDamage;
						uScript.OnProjectileFired += OnProjectileFired;
                        AddPlaceableToList(uScript); //add the Unit to the appropriate list
                        UIManager.AddHealthUI(uScript);
                        break;

                    case Placeable.PlaceableType.Building:
                    case Placeable.PlaceableType.Castle:
                        Building bScript = go.GetComponent<Building>();
                        bScript.Activate(pFaction, pDataRef);
						bScript.OnDealDamage += OnPlaceableDealtDamage;
						bScript.OnProjectileFired += OnProjectileFired;
                        AddPlaceableToList(bScript); //add the Building to the appropriate list
                        UIManager.AddHealthUI(bScript);

                        //special case for castles
                        if(pDataRef.pType == Placeable.PlaceableType.Castle)
                        {
                            bScript.OnDie += OnCastleDead;
                        }
                        
                        navMesh.BuildNavMesh(); //rebake the Navmesh
                        break;

                    case Placeable.PlaceableType.Obstacle:
                        Obstacle oScript = go.GetComponent<Obstacle>();
                        oScript.Activate(pDataRef);
                        navMesh.BuildNavMesh(); //rebake the Navmesh
                        break;

                    case Placeable.PlaceableType.Spell:
                        //Spell sScript = newPlaceable.AddComponent<Spell>();
                        //sScript.Activate(pFaction, cardData.hitPoints);
                        //TODO: activate the spell and… ?
                        break;
                }

                go.GetComponent<Placeable>().OnDie += OnPlaceableDead;
        }

		private void OnProjectileFired(ThinkingPlaceable p)
		{
			Vector3 adjTargetPos = p.target.transform.position;
			adjTargetPos.y = 1.5f;
			Quaternion rot = Quaternion.LookRotation(adjTargetPos-p.projectileSpawnPoint.position);

			Projectile prj = Instantiate<GameObject>(p.projectilePrefab, p.projectileSpawnPoint.position, rot).GetComponent<Projectile>();
			prj.target = p.target;
			prj.damage = p.damage;
			所有投掷物.Add(prj);
		}

		private void OnPlaceableDealtDamage(ThinkingPlaceable p)
		{
			if(p.target.状态 != ThinkingPlaceable.角色状态.死亡)
			{
				float newHealth = p.target.受击处理(p.damage);
				p.target.healthBar.SetHealth(newHealth);
			}
		}

		private void OnCastleDead(Placeable c)
		{
			cinematicsManager.PlayCollapseCutscene(c.faction);
            c.OnDie -= OnCastleDead;
            gameOver = true; //stops the thinking loop

			//stop all the ThinkingPlaceables		
			ThinkingPlaceable thkPl;
			for(int pN=0; pN<所有带AI游戏单位.Count; pN++)
            {
				thkPl = 所有带AI游戏单位[pN];
				if(thkPl.状态 != ThinkingPlaceable.角色状态.死亡)
				{
					thkPl.停止移动V();
					thkPl.transform.LookAt(c.transform.position);
					UIManager.RemoveHealthUI(thkPl);
				}
			}

			audioManager.GoToEndMatchSnapshot();
			CPUOpponent.StopActing();
		}

		public void OnEndGameCutsceneOver()
		{
			UIManager.ShowGameOverUI();
		}

        private void OnPlaceableDead(Placeable p)
        {
            p.OnDie -= OnPlaceableDead; //remove the listener
            
            switch(p.pType)
            {
                case Placeable.PlaceableType.Unit:
					Unit u = (Unit)p;
                    RemovePlaceableFromList(u);
					u.OnDealDamage -= OnPlaceableDealtDamage;
					u.OnProjectileFired -= OnProjectileFired;
					UIManager.RemoveHealthUI(u);
					StartCoroutine(Dispose(u));
                    break;

                case Placeable.PlaceableType.Building:
                case Placeable.PlaceableType.Castle:
					Building b = (Building)p;
                    RemovePlaceableFromList(b);
					UIManager.RemoveHealthUI(b);
					b.OnDealDamage -= OnPlaceableDealtDamage;
					b.OnProjectileFired -= OnProjectileFired;
                    StartCoroutine(RebuildNavmesh()); //need to fix for normal buildings
					
					//we don't dispose of the Castle
					if(p.pType != Placeable.PlaceableType.Castle)
						StartCoroutine(Dispose(b));
                    break;

                case Placeable.PlaceableType.Obstacle:
                    StartCoroutine(RebuildNavmesh());
                    break;

                case Placeable.PlaceableType.Spell:
                    //TODO: can spells die?
                    break;
            }
        }

		private IEnumerator Dispose(ThinkingPlaceable p)
		{
			yield return new WaitForSeconds(3f);

			Destroy(p.gameObject);
		}

        private IEnumerator RebuildNavmesh()
        {
            yield return new WaitForEndOfFrame();

            navMesh.BuildNavMesh();
            //FIX: dragged obstacles are included in the navmesh when it's baked
        }

        private void AddPlaceableToList(ThinkingPlaceable p)
        {
			所有带AI游戏单位.Add(p);

			if(p.faction == Placeable.Faction.Player)
            {
				所有己方游戏单位.Add(p);
            	
				if(p.pType == Placeable.PlaceableType.Unit)
                    所有己方可移动单位.Add(p);
				else
                    所有己方建筑.Add(p);
            }
            else if(p.faction == Placeable.Faction.Opponent)
            {
				所有敌方游戏单位.Add(p);
            	
				if(p.pType == Placeable.PlaceableType.Unit)
                    所有敌方可移动单位.Add(p);
				else
                    所有敌方建筑.Add(p);
            }
            else
            {
                Debug.LogError("Error in adding a Placeable in one of the player/opponent lists");
            }
        }

        private void RemovePlaceableFromList(ThinkingPlaceable p)
        {
			所有带AI游戏单位.Remove(p);

			if(p.faction == Placeable.Faction.Player)
            {
				所有己方游戏单位.Remove(p);
            	
				if(p.pType == Placeable.PlaceableType.Unit)
                    所有己方可移动单位.Remove(p);
				else
                    所有己方建筑.Remove(p);
            }
            else if(p.faction == Placeable.Faction.Opponent)
            {
				所有敌方游戏单位.Remove(p);
            	
				if(p.pType == Placeable.PlaceableType.Unit)
                    所有敌方可移动单位.Remove(p);
				else
                    所有敌方建筑.Remove(p);
            }
            else
            {
                Debug.LogError("Error in removing a Placeable from one of the player/opponent lists");
            }
        }
    }
}