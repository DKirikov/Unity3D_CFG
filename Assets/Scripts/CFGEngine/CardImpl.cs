using System;
using System.Collections.Generic;
using System.Text;


namespace CFGEngine
{
    public class CardImpl : Figure
    {
        public CardImpl(Figure figure) :
			base(figure)
        {
        }

		public CardImpl(CardImpl other) :
			base(other)
		{
		}
		
		public CardImpl()
		{
		}

		public List<Move> GetMoves(GameboardImpl gameboard_ref)
        {
            List<Move> ret = new List<Move>();

			var command = gameboard_ref.commands [gameboard_ref.cur_command_idx];
			if (command.crystals_count >= cost)
            {
				if( isMagic )
				{
					List<GameboardCell> target_cell = gameboard_ref.AddRangedMoves (gameboard_ref.cur_command_idx, healing>0, strength>0);
					foreach (var cell in target_cell) 
					{
						ret.Add(new MagicMoveImpl(cell, command.hand.FindIndex(x => x == this), cell.unit.command_idx == gameboard_ref.cur_command_idx));
					}
				}
				else
				{
					List<OrientedCell> available_cells = gameboard_ref.CalcAvailableCellsForCard(isGiant);

	                foreach (var cell in available_cells)
	                {
						ret.Add(new CardMoveImpl(cell.cell, cell.orientation, command.hand.FindIndex(x => x == this)));
	                }
				}
            }

            return ret;
        }
    }

}