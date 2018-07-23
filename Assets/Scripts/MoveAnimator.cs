using System;
using CFGEngine;
using System.Collections.Generic;
using UnityEngine;

namespace CFG
{

	public abstract class Animation
	{
		public abstract bool GetPosition(double length, out Vector2 pos, out double angle); // time in [0, total_length]
	}

	public class MoveAnimator
	{
		
		private class MoveAnimation : Animation
		{
			public MoveAnimation(Vector2[] points, double speed)
			{
				this.points = points;
				this.speed = speed;
				lengths = new double[points.Length - 1];
				total_length = 0;
				
				for (int i = 0; i < points.Length - 1; ++i)
				{
					lengths[i] = (points[i + 1] - points[i]).magnitude;
					total_length += lengths[i];
				}
			}
			
			public override bool GetPosition(double cur_length, out Vector2 pos, out double angle) // cur_length in [0, total_length]
			{
				pos = new Vector2();
				bool ret = false;
				angle = 0;
				cur_length = speed * cur_length;
				for (int i = 0; i < lengths.Length; ++i)
				{
					if (cur_length <= lengths[i])
					{
						float fraction = (float) (cur_length / lengths[i]);
						pos = points[i] + (points[i + 1] - points[i]) * fraction;
						ret = true;
						angle = 180 / Math.PI * Math.Atan2(points[i + 1].x - points[i].x, points[i + 1].y - points[i].y); //ToDo check sign
						//double a2 = Vector2.Angle(points[i + 1] - points[i], Vector2.up);
						//ToDo change to Vector2.Angle(points[i + 1] - points[i], Vector2.up) or something
						break;
					}
					else
						cur_length -= lengths[i];
				}
				
				return ret;
			}
			
			Vector2[] points;
			double[] lengths;
			double total_length;
			double speed;
		}
		
		static private bool AreCellsInRow(OrientedCell prev_cell, OrientedCell next_cell)
		{
			if (next_cell.orientation != OrientedCell.CellOrientation.Default ||
			   prev_cell.orientation != OrientedCell.CellOrientation.Default)
				return false;//ToDo join cells for giant
			
			//{ { 0, 1 }, { 1, ymax }, { 1, ymin }, { 0, -1 }, { -1, ymin }, { -1, ymax } };
			int dx = Math.Abs(next_cell.cell.board_x - prev_cell.cell.board_x);
			int dy = Math.Abs(next_cell.cell.board_y - prev_cell.cell.board_y);
			return (dx == 0 && dy == 2) || (dx == 2 && dy == 1);
		}

		static public Animation CreateAnimation(MoveImpl move)
		{
			Animation ret = null;
			var cur_route = GameBoard.instance.CalcRouteTo(move);

			if (cur_route != null) 
			{
				List<Vector2> points = new List<Vector2> (cur_route.Count);
				points.Add (GameBoard.instance.FindCellPlace(cur_route [0]));
				for (int i = 1; i < cur_route.Count - 1; ++i) 
				{
					if (!AreCellsInRow (cur_route [i - 1], cur_route [i + 1]))
						points.Add (GameBoard.instance.FindCellPlace(cur_route [i]));
				}
				points.Add (GameBoard.instance.FindCellPlace(cur_route [cur_route.Count - 1]));
			
				ret = new MoveAnimation (points.ToArray (), 1);
			}
			return ret;
		}

		static public Animation CreateAnimation(Vector2 start, Vector2 final)
		{
			return new MoveAnimation (new Vector2[]{start, final}, 1);
		}	
	}
}
