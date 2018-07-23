using UnityEngine;
using UnityEngine.UI;
using CFGEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO; 
using System;

using CFG;
using System.Threading;

public class GameBoard : MonoBehaviour, GameboardNotificator {

	private List<Move> aiMoveQueue = new List<Move>();
	private System.Object lockForQueue = new System.Object();

    public GameObject cell_obj;
    public GameObject end_move;
    public Texture2D walk_cursor;
    public Texture2D attack_cursor;
	public Texture2D bow_attack_cursor;
	public Texture2D cross_cursor;

    public Material red_mat;
    public Material blue_mat;

	public Thread newThread = null;


    public static GameBoard instance = null; //ToDo check for better practice

    private List<CellData> cells_list = new List<CellData>();
    private List<DudeData> dudes_list = new List<DudeData>();
	private List<Move> available_moves = null;
	private CFGEngine.GameboardCell first_selected_cell = null;

    private const int max_units_qty = 5;


	private DateTime end_move_time = DateTime.Now;

	private GameboardImpl gameboard_impl;

    void OnGUI()
    {
		if (gameboard_impl.is_game_finished)
        {
            //Set the GUIStyle style to be label
            GUIStyle style = GUI.skin.GetStyle("box");

            //Set the style font size to increase and decrease over time
            style.fontSize = (int)(15.0f + 5.0f * Mathf.Sin(Time.time));

            style.alignment = TextAnchor.MiddleCenter;

			string message = gameboard_impl.commands[0].is_won == gameboard_impl.commands[1].is_won ?
							 "It is a tie!!!" :
							 (!gameboard_impl.commands[0].is_won ? "Magenda" : "Yellow") + " command is lose";

            GUI.Box(new Rect(Screen.width / 3, Screen.height / 3, Screen.width / 3, Screen.height / 3), message, style);
        }
    }

    // Use this for initialization
	void Start () 
    {
        gameboard_impl = new GameboardImpl();
		gameboard_impl.SetNotificator (this);

        instance = this;

		int X_SIZE = gameboard_impl.cells.GetLength(0);
		int Y_SIZE = gameboard_impl.cells.GetLength(1);

		//ToDo move it to CellData
		float offset_x = ((Y_SIZE - 1) * CellData.cell_width + ((X_SIZE - 1) % 2) * CellData.cell_width / 2) / 2;
		float offset_y = ((X_SIZE - 1) * CellData.cell_radius * 1.5f) / 2;

		foreach (GameboardCell cell in gameboard_impl.cells)
        {
            if (cell.active)
            {
				float x = cell.board_y * CellData.cell_width + (cell.board_x % 2) * CellData.cell_width / 2 - offset_x;
				float y = cell.board_x * CellData.cell_radius * 1.5f - offset_y;

				Vector3 offset = new Vector3(x, 0.02f, y);
                GameObject cll_obj = Instantiate(cell_obj) as GameObject;

                CellData cell_data = cll_obj.GetComponent<CellData>();
				cll_obj.transform.position = offset;
                cell_data.cell = cell;
				cell.notificator = cell_data;

                cells_list.Add(cell_data);
            }            
        }

        for (gameboard_impl.cur_command_idx = 0; gameboard_impl.cur_command_idx < GameboardImpl.players_qty; ++gameboard_impl.cur_command_idx)
        {
            gameboard_impl.commands[gameboard_impl.cur_command_idx] = new CommandInfo();
		}

		for (gameboard_impl.cur_command_idx = 0; gameboard_impl.cur_command_idx < GameboardImpl.players_qty; ++gameboard_impl.cur_command_idx)
		{
			Figure boss = DataBase.Bosses[0];
            GameboardCell cur_cell = gameboard_impl.cells[gameboard_impl.cells.GetLength(0) / 2, (gameboard_impl.cur_command_idx == 0) ? 0 : (gameboard_impl.cells.GetLength(1) - 2)];
			var boss_card = new CardImpl(boss);
			gameboard_impl.commands[gameboard_impl.cur_command_idx].hand.Add(boss_card);
			OnCardSelected(boss_card);
			HitCell(new ClickInfo(FindCell(cur_cell)));
        }

        ChangeCommand();

		var command = gameboard_impl.commands [gameboard_impl.cur_command_idx];
		foreach (UnitImpl unit in command.staff)
        {
            unit.made_move = true;//ToDo this is hack!
        }

		//gameboard_impl.GetCards () [0].GetMoves () [3].MakeMove (gameboard_impl);
    }


	//ToDo it could take a time. change cells_list to Dictionary<GameboardCell, CellData>
	public CellData FindCell(CellPlace cell_place)
    {
		CellData ret = null;
		if (cell_place != null) 
		{
			GameboardCell search_cell = gameboard_impl.cells[cell_place.board_x, cell_place.board_y];
			if (search_cell != null) 
			{
				if (search_cell.notificator != null && search_cell.notificator as CellData != null)
					ret = search_cell.notificator as CellData;
				else
					ret = cells_list.Find (cell_data => cell_data.cell == search_cell);
			}
		}

		return ret;
    }

	public Vector2 FindTargetCellPlace(MoveImpl move)
	{
		return FindCellPlace(move.GetTargetCell (gameboard_impl));
	}

	public Vector2 FindCellPlace(OrientedCell cell)
	{
		Vector2 ret = new Vector2(0,0);

		var occu = gameboard_impl.GetOccupatedCells (cell);
		int cnt = 0;

		Array.ForEach (occu, oc_cell => {
			if (oc_cell != null) { //ToDo change to return Vector3 and rotate board to be xy instead of xz
				Vector3 pos3d = FindCell (oc_cell).gameObject.transform.position;
				ret += new Vector2 (pos3d.x, pos3d.z);
				++cnt;
			}
		});

		if (cnt > 0)
			ret /= cnt;

		return ret;
	}

	//ToDo it could take a time. change dudes_list to Dictionary<UnitImpl, DudeData>
    public DudeData FindDude(UnitImpl search_unit)
    {
		if (search_unit.notificator != null && search_unit.notificator as DudeData != null)
			return search_unit.notificator as DudeData;
		
        return dudes_list.Find(dude_data => dude_data.unit == search_unit);
    }

	public bool IsAICalculating()
	{
		return newThread != null && (newThread.IsAlive || aiMoveQueue.Count > 0);
	}

	public void BlockUI(bool block)
	{
		foreach (var but in GameObject.Find ("Canvas").GetComponentsInChildren<Button> ()) 
		{
			if( but.gameObject.tag != "Player" )
				but.interactable = !block;
		}
	}

	public void OnCardSelected(CardImpl card)
    {
		if (IsDudeSelected() || IsAICalculating()) 
		{
			ClearAvailableCells();
		}
		else
        {
			List<Move> moves = card.GetMoves(gameboard_impl);
			if( moves.Count > 0 )
				NewMovesAvailable(moves);
        }
    }

	
	public void RemoveUnit(DudeData dude)
	{
		dudes_list.Remove(dude);
		gameboard_impl.commands[dude.unit.command_idx].staff.Remove(dude.unit);
    }

    public void ChangeCommand()
    {
		gameboard_impl.ChangeCommand();
        DisplayCurrentCommand();
    }

    private void DisplayCurrentCommand()
    {
        Material cur_mat = gameboard_impl.cur_command_idx == 0 ? red_mat : blue_mat;//ToDo change something
        MeshRenderer[] renders = end_move.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer render in renders)
        {
            render.material = cur_mat;
        }

		PlayerUIUpdated ();
    }

	private void AddMoveToQueue(Move move)
	{
		lock (lockForQueue)
		{
			aiMoveQueue.Add(move);
		}
	}

	private void EndTurn()
	{
		ChangeCommand();
		DoAllMoves();
	}

	private void DoAllMoves()
	{
		BlockUI (true);
		AI ai = new AI(gameboard_impl);
		aiMoveQueue.Clear();

		ai.onCalculateMove += new AI.CalculateMoveEventHandler(AddMoveToQueue);

		newThread = new Thread(new ThreadStart(ai.CalculateAllMoves));
		newThread.Start();
	}

    public void SaveGame()
    {
		SaveLoad.SaveGame (gameboard_impl);
    }

	public void OnLoadGameClick()
	{
		GameObject canvas = GameObject.Find ("Canvas");
		GameObject panel = canvas.GetComponent<UIScript> ().save_load_panel;
		GameObject clone = Instantiate(panel, new Vector2(0, 0), Quaternion.identity) as GameObject;
		clone.transform.SetParent(canvas.transform);
		clone.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

	}
	
	public void LoadGame(GameboardImpl new_gameboard)
	{
	    foreach (DudeData dude in dudes_list)
        {
			gameboard_impl.SetUnitPlace (dude.unit.oriented_cell, null);
            Destroy(dude.gameObject);
        }
        dudes_list.Clear();

        GameboardCell[,] save_cells = gameboard_impl.cells;
		gameboard_impl = (GameboardImpl) new_gameboard;
		gameboard_impl.SetNotificator (this);
		gameboard_impl.cells = save_cells;


        foreach (CommandInfo command in gameboard_impl.commands)
        {
            foreach (UnitImpl unit in command.staff)
            {
				unit.oriented_cell.cell = gameboard_impl.cells[unit.oriented_cell.cell.board_x, unit.oriented_cell.cell.board_y];
				gameboard_impl.SetUnitPlace (unit.oriented_cell, unit);

				UnitAdded(unit);
            }
        }

        DisplayCurrentCommand();
    }
	
	public bool IsDudeSelected()
	{
		return available_moves != null;
	}

	public List<OrientedCell> CalcRouteTo(MoveImpl move)
    {
		List<OrientedCell> to_list = new List<OrientedCell> (1);
		List<OrientedCell>[] routes = new List<OrientedCell>[1];

		to_list.Add (move.GetTargetCell(gameboard_impl));

		new Routing(gameboard_impl).CalcRouteToMulty(move.GetUnitPlace(gameboard_impl), to_list, routes);//ToDo try to remove this

		return routes[0] != null && routes[0].Count > 0 ? routes[0] : null;
    }

	private void ClearAvailableCells()
	{
		foreach (CellData cell_obj in cells_list)
		{
			cell_obj.SetStatus(CellData.Status.Clear);
		}
		available_moves = null;	
		first_selected_cell = null;

		GetComponent<RouteData>().Clear();
	}

	private void NewMovesAvailable(List<Move> moves)
	{
		available_moves = moves;
		foreach (var move in available_moves)
		{
			if( (move as MoveImpl) != null )
			{
				var occu_cel = gameboard_impl.GetOccupatedCells((move as MoveImpl).GetTargetCell(gameboard_impl));
				Array.ForEach (occu_cel, cell_place => FindCell (cell_place).SetStatus (CellData.Status.Available));
			}
		}	
	}

	private List<Move> GetSelectedMove(ClickInfo hit_cell)
	{
		List<Move> ret = null;
		if (IsDudeSelected () && hit_cell != null && hit_cell.cell != null) 
		{
			List<Move> kills = available_moves.FindAll(move => move.Type == Move.MoveType.Kill && 
													  (move as KillMoveImpl).UnitToKillPlace == hit_cell.cell.cell);

			if( ret == null && kills.Count > 0 && first_selected_cell == null ) //ToDo select shortest move
				ret = kills;

			if (ret == null) 
			{
				Move one_move = null;
				List<Move> moves = available_moves.FindAll 
					(
						move => 
						(
							move.Type == Move.MoveType.Move 
							||
							move.Type == Move.MoveType.Card
							||
							(first_selected_cell != null && move.Type == Move.MoveType.Kill)
						) 
						&&
						Array.IndexOf
						(
							gameboard_impl.GetOccupatedCells
							(
								(move as MoveImpl).GetTargetCell(gameboard_impl)
							)
							,
							hit_cell.cell.cell
						) > -1
					);	

				one_move = moves.Count > 0 ? moves [0] : null;

				if( hit_cell.click_point.y != 666 )
				{
					Vector2 pos = new Vector2 (hit_cell.click_point.x, hit_cell.click_point.z);

					Func<Move, float> GetDist = move => (FindTargetCellPlace (move as MoveImpl) - pos).magnitude;

					moves.ForEach 
					(
						move =>
						{
							if( GetDist (move) < GetDist (one_move) )
								one_move = move;
						}
					);
				}

				if (one_move != null) 
				{
					ret = new List<Move> ();
					ret.Add (one_move);
				}
			}
		}
		return ret;
	}

	private void HitCell(ClickInfo hit_cell)
    {
		if (hit_cell != null && hit_cell.cell != null)
        {
			if (!IsDudeSelected())
            {
				UnitImpl hit_dude = hit_cell.cell.cell.unit;
                if (hit_dude != null && !hit_dude.made_move && hit_dude.command_idx == gameboard_impl.cur_command_idx) //There is dude standing on this cell!!!
                {
					hit_cell.cell.SetStatus(CellData.Status.Selected);

					NewMovesAvailable(hit_dude.GetMoves(gameboard_impl));
                }
            }
            else
            {
				List<Move> sel_move = GetSelectedMove(hit_cell);
				if( sel_move != null && sel_move.Count == 1 )
					sel_move[0].MakeMove(gameboard_impl);

				ClearAvailableCells();

				if (sel_move != null && sel_move.Count > 1) 
				{
					NewMovesAvailable (sel_move);
					first_selected_cell = hit_cell.cell.cell;
				}
            }
        }
    }

	private class ClickInfo
	{
		public ClickInfo(CellData cell) {this.cell = cell;}

		public CellData cell;
		public Vector3 click_point = new Vector3(666,666,666);//Inavild value
	}

	private ClickInfo FindCellUnderMouse()
    {
		Ray ray = Camera.main.ScreenPointToRay(UniversalInput.MousePos());
        RaycastHit hit;

		ClickInfo ret = null;

		if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 8))
        {
			if (hit.transform.gameObject.tag == "Cell") 
			{
				ret = new ClickInfo (hit.transform.gameObject.GetComponent<CellData> ());
				ret.click_point = hit.point;
			}
			else if (hit.transform.gameObject.tag == "EndMove")// ToDo move to another method
			{
				if (!IsDudeSelected())
					ChangeCommand();
			}
			/*else
			{
                GameObject obj = hit.transform.gameObject;
                while (obj.transform.parent != null)
                {
                    obj = obj.transform.parent.gameObject;
                }
				if( obj.GetComponent<DudeData>() )
					ret = GameBoard.instance.FindCell(obj.GetComponent<DudeData>().unit.oriented_cell.cell);
            }*/
        }

        return ret;
    }

	void MakeMoveFromQueue()
	{
		Move move = aiMoveQueue[0];
		lock (lockForQueue)
		{
			aiMoveQueue.RemoveAt(0);
		}

		if (move == null) 
		{
			ChangeCommand ();
			BlockUI(false);
		}
		else
			move.MakeMove(gameboard_impl);
	}

	public void PlayerUIUpdated ()
	{
		var command = gameboard_impl.commands [gameboard_impl.cur_command_idx];

		UIScript.instance.SetCards (command.hand, gameboard_impl.commands [1-gameboard_impl.cur_command_idx].hand);
		UIScript.instance.SetCrystalsCount(command.crystals_count, command.crystals_inc);
	}
	
	public void UnitAdded (UnitImpl unit)
	{
		UnityEngine.Object model = Resources.Load(unit.modelName, typeof(GameObject));
		if( model == null )
			model = Resources.Load("Dude", typeof(GameObject)); //Default model
		
		GameObject clone = Instantiate(model) as GameObject;
		DudeData dude = clone.GetComponent<DudeData>();
		dude.Initialize(unit);

		dudes_list.Add(dude);
	}

	private ToolTip prev_selected = null;

	private void CheckToolTip()
	{
		Ray ray = Camera.main.ScreenPointToRay(UniversalInput.MousePos());
		RaycastHit hit;

		ToolTip cur_selected = null;
		if (Physics.Raycast (ray, out hit)) 
		{
			GameObject obj = hit.transform.gameObject;
			while (obj.transform.parent != null)
				obj = obj.transform.parent.gameObject;
			
			cur_selected = obj.GetComponent<ToolTip> ();
		}

		if (cur_selected != prev_selected) 
		{
			if( prev_selected != null )
				prev_selected.OnMouseExit ();
			if( cur_selected != null )
				cur_selected.OnMouseEnter ();

			prev_selected = cur_selected;
		}
	}
	
	// Update is called once per frame
	void Update () 
    {
		Texture2D cursor = null;
		bool block_selection = false;
		foreach(var dude in dudes_list)
		{
			if (!dude.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("DudeIdle"))
			{
				block_selection = true;
				break;
			}
		}

		if (block_selection) 
			end_move_time = DateTime.Now;
		else
			block_selection = (DateTime.Now - end_move_time).TotalMilliseconds < 200; //ToDo fix it properly

		if( !block_selection )//disable selection during animation
		{
			if (aiMoveQueue.Count > 0)
			{
				MakeMoveFromQueue();
				end_move_time = DateTime.Now;
				return;
			}

			if( IsAICalculating() )
				return;

            if (UniversalInput.LMouseClick())
            {
				HitCell(FindCellUnderMouse());
            }

			if (Input.GetKeyDown (KeyCode.Space)) 
			{
				ChangeCommand();
				return;
			}
			
			if (Input.GetKeyDown (KeyCode.Escape)) 
			{
				ClearAvailableCells();
				return;
			}

			CheckToolTip ();

			if (IsDudeSelected())
	        {
				cells_list.ForEach (cell => 
					{
						if( cell.GetStatus() != CellData.Status.Clear ) 
							cell.SetStatus( CellData.Status.Available );
					}
				);
				GetComponent<RouteData>().Clear();

				var sel_moves = GetSelectedMove(FindCellUnderMouse());
				 
				if (sel_moves != null && sel_moves.Count > 0 )
	            {
					foreach (var move in sel_moves)
					{
						var from = move.GetUnitPlace (gameboard_impl);
						if (from != null)
							Array.ForEach (gameboard_impl.GetOccupatedCells (from), 
								cell_place => FindCell (cell_place).SetStatus (CellData.Status.Selected));

						var to = move.GetTargetCell (gameboard_impl);
						if (to != null)
							Array.ForEach (gameboard_impl.GetOccupatedCells (to), 
								cell_place => FindCell (cell_place).SetStatus (CellData.Status.Selected));
					}

					Move sel_move = sel_moves[0];//ToDo Draw all
					
					if (sel_move.Type == Move.MoveType.Kill)
					{
						CellData from_cell = FindCell((sel_move as SkipMoveImpl).UnitPlace); //ToDo incapsulate healing and isRangedAttack in kill move
						cursor = attack_cursor;//ToDo try to OnMouseEnter/OnMouseExit
						if( from_cell != null && from_cell.cell.unit != null )
						{
							if( (sel_move as KillMoveImpl).isHealing )
								cursor = cross_cursor;
							else if( from_cell.cell.unit.isRangedAttack )
								cursor = bow_attack_cursor;
						}
					}
					else if(sel_move as KillMoveImpl != null)
						cursor = (sel_move as KillMoveImpl).isHealing ? cross_cursor : attack_cursor;
					else if(sel_move as MoveImpl != null)
						cursor = walk_cursor;

					if(sel_move as MoveImpl != null &&  (sel_move as MoveImpl).Steps > 0 && sel_moves.Count == 1)
					{
						GetComponent<RouteData>().CreateByCell(sel_move as MoveImpl);
					}
	            }
	        }
		}

        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
	}
}
