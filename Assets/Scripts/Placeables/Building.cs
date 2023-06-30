using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace UnityRoyale
{
    public class Building : ThinkingPlaceable
    {
		//Inspector references
		[Header("Timelines")]
		public PlayableDirector constructionTimeline;
		public PlayableDirector destructionTimeline;

		private void Awake()
		{
			audioSource = GetComponent<AudioSource>();
		}

        public void Activate(Faction pFaction, PlaceableData pData)
        {
			pType = pData.pType;
            faction = pFaction;
            hitPoints = pData.hitPoints;
            targetType = pData.targetType;
			attackAudioClip = pData.attackClip;
			dieAudioClip = pData.dieClip;
			//TODO: add more as necessary

			attackRange = pData.attackRange;
			attackRatio = pData.attackRatio;
			damage = pData.damagePerAttack;

			constructionTimeline.Play();
        }

		public override void 开始攻击V()
		{
			base.开始攻击V();

			// 发射子弹
		}

		protected override void 死亡V()
        {
            base.死亡V();
			audioSource.PlayOneShot(dieAudioClip, 1f);

            //Debug.Log("Building is dead", gameObject);
			destructionTimeline.Play();
        }
    }
}