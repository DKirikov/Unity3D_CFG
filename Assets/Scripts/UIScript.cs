using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using CFGEngine;




public class UIScript : MonoBehaviour 
{
    public GameObject card_obj;
    public GameObject crystal_txt;
	public GameObject save_load_panel;

    public static UIScript instance = null; //ToDo check for better practice
	
	//private GameObject[] crystals = new GameObject[GameboardImpl.max_crystal_qty];

	private List<GameObject> card_objs = new List<GameObject>();

	// Use this for initialization
	void Start () 
    {
        instance = this;


		/*Vector2 sizeDelta = crystal_img.GetComponent<RectTransform>().sizeDelta;
		Vector2 pos = new Vector2(-sizeDelta.x / 2, -sizeDelta.y / 2);
		for (int i = 0; i < GameboardImpl.max_crystal_qty; ++i)
        {
            crystals[i] = Instantiate(crystal_img, pos, Quaternion.identity) as GameObject;
            crystals[i].transform.SetParent(gameObject.transform);
            crystals[i].GetComponent<RectTransform>().anchoredPosition = pos;

            pos.x -= sizeDelta.x;
        }*/

        //SetCrystalsCount(0);
    }

	private void SetCards(List<CardImpl> cards, bool passive)
	{
		Vector2 sizeDelta = card_obj.GetComponent<RectTransform>().sizeDelta;
		Vector2 pos = new Vector2(-(cards.Count*sizeDelta.x) / 2, 0);

		if( passive )
			pos.y = GetComponent<RectTransform>().rect.height - sizeDelta.y;

		foreach (var card in cards)
		{
			GameObject clone = Instantiate(card_obj, pos, Quaternion.identity) as GameObject;
			bool interactable = !passive && !GameBoard.instance.IsAICalculating();
			System.Array.ForEach(clone.GetComponentsInChildren<Button>(), x => x.interactable = interactable);
			clone.GetComponent<Button>().interactable = interactable;
			if( passive )
				clone.tag = "Player";

			clone.transform.SetParent(gameObject.transform);
			clone.GetComponent<RectTransform>().anchoredPosition = pos;
			clone.transform.localScale = new Vector3(1,1,1);
			clone.GetComponent<CardData>().SetFigure(card);

			clone.GetComponent<ToolTip> ().SetToolTip (card);

			
			card_objs.Add(clone);
			
			pos.x += sizeDelta.x;
		}		
	}

	public void SetCards(List<CardImpl> cards, List<CardImpl> other_cards)
	{
		foreach (var obj in card_objs) 
		{
			DestroyObject(obj);
		}
		card_objs.Clear ();

		SetCards (cards, false);
		SetCards (other_cards, true);
	}

    public void SetCrystalsCount(int count, int income)
    {
		/*for (int i = 0; i < GameboardImpl.max_crystal_qty; ++i)
        {
            crystals[i].SetActive(i < count);
        }*/
		crystal_txt.GetComponent<Text>().text = count.ToString() + " (+" + income.ToString() + ")";
    }
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
