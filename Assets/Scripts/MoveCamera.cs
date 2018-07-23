using UnityEngine;
using System.Collections;

public class MoveCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

	public float distanceMin = .5f;
    public float distanceMax = 15f;

    //private Rigidbody rigidbody;

    float x = 0.0f;
    float y = 0.0f;

    // Use this for initialization
    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        /*rigidbody = GetComponent<Rigidbody>();

        // Make the rigid body not change rotation
        if (rigidbody != null)
        {
            rigidbody.freezeRotation = true;
        }*/

        RecalcPos();
    }

    void LateUpdate()
    {
        if (target && GameBoard.instance && !GameBoard.instance.IsDudeSelected())
        {
            // ToDo make unique code for input
			if (UniversalInput.RMouseDown() || UniversalInput.MouseScroll())
            {
                RecalcPos();
            }
        }
        //ToDo make unique code for input
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    private void RecalcPos()
    {
		Vector2 delta = UniversalInput.MousePosDelta();
		float dist_delta = UniversalInput.MouseScrollDelta();

		delta.y = 0;

        x += delta.x * xSpeed * distance * 0.02f;
        y -= delta.y * ySpeed * 0.02f;

        y = ClampAngle(y, yMinLimit, yMaxLimit);

        Quaternion rotation = Quaternion.Euler(y, x, 0);

        distance = Mathf.Clamp(distance + dist_delta, distanceMin, distanceMax);

        /*RaycastHit hit;
		if (Physics.Linecast(transform.position, target.position, out hit) && hit.transform.gameObject.GetComponent<DudeData>() != null)
        {
            distance -= hit.distance;
        }*/

		Vector3 position;
		float collision_delta = 0.1f;

		do
		{
        	Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        	position = rotation * negDistance + target.position;
		}
		while(Physics.CheckSphere(position, collision_delta) && (distance += collision_delta) < distanceMax );

        transform.rotation = rotation;
        transform.position = position;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
    /*
    public Transform target;
    public Vector3 offset;
    public float sensitivity = 3f;
    public float up_limit = 80f;
    public float down_limit = -80f;
    private float Y;
    private float X;

    void Start()
    {
        transform.position = target.position + offset;
    }

    void Update()
    {
        if (Input.GetMouseButton(1) || Input.touchCount == 1)
        {
            X = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivity;
            Y += Input.GetAxis("Mouse Y") * sensitivity;
            Y = Mathf.Clamp(Y, -up_limit, -down_limit);
            transform.localEulerAngles = new Vector3(-Y, X, 0);
            transform.position = transform.localRotation * offset + target.position;
        }
    }*/
}