using UnityEngine;

namespace BAPointCloudRenderer.Controllers {
    /*
     * CameraController for flying-controls
     */
    public class CameraController : MonoBehaviour {

        //Current yaw
        private float yaw = 0.0f;
        //Current pitch
        private float pitch = 0.0f;

        public float normalSpeed = 100;

        void Start() {
            //Hide the cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update() {
            if (Input.GetKey(KeyCode.Escape)) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (Input.GetMouseButtonDown(0)) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        void FixedUpdate() {
            //React to controls. (WASD, EQ and Mouse)
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");
            float moveUp = Input.GetKey(KeyCode.E) ? 1 : Input.GetKey(KeyCode.Q) ? -1 : 0;

            float speed = normalSpeed;
            if (Input.GetKey(KeyCode.C)) {
                speed /= 10; ;
            } else if (Input.GetKey(KeyCode.LeftShift)) {
                speed *= 5;
            }
            transform.Translate(new Vector3(moveHorizontal * speed * Time.deltaTime, moveUp * speed * Time.deltaTime, moveVertical * speed * Time.deltaTime));

            yaw += 2 * Input.GetAxis("Mouse X");
            pitch -= 2 * Input.GetAxis("Mouse Y");
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
    }

}
