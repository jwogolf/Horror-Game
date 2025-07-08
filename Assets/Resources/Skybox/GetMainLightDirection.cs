using UnityEngine;

public class GetMainLightDirection : MonoBehaviour
{
    [SerializeField] private Material skybox;

    // Update is called once per frame
    void Update()
    {
        skybox.SetVector("_MainLightDirection", transform.forward);
    }
}
