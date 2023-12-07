using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTestController : MonoBehaviour
{
    public string sceneToLoad;

    public int delaySeconds;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DelayHostStart());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator DelayHostStart()
    {
        yield return new WaitForSeconds(delaySeconds);

        NetworkManager.Instance.Stop();

        NetworkManager.Instance.StartHost(696, 1, sceneToLoad);
    }

    private void OnApplicationQuit()
    {
        NetworkManager.Instance.Stop();
    }
}