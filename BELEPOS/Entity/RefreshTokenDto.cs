using System;

using System.ComponentModel.DataAnnotations;
namespace BELEPOS.Entity.Auth
{
    public class RefreshTokenDto
    {
        public string Token { get; set; } = default!;
    }
}
