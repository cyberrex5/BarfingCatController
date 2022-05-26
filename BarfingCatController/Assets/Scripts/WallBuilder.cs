using UnityEngine;

public class WallBuilder : MonoBehaviour
{
    [SerializeField] private string wallEndTag;

    private Transform wallEnd;
    private Vector3 wallStart;

    private void Start()
    {
        wallEnd = GameObject.FindWithTag(wallEndTag).transform;
        transform.position = wallEnd.position;
        wallStart = transform.position;
    }

    private void Update()
    {
        UpdateWall();
    }

    public void ForceWallEndPosition(Vector3 endPos)
    {
        wallEnd.position = endPos;
        UpdateWall();
    }

    public void SwitchWallSides()
    {
        Vector3 oldScale = transform.localScale;
        transform.localScale = new Vector3(oldScale.z, oldScale.y, oldScale.x);
        transform.Rotate(Vector3.up, 90f);

        Vector3 forwardFace = 0.5f * transform.localScale.z * transform.forward;
        wallStart = transform.position - forwardFace;
        wallEnd.position = transform.position + forwardFace;
    }

    private void UpdateWall()
    {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, (wallEnd.position - wallStart).magnitude);
        transform.position = (wallEnd.position + wallStart) / 2;
        transform.Rotate(new Vector3(0f, Vector3.SignedAngle(transform.forward, wallEnd.position - transform.position, Vector3.up), 0f));
    }
}
