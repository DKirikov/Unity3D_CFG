using UnityEngine;
using System.Collections;

public class FaceTargetScript : MonoBehaviour {

    void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
