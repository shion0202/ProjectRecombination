using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStat : MonoBehaviour
{
    
}

// 파츠 변경 시 전?체 변경 (상하체 폼이어도 상체 스탯과 하체 스탯이 공존, 즉 베이스 + 파츠 스탯 구조는 유지되어야 함)
// 파츠 전용 스탯을 캐릭터가 가지고 있는 구조여도 괜찮은가? = 내가 결정해도 되는가?
// 파츠-인벤토리-캐릭터-스탯 구조는 좀 더 생각해보기
// 베이스 스탯의 기준은? = 그냥 싹 다 0 박아놓고 파츠만 반영되어도 되긴 함
// CSV Read 이후에 생성(혹은 초기화) 되어야 함
