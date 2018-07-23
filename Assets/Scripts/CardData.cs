using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System; 

using CFGEngine;

public class CardData : MonoBehaviour
{
	public CardImpl card;
	public Sprite bow_texture;
	public Sprite shield_texture;
	public Sprite teeth_texture;
	public Sprite cross_texture;
	public Sprite giant_texture;


	// Use this for initialization
	void Start () 
    {
	}


    //ToDo disable during animation
	public void SetFigure(CardImpl card)
    {
		this.card = card;
		GetComponentInChildren<Text>().text = card.name;

        Text[] prop_vals = GetComponentsInChildren<Text>();
        foreach (Text prop_val in prop_vals)
        {
            if (prop_val.gameObject.transform.parent.name == "Speed")
				prop_val.text = card.speed >= 0 ? card.speed.ToString() : "";
            if (prop_val.gameObject.transform.parent.name == "Health")
				prop_val.text = card.defaultHP >= 0 ? card.defaultHP.ToString() : "";
            if (prop_val.gameObject.transform.parent.name == "Strength")
				prop_val.text = card.strength >= 0 ? card.strength.ToString() : "";
            if (prop_val.gameObject.transform.parent.name == "Cost")
				prop_val.text = card.cost >= 0 ? card.cost.ToString() : "";
        }

		foreach (var image in GetComponentsInChildren<Image>() )
		{
			if( card.isRangedAttack && image.sprite.name == "sword" )
			{
				image.sprite = bow_texture;
			}

			if( card.defaultHP == -1 && image.sprite.name == "heart" )
			{
				image.gameObject.SetActive(false);
			}	

			if( card.speed == -1 && image.sprite.name == "steps" )
			{
				image.gameObject.SetActive(false);
			}

			if( card.strength == -1 && image.sprite.name == "sword" )
			{
				image.gameObject.SetActive(false);
			}

			if( card.cost == -1 && image.sprite.name == "diamond" )
			{
				image.gameObject.SetActive(false);
			}

			if( card.imageName != "" && image.sprite.name == "Dude" )
			{
				image.sprite = Resources.Load<Sprite>("Images/" + card.imageName);
			}

		}

		if (card.isHaste || card.isVampire || card.armor > 0 || card.healing > 0 || card.isGiant ) 
		{
			foreach (var image in GetComponentsInChildren<Image>() )
			{
				if( image.name == "Ability" )
				{
					var text = System.Array.Find(GetComponentsInChildren<Text>(), x => x.name == "Ability_val");
					image.enabled = true;
					if( card.armor > 0 )//ToDo if armor AND haste AND vampire ???
					{
						image.sprite = shield_texture;

						text.enabled = true;
						text.text = card.armor.ToString();
					}
					else if( card.isVampire )
						image.sprite = teeth_texture;
					else if( card.isGiant )
						image.sprite = giant_texture;					
					else if( card.healing > 0 )
					{
						image.sprite = cross_texture;
						
						text.enabled = true;
						text.text = card.healing.ToString();
					}
				}
			}		
		}
			
    }


	// Update is called once per frame
	void Update () 
	{
	}

    public void ApplyCard()
    {
		GameBoard.instance.OnCardSelected(card);
        Canvas.ForceUpdateCanvases();
    }
}
