using System;
using System.Collections.Generic;
using System.Text;


namespace CFGEngine
{
	public interface GameboardNotificator
	{
		void PlayerUIUpdated ();

		void UnitAdded (UnitImpl unit);
	}

	public class CellPlace
	{
		public CellPlace(int board_x, int board_y)
		{
			this.board_x = board_x;
			this.board_y = board_y;
		}

		public CellPlace()
		{
		}
		
		public readonly int board_x = -1, board_y = -1;
	}
	
	public class GameboardCell : CellPlace
    {
		public interface CellNotificator
		{
			void UnitAdded (UnitImpl unit);
		}

        public GameboardCell() //ToDo remove
        {
        }

        public GameboardCell(int board_x, int board_y, bool active)
			: base(board_x, board_y)
        {
            this.active = active;
        }

        [System.Xml.Serialization.XmlIgnoreAttribute]
		public readonly bool active = false;

        [System.Xml.Serialization.XmlIgnoreAttribute]
        public UnitImpl unit = null;

		[System.Xml.Serialization.XmlIgnoreAttribute]
		public CellNotificator notificator = null;
	}

	public class OrientedCell : IEquatable<OrientedCell>
	{
		public GameboardCell cell;

		public enum CellOrientation
		{
			Default = 0, EastNorth = 1, EastSouth = 2, East = 3
		} 

		public CellOrientation orientation = CellOrientation.Default;


		public bool Equals(OrientedCell other)
		{
			return other.cell.Equals(cell) && other.orientation.Equals(orientation);
		}

		public override bool Equals(object o)
		{
			return this.Equals(o as OrientedCell);
		}

		public override int GetHashCode()
		{
			return cell.GetHashCode() ^ orientation.GetHashCode();
		}
	}

    public class GameboardImpl
    {
        public GameboardImpl()
        {
			const int X_SIZE = 7;
			const int Y_SIZE = 8;
			cells = new GameboardCell[X_SIZE, Y_SIZE];

			for (int x = 0; x < X_SIZE; ++x)
            {
				for (int y = 0; y < Y_SIZE; ++y)
				{
                    bool is_active = true;
                    //ToDo make it cool
					if (y == 0 && (x == 0 || x == X_SIZE - 1))
						is_active = false;
					if (y == Y_SIZE - 1 && (x <= 1 || x == X_SIZE/2 || x >= X_SIZE - 2))
                        is_active = false;

                    cells[x, y] = new GameboardCell(x, y, is_active);
				}
            }
        }


		public GameboardCell GetNearCell(CellPlace place, int idx)
        {
			int[,] deltas = (place.board_x % 2) == 0 ? neighbor_cells_deltas1 : neighbor_cells_deltas2;

			int x = place.board_x + deltas [idx, 0];
			int y = place.board_y + deltas [idx, 1];

            if (x >= 0 && x < cells.GetLength(0) &&
                y >= 0 && y < cells.GetLength(1) &&
                cells[x, y].active)
                return cells[x, y];
            return null;
        }

		private static int[,] neighbor_cells_deltas1 = { { 0, 1 }, { 1, 0 }, { 1, -1 }, { 0, -1 }, { -1, -1 }, { -1, 0 } };
		private static int[,] neighbor_cells_deltas2 = { { 0, 1 }, { 1, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 }, { -1, 1 } };

		private List<GameboardCell> GetNeighborCells(CellPlace cell)
        {
            List<GameboardCell> ret = new List<GameboardCell>(6);

            for (int i = 0; i < 6/*deltas.GetLength(0)*/; ++i)
            {
				GameboardCell cur_cell = GetNearCell(cell, i);
                if (cur_cell != null)
                    ret.Add(cur_cell);
            }

            return ret;
        }

		public List<GameboardCell> GetNeighborCells(OrientedCell cell)//ToDo it could be optimized
		{
			List<GameboardCell> ret = new List<GameboardCell>(6);

			var occupated = GetOccupatedCells (cell);

			foreach (var occu_cell in occupated) 
			{
				var neigh = GetNeighborCells (occu_cell);
				foreach (var neigh_cell in neigh) 
				{
					if (!ret.Contains (neigh_cell) && Array.IndexOf (occupated, neigh_cell) == -1)
						ret.Add (neigh_cell);
				}
			}
			return ret;
		}

		public List<OrientedCell> GetOneMoveCells(OrientedCell cell, UnitImpl excl_unit)//ToDo it could be optimized
		{
			List<OrientedCell> ret = null;

			if (cell.orientation == OrientedCell.CellOrientation.Default) 
			{
				ret = new List<OrientedCell> (6);
				var neigh = GetNeighborCells (cell.cell);
				foreach (var neigh_cell in neigh) 
				{
					ret.Add (new OrientedCell{ cell = neigh_cell });
				}			
			}
			else
			{
				ret = new List<OrientedCell> (4);

				GameboardCell[] oc_cell = GetOccupatedCells(cell);

				Func<int,int> p = x => (x > 1 ? x - 1 : 3);
				Func<int,int> n = x => (x < 3 ? x + 1 : 1);

				int o = (int)cell.orientation;

				ret.Add (new OrientedCell{ cell = oc_cell[n(o)], orientation = (OrientedCell.CellOrientation)n(o) });
				ret.Add (new OrientedCell{ cell = oc_cell[n(o)], orientation = (OrientedCell.CellOrientation)p(o) });
				ret.Add (new OrientedCell{ cell = oc_cell[p(o)], orientation = (OrientedCell.CellOrientation)n(o) });
				ret.Add (new OrientedCell{ cell = oc_cell[o]   , orientation = (OrientedCell.CellOrientation)p(o) });			
			}

			ret.RemoveAll(or_cell => or_cell.cell == null || 
				Array.Exists(GetOccupatedCells (or_cell), oc_cell => oc_cell == null || 
					(oc_cell.unit != null && oc_cell.unit != excl_unit)));
			return ret;
		}

		public GameboardCell[] GetOccupatedCells(OrientedCell cell)
		{
			GameboardCell[] ret = null;
			if (cell != null) 
			{
				if (cell.orientation == OrientedCell.CellOrientation.Default) 
				{
					ret = new GameboardCell[1]{ cell.cell };
				} 
				else 
				{
					ret = new GameboardCell[4];
					ret [1] = cell.cell;

					ret [0] = GetNearCell (cell.cell, 0);

					switch (cell.orientation) 
					{
					case OrientedCell.CellOrientation.EastNorth:
						ret [2] = GetNearCell (cell.cell, 4);
						ret [3] = GetNearCell (cell.cell, 5);				
						break;
					case OrientedCell.CellOrientation.EastSouth:
						ret [2] = GetNearCell (cell.cell, 1);
						ret [3] = GetNearCell (cell.cell, 2);
						break;
					case OrientedCell.CellOrientation.East:
						ret [2] = GetNearCell (cell.cell, 1);
						ret [3] = GetNearCell (cell.cell, 5);						
						break;
					}
				}
			}
			return ret;
		}

		public void SetUnitPlace(OrientedCell oriented_cell, UnitImpl unit)
		{
			foreach (var cell in GetOccupatedCells(oriented_cell)) 
			{
				cell.unit = unit;
				if (cell.notificator != null)
					cell.notificator.UnitAdded (unit);
			}		
		}

		public List<OrientedCell> CalcAvailableCellsForCard(bool is_giant)//ToDo do it quicker //ToDo calculate for giant
        {
			List<OrientedCell> ret = new List<OrientedCell>();

			Func<int, int> y_st = x => (cur_command_idx == 0) ? 0 : cells.GetLength(1) - 2 - (x%2);
            int y_en = (cur_command_idx == 0) ? 2 : cells.GetLength(1);

            for (int x = 0; x < cells.GetLength(0); ++x)
            {
                for (int y = y_st(x); y < y_en; ++y)
                {
                    if( cells[x, y].active && cells[x, y].unit == null )
						ret.Add(new OrientedCell{cell = cells[x, y]});
                }
            }

			if (is_giant) 
			{
				List<OrientedCell> giant_cells = new List<OrientedCell>();
				foreach (var cell in ret) 
				{
					for (int or = 1; or <= 3; ++or) 
					{
						OrientedCell giant_cell = new OrientedCell{ cell = cell.cell, 
							orientation = (OrientedCell.CellOrientation)or };

						var occu = GetOccupatedCells(giant_cell);

						if (Array.TrueForAll (occu, oc_cell => oc_cell != null && oc_cell.active && oc_cell.unit == null &&
							ret.Contains (new OrientedCell {cell = oc_cell})))
							giant_cells.Add (giant_cell);
					}
				}
				ret = giant_cells;
			}

            return ret;
        }

        static public bool AreCellsNeighbor(GameboardCell cell, GameboardCell next_cell)
        {
            int ymin = (cell.board_x % 2) - 1;
            int ymax = cell.board_x % 2;
            int adx = Math.Abs(next_cell.board_x - cell.board_x);
            int dy = next_cell.board_y - cell.board_y;
            //{ { 0, 1 }, { 1, ymax }, { 1, ymin }, { 0, -1 }, { -1, ymin }, { -1, ymax } };
            return (adx == 0 && Math.Abs(dy) == 1) || (adx == 1 && dy >= ymin && dy <= ymax);
        }

		public List<GameboardCell> AddRangedMoves(int command_idx, bool friends, bool enemies )
		{
			List<GameboardCell> ret = new List<GameboardCell> ();
			for(int other_comm_idx = 0; other_comm_idx < commands.Length; ++other_comm_idx)
			{
				if( (enemies && other_comm_idx != command_idx) || (friends && other_comm_idx == command_idx) )
				{
					foreach(var enemy in commands[other_comm_idx].staff)
					{
						ret.Add (enemy.oriented_cell.cell);
					}					
				}
			}		
			return ret;
		}

		public void AddUnit(CardImpl card, OrientedCell new_cell)
		{
			var command = commands [cur_command_idx];

			if (new_cell != null) 
			{
				UnitImpl unit = new UnitImpl (card, cur_command_idx, new_cell);

				SetUnitPlace (new_cell, unit);

				command.staff.Add(unit);

				if (notificator != null)
					notificator.UnitAdded(unit);
			}

			command.hand.Remove (card);

			command.crystals_count -= card.cost;
			if (notificator != null)
				notificator.PlayerUIUpdated ();
        }


        public GameboardImpl MakeCopy()
        {
            GameboardImpl ret = new GameboardImpl();

            for (int player_idx = 0; player_idx < players_qty; ++player_idx)
            {
				ret.commands[player_idx] = new CommandInfo();

				foreach (UnitImpl unit in commands[player_idx].staff)
                {
					UnitImpl new_unit = new UnitImpl(unit);

					new_unit.oriented_cell.orientation = unit.oriented_cell.orientation;
					new_unit.oriented_cell.cell = ret.cells[unit.oriented_cell.cell.board_x, unit.oriented_cell.cell.board_y];
					SetUnitPlace (new_unit.oriented_cell, new_unit);

                    //ToDo check for something else

                    ret.commands[player_idx].staff.Add(new_unit);
                }

                ret.commands[player_idx].crystals_count = commands[player_idx].crystals_count;
                ret.commands[player_idx].crystals_inc = commands[player_idx].crystals_inc;

				foreach(CardImpl cardImpl in commands[player_idx].hand)
				{
					ret.commands[player_idx].hand.Add(new CardImpl(cardImpl));
				}

				foreach(CardImpl cardImpl in commands[player_idx].deck)
				{
					ret.commands[player_idx].deck.Add(new CardImpl(cardImpl));
				}
            }

            ret.cur_command_idx = cur_command_idx;

            return ret;
        }

		public void ChangeCommand()
		{
			++cur_command_idx;
			if (cur_command_idx >= players_qty)
				cur_command_idx = 0;
			
			foreach (UnitImpl unit in commands[cur_command_idx].staff)
			{
				unit.made_move = false;
			}

			commands [cur_command_idx].DrawCard ();
			
			commands[cur_command_idx].crystals_count = commands[cur_command_idx].crystals_inc;
			++commands[cur_command_idx].crystals_inc;
			
			if (commands[cur_command_idx].crystals_count > GameboardImpl.max_crystal_qty)
				commands[cur_command_idx].crystals_count = GameboardImpl.max_crystal_qty;
			if (commands[cur_command_idx].crystals_inc > GameboardImpl.max_crystal_qty)
				commands[cur_command_idx].crystals_inc = GameboardImpl.max_crystal_qty;
		}

		public void SetNotificator(GameboardNotificator notificator)
		{
			this.notificator = notificator;
		}

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////


        [System.Xml.Serialization.XmlIgnoreAttribute]
        public GameboardCell[,] cells; //ToDo make protected

        public const int players_qty = 2;
        public CommandInfo[] commands = new CommandInfo[players_qty];

        public int cur_command_idx = players_qty - 1;
    
		[System.Xml.Serialization.XmlIgnoreAttribute]
		public bool is_game_finished = false;

		public const int max_crystal_qty = 10;

		[System.Xml.Serialization.XmlIgnoreAttribute]
		private GameboardNotificator notificator = null;
		
	}

}