using System.Collections.Generic;
using UnityEditor;

namespace Managers
{
    public static class LocalizationManager
    {
        private static Dictionary<string, string> englishTable = new Dictionary<string, string>
        {
            { "환경설정", "Options" },
            { "배경음", "BGM" },
            { "효과음", "SFX" },
            { "언어", "Language" },
            { "카메라 감도 (좌우)", "Camera Sensitivity (X-Axis)" },
            { "카메라 감도 (상하)", "Camera Sensitivity (Y-Axis)" },
            { "HDR", "HDR" },
            { "목표", "Objectives" },
            { "월드 맵", "World Map" },
            { "튜토리얼", "Tutorial" },
            { "키 가이드", "Controls" },
            { "공격", "Attack" },
            { "취소", "Cancel" },
            { "터치하여 닫기", "Tap to close" },
            { "클릭하여 닫기", "Click to close" },
            { "Esc로 닫기", "Press Esc to close" },

            { "기획\n김진윤\n조윤재",
@"Game Design
Kim Jinyun
Cho Yunjae" },
            { "원화\n김성주\n민지홍",
@"Concept Art
Kim Seongju
Min Jihong" },
            { "3D 모델링\n임상섭\n장국경\n정연종",
@"3D Modeling
Im Sangseop
Jang Gukgyeong
Jeong Yeonjong" },
            { "프로그래밍\n정수용\n정재호",
@"Programming
Jeong Suyong
Jeong Jaeho" },
            { "영상 제작\n오은영",
@"Motion Graphics
Oh Eunyeong" },

            { "문 개방", "Open" },
            { "전력 복구", "Restore Power" },
            { "경보 해제", "Deactive Alarm" },
            { "레이저 파츠 획득", "Acquire Laser Parts" },
            { "래피드 파츠 획득", "Acquire Rapid Parts" },
            { "헤비 파츠 획득", "Acquire Heavy Parts" },
            { "회로 변경", "Modify Circuit" },
            { "전류 변경", "Reroute Current" },
            { "승강기 호출", "Call Elevator" },
            { "승강기 가동", "Operate Elevator" },
            { "전류 공급", "Supply Power" },
            { "방벽 해제", "Disable Barrier" }
        };

        public static bool IsKorean = true;
        public static string[] CurrentObjective = new string[2];

        public static string GetLocalizedString(string koreanText)
        {
            if (IsKorean) return koreanText;

            if (englishTable.TryGetValue(koreanText, out string englishText))
            {
                return englishText;
            }
            return koreanText;
        }
    }
}
