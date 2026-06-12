using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class GPTFeedbackManager : MonoBehaviour
{
    [Header("OpenAI API 설정")]
    [Tooltip("OpenAI 플랫폼에서 발급받은 비밀키(sk-...)를 입력해 주세요.")]
    public string apiKey = "AIzaSyAWINRJs2vBUtyYvYLXJjS03FQhKVNVJ9I";
    private readonly string url = "https://api.openai.com/v1/chat/completions";

    void Start()
    {
        // 🚀 [테스트 실행] 올리브영에서 54,300원을 지출한 상황을 가정하여 정밀 분석을 시작합니다.
        // 추후 구글 OCR이 연동되면 이 자리에 추출된 상호명과 금액 변수를 넣어주시면 됩니다.
        GetStrictAnalysis("올리브영", 54300);
    }

    /// <summary>
    /// 외부에서 상호명과 금액을 받아와 GPT 서버에 정밀 분석 보고서를 요청하는 함수입니다.
    /// </summary>
    public void GetStrictAnalysis(string storeName, int amount)
    {
        StartCoroutine(RequestGPT(storeName, amount));
    }

    private IEnumerator RequestGPT(string storeName, int amount)
    {
        // 1. 프롬프트 시스템 설정: 데이터에 기반하여 객관적이고 정확한 진단을 내리도록 페르소나를 정의합니다.
        string systemRole = "당신은 금융 데이터 분석가이자 자산 관리 AI 비서입니다. 유저의 단일 소비 내역을 분석하여 다음 3가지 항목을 정확하게 작성해 주세요. " +
                            "1. 카테고리(식비/교통/패션·뷰티/문화·여가/의료/기타 중 해당되는 항목 하나를 선택), " +
                            "2. 소비 성향 분석(상호명과 금액을 고려하여 이 소비가 가지는 목적과 성격을 요약), " +
                            "3. 금융 제언(대학생 평균 지출 대비 예산 관리 및 절약 방안 리포트). " +
                            "가독성을 위해 줄바꿈(\n)을 적절히 활용하고, 신뢰감 있고 정중한 문체로 팩트에 기반하여 기술해 주세요.";

        string userMessage = $"지출처: {storeName}, 지출 금액: {amount}원. 이 지출 데이터에 대한 정밀 금융 분석 보고서를 생성해 주세요.";

        // 2. OpenAI API 규격에 맞는 JSON 데이터 요청 본문을 조립합니다.
        // gpt-4o-mini 모델은 비용이 가장 저렴하면서도 구조적 텍스트 분석 성능이 매우 뛰어납니다.
        string jsonRequestBody = "{" +
            "\"model\": \"gpt-4o-mini\"," +
            "\"messages\": [" +
                "{\"role\": \"system\", \"content\": \"" + systemRole + "\"}," +
                "{\"role\": \"user\", \"content\": \"" + userMessage + "\"}" +
            "]," +
            "\"temperature\": 0.2" + // 💡 수치를 0.2로 낮추어 AI의 임의적 상상을 제한하고 팩트 중심의 일관된 답변을 유도합니다.
        "}";

        // 3. UnityWebRequest를 사용하여 안전하게 웹 통신을 시작합니다.
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // HTTP 헤더 인증 정보를 세팅합니다.
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            Debug.Log($"📡 [인공지능 분석 중] 구글 OCR 데이터 통신을 가상하여 GPT 서버로 분석을 요청합니다. (지출처: {storeName})");

            // AI 서버가 연산을 마치고 답변을 줄 때까지 유니티 메인 루프를 방해하지 않고 비동기로 대기합니다.
            yield return request.SendWebRequest();

            // 4. 통신 결과 처리 단원입니다.
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"❌ [분석 실패] 통신 중 오류가 발생했습니다: {request.error}\n상세 에러 내용: {request.downloadHandler.text}");
            }
            else
            {
                string responseText = request.downloadHandler.text;

                // 받아온 원본 JSON 덩어리에서 필요한 답변 텍스트만 깔끔하게 정제합니다.
                string cleanReport = ParseGPTResponse(responseText);

                // 최종 정밀 금융 리포트를 유니티 콘솔창에 노란색 경고 로그로 가독성 높게 출력합니다.
                Debug.LogWarning($"📊 [영차! AI 금융 분석 보고서 수신 완료]\n{cleanReport}");
            }
        }
    }

    /// <summary>
    /// OpenAI가 반환한 복잡한 응답 JSON에서 content 본문 텍스트만 안전하게 추출하는 파서 함수입니다.
    /// </summary>
    private string ParseGPTResponse(string json)
    {
        try
        {
            int contentIndex = json.IndexOf("\"content\": \"") + 12;
            int endIndex = json.IndexOf("\"", contentIndex);
            string result = json.Substring(contentIndex, endIndex - contentIndex);
            return RegexUnescape(result);
        }
        catch
        {
            return "응답 데이터를 가공하는 과정에서 알 수 없는 포맷 오류가 발생했습니다.";
        }
    }

    private string RegexUnescape(string str)
    {
        return Regex.Unescape(str);
    }
}