using UnityEngine;

public class FollowMain : MonoBehaviour
{
    public Transform adventurer;

    private float _offsetX;

    void Start()
    {
        _offsetX = transform.position.x - adventurer.position.x;
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.x = adventurer.position.x + _offsetX;
        transform.position = pos;
    }
}