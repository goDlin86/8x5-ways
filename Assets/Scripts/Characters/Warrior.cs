using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//public enum Factions
//{
//	RED,
//	BLUE
//}


public class Warrior : MonoBehaviour {

	public float speed;

	public AwarenessBubble awarenessBubble;
	[HideInInspector]
	public bool isEnemy = false;
	//public Factions faction;

	Animator anim;

	protected Attacker attacker;
	protected Navigator navigator;
	protected Life life;
	protected Vector3 initialAttackPosition;

	void Start () {
		anim = GetComponent<Animator> ();

		attacker = GetComponent<Attacker>();
		attacker.OnTargetLeaveRange += targetLeftRange;
		attacker.OnTargetInRange += targetInRange;
		attacker.OnTargetDeath += targetDied;
		attacker.OnBeginAttacking += startedAttacking;
		attacker.OnActualAttack += attack;

		navigator = GetComponent<Navigator>();
		navigator.OnWalking += walking;
		navigator.OnStartWalk += startWalking;
		navigator.OnStopWalk += stopWalking;

		life = GetComponent<Life>();
		life.OnBeingAttacked += wasAttacked;

		navigator.getRoute ();
		Invoke ("StartProwl", 1f);
	}
	
	void OnDestroy()
	{
		attacker.OnTargetLeaveRange -= targetLeftRange;
		attacker.OnTargetInRange -= targetInRange;
		attacker.OnTargetDeath -= targetDied;
		attacker.OnBeginAttacking -= startedAttacking;
		attacker.OnActualAttack -= attack;

		navigator.OnWalking -= walking;
		navigator.OnStartWalk -= startWalking;
		navigator.OnStopWalk -= stopWalking;

		life.OnBeingAttacked -= wasAttacked;
	}

	void StartProwl() {
		StartCoroutine("prowl");
	}

	virtual protected IEnumerator prowl()
	{
		GameObject target;
		attacker.StopAttacking();
		gameObject.SendMessage("StartMoving");
		do
		{
			yield return 0;
			target = awarenessBubble.GetTarget();
		}
		while(target == null);
		gameObject.SendMessage("StopMoving");
		print("Found target!! ATTACK!");
		attacker.ShootTarget(target);
	}


	public void Die()
	{
		gameObject.SendMessage("PlayEffect", 0, SendMessageOptions.DontRequireReceiver);
		anim.SetBool ("death", true);
		Destroy(gameObject, 3.5f);
		//gameObject.SetActiveRecursively(false);
		//StartCoroutine(cycleBack());
	}

	protected void startedAttacking(GameObject target)
	{
		//Set for kiting limits.
		initialAttackPosition = transform.position;

		anim.SetBool ("move", false);
		anim.SetBool ("attack", true);
	}

	virtual protected void targetLeftRange()
	{
		StopCoroutine("prowl");
		StartCoroutine("prowl");
	}

	protected void targetInRange()
	{
		gameObject.SendMessage("StopMoving");
	}

	protected void targetDied()
	{
		StopCoroutine("prowl");
		StartCoroutine("prowl");
	}

	virtual protected void wasAttacked(GameObject whoAttackedMe)
	{
		if(whoAttackedMe != null)
		{
			//reportToAttacker(whoAttackedMe);

			if(life.IsAlive)	
				reactToAttacker(whoAttackedMe);
		}

		if(!life.IsAlive)
			Die();
		//else
		//	anim.SetBool ("hit", true);
	}

	virtual protected void reportToAttacker(GameObject whoAttackedMe)
	{
		if(life.IsAlive)
			whoAttackedMe.SendMessage("OnAttackNoKill", SendMessageOptions.DontRequireReceiver);
		else
		{
			whoAttackedMe.SendMessage("OnAttackKill", this, SendMessageOptions.DontRequireReceiver);
		}
	}

	virtual protected void reactToAttacker(GameObject whoAttackedMe)
	{
		attacker.ShootTarget(whoAttackedMe);
	}	


	protected void attack(GameObject target)
	{
		var rotation = Quaternion.LookRotation (target.transform.position - transform.position);
		StartCoroutine (rotateToTarget (rotation));

		StartCoroutine (Hit(target));
	}

	IEnumerator rotateToTarget(Quaternion rotation) {
		var time = 0.3f;
		var elapsedTime = 0f;
		while (elapsedTime < time) {
			transform.position = Vector3.Lerp (transform.position, initialAttackPosition, elapsedTime / time); //???????
			transform.rotation = Quaternion.Slerp (transform.rotation, rotation, elapsedTime / time);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
	}

	IEnumerator Hit(GameObject target) {
		yield return new WaitForSeconds (0.5f);
		if (target != null && target.GetComponent<Life> ().IsAlive)
			target.GetComponent<Life> ().Hurt (gameObject);
	}

	protected void startWalking()
	{
		anim.SetBool ("attack", false);
		anim.SetBool ("move", true);
	}

	protected void walking(Vector3 walkTarget)
	{

		walkTarget.y = transform.position.y;
		//print(walkTarget);
		//transform.LookAt(walkTarget);
	}

	protected void stopWalking()
	{
		anim.SetBool ("move", false);
	}




}
