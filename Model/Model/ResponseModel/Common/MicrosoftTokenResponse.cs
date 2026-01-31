namespace Model.ResponseModel.Common;

public class MicrosoftTokenResponse
{
    public string access_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string scope { get; set; }
}

public class MicrosoftAccessTokenPayload
{
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Scope { get; set; }
    public string Name { get; set; }
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
    public string Email { get; set; }
    public string ObjectId { get; set; }
    public string TenantId { get; set; }
    public string IpAddress { get; set; }
}