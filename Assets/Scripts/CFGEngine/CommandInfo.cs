
using System.Collections;
using System.Collections.Generic;

namespace CFGEngine
{
	public class CommandInfo
	{
		public List<UnitImpl> staff = new List<UnitImpl>();
		public int crystals_count = 0;
		public int crystals_inc = 1;
		
		[System.Xml.Serialization.XmlIgnoreAttribute]
		public bool is_won = true;

		public List<CardImpl> deck = new List<CardImpl>();

		public List<CardImpl> hand = new List<CardImpl>();

		public CommandInfo()
		{
			foreach (Figure figure in DataBase.Figures)
				deck.Add(new CardImpl(figure));
		}

		public void DrawCard()
		{
			if (deck.Count > 0) 
			{
				CardImpl card = deck [0];
				deck.Remove (card);
				hand.Add (card);
			}
		}
	}
}
