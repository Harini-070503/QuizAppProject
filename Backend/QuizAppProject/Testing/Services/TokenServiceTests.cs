using Microsoft.Extensions.Configuration;
using QuizAppProject.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Testing.Services
{
    public class TokenServiceTests
    {
        private static TokenService BuildService(
            string key    = "SuperSecretTestKey1234567890ABCDEF",
            string issuer = "TestIssuer",
            string aud    = "TestAudience",
            string dur    = "60",
            string reset  = "15")
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"]                  = key,
                    ["Jwt:Issuer"]               = issuer,
                    ["Jwt:Audience"]             = aud,
                    ["Jwt:DurationInMinutes"]    = dur,
                    ["Jwt:PasswordResetMinutes"] = reset
                })
                .Build();

            return new TokenService(config);
        }

        private static JwtSecurityToken Decode(string token)
            => new JwtSecurityTokenHandler().ReadJwtToken(token);

        // ── GenerateToken ─────────────────────────────────────────────────────

        [Fact]
        public void GenerateToken_ValidInputs_ReturnsNonEmptyToken()
        {
            var svc    = BuildService();
            var userId = Guid.NewGuid();

            var token = svc.GenerateToken(userId, "alice", "alice@test.com", "Taker");

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void GenerateToken_ContainsCorrectClaims()
        {
            var svc    = BuildService();
            var userId = Guid.NewGuid();

            var raw = svc.GenerateToken(userId, "alice", "alice@test.com", "Taker");
            var jwt = Decode(raw);

            Assert.Equal(userId.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal("alice",           jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value);
            Assert.Equal("alice@test.com",  jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
            Assert.Equal("Taker",           jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }

        [Fact]
        public void GenerateToken_ExpiresAfterConfiguredDuration()
        {
            var svc = BuildService(dur: "30");
            var raw = svc.GenerateToken(Guid.NewGuid(), "bob", "bob@test.com", "Creator");
            var jwt = Decode(raw);

            var expectedExpiry = DateTime.UtcNow.AddMinutes(30);
            Assert.True(jwt.ValidTo <= expectedExpiry.AddSeconds(10));
            Assert.True(jwt.ValidTo >= expectedExpiry.AddSeconds(-10));
        }

        [Fact]
        public void GenerateToken_HasCorrectIssuerAndAudience()
        {
            var svc = BuildService(issuer: "MyIssuer", aud: "MyAudience");
            var raw = svc.GenerateToken(Guid.NewGuid(), "u", "u@t.com", "Taker");
            var jwt = Decode(raw);

            Assert.Equal("MyIssuer",   jwt.Issuer);
            Assert.Contains("MyAudience", jwt.Audiences);
        }

        [Fact]
        public void GenerateToken_MissingKey_ThrowsInvalidOperation()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"]               = "",
                    ["Jwt:Issuer"]            = "I",
                    ["Jwt:Audience"]          = "A",
                    ["Jwt:DurationInMinutes"] = "60"
                })
                .Build();

            var svc = new TokenService(config);

            Assert.Throws<InvalidOperationException>(() =>
                svc.GenerateToken(Guid.NewGuid(), "u", "u@t.com", "Taker"));
        }

        [Fact]
        public void GenerateToken_InvalidDurationFallsBackTo60Minutes()
        {
            var svc = BuildService(dur: "not-a-number");
            var raw = svc.GenerateToken(Guid.NewGuid(), "u", "u@t.com", "Taker");
            var jwt = Decode(raw);

            var expected = DateTime.UtcNow.AddMinutes(60);
            Assert.True(jwt.ValidTo <= expected.AddSeconds(10));
            Assert.True(jwt.ValidTo >= expected.AddSeconds(-10));
        }

        // ── GeneratePasswordResetToken ────────────────────────────────────────

        [Fact]
        public void GeneratePasswordResetToken_ReturnsNonEmptyToken()
        {
            var svc   = BuildService();
            var token = svc.GeneratePasswordResetToken(Guid.NewGuid(), "alice");

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void GeneratePasswordResetToken_ContainsPurposeClaim()
        {
            var svc = BuildService();
            var raw = svc.GeneratePasswordResetToken(Guid.NewGuid(), "alice");
            var jwt = Decode(raw);

            var purpose = jwt.Claims.FirstOrDefault(c => c.Type == "purpose")?.Value;
            Assert.Equal("password_reset", purpose);
        }

        [Fact]
        public void GeneratePasswordResetToken_ExpiresAfterConfiguredMinutes()
        {
            var svc = BuildService(reset: "10");
            var raw = svc.GeneratePasswordResetToken(Guid.NewGuid(), "alice");
            var jwt = Decode(raw);

            var expected = DateTime.UtcNow.AddMinutes(10);
            Assert.True(jwt.ValidTo <= expected.AddSeconds(10));
            Assert.True(jwt.ValidTo >= expected.AddSeconds(-10));
        }

        [Fact]
        public void GeneratePasswordResetToken_CustomLifetime_Overrides()
        {
            var svc = BuildService(reset: "15");
            var raw = svc.GeneratePasswordResetToken(Guid.NewGuid(), "alice", TimeSpan.FromMinutes(5));
            var jwt = Decode(raw);

            var expected = DateTime.UtcNow.AddMinutes(5);
            Assert.True(jwt.ValidTo <= expected.AddSeconds(10));
            Assert.True(jwt.ValidTo >= expected.AddSeconds(-10));
        }

        // ── ValidatePasswordResetToken ────────────────────────────────────────

        [Fact]
        public void ValidatePasswordResetToken_ValidToken_ReturnsPrincipal()
        {
            var svc    = BuildService();
            var userId = Guid.NewGuid();
            var raw    = svc.GeneratePasswordResetToken(userId, "alice");

            var principal = svc.ValidatePasswordResetToken(raw);

            Assert.NotNull(principal);
            var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Assert.Equal(userId.ToString(), id);
        }

        [Fact]
        public void ValidatePasswordResetToken_WrongPurpose_ThrowsUnauthorized()
        {
            var svc = BuildService();
            // Generate a regular auth token (no purpose=password_reset claim)
            var raw = svc.GenerateToken(Guid.NewGuid(), "alice", "alice@test.com", "Taker");

            Assert.Throws<UnauthorizedAccessException>(() =>
                svc.ValidatePasswordResetToken(raw));
        }

        [Fact]
        public void ValidatePasswordResetToken_ExpiredToken_ThrowsSecurityException()
        {
            var svc = BuildService();
            // Generate a token that expires in -1 minutes (already expired)
            var raw = svc.GeneratePasswordResetToken(Guid.NewGuid(), "alice", TimeSpan.FromMinutes(-1));

            Assert.ThrowsAny<Exception>(() =>
                svc.ValidatePasswordResetToken(raw, validateLifetime: true));
        }

        [Fact]
        public void ValidatePasswordResetToken_ExpiredToken_SkipLifetime_Succeeds()
        {
            var svc = BuildService();
            var raw = svc.GeneratePasswordResetToken(Guid.NewGuid(), "alice", TimeSpan.FromMinutes(-1));

            // Should NOT throw when validateLifetime = false
            var principal = svc.ValidatePasswordResetToken(raw, validateLifetime: false);
            Assert.NotNull(principal);
        }

        [Fact]
        public void ValidatePasswordResetToken_TamperedToken_ThrowsException()
        {
            var svc = BuildService();
            var raw = svc.GeneratePasswordResetToken(Guid.NewGuid(), "alice");
            var tampered = raw[..^5] + "XXXXX"; // corrupt last 5 chars

            Assert.ThrowsAny<Exception>(() =>
                svc.ValidatePasswordResetToken(tampered));
        }
    }
}
