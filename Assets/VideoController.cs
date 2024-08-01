using System;
using System.IO;
using System.Windows.Forms;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using UnityEngine.UI;
using Application = UnityEngine.Application;
using Button = UnityEngine.UI.Button;

public class VideoController : MonoBehaviour
{
  public const string folderName = "video";
  public string[] videoFiles = new string[10];

  #region Video Control
  [SerializeField] private VideoPlayer videoPlayer;
  [SerializeField] private Button fileExploerButton;
  [SerializeField] private TextMeshProUGUI timeText;
  [SerializeField] private TextMeshProUGUI totalTimeText;
  public Slider timeSlider;       // 재생 시간을 조절할 슬라이더
    public Button forwardButton;    // 5초 앞으로 이동 버튼
    public Button backwardButton;   // 5초 뒤로 이동 버튼

    private bool isPointerOverSlider = false; // 슬라이더에 마우스가 올라갔는지 확인

    void InitVideoUI()
    {
      Application.targetFrameRate = 60;
      
      // 비디오가 준비될 때 슬라이더의 최소값과 최대값을 설정합니다.
      videoPlayer.prepareCompleted += OnVideoPrepared;
      videoPlayer.Prepare();
      // 이벤트 리스너를 등록합니다.
      timeSlider.onValueChanged.AddListener(OnSliderValueChanged);
      forwardButton.onClick.AddListener(Forward);
      backwardButton.onClick.AddListener(Backward);

      // 슬라이더에 마우스 이벤트 핸들러를 추가합니다.
      EventTrigger trigger = timeSlider.gameObject.AddComponent<EventTrigger>();
        
      EventTrigger.Entry entryEnter = new EventTrigger.Entry();
      entryEnter.eventID = EventTriggerType.PointerEnter;
      entryEnter.callback.AddListener((eventData) => { OnPointerEnter(); });
      trigger.triggers.Add(entryEnter);

      EventTrigger.Entry entryExit = new EventTrigger.Entry();
      entryExit.eventID = EventTriggerType.PointerExit;
      entryExit.callback.AddListener((eventData) => { OnPointerExit(); });
      trigger.triggers.Add(entryExit);
    }
    void FixedUpdate()
    {
        if (!isPointerOverSlider)
        {
            // 슬라이더의 값을 비디오 플레이어의 현재 시간으로 설정합니다.
            timeSlider.value = (float)videoPlayer.time;
            ShowCurrentVideoTime();
        }
    }
    void OnVideoPrepared(VideoPlayer vp)
    {
      // 비디오가 준비되었을 때 슬라이더의 최소값과 최대값을 설정합니다.
      timeSlider.minValue = 0;
      timeSlider.maxValue = (float)videoPlayer.length;
      TimeSpan time = TimeSpan.FromSeconds(videoPlayer.length); 
      totalTimeText.text = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds); // "분:초" 형식으로 문자열을 포맷하고 timeText 텍스트 설정
    }
    public void OnSliderValueChanged(float value)
    {
        if (isPointerOverSlider)
        {
            // 마우스가 슬라이더 위에 있는 경우에만 비디오 플레이어의 시간을 변경합니다.
            videoPlayer.time = value;
            ShowCurrentVideoTime();
        }
    }

    void OnPointerEnter()
    {
        isPointerOverSlider = true;
    }

    void OnPointerExit()
    {
        isPointerOverSlider = false;
    }

    void Forward()
    {
        // 5초 앞으로 이동합니다.
        videoPlayer.time += 5;
        // 슬라이더 값도 업데이트 합니다.
        timeSlider.value = (float)videoPlayer.time;
    }

    void Backward()
    {
        // 5초 뒤로 이동합니다.
        videoPlayer.time -= 5;
        if (videoPlayer.time < 0)
        {
            videoPlayer.time = 0;
        }
        // 슬라이더 값도 업데이트 합니다.
        timeSlider.value = (float)videoPlayer.time;
    }

    void ShowCurrentVideoTime()
    {
      // 비디오 플레이어의 현재 시간을 TimeSpan으로 변환
      TimeSpan time = TimeSpan.FromSeconds(videoPlayer.time); // 비디오 플레이어의 현재 시간을 TimeSpan 객체로 변환

      // TimeSpan을 "분:초" 형식으로 변환하여 텍스트로 설정
      timeText.text = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds); // "분:초" 형식으로 문자열을 포맷하고 timeText 텍스트 설정
    }

  #endregion
  private void Start()
  {
    videoFiles = GetVideoFilesFromDesktopFolder();
    fileExploerButton.onClick.AddListener(OpenFileExplorer);
    InitVideoUI();
  }

  #region Video File Managing
  // 현재 사용자의 바탕화면 경로를 반환
  public string GetDesktopPath()
  {
    return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
  } 
  
  // 바탕화면의 지정된 폴더에서 비디오 파일들의 이름을 배열로 반환
  public string[] GetVideoFilesFromDesktopFolder()
  {
    string desktopPath = GetDesktopPath();
    string videoFolderPath = Path.Combine(desktopPath, folderName);

    if (Directory.Exists(videoFolderPath))
    {
      // 비디오 파일 확장자를 지정하여 파일들을 가져옴.
      string[] videoFiles = Directory.GetFiles(videoFolderPath, "*.mp4");
      for (int i = 0; i < videoFiles.Length; i++)
      {
        videoFiles[i] = Path.GetFileName(videoFiles[i]);
      }

      return videoFiles;
    }
    else
    {
      Debug.LogWarning($"The folder {videoFolderPath} does not exist.");
      return new string[0];
    }
  }
  
  // 재생할 영상을 선택함
  // public void Play
  
  // 파일 탐색기 열기
  public void OpenFileExplorer()
  {
    OpenFileDialog openFileDialog = new OpenFileDialog();

    string desktopPath = GetDesktopPath();
    string videoFolderPath = Path.Combine(desktopPath, "video");
    
    openFileDialog.InitialDirectory = videoFolderPath;
    openFileDialog.Filter = "Videp Files|*.mp4;*.avi;*.mov";
    openFileDialog.RestoreDirectory = true;

    if (openFileDialog.ShowDialog() == DialogResult.OK)
    {
      // 선택된 파일 경로
      string selectedFilePath = openFileDialog.FileName;
      Debug.Log("Selected file: " + selectedFilePath);
      
      // VideoPlayer에 파일 경로 설정
      videoPlayer.url = "file://" + selectedFilePath;
      videoPlayer.Play();
      videoPlayer.Pause();
    }

  }
  #endregion
}
