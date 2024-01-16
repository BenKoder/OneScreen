using UnityEngine;

public class Quit : MonoBehaviour
{
    //I know I could've (should've) made a menu for this, but ah well what can you do
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }
}
