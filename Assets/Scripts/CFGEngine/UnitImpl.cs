using System;
using System.Collections.Generic;
using System.Text;


namespace CFGEngine
{
	public interface UnitNotificator
	{
		void PlayRunAnimation (MoveImpl move);

		void PlayHitAnimation ( UnitImpl unit_to_kill);

		void PlayBlockAnimation (bool healing);
	}
	
    public class UnitImpl : Figure
    {
        public int command_idx = -1;
        public bool made_move = true;

		public OrientedCell oriented_cell = new OrientedCell();
		public int hp;

		[System.Xml.Serialization.XmlIgnoreAttribute]
		public UnitNotificator notificator = null;

		public UnitImpl() // only for deserialize
		{
		}

		public UnitImpl(UnitImpl otherUnit)
			: base (otherUnit)
		{
			command_idx = otherUnit.command_idx;
			made_move = otherUnit.made_move;
			hp = otherUnit.hp;
		}
		
		public UnitImpl(Figure other, int command_idx, OrientedCell cell_ref)
			: base (other)
		{
			hp = defaultHP;
			this.command_idx = command_idx;
			this.oriented_cell = cell_ref;
			made_move = !other.isHaste;
		}

		public void SetNotificator(UnitNotificator notificator)
		{
			this.notificator = notificator;
		}

		public double CalculateWeight()
		{
			if (hp == 0) 
			{
				if (isBoss)
					return -1000;
				
				return 0;
			}
						
			double w = weight;
			int dStr = strength;
			int str = strength;
			
			double k = w / (double)(defaultHP + dStr);

			double cHP = hp;
			
			if (hp <= defaultHP / 4)
				cHP *= 1.5; 
			else if (hp <= defaultHP / 2)
				cHP = 0.1 * defaultHP + 1.1 * hp; //(defaultHP / 4) * 1.5 + (hp - defaultHP / 4) * 1.1; 
			else if (hp <= defaultHP * 3 / 4)
				cHP = 0.2 * defaultHP + 0.9 * hp; //(defaultHP / 4) * 1.5 + (defaultHP / 4) * 1.1 + (hp - defaultHP / 2) * 0.9; //(defaultHP / 4) * 1.5 + (defaultHP / 4) * 1.1 + (hp - defaultHP / 2) * 0.9; 
			else
				cHP = 0.5 * defaultHP + 0.5 * hp; //(defaultHP / 4) * 1.5 + (defaultHP / 4) * 1.1 + (defaultHP / 4) * 0.9 + (hp - defaultHP * 3 / 4) * 0.5; 
			
			return (cHP + str) * k;
		}


		public List<Move> GetMoves(GameboardImpl gameboard_ref)
        {
            List<Move> ret = new List<Move>();
            List<Move> simpleMoves = new List<Move>();

            if (!made_move)
            {
				List<Routing.CAvailableCells> available_cells = new List<Routing.CAvailableCells>();

				if (isTeleport) //ToDo fill available_cells in case of giant teleport
				{
					foreach (var cell in gameboard_ref.cells) 
					{
						if( cell.active && cell.unit == null )//ToDo move to GameboardImpl
							available_cells.Add(new Routing.CAvailableCells( new OrientedCell{cell = cell}, 0));					
					}
				}
				else
					available_cells = new Routing(gameboard_ref).CalcAvailableCells(oriented_cell, isGiant ? speed*2 : speed);

				if( isRangedAttack )
				{
					List<GameboardCell> target_cell = gameboard_ref.AddRangedMoves (command_idx, healing>0, strength>0);
					foreach (var cell in target_cell) 
					{
						ret.Add(new KillMoveImpl(oriented_cell.cell, oriented_cell.orientation, oriented_cell.cell, cell, 0, cell.unit.command_idx == command_idx));
					}
				}

				if( !isRangedAttack )
					AddNeighborKills(gameboard_ref, oriented_cell, ret, 0);

				foreach (var cell in available_cells)
                {
					simpleMoves.Add(new MoveImpl(cell.cell, cell.orientation, oriented_cell.cell, cell.steps));

					if( !isRangedAttack )
						AddNeighborKills(gameboard_ref, cell, ret, cell.steps);
                }

				ret.Sort((x, y) => x.Steps - y.Steps);
				simpleMoves.Sort((x, y) => y.Steps - x.Steps);

				ret.AddRange(simpleMoves);

				ret.Add(new SkipMoveImpl(oriented_cell.cell));
            }

            return ret;
        }
		
		private void AddNeighborKills(GameboardImpl gameboard_ref, OrientedCell cell, List<Move> ret, int steps)
        {
            List<GameboardCell> neighbor_cells = gameboard_ref.GetNeighborCells(cell);

            foreach (GameboardCell neighbor_cell in neighbor_cells)
            {
                if (neighbor_cell.unit != null && neighbor_cell.unit.command_idx != command_idx)//Frendly fire
                {
					ret.Add(new KillMoveImpl(cell.cell, cell.orientation, oriented_cell.cell, neighbor_cell, steps, false));
                }
            }
        }
			

    }

}