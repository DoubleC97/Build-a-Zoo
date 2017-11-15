using UnityEngine;
using System.Collections;

public class Floater : MonoBehaviour
{
    public float waterLevel, floatHeight;
    public Vector3 buoyancyCentreOffset;
    public float bounceDamp;
    Rigidbody rb;

    void Start()
    {

    }
    void FixedUpdate()
    {
            Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 actionPoint = transform.position + transform.TransformDirection(buoyancyCentreOffset);
        float forceFactor = 1f - ((actionPoint.y - waterLevel) / floatHeight);

        if (forceFactor > 0f)
        {
            Vector3 uplift = -Physics.gravity * (forceFactor - rb.velocity.y * bounceDamp);
            rb.AddForceAtPosition(uplift, actionPoint);
        }
    }
}
