namespace WorkerService1.Api.Contracts;

// 로그인 요청 시 클라이언트가 보낼 데이터 모델입니다.
// C# 9.0의 레코드를 사용하여 간결하고 불변적인 DTO를 정의합니다.
public record LoginRequest(string Username, string Password);