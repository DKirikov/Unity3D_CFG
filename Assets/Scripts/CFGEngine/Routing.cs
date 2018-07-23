using System.Collections;
using System.Collections.Generic;
using System;
using CFGEngine;

namespace CFGEngine
{
		
	public class Routing 
	{
		private GameboardImpl gameboard;

		public Routing(GameboardImpl gameboard)
		{
			this.gameboard = gameboard;
		}

		private class CellCandidate
		{
			public CellCandidate(OrientedCell cell_ref, int route_cnt, CellCandidate prev_cand)
			{
				this.cell_ref = cell_ref;
				this.route_cnt = route_cnt;
				this.prev_cand = prev_cand;
			}

			public OrientedCell cell_ref;
			public int route_cnt;
			public CellCandidate prev_cand = null;
		}

		private class CandidatesQueue : Queue<CellCandidate>
		{
			public new void Enqueue(CellCandidate item)
			{
				base.Enqueue (item);
				candidats_map [item.cell_ref] = item;
			}

			public CellCandidate FindCandidate(OrientedCell cell_ref)
			{
				CellCandidate ret = null;
				candidats_map.TryGetValue (cell_ref, out ret);
				return ret;
			}

			public Dictionary<OrientedCell, CellCandidate> candidats_map = new Dictionary<OrientedCell, CellCandidate> ();
		}
			
		private void CalculateRoute(OrientedCell from, Func<CellCandidate, bool> CheckCand, Func<CellCandidate, bool> NewCand )
		{
			var candidats = new CandidatesQueue();//ToDo calculate for giant
			bool end = false;

			UnitImpl unit = from.cell.unit;
			candidats.Enqueue(new CellCandidate(from, 0, null));
			while (candidats.Count > 0 && !end)
			{
				CellCandidate cand = candidats.Dequeue();
				if (CheckCand(cand))
				{
					List<OrientedCell> neighbor_cells = gameboard.GetOneMoveCells(cand.cell_ref, unit);
					foreach (var cell in neighbor_cells)
					{
						CellCandidate cell_cand = candidats.FindCandidate(cell);
						if (cell_cand == null || cell_cand.route_cnt > cand.route_cnt + 1)
						{
							var new_cand = new CellCandidate(cell, cand.route_cnt + 1, cand);
							candidats.Enqueue(new_cand);

							if (!NewCand (new_cand)) 
							{
								end = true;
								break;
							}

						}
					}
				}
			}			
		}

		public class CAvailableCells : OrientedCell
		{
			public CAvailableCells(OrientedCell cell, int steps) 
			{
				this.cell = cell.cell; this.orientation = cell.orientation; this.steps = steps;
			}
			public int steps;
		}

		public List<CAvailableCells> CalcAvailableCells(OrientedCell from, int steps)
		{
			List<CAvailableCells> ret = new List<CAvailableCells>();

			CalculateRoute(from, 
				cand => cand.route_cnt < steps, 
				new_cand => 
				{
					ret.Add(new CAvailableCells( new_cand.cell_ref, new_cand.route_cnt)); 
					return true;
				}
			);
			
			return ret;
		}

		public int[] CalcRouteToMulty(OrientedCell from, List<OrientedCell> to, List<OrientedCell>[] routes)//ToDo calculate for giant
		{
			var to_idx = new Dictionary<OrientedCell, int>();// to[i] -> i

			for (int i = 0; i < to.Count; ++i) 
			{
				to_idx[to[i]] = i;
			}

			int[] ret = new int[to.Count];

			CalculateRoute(from, 
				_ => true, 
				new_cand => 
				{
					int idx;
					if (to_idx.TryGetValue (new_cand.cell_ref, out idx)) 
					{
						ret[idx] = new_cand.route_cnt;
						if (routes != null) 
						{
							routes[idx] = new List<OrientedCell>();

							routes[idx].Add(new_cand.cell_ref);

							while(new_cand.prev_cand != null)
							{
								routes[idx].Add(new_cand.prev_cand.cell_ref);
								new_cand = new_cand.prev_cand;
							}
							routes[idx].Reverse();
						}

						to_idx.Remove(new_cand.cell_ref);

						if( to_idx.Count == 0 )
							return false;
					}
					return true;
				}
			);

			return ret;
		}

	}

}