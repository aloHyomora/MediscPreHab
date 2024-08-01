using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WebcamPlayer : MonoBehaviour
{
  public RawImage webcamRawImage;
  private WebCamTexture _webCamTexture;
  public TextMeshProUGUI text;

  public int webcamWidth;
  public int webcamHeight;
  private void Awake()
  {
    InitWebcam();
  }

  void InitWebcam()
  {
    ResetWebCam();
    
    WebCamDevice device = WebCamTexture.devices[0];
    _webCamTexture = new WebCamTexture(device.name,640,360);
    
    /*Debug.Log(webcamRawImage.texture.height);
    Debug.Log(webcamRawImage.texture.width);*/
    /*Debug.Log(  "[R]width "+_webCamTexture.requestedWidth+" hegith" + _webCamTexture.requestedHeight);
    Debug.Log(  "width "+_webCamTexture.autoFocusPoint+" hegith " + _webCamTexture.mipmapCount);

    Debug.Log(  _webCamTexture.texelSize);*/

    //Debug.Log(  "width "+_webCamTexture. +" hegith " + _webCamTexture.height);
    Debug.Log(  "width "+_webCamTexture.width+" hegith " + _webCamTexture.height);
    
    webcamRawImage.texture = _webCamTexture;
    
    _webCamTexture.Play();
    webcamWidth = _webCamTexture.width;
    webcamHeight = _webCamTexture.height;

    Debug.Log(  "[W]width "+_webCamTexture.width+" hegith " + _webCamTexture.height);
    Debug.Log(  "[I]width "+ webcamRawImage.texture.width+" hegith " + webcamRawImage.texture.height);
    
    //text.text = $"{webcamRawImage.texture.width}x{webcamRawImage.texture.height}";
    //ResetWebCam();
    


    RectTransform webcamRectTransform = webcamRawImage.GetComponent<RectTransform>();
    webcamRectTransform.sizeDelta = new Vector2(1920, 1920 * webcamRawImage.texture.height / webcamRawImage.texture.width);
    Debug.Log(1920 * webcamRawImage.texture.height / webcamRawImage.texture.width);
    
    // webcamRectTransform.sizeDelta = new Vector2(webcamWidth, webcamHeight);  // .rect.width = new Rect(webcamRawImage.texture.width, webcamRawImage.texture.height);
    // Debug.Log(webcamRectTransform.sizeDelta);
    // 1920x1080 -> 1920x1200
    // 1944 * 1920 / 2592
  }
  // 카메라 해상도의 비율을 16:9 비율에 맞게 적용
  private void ResizeWebcam()
  {
    
  }
  private void ResetWebCam()
  {
    if (_webCamTexture != null)
    {
      webcamRawImage.texture = null;
      _webCamTexture.Stop();
      _webCamTexture = null;
    }
  }
}
