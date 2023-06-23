using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ProjectileController : MovementModel
{
    [System.Serializable]
    public struct targetData
    {
        public GameObject target;
        public int pushIndex;

        public targetData(GameObject target_in, int pushIndex_in)
        {
            target = target_in;
            pushIndex = pushIndex_in;
        }
    };
    
    public GameObject currentTarget;
    

    public List<targetData> targets = new List<targetData>();

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
        if(explode)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, explode.blastRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                OnAttackStay(hits[i].gameObject);
            }
        }

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
            Destroy(this.gameObject);
        }
    }
    
    private void OnAttackStay(GameObject vic)
    {
        if(gravity)
        {
            gravity.OnAttackStay(vic, ref targets);
        }
    }
    public virtual void OnDestroy()
    {
        if(explode)
        {
            explode.OnExplode(ref targets);
        }
        EntityManager.Instance.Remove(ID);
    }
}
