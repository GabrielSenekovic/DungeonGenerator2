using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public enum CameraMode
    {
        TopDown = 0,
        Side = 1
    }
    public enum CameraMovementMode
    {
        SingleRoom = 0,
        Free = 1
    }

    static CameraMovement instance;
    public static CameraMovement Instance
    {
        get
        {
            return instance;
        }
    }
    public static float rotationSideways = 0;
    public int rotationSpeed;
    [SerializeField]Party party;
    public CameraMode mode = CameraMode.Side;
    public static CameraMovementMode movementMode = CameraMovementMode.Free;
    public Vector2 cameraAnchor_horizontal;
    public Vector2 cameraAnchor_vertical;

    bool movingRoom = false;
    public GameObject cameraRotationObject;
    public Vector3 prevCameraPosition;
    public float transitionSpeed;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        transform.eulerAngles = new Vector3(-53, transform.rotation.y, transform.rotation.z);
        transform.position = new Vector3(transform.position.x, -15, -10); //-11, -8.2f
    }
    private void Update() 
    {
        if(!movingRoom)
        {
            Vector3 pos = Party.instance.GetPartyLeader().transform.position;
            float x = pos.x < cameraAnchor_horizontal.x ? cameraAnchor_horizontal.x : pos.x > cameraAnchor_horizontal.y ? cameraAnchor_horizontal.y : pos.x; 
            float y = pos.y < cameraAnchor_vertical.x ? cameraAnchor_vertical.x : pos.y > cameraAnchor_vertical.y ? cameraAnchor_vertical.y : pos.y; 
            cameraRotationObject.transform.position = new Vector3(x,y, pos.z);
            prevCameraPosition = cameraRotationObject.transform.position;

            //if(CameraMovement.cameraAnchor_in != Vector2.zero) //Do not move the cmaera object if there is an anchor
            //{
                //cameraRotationObject.transform.position = cameraAnchor;
                //}
        }
    }
    public void Rotate(int direction)
    {
        OnRotate(rotationSpeed * direction);
        rotationSideways+= rotationSpeed * direction;
    }
    void OnRotate(float speed)
    {
        transform.RotateAround(new Vector3(cameraRotationObject.transform.position.x, cameraRotationObject.transform.position.y, 0), Vector3.forward, speed);
        VisualsRotator.RotateAll(speed);
    }
    void ToggleCameraMode()
    {
        if(transform.position.z == -19)
        {
            transform.eulerAngles = new Vector3(-45, transform.rotation.y, transform.rotation.z);
            transform.position = new Vector3(transform.position.x, -8.5f, -8.2f);
            transform.RotateAround(new Vector3(party.GetPartyLeader().transform.position.x, party.GetPartyLeader().transform.position.y, 0), Vector3.forward, rotationSideways);
            mode = CameraMode.Side;
        }
        else
        {
            transform.position = new Vector3(party.GetPartyLeader().transform.position.x, party.GetPartyLeader().transform.position.y, -19);
            transform.eulerAngles = new Vector3(0, 0, rotationSideways);
            mode = CameraMode.TopDown;
        }
    }
    void ZoomInOut(float value)
    {
        //transform.position = new Vector3(transform.position.x + rotatedPosition.x, transform.position.y + rotatedPosition.y, transform.position.z + value);
    }
    public bool MoveCamera(Vector3 newPosition, Vector3 currentPosition)
    {
        cameraRotationObject.transform.position = Math.Transition(cameraRotationObject.transform.position, newPosition, currentPosition, transitionSpeed);

        if(cameraRotationObject.transform.position == newPosition)
        {
            Party.instance.GetPartyLeader().GetStatusConditionModel().RemoveCondition(Condition.Cutscene);
            movingRoom = false;
            return true;
        }
        return false;
    }
    public static void SetCameraAnchor(Vector2 horizontal_in, Vector2 vertical_in)
    {
        instance.cameraAnchor_horizontal = horizontal_in;
        instance.cameraAnchor_vertical = vertical_in;
    }
    public static void SetMovingRoom(bool value)
    {
        instance.movingRoom = value;
    }
    public static bool GetMovingRoom()
    {
        return instance.movingRoom;
    }
    public static GameObject GetRotationObject()
    {
        return instance.cameraRotationObject;
    }
}
