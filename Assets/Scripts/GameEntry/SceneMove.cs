using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMove : MonoBehaviour
{
    private CharacterController _controller = null;
    // Start is called before the first frame update
    void Start()
    {
        RenderSettings.fog = true;
        ColorUtility.TryParseHtmlString("#35CBDD", out Color fogColor);
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.01f;
        
        _controller = GetComponent<CharacterController>();
    }

    private void FixedUpdate()
    {
        //if (_controller.isGrounded)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 move = new Vector3(horizontal, 0, vertical) * Time.fixedTime;
            _controller.SimpleMove(move);
        }
    }

    private void OnDestroy()
    {
        RenderSettings.fog = false;
    }
}
