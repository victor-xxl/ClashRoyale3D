using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityRoyale
{
    public class ThinkingPlaceable : Placeable
    {
        public enum 角色状态
        {
            被拖拽, // 玩家正在拖拽一张游戏单位卡牌，尚未放到游戏区域内（预览状态）
            空闲, // 卡牌变兵后的初始状态
            寻路中, // 走向攻击目标
            攻击中, // 循环播放攻击动画，不移动
            死亡, // 播放死亡动画，然后从游戏区域销毁
        }
        [HideInInspector] public 角色状态 状态 = 角色状态.被拖拽; // 角色当前的状态
        public enum AttackType
        {
	        Melee, // Melee
	        Ranged, // Ranged
        }
        [HideInInspector] public AttackType attackType;
        [HideInInspector] public ThinkingPlaceable target; // 攻击目标（只能是ThinkingPlaceable）
        [HideInInspector] public HealthBar healthBar; // 血条

        [HideInInspector] public float hitPoints; // 血值
        [HideInInspector] public float attackRange; // 攻击范围
        [HideInInspector] public float attackRatio; // 攻击速率
        [HideInInspector] public float lastBlowTime = -1000f; // 上次打击时间（因为攻击之间要有间隔时间）
        [HideInInspector] public float damage; // 攻击伤害值
		[HideInInspector] public AudioClip attackAudioClip; // 攻击音效
        
        [HideInInspector] public float timeToActNext = 0f; // 下一次造成伤害的事件

		//Inspector references
		[Header("Projectile for Ranged")]
		public GameObject projectilePrefab; // 投掷物预制体
		public Transform projectileSpawnPoint; // 投掷物的生成位置（弓箭手、法师的手部）

		private Projectile projectile; // projectilePrefab创建的投掷物实例
        protected AudioSource audioSource; // 攻击音效

		public UnityAction<ThinkingPlaceable> OnDealDamage, OnProjectileFired; // 攻击造成伤害的回调函数，投掷物发射的回调函数

        public virtual void 设置攻击目标V(ThinkingPlaceable t)
        {
            target = t;
            t.OnDie += 判断目标是否已死亡;
        }

        public virtual void 开始攻击V()
        {
            状态 = 角色状态.攻击中;
        }

        public virtual void 处理打击效果V()
        {
            lastBlowTime = Time.time;
        }

        public virtual void 寻路V()
        {
	        状态 = 角色状态.寻路中;
        }

        public virtual void 停止移动V()
        {
	        状态 = 角色状态.空闲;
        }

        protected virtual void 死亡V()
        {
	        状态 = 角色状态.死亡;
	        audioSource.pitch = Random.Range(.9f, 1.1f);
	        audioSource.PlayOneShot(dieAudioClip, 1f);

	        if (OnDie != null)
		        OnDie(this);
        }

        // 被Animation的Event调用
        public void 处理攻击伤害()
        {
			//only melee units play audio when the attack deals damage
			if(attackType == AttackType.Melee)
				audioSource.PlayOneShot(attackAudioClip, 1f);

			if(OnDealDamage != null)
				OnDealDamage(this);
		}

		// 被Animation的Event调用
		public void 发射投掷物()
        {
			//ranged units play audio when the projectile is fired
			audioSource.PlayOneShot(attackAudioClip, 1f);

			if(OnProjectileFired != null)
				OnProjectileFired(this);
		}
        
        public bool 目标是否在攻击范围内()
        {
            return (transform.position-target.transform.position).sqrMagnitude <= attackRange*attackRange;
        }

        public float 受击处理(float amount)
        {
            hitPoints -= amount;
            //Debug.Log("Suffering damage, new health: " + hitPoints, gameObject);
            if(状态 != 角色状态.死亡
				&& hitPoints <= 0f)
            {
                死亡V();
            }

            return hitPoints;
        }

        protected void 判断目标是否已死亡(Placeable p)
        {
	        //Debug.Log("My target " + p.name + " is dead", gameObject);
	        状态 = 角色状态.空闲;

	        target.OnDie -= 判断目标是否已死亡;

	        timeToActNext = lastBlowTime + attackRatio;
        }
    }
}
