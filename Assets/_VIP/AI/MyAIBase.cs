using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum AIState
{
	Idle,
	Seek,
	Attack,
	Die,
}

public class MyAIBase : MonoBehaviour
{
	public MyAIBase target = null; // 攻击目标

	public AIState state = AIState.Idle;
	public float lastBlowTime = 0; // 上次攻击时间

	public virtual void OnIdle() { }

	public virtual void OnSeeking() { }

	public virtual void OnAttack() { }
	public virtual void OnDie() { }
}
