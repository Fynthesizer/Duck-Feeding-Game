using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ThumbnailCamera : MonoBehaviour
{
    [SerializeField] private string outputName;
    [SerializeField] private Vector2Int outputDimensions;

    private Camera camera;

    private Texture2D texture;

    void Start()
    {
        camera = gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Capture")]
    public void Capture()
    {
        texture = new Texture2D(outputDimensions.x, outputDimensions.y);
        //texture.ReadPixels(new Rect(0, 0, ))
    }
}
