using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMovementBehavior : MonoBehaviour
{
    public enum SineAxis
    {
        NONE = 0,
        X = 1,
        Y = 2,
        Z = 3
    }
    Rigidbody body;
    float timer;
    SphereCollider sphereCollider;
    [SerializeField]GameObject visuals;
    [SerializeField] GameObject light;
    [SerializeField] float curveSpeed;
    [SerializeField] SineAxis axis;
    void Start()
    {
        body = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    public void Move()
    {
        float angle = 0;
        Vector3 rotatedSine = Vector3.zero;
        switch (axis)
        {
            case SineAxis.Z:
                break;
            case SineAxis.X:
                angle = Vector3.Angle(body.velocity.normalized, Vector3.up);
                rotatedSine = Quaternion.Euler(angle, 0, 0) * new Vector3(Mathf.Sin(timer), 0);
                break;
            case SineAxis.Y: //confirme correct
                angle = Vector3.Angle(body.velocity.normalized, Vector3.forward);
                rotatedSine = Quaternion.Euler(0, 0, angle) * new Vector3(Mathf.Sin(timer), 0);
                break;
        }
        sphereCollider.center = rotatedSine;
        light.transform.localPosition = rotatedSine;
        visuals.transform.localPosition = rotatedSine;
        timer+=curveSpeed;
    }
}
