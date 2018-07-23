using UnityEngine;
using System.Collections;
using CFGEngine;

public class CellData : MonoBehaviour, CFGEngine.GameboardCell.CellNotificator
{
    public Material cell_mat;
    public Material cell_sel_mat;
    public Material cell_avail_mat;

    public GameboardCell cell;
	
    public enum Status
    {
        Clear = 0, Selected = 1, Available = 2
    }

	public const float cell_radius = 1.14f; // ToDo make optional
	public const float cell_width = cell_radius * 1.7320508075688772935274463415059f; //Math.Sqrt(3);

	private Status status = Status.Clear;

    public void SetStatus(Status status)
    {
		this.status = status;
        Material mat = cell_mat;
        switch (status)
        {
            case Status.Available: mat = cell_avail_mat; break;
            case Status.Selected: mat = cell_sel_mat; break;
        }
        gameObject.GetComponent<MeshRenderer>().material = mat;
    }

	public Status GetStatus()
	{
		return status;
	}

	public void UnitAdded (UnitImpl unit)
	{
		//ToDo get colors from somewhrere
		Color color = Color.clear;
		if (unit != null) 
		{
			color = (unit.command_idx == 0 ? Color.red : Color.green);
			color.a = 0.5f;
		}

		GetComponentInChildren<SpriteRenderer>().color = color;
	}

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
