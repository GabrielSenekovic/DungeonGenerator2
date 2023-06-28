using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ProjectileController : MovementModel
{
    public GameObject currentTarget;

    public bool placedProjectile;

    public bool collideWithCaster;
    MeshRenderer renderer;

    [SerializeField]List<GameObject> visuals;

    public int lifeLength;

    public int ID = 0;
    int lifeTimer = 0;

    Explode explode;
    Gravity gravity;
    Homing homing;
    ProjectileAcceleration acceleration;
    private new void Awake()
    {
        base.Awake();
        TryGetComponent(out explode);
        TryGetComponent(out gravity);
        TryGetComponent(out homing);
        TryGetComponent(out acceleration);
    }
    void Start()
    {
        VisualsRotator.renderers.AddRange(visuals);
        renderer = GetComponentInChildren<MeshRenderer>();
        if(GetComponentInChildren<Light>())
        {
            GetComponentInChildren<Light>().color = renderer.sharedMaterial.color;
        }
        if(placedProjectile)
        {
            VisualsRotator.Add(renderer);
            GetComponent<SphereCollider>().isTrigger = false;
        }
        else
        {
            GetComponent<SphereCollider>().isTrigger = true;
        }
    }
    new private void FixedUpdate() 
    {
        OnAttackStay();

        lifeTimer++;
        if(placedProjectile && explode)
        {
            renderer.material.SetFloat("_IsExploding", (float)lifeTimer/(float)lifeLength);
        }
        if(acceleration){acceleration.CheckAccelerationMode();}
        if(homing){homing.CheckHomingMode();}
        Move();
        if(lifeTimer >= lifeLength)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnAttackStay()
    {
        gravity?.OnAttackStay();
    }
    public virtual void OnDestroy()
    {
        if(explode)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, explode.blastRadius);
            explode.OnExplode(hits);
        }
        EntityManager.Instance.Remove(ID);
    }
}
