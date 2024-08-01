using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using Application = UnityEngine.Application;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

public class OptimizedVideoPlayer : MonoBehaviour
{
    public const string folderName = "video";
    public string[] videoFiles = new string[10];

    public VideoPlayer videoPlayer;
    public AudioSource audioSource;
    private Queue<VideoFrame> frameBuffer = new Queue<VideoFrame>();
    public bool isBuffering = true;
    public bool isPlaying = false;
    private int bufferSize = 20;

    [Space] public int testBufferCount;

    private void Update()
    {
        testBufferCount = frameBuffer.Count;
    }

    // ====================
    // =========UI=========
    // ====================
    [SerializeField] private UnityEngine.UI.Button fileExploerButton;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI totalTimeText;

    public Slider timeSlider; // 재생 시간을 조절할 슬라이더
    private bool isPointerOverSlider = false; // 슬라이더에 마우스가 올라갔는지 확인
    private long lastEnqueuedFrame = -1; // 마지막으로 Enqueue한 프레임 번호를 추적

    void Start()
    {
        Application.targetFrameRate = 60;
        bufferSize = 100;
        // VideoPlayer 설정
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        InitializeVideoFile();
        InitVideoUI();

        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    void InitializeVideoFile()
    {
        videoFiles = GetVideoFilesFromDesktopFolder();
        fileExploerButton.onClick.AddListener(OpenFileExplorer);
    }

    void InitVideoUI()
    {
        // 이벤트 리스너를 등록합니다.
        timeSlider.onValueChanged.AddListener(OnSliderValueChanged);

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

    public void StartVideoRoutine()
    {
        Debug.Log("Video Routine start");
        StartCoroutine(PrepareVideo());
    }

    IEnumerator PrepareVideo()
    {
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        isBuffering = true;

        // 비디오 프레임을 버퍼링하는 코루틴 시작
        StartCoroutine(BufferFrames());

        // 프레임 버퍼가 최소 50프레임을 가질 때까지 대기
        yield return new WaitUntil(() => frameBuffer.Count >= 50);

        // 비디오 재생 코루틴 시작
        StartCoroutine(PlayBufferedVideo());
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        // 슬라이더: 비디오가 준비되었을 때 슬라이더의 최소값과 최대값을 설정합니다.
        timeSlider.minValue = 0;
        timeSlider.maxValue = (float)videoPlayer.length;
        TimeSpan time = TimeSpan.FromSeconds(videoPlayer.length);

        // "분:초" 형식으로 문자열을 포맷하고 totalTimeText 텍스트 설정
        totalTimeText.text = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
        Debug.Log("Video is prepared and ready to play.");
    }

    IEnumerator BufferFrames()
    {
        Debug.Log($"Buffer Frame Start : isBuffering {isBuffering}");

        while (isBuffering)
        {
            // 현재 프레임이 마지막으로 Enqueue된 프레임과 다른 경우에만 Enqueue
            if (frameBuffer.Count < bufferSize && videoPlayer.frame != lastEnqueuedFrame)
            {
                Debug.Log($"Frame Enqueue: {videoPlayer.frame}");
                frameBuffer.Enqueue(new VideoFrame(videoPlayer.frame));
                lastEnqueuedFrame = videoPlayer.frame;

                // 다음 프레임으로 진행
                videoPlayer.StepForward();

                // 프레임 지속 시간 대기
                yield return new WaitForSeconds((float)1 / videoPlayer.frameRate);
            }
            else
            {
                yield return null;
            }
        }

        Debug.Log("Buffer Frame End");
    }

    IEnumerator PlayBufferedVideo()
    {
        while (isBuffering)
        {
            // 프레임 버퍼에 충분한 프레임이 있는 경우 재생
            if (frameBuffer.Count > 0)
            {
                VideoFrame frame = frameBuffer.Dequeue();
                Debug.Log($"Frame Dequeue: {frame.frameNumber}");
                videoPlayer.frame = frame.frameNumber;
                videoPlayer.Play();

                // 프레임 재생 대기
                yield return new WaitForSeconds((float)1 / videoPlayer.frameRate);
            }
            else
            {
                Debug.Log("Frame Buffer is empty.");
                // 프레임 버퍼가 비어있을 때 대기
                yield return null;
            }
        }

        if (audioSource != null)
        {
            audioSource.Play();
            Debug.Log("Audio is playing");
        }

        Debug.Log("Play Buffered Video End");
    }

    public void StopVideo()
    {
        isBuffering = false;
        isPlaying = false;
        videoPlayer.Pause();
        audioSource.Pause();
        frameBuffer.Clear();
    }

    public void SkipTime(float time)
    {
        videoPlayer.time += time;

        // 비디오와 오디오를 동기화
        videoPlayer.Pause();
        audioSource.Pause();
        frameBuffer.Clear();

        // 시간 이동 후 버퍼링 다시 시작
        StartCoroutine(PrepareVideo());
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

    void ShowCurrentVideoTime()
    {
        // 비디오 플레이어의 현재 시간을 TimeSpan으로 변환
        TimeSpan time = TimeSpan.FromSeconds(videoPlayer.time);

        // TimeSpan을 "분:초" 형식으로 변환하여 텍스트로 설정
        timeText.text = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
    }

    void OnPointerEnter()
    {
        isPointerOverSlider = true;
    }

    void OnPointerExit()
    {
        isPointerOverSlider = false;
    }

    #region Video File Managing

    public string GetDesktopPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    public string[] GetVideoFilesFromDesktopFolder()
    {
        string desktopPath = GetDesktopPath();
        string videoFolderPath = Path.Combine(desktopPath, folderName);

        if (Directory.Exists(videoFolderPath))
        {
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


    public void OpenFileExplorer()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();

        string desktopPath = GetDesktopPath();
        string videoFolderPath = Path.Combine(desktopPath, folderName);

        openFileDialog.InitialDirectory = videoFolderPath;
        openFileDialog.Filter = "Video Files|*.mp4;*.avi;*.mov";
        openFileDialog.RestoreDirectory = true;

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            string selectedFilePath = openFileDialog.FileName;
            Debug.Log("Selected file: " + selectedFilePath);

            videoPlayer.url = "file://" + selectedFilePath;
            videoPlayer.Play();
            videoPlayer.Pause();
        }
    }

    #endregion
}

public class VideoFrame
{
    public long frameNumber;

    public VideoFrame(long frameNumber)
    {
        this.frameNumber = frameNumber;
    }
}
