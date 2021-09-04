using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    //This controller will control whoever is the party leader

    string clipToPlay;
    Party party;

    [SerializeField] GameObject camera;

    public UIManager UI;

    bool controllable;

    public void Awake()
    {
        party = GetComponent<Party>();
    }

    void Start() 
    {
        clipToPlay = "menu_open";
        VisualsRotator.quads.Add(GetComponentInChildren<MeshRenderer>().gameObject);
        controllable = true;
    }
    public void Update()
    {
        if(Input.GetKey(KeyCode.F3))
        {
            if(Input.GetKeyDown(KeyCode.M)) //!CAMERA DEBUG
            {
               // Camera.main.GetComponent<CameraMovement>().ToggleCameraMode();
            }
            if(Input.GetKeyDown(KeyCode.T)) //!HEALTH DEBUG
            {
                GetComponent<HealthModel>().TakeDamage(1);
            }
            if(Input.GetKeyDown(KeyCode.Q)) //!COMMAND DEBUG
            {
                UIManager.Instance.OpenCommandBox(); controllable = false;
                return;
            }
        }
        if(Input.GetKeyDown(KeyCode.Return))
        {
            if(UIManager.Instance.CloseCommandBox()){controllable = true; return;}
        }
        if(Input.GetKeyDown(KeyCode.Q))
        {
            AudioManager.PlaySFX(clipToPlay);
            clipToPlay = clipToPlay =="menu_open"? "menu_close": "menu_open";
            UI.OpenOrClose(UIManager.UIScreen.MainMenu);
            UIManager.ToggleHUD();
        }
        if(!controllable){return;}

        Move();
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Interact();
        }
        GetComponentInChildren<PlayerAttackModel>().UpdateAttack();
    }
    private void LateUpdate() 
    {
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            Camera.main.GetComponent<CameraMovement>().Rotate(1);
        }
        else if(Input.GetKey(KeyCode.RightArrow))
        {
            Camera.main.GetComponent<CameraMovement>().Rotate(-1);
        }
    }

    void Move()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            if(party.GetPartyLeader().GetComponent<StatusConditionModel>().IfHasCondition(Condition.Rigid))
            {
                if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
                {
                    OnMove(KeyCode.W);
                }
                if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W))
                {
                    OnMove(KeyCode.S);
                }
                if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                {
                    OnMove(KeyCode.A);
                }
                if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
                {
                    OnMove(KeyCode.D);
                }
            }
            else
            {
                Vector2 temp = Vector2.zero;
                if(Input.GetKey(KeyCode.A)) { temp.x =-1;}
                if(Input.GetKey(KeyCode.D)) { temp.x = 1;}
                if(Input.GetKey(KeyCode.W)) { temp.y = 1;}
                if(Input.GetKey(KeyCode.S)) { temp.y =-1;}

                party.GetPartyLeader().GetPMM().SetMovementDirection(Quaternion.Euler(0, 0, camera.transform.rotation.eulerAngles.z) * temp);
            }
            GetComponentInChildren<Animator>().SetBool("Walking", true);
        }
        else
        {
            GetComponentInChildren<Animator>().SetBool("Walking", false);
        }
    }
    public void OnMove(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W:
                party.GetPartyLeader().GetPMM().SetMovementDirection(new Vector2(0, 1));
                break;
            case KeyCode.A:
                party.GetPartyLeader().GetPMM().SetMovementDirection(new Vector2(-1, 0));
                break;
            case KeyCode.S:
                party.GetPartyLeader().GetPMM().SetMovementDirection(new Vector2(0, -1));
                break;
            case KeyCode.D:
                party.GetPartyLeader().GetPMM().SetMovementDirection(new Vector2(1, 0));
                break;
            default:
                break;
        }
    }
    public void Interact()
    {
        party.GetPartyLeader().GetPIM().OnInteract();
    }
}
