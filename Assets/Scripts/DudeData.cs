using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

using CFG;
using CFGEngine;

[System.Serializable]
public class AnimationInfo
{
	public AnimationClip animation;
	public AudioClip sound;
	public enum EType {hit, walk, die, idle, block, idle_action};
	public EType type;
}

public class DudeData : MonoBehaviour, CFGEngine.UnitNotificator
{
	public Texture[] colored_textures;

	public float color_intensity = 1.0f;
	public float moving_speed = 1.0f;

	public AnimationInfo[] animations = new AnimationInfo[]
	{
		new AnimationInfo{animation = null, sound = null, type = AnimationInfo.EType.idle},
		new AnimationInfo{animation = null, sound = null, type = AnimationInfo.EType.walk},
		new AnimationInfo{animation = null, sound = null, type = AnimationInfo.EType.die},
		new AnimationInfo{animation = null, sound = null, type = AnimationInfo.EType.block},
		new AnimationInfo{animation = null, sound = null, type = AnimationInfo.EType.hit},
	};

	public Vector3 orientation = new Vector3 (0, 0, 1);

	public DudeData dude_to_kill = null;//ToDo remove if possible
	public CFG.Animation cur_animation = null;//ToDo remove if possible

    public UnitImpl unit;
	public bool first_strike = false;//ToDo remove if possible

	private Dictionary<AnimationInfo.EType, List<AnimationInfo> > animation_by_type = null;

	private Dictionary<string, AudioClip> current_sounds = new Dictionary<string, AudioClip>();

	private DateTime time_to_action = DateTime.MaxValue;

	private const double min_action_min = 0.5, max_action_min = 1;
	private Func<double> GetTimeDilta = () => min_action_min + (max_action_min-min_action_min)*
		new System.Random ((int)DateTime.Now.Ticks & 0x0000FFFF).NextDouble ();

	private RuntimeAnimatorController myController;

	private AnimationClip cur_idle;
    //[RequireComponent(typeof(Animator))]//ToDo and so one

    // Use this for initialization
	void Start () 
    {

		//m_Animator = GetComponent<Animator>(); //ToDo and so one

		gameObject.AddComponent<AudioSource> ();

		if(unit != null)
			gameObject.AddComponent<ToolTip> ().SetToolTip (new CardImpl(unit));

		/*var mesh_coll = gameObject.GetComponentInChildren<MeshCollider> ();
		if (mesh_coll != null)
			mesh_coll.enabled = false;

		var cap_coll = gameObject.GetComponentInChildren<CapsuleCollider> ();
		if (cap_coll != null)
			cap_coll.enabled = false;*/
		
		animation_by_type = new Dictionary<AnimationInfo.EType, List<AnimationInfo> >();
		foreach(var anim in animations)
		{
			List<AnimationInfo> list = null;
			if( !animation_by_type.TryGetValue(anim.type, out list) )
			{
				list = new List<AnimationInfo>();
				animation_by_type.Add(anim.type, list);
			}

			list.Add(anim);
		}

		if (animation_by_type [AnimationInfo.EType.idle].Count > 1) 
		{
			List<AnimationInfo> idles = null;
			if (!animation_by_type.TryGetValue (AnimationInfo.EType.idle_action, out idles)) 
			{
				idles = new List<AnimationInfo> ();
				animation_by_type.Add (AnimationInfo.EType.idle_action, idles);
			}

			idles.AddRange (animation_by_type [AnimationInfo.EType.idle]);
		}

		if( animation_by_type.ContainsKey(AnimationInfo.EType.idle_action) )
			time_to_action = DateTime.Now.AddMinutes(GetTimeDilta());

		if (gameObject.tag != "Dude") 
		{
			cur_idle = animation_by_type [AnimationInfo.EType.idle] [0].animation;
			Animator animator = GetComponent<Animator> ();
			myController = animator.runtimeAnimatorController;

			ResetAnimations(false, false);
		}

		if (unit != null && unit.oriented_cell.cell != null) 
		{
			unit.SetNotificator (this);

			Quaternion rotation = Quaternion.identity;
			//rotation.eulerAngles = new Vector3 (0, unit.command_idx == 0 ? 90 : -90, 0);
			rotation.SetFromToRotation(orientation, unit.command_idx == 0 ? Vector3.right : Vector3.left);

			Vector2 cur_pos = GameBoard.instance.FindCellPlace (unit.oriented_cell);
			transform.rotation = rotation;
			if (unit.isTeleport) 
			{
				transform.position = new Vector3 (cur_pos.x, 0.1f, cur_pos.y);
			} 
			else 
			{
				GetComponent<Animator> ().SetTrigger ("walk");

				float shadow = 10.0f;
				Vector2 from_pos = new Vector2 (unit.command_idx == 0 ? -shadow : shadow, cur_pos.y);
				transform.position = new Vector3 (from_pos.x, 0.1f, from_pos.y);
				cur_animation = CFG.MoveAnimator.CreateAnimation (from_pos, cur_pos);
			}
		
			UnityEngine.Object properties = Resources.Load("Properties", typeof(GameObject));
			GameObject prop_clone = Instantiate (properties) as GameObject;
			prop_clone.GetComponent<Properties>().dude = this;

			foreach (var render in gameObject.GetComponentsInChildren<Renderer> ()) 
			{
				foreach( var mat in render.materials )
				{
					if( Array.Find(colored_textures, obj => mat.mainTexture == obj) != null )
					{
						Color col = (unit.command_idx == 0 ? Color.red : Color.green);

						if( mat.HasProperty("_EmissionMap") && mat.GetTexture("_EmissionMap") != null )
						{
							mat.SetColor("_EmissionColor", Color.Lerp(Color.black, col, color_intensity));
						}
						else
							mat.color = Color.Lerp(Color.white, col, color_intensity);;
					}
				}
			}

			if( unit.isVampire )
			{
				UnityEngine.Object dark_circle = Resources.Load("Effects/DarkCircle", typeof(GameObject));
				GameObject dark_circle_clone = Instantiate (dark_circle) as GameObject;
				
				dark_circle_clone.transform.SetParent (transform);
				dark_circle_clone.transform.position = transform.position;
			}

			if( unit.armor > 0 )
			{
				UnityEngine.Object shield = Resources.Load("Effects/Shield", typeof(GameObject));
				GameObject shield_clone = Instantiate (shield) as GameObject;

				shield_clone.transform.SetParent (transform);
				shield_clone.transform.position = transform.position;
			}
		}
	
	}
	
	public void Initialize(UnitImpl new_unit)
	{
		this.unit = new_unit;
	}
	
	public void PlayRunAnimation(MoveImpl move)
	{
		if (gameObject.tag != "Dude") 
		{
			ResetAnimations(false, false);
		}
			
		if (unit.isTeleport) 
		{
			//ToDo change GetPosition to return Vector3 and rotate board to be xy instead of xz
			Vector2 cur_pos = GameBoard.instance.FindTargetCellPlace(move);
			Vector3 offset = new Vector3((float)cur_pos.x, 0.1f, (float)cur_pos.y);
			gameObject.transform.position = offset;
		}
		else
		{
			GetComponent<Animator> ().SetTrigger ("walk");
		
			cur_animation = CFG.MoveAnimator.CreateAnimation (move);
		}
	}

	public void PlayHitAnimation (UnitImpl unit_to_kill)
	{
		if (gameObject.tag != "Dude") 
		{
			ResetAnimations(false, false);//ToDo change only jit animation and so one
		}

		DudeData suffered_dude = GameBoard.instance.FindDude(unit_to_kill);

		if (suffered_dude.gameObject.tag != "Dude" && suffered_dude != this) 
		{
			//ToDo fliendly fire ???
			suffered_dude.ResetAnimations(unit.command_idx == suffered_dude.unit.command_idx, false);
		}

		if (cur_animation == null) 
			GetComponent<Animator> ().SetTrigger ("hit");

		dude_to_kill = suffered_dude;
		first_strike = true;
	}

	public void PlayBlockAnimation (bool healing)
	{
		if (gameObject.tag != "Dude") 
		{
			ResetAnimations(healing, false);//ToDo change only jit animation and so one
		}
			

		if (cur_animation == null) 
			GetComponent<Animator> ().SetTrigger ("block");
	}

	public void PlaySound(Func<string, bool> name_pred, AudioClip default_sound)
	{
		AudioClip sound = null;
		foreach (var elem in current_sounds) 
		{
			if( name_pred(elem.Key) )
				sound = elem.Value;
		}
			
		if (sound == null)
			sound = default_sound;

		if (sound != null) 
		{
			var source = GetComponent<AudioSource> ();
			source.loop = name_pred("DudeIdle") || name_pred("DudeWalk");
			source.clip = sound;
			source.Play ();
		}
	}
	
	private void SelectAnimation(AnimatorOverrideController animatorOverride, 
	                        	 System.Random rand,
	                             AnimationInfo.EType type,
	                        	 string name)
	{
		List<AnimationInfo> infos = null;
		if( animation_by_type.TryGetValue(type, out infos) && infos.Count > 0 ) 
		{
			AnimationInfo info = infos[rand.Next(infos.Count)];
			animatorOverride [name] = info.animation;
			current_sounds [name] = info.sound;		
		}
	}

	private void ResetAnimations(bool is_healed, bool idle_from_action)
	{
		Animator animator = GetComponent<Animator> ();

		AnimatorOverrideController animatorOverride = new AnimatorOverrideController ();
		
		animatorOverride.runtimeAnimatorController = myController;

		System.Random rand = new System.Random((int) DateTime.Now.Ticks & 0x0000FFFF);

		SelectAnimation(animatorOverride, rand, AnimationInfo.EType.hit,   "DudeHit");
		SelectAnimation(animatorOverride, rand, AnimationInfo.EType.walk,  "DudeWalk");
		SelectAnimation(animatorOverride, rand, AnimationInfo.EType.die,   "DudeDie");
		//SelectAnimation(animatorOverride, rand, AnimationInfo.EType.idle,  "DudeIdle");
		SelectAnimation(animatorOverride, rand, AnimationInfo.EType.idle_action,  "DudeIdleAction");

		if( is_healed )
			SelectAnimation(animatorOverride, rand, AnimationInfo.EType.idle, "DudeBlock");
		else
			SelectAnimation(animatorOverride, rand, AnimationInfo.EType.block, "DudeBlock");

		if (idle_from_action) 
		{
			AnimationClip anim = (animator.runtimeAnimatorController as AnimatorOverrideController) ["DudeIdleAction"];

			animatorOverride ["DudeIdle"] = anim;		
			animatorOverride ["DudeIdleAction"] = anim;
			cur_idle = anim;
		}
		else
		{
			animatorOverride ["DudeIdle"] = cur_idle;		
		}

		animator.runtimeAnimatorController = animatorOverride;

		if( animation_by_type.ContainsKey(AnimationInfo.EType.idle_action) )
			time_to_action = DateTime.Now.AddMinutes(GetTimeDilta());
	}

	// Update is called once per frame
	void Update () 
    {
		Animator animator = GetComponent<Animator> ();
		if (DateTime.Now > time_to_action) 
		{
			if (animator.GetCurrentAnimatorStateInfo (0).IsName ("DudeIdle")) 
			{
				if (gameObject.tag != "Dude") 
				{ //ToDo think harder
					ResetAnimations (false, false);
				}


				animator.SetTrigger ("action");
			}

			time_to_action = DateTime.Now.AddMinutes(GetTimeDilta());
		}

		if (gameObject.tag != "Dude" && animator.GetCurrentAnimatorStateInfo (0).IsName ("DudeIdleAction")) 
		{
			var animatorOverride = animator.runtimeAnimatorController as AnimatorOverrideController;
			if (animatorOverride ["DudeIdle"] != animatorOverride ["DudeIdleAction"]
				&&
				animation_by_type [AnimationInfo.EType.idle].Exists (info => info.animation == animatorOverride ["DudeIdleAction"])) 
			{
				ResetAnimations(false, true);
			}
		}
	}
}
