using UnityEngine;

public class WallBuilder : MonoBehaviour
{
    private Transform wallEnd;
    private Vector3 wallStart;

    private void Start()
    {
        wallEnd = GameObject.FindWithTag("WallStart").transform;
        transform.position = wallEnd.position;
        wallStart = transform.position;
    }

    private void Update()
    {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, (wallEnd.position - wallStart).magnitude);
        transform.position = (wallEnd.position + wallStart) / 2;
        transform.Rotate(new Vector3(0f, Vector3.SignedAngle(transform.forward, wallEnd.position - transform.position, Vector3.up), 0f));
    }
}
