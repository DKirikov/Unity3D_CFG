using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class UniversalInput 
{
	static private bool is_down = false;

	static private bool IsPointerOverGameObject()
	{
		if (Input.touches.Length == 0 && EventSystem.current.IsPointerOverGameObject ()) 
		{
			return true;
		}
		foreach (var touch in Input.touches) 
		{
			if( EventSystem.current.IsPointerOverGameObject (touch.fingerId) )
				return true;
		}
		return false;
	}

	static private bool MouseStatus(int pc_button, bool click)//or down
	{
		if (click) 
		{

			if (Input.touches.Length == 1)
			{
				if(Input.touches [0].phase == TouchPhase.Began)
				{
					is_down = !IsPointerOverGameObject ();
					return false;
				}
			}
			else if( Input.touches.Length == 0 && is_down )
			{
				is_down = false;
				return true;
			}	

		} 
		else 
		{
		
			if (Input.touches.Length == 1 && Input.touches [0].phase == TouchPhase.Moved)
				return !IsPointerOverGameObject ();
		}

		if ((click ? Input.GetMouseButtonDown (pc_button) : Input.GetMouseButton (pc_button))) 
		{
			return !IsPointerOverGameObject ();
		}
		
		return false;
	}

	static public bool LMouseDown()
	{
		return MouseStatus(0, false);
	}
	
	static public bool RMouseDown()
	{
		return MouseStatus(1, false);
	}
	
	static public bool LMouseClick()
	{
		return MouseStatus(0, true);
	}
	
	static public bool RMouseDownClick()
	{
		return MouseStatus(1, true);
	}

	static public bool MouseScroll()
	{
		return Input.touches.Length == 2 || Input.GetAxis("Mouse ScrollWheel") != 0;
	}

	static public Vector2 MousePosDelta()
	{
		Vector2 ret = new Vector2(0,0);
		if (Input.touchCount >= 1)
		{
			ret = Input.GetTouch(0).deltaPosition;
		}
		else
		{
			ret = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		}	
		return ret;
	}
	
	static public float MouseScrollDelta()
	{
		float ret = 0;
		if (Input.touchCount == 2)
		{
			// Store both touches.
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);
			
			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
			
			// Find the magnitude of the vector (the distance) between the touches in each frame.
			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
			
			// Find the difference in the distances between each frame.
			ret = (prevTouchDeltaMag - touchDeltaMag)*0.5f;
		}
		else
		{
			ret = -Input.GetAxis("Mouse ScrollWheel") * 5;
		}	
		return ret;
	}

	static public Vector3 MousePos()
	{
		return IsPointerOverGameObject() ? new Vector3(-1000,-1000,-1000) : Input.mousePosition;
	}
}
