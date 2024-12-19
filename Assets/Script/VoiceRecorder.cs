using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using Samples.Whisper;
using OpenAI;

public class VoiceRecorder : MonoBehaviour
{
    private string serverUrl = "http://127.0.0.1:5001/upload_text";
    private AudioClip recordedClip;
    private bool isRecording = false;
    private readonly int recordDuration = 10; // 최대 녹음 시간 (초)
    private string resText = ""; // 변환된 텍스트 저장
    private string selectedDevice; // 선택된 오디오 장치

    private void Start()
    {
        // 사용 가능한 오디오 장치 확인 및 기본 장치 설정
        InitializeAudioDevice();
    }
    private void InitializeAudioDevice()
    {
        if (Microphone.devices.Length > 0)
        {
            // 첫 번째 오디오 장치를 선택
            selectedDevice = Microphone.devices[0];
            Debug.Log("Default audio device selected: " + selectedDevice);

            // PlayerPrefs에 저장 (다음 실행 시 유지)
            PlayerPrefs.SetString("default-audio-device", selectedDevice);
        }
        else
        {
            Debug.LogError("No audio devices available!");
        }
    }
    public void OnVoice(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // V 키를 누르기 시작했을 때 녹음 시작
            StartRecording();
        }
        else if (context.canceled)
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            Debug.Log("Recording started...");
            recordedClip = Microphone.Start(null, false, recordDuration, 44100);
        }
    }

    private async void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            Debug.Log("Recording stopped.");

            // 녹음 종료
            Microphone.End(null);

            // WAV 데이터 생성
            byte[] wavData = SaveWav.Save("recording.wav", recordedClip);

            // OpenAI Whisper API로 변환 요청
            var openai = new OpenAIApi();
            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData { Data = wavData, Name = "audio.wav" },
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);
            resText = res.Text;

            Debug.Log("Transcription result: " + resText);

            // 서버로 텍스트 전송
            StartCoroutine(SendTextToServer(resText));
        }
    }

    private IEnumerator SendTextToServer(string text)
    {
        string jsonData = JsonUtility.ToJson(new { transcribed_text = text });
        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Text sent successfully to server!");
            }
            else
            {
                Debug.LogError("Failed to send text to server: " + request.error);
            }
        }
    }
}