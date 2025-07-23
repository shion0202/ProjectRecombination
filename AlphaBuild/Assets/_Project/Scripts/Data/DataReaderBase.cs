using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 구글 스프레드시트로부터 데이터를 읽어오기 위한 베이스 클래스
public abstract class DataReaderBase : ScriptableObject
{
    [Header("시트 주소"), Tooltip("스프레드시트의 주소 링크\n\'d/\'의 뒤부터 \'/edit\'의 앞까지의 주소 값을 입력한다.")]
    [SerializeField] public string associatedSheet = "";

    [Header("스프레드 시트 이름"), Tooltip("문서 하단에 위치한 불러올 시트의 이름")]
    [SerializeField] public string associatedWorksheet = "";

    [Header("읽기 시작할 행 번호")]
    [SerializeField] public int START_ROW_LENGTH = 2;

    [Header("읽을 마지막 행 번호")]
    [SerializeField] public int END_ROW_LENGTH = -1;
}
