


namespace CFGEngine
{
	public abstract class Move
	{
		public enum MoveType
		{
			Kill = 0,
			Skip = 1,
			Move = 2,
			Card = 3
		}

		// Modifies gameboard according to the move.
		// Returns falce if such move is not possible for yhis board
		public abstract bool MakeMove(GameboardImpl gameboard); 

		public virtual CellPlace TargetCell
		{
			get {return null;}
		}
		
		public virtual CellPlace UnitPlace
		{
			get {return null;}
		}

		public abstract OrientedCell GetTargetCell(GameboardImpl gameboard);

		public abstract OrientedCell GetUnitPlace(GameboardImpl gameboard);
		
		public virtual int Steps
		{
			get {return 0;}
		}

		public abstract MoveType Type 
		{
			get;
		}
	}

	
	public class SkipMoveImpl : Move
	{
		protected CellPlace unit_place;
		
		public override MoveType Type
		{
			get {return MoveType.Skip;}
		}
		
		public override CellPlace UnitPlace
		{
			get {return unit_place;}
		}

		public override CellPlace TargetCell
		{
			get {return unit_place;}
		}

		public override OrientedCell GetTargetCell(GameboardImpl gameboard)
		{
			return GetUnitPlace(gameboard);
		}

		public override OrientedCell GetUnitPlace(GameboardImpl gameboard)
		{
			return unit_place == null ? null : gameboard.cells[unit_place.board_x, unit_place.board_y].unit.oriented_cell;
		}

		public SkipMoveImpl(CellPlace unit_place)
		{
			this.unit_place = unit_place;
		}
		
		public override bool MakeMove(GameboardImpl gameboard)
		{
			UnitImpl new_unit = gameboard.cells[unit_place.board_x, unit_place.board_y].unit;
			if (new_unit == null)
				return false;

			new_unit.made_move = true;
			
			return true;
		}
	}
	
	public class MoveImpl : SkipMoveImpl
	{
		protected CellPlace target_cell;
		protected OrientedCell.CellOrientation orient;
		protected int steps;
		
		public override CellPlace TargetCell
		{
			get {return target_cell;}
		}
		
		public override MoveType Type
		{
			get {return MoveType.Move;}
		}
		
		public override int Steps
		{
			get {return steps;}
		}

		public override OrientedCell GetTargetCell(GameboardImpl gameboard)
		{
			return new OrientedCell{cell = gameboard.cells[target_cell.board_x, target_cell.board_y], orientation = orient};
		}

		public MoveImpl( CellPlace target_cell, OrientedCell.CellOrientation orient, CellPlace unit_place, int steps)
			: base(unit_place)
		{
			this.target_cell = target_cell;
			this.steps = steps;
			this.orient = orient;
		}
		
		public override bool MakeMove(GameboardImpl gameboard)
		{
			UnitImpl new_unit = gameboard.cells[unit_place.board_x, unit_place.board_y].unit;
			
			GameboardCell new_cell = gameboard.cells[target_cell.board_x, target_cell.board_y];

			if (new_unit == null || new_cell.unit != null)
				return false;

			OrientedCell or_cell = new OrientedCell{ cell = new_cell, orientation = orient };

			new_unit.made_move = true;

			if (new_unit.notificator != null)
				new_unit.notificator.PlayRunAnimation (this);

			gameboard.SetUnitPlace (new_unit.oriented_cell, null);
			new_unit.oriented_cell = or_cell;
			gameboard.SetUnitPlace (new_unit.oriented_cell, new_unit);

			return true; 
		}
	}
	
	public class KillMoveImpl : MoveImpl
	{
		protected CellPlace unit_to_kill_place;

		public bool isHealing;
		
		public CellPlace UnitToKillPlace
		{
			get {return unit_to_kill_place;}
		}
		
		public override MoveType Type
		{
			get {return MoveType.Kill;}
		}
		
		public KillMoveImpl(CellPlace target_cell, OrientedCell.CellOrientation orient, CellPlace unit_place, CellPlace unit_to_kill_place, int steps, bool isHealing):
			base(target_cell, orient, unit_place, steps)
		{
			this.isHealing = isHealing;
			this.unit_to_kill_place = unit_to_kill_place;
		}
		
		public override bool MakeMove(GameboardImpl gameboard)
		{
			UnitImpl new_unit = gameboard.cells[unit_place.board_x, unit_place.board_y].unit;
			UnitImpl new_unit_to_kill = gameboard.cells[unit_to_kill_place.board_x, unit_to_kill_place.board_y].unit;

			if (new_unit == null || 
			    new_unit_to_kill == null)
				return false;

			if (steps > 0) 
			{
				if (!base.MakeMove (gameboard))
					return false;
			}
				
			new_unit.made_move = true;

			if (new_unit.notificator != null)
				new_unit.notificator.PlayHitAnimation (new_unit_to_kill);
			GetDamage(gameboard, new_unit_to_kill, new_unit);
			if( !new_unit.isRangedAttack && new_unit.healing == 0 && new_unit_to_kill.hp > 0 )
				GetDamage(gameboard, new_unit, new_unit_to_kill);
			
			return true;
		}

		static protected void GetDamage(GameboardImpl gameboard_ref, UnitImpl me, UnitImpl who_hit_me)
		{
			int dammage = who_hit_me.strength;

			if( who_hit_me.command_idx != me.command_idx )
			{
				dammage -= me.armor;
				if (dammage < 0)
					dammage = 0;

				if (who_hit_me.isVampire && dammage > 0) 
				{
					who_hit_me.hp += dammage;
					if( who_hit_me.hp > who_hit_me.defaultHP )
						who_hit_me.hp = who_hit_me.defaultHP;
				}

				me.hp -= dammage;
			}
			else
				me.hp += who_hit_me.healing; //ToDo frendly fire ???

			if( me.hp > me.defaultHP )
				me.hp = me.defaultHP;

			if (me.hp <= 0)
			{
				me.hp = 0;

				gameboard_ref.SetUnitPlace (me.oriented_cell, null);

				gameboard_ref.commands [me.command_idx].staff.Remove (me);
				if (me.IsBoss) 
				{
					gameboard_ref.commands [me.command_idx].is_won = false;
					gameboard_ref.is_game_finished = true;
				}
			}
		}		
	}
	
	public class CardMoveImpl : MoveImpl
	{
		protected int card_idx;
		
		public override MoveType Type
		{
			get {return MoveType.Card;}
		}
		
		public CardMoveImpl(CellPlace target_cell, OrientedCell.CellOrientation orient, int card_idx):
			base(target_cell, orient, null, 0)
		{
			this.card_idx = card_idx;
		}
		
		public override bool MakeMove(GameboardImpl gameboard)
		{
			GameboardCell new_cell = gameboard.cells[target_cell.board_x, target_cell.board_y];
			if (new_cell.unit != null)
				return false;

			OrientedCell or_cell = new OrientedCell{ cell = new_cell, orientation = orient };

			gameboard.AddUnit(gameboard.commands[gameboard.cur_command_idx].hand[card_idx], or_cell);
			
			return true;
		}
	}

	public class MagicMoveImpl : KillMoveImpl
	{
		protected int card_idx;

		public override MoveType Type
		{
			get {return MoveType.Card;}
		}

		public MagicMoveImpl(CellPlace target_cell, int card_idx, bool isHealing):
		base(target_cell, OrientedCell.CellOrientation.Default, null, target_cell, 0, isHealing)
		{
			this.card_idx = card_idx;
		}

		public override bool MakeMove(GameboardImpl gameboard)
		{
			UnitImpl new_unit_to_kill = gameboard.cells[unit_to_kill_place.board_x, unit_to_kill_place.board_y].unit;

			if (new_unit_to_kill == null)
				return false;

			UnitImpl new_unit = new UnitImpl (gameboard.commands [gameboard.cur_command_idx].hand [card_idx], gameboard.cur_command_idx, new_unit_to_kill.oriented_cell);

			gameboard.AddUnit(gameboard.commands[gameboard.cur_command_idx].hand[card_idx], null);

			if (new_unit_to_kill.notificator != null)
				new_unit_to_kill.notificator.PlayBlockAnimation (isHealing);
			GetDamage(gameboard, new_unit_to_kill, new_unit);

			return true;
		}
	}}
