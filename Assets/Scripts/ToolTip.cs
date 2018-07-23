using UnityEngine;
using UnityEngine.UI;
using System;
using CFGEngine;

public class ToolTip: MonoBehaviour
{
	private GameObject tool_tip = null;
	private DateTime show_card_timer = DateTime.MaxValue;

	public void OnMouseEnter() 
	{
		if( tool_tip != null )
			show_card_timer = DateTime.Now.AddSeconds(2);
	}

	public void OnMouseExit() 
	{
		if (tool_tip != null)
		{
			tool_tip.SetActive (false);
			show_card_timer = DateTime.MaxValue;
		}
	}

	public void SetToolTip(CardImpl card)
	{
		GameObject canvas = GameObject.Find ("Canvas");

		tool_tip = Instantiate(canvas.GetComponent<UIScript>().card_obj) as GameObject;

		tool_tip.GetComponent<RectTransform> ().localScale = new Vector3(3,3,3);
		tool_tip.transform.SetParent(canvas.transform);
		tool_tip.SetActive (false);
		tool_tip.GetComponent<CardData> ().SetFigure (card);
		System.Array.ForEach(tool_tip.GetComponentsInChildren<Button>(), x => x.interactable = true);

	}

	// Update is called once per frame
	void Update () 
	{
		if (tool_tip != null && DateTime.Now > show_card_timer) 
		{
			tool_tip.SetActive (true);

			Vector3 pos = gameObject.transform.position;
			if (gameObject.transform.parent != tool_tip.transform.parent) 
			{
				pos = UniversalInput.MousePos ();
				pos.x += 60*3 / 2 + 20;//ToDo remove 60x80 as default card size;

				if( pos.x + 60*3/2 > Screen.width )
					pos.x -= 2*(60*3 / 2 + 20);

				if( pos.y + 80*3 > Screen.height )
					pos.y = Screen.height - 80*3;
				//pos = Camera.main.WorldToScreenPoint(gameObject.transform.position);
			} 
			else 
			{
				pos.x += 60;
				pos.y += 80;
			}

			tool_tip.transform.position = pos;

			//clone.transform.position = gameObject.GetComponent<RectTransform> ().rect.max;

			show_card_timer = DateTime.MaxValue;
		}
	}}
