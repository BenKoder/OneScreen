using UnityEngine;
public class Despawn : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Started!");
    }
    private void OnTriggerEnter(Collider fallen)
    {
        Debug.Log("Hit " + fallen.gameObject);
        Destroy(fallen.gameObject);
    }
}
