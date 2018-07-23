using UnityEngine;
using System.Collections;

using System;//ToDo remove

public class DudeHit : StateMachineBehaviour {

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		DudeData dude = animator.gameObject.GetComponent<DudeData> ();
		DudeData suffered_dude = dude.dude_to_kill;
		
		//ToDo change it
		Vector2 delta = GameBoard.instance.FindCellPlace(suffered_dude.unit.oriented_cell) - 
			GameBoard.instance.FindCellPlace(dude.unit.oriented_cell);
		
		double angle = 180 / Math.PI * Math.Atan2(delta.x, delta.y); //ToDo check sign
		//ToDo change to Vector2.Angle(points[i + 1] - points[i], Vector2.up) or something
		
		Quaternion rotation = Quaternion.identity;
		rotation.SetFromToRotation(dude.orientation, Quaternion.Euler(0, (float)angle, 0) * Vector3.forward);
		//rotation.eulerAngles = new Vector3(0, (float)angle, 0);
		dude.gameObject.transform.rotation = rotation;

		suffered_dude.GetComponent<Animator>().SetTrigger("block");

		//ToDo same logic is present in UnitImpl.RunAndHit
		if (suffered_dude.dude_to_kill == null && 
		    !dude.unit.isRangedAttack && 
		    dude.unit.healing == 0 && 
		    dude.first_strike && 
		    suffered_dude.unit.hp > 0)
		{
			suffered_dude.dude_to_kill = dude;
			suffered_dude.GetComponent<Animator>().SetTrigger("hit");		
		}
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		DudeData dude = animator.gameObject.GetComponent<DudeData> ();

		dude.dude_to_kill = null;
		dude.first_strike = false;

		if( dude.unit.isVampire )
			animator.gameObject.GetComponentInChildren<Properties> ().UpdateHealth ();
	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
