using UnityEngine;
using System.Collections;
using CFGEngine;

public class DudeWalk : StateMachineBehaviour 
{


	private double anim_length = 0;
	private bool is_hit_set = false;// ToDo fix hack
	
	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		anim_length = 0;
		is_hit_set = false;
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	//{
	//
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		DudeData dude = animator.gameObject.GetComponent<DudeData> ();
		
		anim_length = 0;
		dude.cur_animation = null;
	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		DudeData dude = animator.gameObject.GetComponent<DudeData> ();
		if (dude.cur_animation != null)
		{
			double angle;
			Vector2 cur_pos = new Vector2();
			
			if (dude.cur_animation.GetPosition(anim_length, out cur_pos, out angle))
			{
				//ToDo change GetPosition to return Vector3 and rotate board to be xy instead of xz
				Vector3 offset = new Vector3((float)cur_pos.x, 0.1f, (float)cur_pos.y);
				animator.gameObject.transform.position = offset;
				
				Quaternion rotation = Quaternion.identity;
				//rotation.eulerAngles = new Vector3(0, (float)angle, 0);
				rotation.SetFromToRotation(dude.orientation, Quaternion.Euler(0, (float)angle, 0) * Vector3.forward);
				animator.gameObject.transform.rotation = rotation;
				
				anim_length += Time.deltaTime * 5 * dude.moving_speed;//ToDo this is not good
			}
			else if( !is_hit_set )
			{
				is_hit_set = true;
				if( dude.dude_to_kill != null )
					animator.SetTrigger("hit");
				else
					animator.SetTrigger("walk");
			}}

	}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
