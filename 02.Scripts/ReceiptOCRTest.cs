using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ReceiptOCRTest : MonoBehaviour
{
    [Header("구글 API 세팅")]
    public string apiKey = "AIzaSyAWINRJs2vBUtyYvYLXJjS03FQhKVNVJ9I";

    [Header("테스트용 영수증 이미지")]
    public string testImageFileName = "receipt.jpg";

    void Start()
    {
        // 1. 앱이 켜지면 코루틴(비동기 함수)을 통해 구글 서버에 요청을 보냅니다.
        StartCoroutine(RequestOCR());
    }

    IEnumerator RequestOCR()
    {
        // ==========================================
        // 세부단계 A: 내 컴퓨터에서 영수증 이미지 파일 로드하기
        // ==========================================
        // Application.streamingAssetsPath는 유니티가 외부 파일을 가공하지 않고 원본 그대로 보관하는 특수 폴더 경로입니다.
        string imagePath = Path.Combine(Application.streamingAssetsPath, testImageFileName);

        if (!File.Exists(imagePath))
        {
            Debug.LogError($"💥 [파일 에러] {imagePath} 경로에 파일이 없습니다! 3단계를 다시 확인해 주세요.");
            yield break;
        }

        // 이미지 파일을 바이트 배열(컴퓨터가 읽는 이진 데이터)로 읽어옵니다.
        byte[] imageBytes = File.ReadAllBytes(imagePath);

        // ==========================================
        // 세부단계 B: 이미지를 '문자열'로 압축하기 (Base64 인코딩)
        // ==========================================
        // 구글 서버는 우리가 이미지 파일 자체를 보내는 걸 이해하지 못합니다. 
        // 그래서 이미지를 텍스트 형태의 문자열(Base64)로 변환해서 JSON 문서에 인쇄하듯 집어넣어야 합니다.
        string base64Image = Convert.ToBase64String(imageBytes);

        // ==========================================
        // 세부단계 C: 구글 요청 규격(JSON 포맷) 조립하기
        // ==========================================
        // 구글 클라우드 비전 API가 약속한 데이터 양식입니다. 
        // "content" 뒤에 방금 우리가 변환한 이미지 문자열을 넣고, "type"에는 텍스트를 추출하겠다는 뜻의 "TEXT_DETECTION"을 선언합니다.
        string jsonRequest = "{\"requests\":[{\"image\":{\"content\":\"" + base64Image + "\"},\"features\":[{\"type\":\"TEXT_DETECTION\"}]}]}";

        // ==========================================
        // 세부단계 D: 구글 AI 서버 주소(URL) 설정하기
        // ==========================================
        // 구글 비전 API의 고유 엔드포인트 주소입니다. 맨 뒤에 님이 발급받은 apiKey를 매개변수로 붙여서 통신합니다.
        string url = $"https://vision.googleapis.com/v1/images:annotate?key={apiKey}";

        // ==========================================
        // 세부단계 E: 유니티 순정 기능으로 인터넷 통신 요청(POST)하기
        // ==========================================
        // UnityWebRequest를 사용해 구글 서버 주소로 웹 데이터 전송을 준비합니다.
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            // 위에서 조립한 JSON 요청(텍스트)을 바이트로 변환해 전송 바디(Body)에 담습니다.
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonRequest);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // "우리가 보내는 데이터는 일반 텍스트가 아니라 JSON 문서야!"라고 헤더에 알려줍니다.
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("📡 [통신 시작] 구글 AI 서버로 영수증을 보내는 중입니다...");

            // 구글 서버가 분석해서 응답을 줄 때까지 유니티 화면이 멈추지 않고 비동기로 대기(yield return)합니다.
            yield return request.SendWebRequest();

            // ==========================================
            // 세부단계 F: 구글 서버의 응답 결과 확인하기
            // ==========================================
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                // 인터넷 연결이 끊겼거나, API 키가 잘못되었을 때 타는 에러 코드 블록
                Debug.LogError($"❌ [OCR 요청 실패] 에러 원인: {request.error}\n구글 응답 메시지: {request.downloadHandler.text}");
            }
            else
            {
                // 통신 성공! 구글이 텍스트 분석 결과를 담아 보낸 JSON 본문을 변수에 담습니다.
                string rawJsonResult = request.downloadHandler.text;

                Debug.Log("🎉 [OCR 요청 성공] 구글 서버로부터 데이터를 무사히 수신했습니다!");
                Debug.Log($"📄 [구글 OCR 원본 데이터 콘솔 출력]:\n{rawJsonResult}");
            }
        }
    }
}