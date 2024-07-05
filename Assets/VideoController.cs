using System;
using System.IO;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class VideoController : MonoBehaviour
{
  public string folderName = "video";
  public string[] videoFiles = new string[20];

  #region Video Control
  [SerializeField] private VideoPlayer videoPlayer;
  [SerializeField] private Button fileExploerButton;

  

  #endregion
  private void Start()
  {
    videoFiles = GetVideoFilesFromDesktopFolder();
    fileExploerButton.onClick.AddListener(OpenFileExplorer);
  }

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
  private void OpenFileExplorer()
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
    }

  }
}
