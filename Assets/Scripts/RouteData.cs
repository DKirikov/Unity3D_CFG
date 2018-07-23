using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CFGEngine;
using CFG;

public class RouteData : MonoBehaviour 
{
    public GameObject route_marker;
    private List<GameObject> route = new List<GameObject>();

    public void Clear()
    {
        foreach (GameObject marker in route)
        {
            Destroy(marker);
        }
        route.Clear();
    }

	public void CreateByCell(MoveImpl move)
    {
        foreach (GameObject marker in route)
        {
            Destroy(marker);
        }
        route.Clear();

		CFG.Animation route_animation = MoveAnimator.CreateAnimation(move);

		if (route_animation == null)
			return;

		double cur_length = 0;
        double angle;
		Vector2 cur_pos = new Vector2();

		while (route_animation.GetPosition(cur_length, out cur_pos, out angle))
        {
			//ToDo change GetPosition to return Vector3 and rotate board to be xy instead of xz
            Vector3 offset = new Vector3((float)cur_pos.x, 0.1f, (float)cur_pos.y);
            GameObject route_marker_inst = Instantiate(route_marker, offset, transform.rotation) as GameObject;
            route.Add(route_marker_inst);

            cur_length += 0.5;//ToDo this is not good
        }
    }

    public bool IsActive()
    {
        return route.Count > 0;
    }
}
