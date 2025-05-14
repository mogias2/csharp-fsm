# csharp-fsm
1. 개요
- FSM을 C# 으로 구현

2. 빌드 환경
- Microsoft Visual Studio Community 2022 Version 17.14.0

3. 디렉토리 구조
- src : fsm 관련 소스파일

4. 데이터 설정
- 상태는 3가지 행동 양식(Enter, Update, Exit) 을 가지고, 각각의 행동 양식에 적합한 내용을 기술.
- 특정 입력의 전이에 대한 규칙을 정함.
- FSMTest.cs의 Init 참고

5. 결과
- FSMTest.cs의 Start 참고
- idle -> battle -> die -> climb -> idle -> swim -> down(5초 대기) -> end

* end 상태가 될 때까지 Update 함수에서 무한 루프 (테스트용으로 Sleep으로 1초 마다 실행)
* Update는 일정 시간 후 상태 전이를 위한 함수