namespace Inkdrop.Api.DTOs.Requests;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
