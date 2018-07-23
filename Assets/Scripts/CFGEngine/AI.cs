using System;
using System.Collections.Generic;

using System.Threading;

namespace CFGEngine
{
	public class AI
	{
		private GameboardImpl currentBoard;

		private int c;
		private int makeMoveCount;
		private double makeMoves;
		private int getMovesCount;
		private double getMoves;

    	private int calcRouteToMultyCount;
    	private double calcRouteToMulty;

		private int maxDepth;

		public delegate void CalculateMoveEventHandler(Move moveInfo);
		public event CalculateMoveEventHandler onCalculateMove;
				
		public AI(GameboardImpl board)
		{
			this.currentBoard = board.MakeCopy();
		}

		public void CalculateAllMoves()
		{
			CalculateAllCardMoves();

			CalculateAllUnitMoves();
		}

		private void CalculateAllUnitMoves()
		{
			MoveInfo move = SelectBestMove();
			
			if (onCalculateMove != null)
				onCalculateMove(move == null ? null : move.move);
			
			if (move == null)
				return;
			
			move.move.MakeMove(currentBoard);
			
			while(true)
			{
				MoveInfo nextMove = move.nextMove;
				if (nextMove == null)
				{
					if (onCalculateMove != null)
						onCalculateMove(null);
					
					return;
				}
				
				if (nextMove.dt < 1000)
					break;
				
				UnityEngine.Debug.Log("Move from subtree! dt ==" + ((int)nextMove.dt).ToString() + "ms");
				
				if (onCalculateMove != null)
					onCalculateMove(nextMove.move);
				
				nextMove.move.MakeMove(currentBoard);
				
				move = nextMove;
				continue;
			}
			
			CalculateAllUnitMoves();
		}

		private void CalculateAllCardMoves()
		{
			MoveInfo move = null;

			DateTime t0 = DateTime.Now;
			c = 0;

			List<MoveInfo> cardMoves = GetAvailableCardMoves(currentBoard);

      		c++;
			double bestWeight = AIHelper.GetBoardWeight(currentBoard, true);
			GameboardImpl bestBoard = currentBoard;
			foreach (MoveInfo cardMove in cardMoves)
			{
				GameboardImpl newBoard = currentBoard.MakeCopy();
				cardMove.move.MakeMove(newBoard);
				
				c++;
				double newWeight = AIHelper.GetBoardWeight(newBoard, true);
				if(newWeight > bestWeight)
				{
					bestWeight = newWeight;
					bestBoard = newBoard;
					move = cardMove;
				}
			}

			currentBoard = bestBoard;

			if(move == null)
				return;
			
			if (onCalculateMove != null)
				onCalculateMove(move.move);
			
			move.c = c;
			move.dt = (DateTime.Now - t0).TotalMilliseconds;
			move.makeMoveCount = makeMoveCount;
			move.makeMoves = makeMoves;
			move.getMovesCount = getMovesCount;
			move.getMoves = getMoves;
      		move.calcRouteToMultyCount = calcRouteToMultyCount;
      		move.calcRouteToMulty = calcRouteToMulty;
			
			AIHelper.ShowLogForMove(move);
			
			CalculateAllCardMoves();
		}
		
		private MoveInfo SelectBestMove()
		{
			c = 0;
			makeMoveCount = 0;
			makeMoves = 0;
			getMovesCount = 0;
			getMoves = 0;
      		calcRouteToMultyCount = 0;
      		calcRouteToMulty = 0;

			maxDepth = 1;

			double bestWeight;

			int aInd = currentBoard.cur_command_idx;
			int bInd = aInd == 0 ? 1 : 0;
      		int figuresC = NoActiveUnitsCount(currentBoard.commands[aInd].staff);
			int figuresB = currentBoard.commands[bInd].staff.Count;
			int figuresA = currentBoard.commands[aInd].staff.Count;

			if (figuresC == 0)
				return null;

			int sum = figuresC * (3 + figuresB);
			sum *= 3 + figuresA;

			bool curA = false;

			while (sum < 1000)
			{
				maxDepth++;

				if(curA)
					sum *= 3 + figuresA;
				else
					sum *= 3 + figuresB;

				curA = !curA;
			}

			//maxDepth = 3;

			MoveInfo move = CalculateBoardWeight(currentBoard, out bestWeight, maxDepth, true, double.MinValue, double.MaxValue);

			if (move != null)
			{
				move.c = c;
				move.makeMoveCount = makeMoveCount;
				move.makeMoves = makeMoves;
				move.getMovesCount = getMovesCount;
				move.getMoves = getMoves;
        		move.calcRouteToMultyCount = calcRouteToMultyCount;
        		move.calcRouteToMulty = calcRouteToMulty;

				move.depth = maxDepth;

				AIHelper.ShowLogForMove(move);
			}

			return move;
		}

		private MoveInfo CalculateBoardWeight(GameboardImpl board, out double bestWeight, int depth, bool isMax, double alpha, double beta)
		{
			DateTime time0 = DateTime.Now;
			MoveInfo bestMoveInfo = null;

      		bestWeight = isMax ? alpha : beta;

			CommandInfo command = board.commands[board.cur_command_idx];
			foreach (UnitImpl unit in command.staff)
			{
				if (unit.made_move)
					continue;

				double candidateWeight;

				List<MoveInfo> moves = GetBestMoves(board, unit, isMax);

        		bool allUnitsMoved = NoActiveUnitsCount(command.staff) == 1;
				foreach (MoveInfo move in moves)
				{
					DateTime t0 = DateTime.Now;
					GameboardImpl newBoard = board.MakeCopy();
					move.move.MakeMove(newBoard);
					makeMoveCount++;
					makeMoves += (DateTime.Now - t0).TotalMilliseconds;

					MoveInfo nextMove = null;

					if(newBoard.is_game_finished)
					{
						c++;
						candidateWeight = AIHelper.GetBoardWeight(newBoard, isMax);
					}
					else
					{
						if(allUnitsMoved)
						{
							if (depth > 1)// && !newBoard.is_game_finished
							{
								newBoard = MakeEndOfTurn(newBoard);
								if (isMax)
									CalculateBoardWeight(newBoard, out candidateWeight, depth - 1, !isMax, bestWeight, beta);
								else
									CalculateBoardWeight(newBoard, out candidateWeight, depth - 1, !isMax, alpha, bestWeight);
							}
							else
							{
								c++;
								candidateWeight = AIHelper.GetBoardWeight(newBoard, isMax);
							}
						}
						else
						{
							if (isMax)
								nextMove = CalculateBoardWeight(newBoard, out candidateWeight, depth, isMax, bestWeight, beta);
							else
								nextMove = CalculateBoardWeight(newBoard, out candidateWeight, depth, isMax, alpha, bestWeight);
						}
					}

					//if (UpdateWeight(move, ref bestWeight, ref bestMoveInfo, isMax, candidateWeight) && depth == maxDepth)
						//bestMoveInfo.nextMove = nextMove;

					if (isMax && candidateWeight > bestWeight ||
					  !isMax && candidateWeight < bestWeight)
					{
						bestWeight = candidateWeight;
						bestMoveInfo = move;

						if(depth == maxDepth)
							bestMoveInfo.nextMove = nextMove;
					}

					if ((!isMax && candidateWeight <= alpha) || (isMax && candidateWeight >= beta))
						break;
				}

				break; //for remove unnecessary moves
			}

			if (bestMoveInfo != null)
				bestMoveInfo.dt = (DateTime.Now - time0).TotalMilliseconds;

			return bestMoveInfo;
		}

		private bool CheckBoard(GameboardImpl board, List<GameboardImpl> uniqueBoards)
		{
			foreach(GameboardImpl b in uniqueBoards)
			{
				if(b.commands[0].staff.Count != b.commands[0].staff.Count || b.commands[1].staff.Count != b.commands[1].staff.Count)
					continue;

				bool isBreak = false;
				for (int n = 0; n < 2; n++)
				{
					for (int i = 0; i < b.commands[n].staff.Count; i++)
					{
						UnitImpl unit0 = b.commands[n].staff[i];
						UnitImpl unit1 = board.commands[n].staff[i];

						if(unit0.id == unit1.id && unit0.hp == unit1.hp &&
							unit0.oriented_cell.orientation == unit1.oriented_cell.orientation &&
							Math.Abs(unit0.oriented_cell.cell.board_x - unit1.oriented_cell.cell.board_x) < 1 &&
							Math.Abs(unit0.oriented_cell.cell.board_y - unit1.oriented_cell.cell.board_y) < 1)
							continue;

						isBreak = true;
						break;
					}

					if(isBreak)
						break;
				}

				if(!isBreak)
					return false;
			}

			return true;
		}

		private List<MoveInfo> GetAvailableCardMoves(GameboardImpl board)//Todo: change it on GetBestMoves
		{
			List<MoveInfo> res = new List<MoveInfo>();

			CommandInfo command = board.commands[board.cur_command_idx];

			List<CardImpl> cards = command.hand;

			foreach (CardImpl card in cards)
			{
				List<Move> moves = card.GetMoves(board);

				//foreach (Move move in moves)
					//res.Add(new MoveInfo(move));

				if(moves.Count > 0)
				{
					Random rand = new Random();
					int index = rand.Next(moves.Count);
					res.Add(new MoveInfo(moves[index]));
				}
			}

			return res;
		}

	    private List<MoveInfo> GetBestMoves(GameboardImpl board, UnitImpl unit, bool isMax)
	    {
			List<MoveInfo>[] movesByDist = GetMovesByDistForWalkerMode(board, unit, isMax);

			//3:
			int maxDist = movesByDist.Length;

			List<MoveInfo>[] bestMovesByDist = new List<MoveInfo>[maxDist];
			for (int i = 1; i < maxDist; i++)
			{
				bestMovesByDist[i] = new List<MoveInfo>();
				double bestW = isMax ? double.MinValue : double.MaxValue;

				foreach (MoveInfo move in movesByDist[i])
				{
					if ((isMax && move.weight > bestW) ||
					    (!isMax && move.weight < bestW))
						bestW = move.weight;
				}

				foreach (MoveInfo move in movesByDist[i])
				{
					if (move.weight == bestW)
						bestMovesByDist[i].Add(move);
				}
			}

			//4:
			//and 5 and 6:
			c++;
			double bestWeight = AIHelper.GetBoardWeight(board, isMax);
			int bestNearestDistIndex = 1;
			int bestDistIndex = 1;

			for (int i = 1; i < maxDist; i++)
			{
				double w = bestMovesByDist[i][0].weight;
				if ((isMax && w > bestWeight) ||
				   (!isMax && w < bestWeight))
				{
					bestWeight = w;
					bestDistIndex = i;
					if (bestNearestDistIndex == 1)
					bestNearestDistIndex = i;
				}
			}

			List<MoveInfo> res = new List<MoveInfo>();
			if (bestNearestDistIndex == 1)
			{
				//Random rand = new Random();
				//int index = rand.Next(bestMovesByDist[1].Count);
				res.Add(bestMovesByDist[1][bestMovesByDist[1].Count - 1]);
			}
			else
			{
				MoveInfo moveInfo = BestMoveForGoToTargetCell(movesByDist[1], bestMovesByDist[bestNearestDistIndex], bestNearestDistIndex - 1, unit.speed);
				res.Add(moveInfo);
			}

			if (bestNearestDistIndex != bestDistIndex)
			{
				MoveInfo moveInfo = BestMoveForGoToTargetCell(movesByDist[1], bestMovesByDist[bestDistIndex], bestDistIndex - 1, unit.speed);

				if (res[0] != moveInfo)
					res.Add(moveInfo);
			}

			return res;
	    }

	    private List<MoveInfo>[] GetMovesByDistForWalkerMode(GameboardImpl board, UnitImpl unit, bool isMax)//ToDo: move it to AIHelper
	    {
			//1:
			int oldSpeed = unit.speed;
			unit.speed = 100;

			DateTime t0 = DateTime.Now;
			List<Move> simpleMoves = unit.GetMoves(board);
			getMovesCount++;
			getMoves += (DateTime.Now - t0).TotalMilliseconds;
			unit.speed = oldSpeed;

			List<MoveInfo> moves = new List<MoveInfo>();
			//2:
			int maxDist = 0;

			foreach (Move move in simpleMoves)
			{
				MoveInfo moveInfo = new MoveInfo(move);
				moves.Add(moveInfo);

				t0 = DateTime.Now;
				//2.1:
				moveInfo.board = board.MakeCopy();
				move.MakeMove(moveInfo.board);
				makeMoveCount++;
				makeMoves += (DateTime.Now - t0).TotalMilliseconds;

				//2.1:
				c++;
				moveInfo.weight = AIHelper.GetBoardWeight(moveInfo.board, isMax);

				//2.2:
				moveInfo.distance = (int)Math.Ceiling(move.Steps / (double)oldSpeed);
				if (moveInfo.distance == 0)
					moveInfo.distance = 1;

				if (moveInfo.distance > maxDist)
					maxDist = moveInfo.distance;
			}

			maxDist++;//it because array size will be count + 1

			//3:
			List<MoveInfo>[] movesByDist = new List<MoveInfo>[maxDist];
			for (int i = 0; i < maxDist; i++)
				movesByDist[i] = new List<MoveInfo>();

			foreach (MoveInfo move in moves)
				movesByDist[move.distance].Add(move);

			return movesByDist;
	    }

	    private MoveInfo BestMoveForGoToTargetCell(List<MoveInfo> candidates, List<MoveInfo> targetMoves, int minDist, int speed)//To do: it can return List<MoveInfo> - think about it
		{
	    	int[] scoreArr = new int[candidates.Count];
	    	for (int i = 0; i < scoreArr.Length; i++)
	        scoreArr[i] = 0;

			foreach (MoveInfo targetMove in targetMoves)
			{
				OrientedCell targetCell = targetMove.move.GetTargetCell(targetMove.board);

				List<OrientedCell> startCells = new List<OrientedCell>();
	        	foreach (MoveInfo startMove in candidates)
	        	{
					OrientedCell startCell = startMove.move.GetTargetCell(startMove.board);
	          		startCells.Add(startCell);
	        	}

	        	DateTime t0 = DateTime.Now;
				int[] distances = new Routing(targetMove.board).CalcRouteToMulty(targetCell, startCells, null);
	        	calcRouteToMultyCount++;
	        	calcRouteToMulty += (DateTime.Now - t0).TotalMilliseconds;

	        	for (int i = 0; i < scoreArr.Length; i++)
	        	{
	          		int distance = (int)Math.Ceiling(distances[i] / (double)speed);
	          		if (distance == minDist)
	            		scoreArr[i]++;
	        	}
			}

			MoveInfo res = null;//To do: it can return List<MoveInfo> - think about it
			int bestScore = -1;
			for (int i = 0; i < scoreArr.Length; i++)
			{
	        	if (scoreArr[i] > bestScore)
	        	{
	          		bestScore = scoreArr[i];
	          		res = candidates[i];
	        	}
			}

	      	return res;
		}

		private int NoActiveUnitsCount(List<UnitImpl> staff)//ToDo: move it to AIHelper
		{
			int res = 0;
			foreach (UnitImpl unit in staff)
			{
				if (!unit.made_move)
					res++;
			}
			
			return res;
		}

    	private GameboardImpl MakeEndOfTurn(GameboardImpl board)//ToDo: remove it
		{
			GameboardImpl newBoard = board.MakeCopy();
			if(newBoard.cur_command_idx == 0)
				newBoard.cur_command_idx = 1;
			else
				newBoard.cur_command_idx = 0;

			CommandInfo command = newBoard.commands[newBoard.cur_command_idx];
			foreach (UnitImpl unit in command.staff)
				unit.made_move = false;

			return newBoard;
		}
	}
}