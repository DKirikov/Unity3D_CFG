using System;

namespace CFGEngine
{
	public class MoveInfo
	{
		public MoveInfo(Move move)
		{
			this.move = move;
		}
		
		public MoveInfo nextMove = null;
		
		public double dt;
		public int c;
		public int makeMoveCount;
		public double makeMoves;
		public int getMovesCount;
		public double getMoves;
		
		public int calcRouteToMultyCount;
		public double calcRouteToMulty;
		
		public int depth;
		
		public Move move;
		public GameboardImpl board;
		public double weight;
		public int distance;
	}

	public static class AIHelper
	{
		public static double GetBoardWeight(GameboardImpl board, bool isMax)//ToDo: improve it
		{
			double res = 0;
			for (int j = 0; j < board.commands.Length; j++) 
			{
				double weight = 0;
				CommandInfo command = board.commands[j];
				
				foreach (UnitImpl unit in command.staff)
					weight += unit.CalculateWeight();
				
				if(j == board.cur_command_idx)
					res += weight;
				else
					res -= weight;
			}

			res += board.commands[board.cur_command_idx].crystals_count * 0.9;
			
			if (!isMax) // is Min
				res *= -1;
			
			return res;
		}

		public static void ShowLogForMove(MoveInfo move)
		{
			UnityEngine.Debug.Log("Move(c == " + move.c.ToString() + ", depth == " + move.depth + "):" + 
			           " dt ==" + ((int)move.dt).ToString() + "ms" + 
			           ", makeMoves(" +  move.makeMoveCount + ")  - " + (move.dt == 0 ? 0 : (int)((move.makeMoves / move.dt) * 100)).ToString() + "%" +
			           ", getMoves(" +  move.getMovesCount + ") - " + (move.dt == 0 ? 0 : (int)((move.getMoves / move.dt) * 100)).ToString() + "%" +
			           ", calcRouteToMulty(" +  move.calcRouteToMultyCount + ") - " + (move.dt == 0 ? 0 : (int)((move.calcRouteToMulty / move.dt) * 100)).ToString() + "%"
			           );
		}
	}
}

