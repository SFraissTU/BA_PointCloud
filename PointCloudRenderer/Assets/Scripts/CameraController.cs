using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * CameraController for flying-controls
 */
public class CameraController : MonoBehaviour
{

    //Current yaw
    private float yaw = 0.0f;
    //Current pitch
    private float pitch = 0.0f;

    void Start()
    {
        //Hide the cursor
        Cursor.visible = false;
    }


    void FixedUpdate()
    {
        //React to controls. (WASD, EQ and Mouse)
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        float moveUp = Input.GetKey(KeyCode.E) ? 1 : Input.GetKey(KeyCode.Q) ? -1 : 0;

        transform.Translate(new Vector3(moveHorizontal * 10 * Time.deltaTime, moveUp * 10 * Time.deltaTime, moveVertical * 10 * Time.deltaTime));

        yaw += 2 * Input.GetAxis("Mouse X");
        pitch -= 2 * Input.GetAxis("Mouse Y");
        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }
}
