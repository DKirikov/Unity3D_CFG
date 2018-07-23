using UnityEngine;
using System.Collections;

public class Properties : MonoBehaviour {

	public DudeData dude;
	public Sprite bow_texture;
	// Use this for initialization
	void Start () 
	{
		if (dude == null)
			return;

		transform.SetParent (dude.transform);
		transform.position = dude.transform.position + new Vector3 (0, 2.2f, 0);

		TextMesh[] prop_vals = gameObject.GetComponentsInChildren<TextMesh> (); // ToDo here and everythere components could be accessed without gameObject (probably)
		foreach (TextMesh prop_val in prop_vals) 
		{
			if( prop_val.name == "Value" )
			{
				if (prop_val.gameObject.transform.parent.name == "Speed")
					prop_val.text = dude.unit.speed.ToString ();
				if (prop_val.gameObject.transform.parent.name == "Health")
					prop_val.text = dude.unit.hp.ToString ();
				if (prop_val.gameObject.transform.parent.name == "Strength")
					prop_val.text = dude.unit.strength.ToString ();
			}
		}
		
		if (dude.unit.isRangedAttack) 
		{
			foreach (var image in GetComponentsInChildren<SpriteRenderer>() )
			{
				if( image.sprite.name == "sword" )
				{
					image.sprite = bow_texture;
				}
			}
		}	
	}

	public void UpdateHealth()
	{
		DudeData dude = gameObject.transform.parent.GetComponent<DudeData> ();
		
		TextMesh[] prop_vals = GetComponentsInChildren<TextMesh>();
		int delta = 0;
		foreach (TextMesh prop_val in prop_vals)
		{
			if (prop_val.gameObject.transform.parent.name == "Health")
			{
				if( prop_val.name == "Value" )
				{
					delta = dude.unit.hp - int.Parse(prop_val.text);
					prop_val.text = dude.unit.hp.ToString();
				}
				else if( prop_val.name == "hp_delta" && delta != 0 )
				{
					prop_val.text = (delta > 0 ? "+" : "-") + delta.ToString();
					
					var prop_anim = prop_val.GetComponent<Animator> ();
					prop_anim.SetTrigger ("StartAnim");
					prop_anim.Update(0);
				}
			}
		}
	}
	
	// Update is called once per frame
	/*void Update () {
	
	}*/
}
