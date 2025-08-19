namespace WorkerService1.Api.Contracts;

// 로그인 성공 시 클라이언트에게 반환할 데이터 모델입니다.
// 발급된 JWT 토큰을 포함합니다.
public record LoginResponse(string Token);