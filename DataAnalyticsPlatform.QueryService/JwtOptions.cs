/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Microsoft.IdentityModel.Tokens;
using System;

namespace DataAnalyticsPlatform.QueryService
{
    public class JwtOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Subject { get; set; }
        public byte[] Secret { get; set; }
        public DateTime IssuedAt => DateTime.UtcNow;
        public TimeSpan ValidFor { get; set; } = TimeSpan.FromMinutes(600);
        public DateTime NotBefore => DateTime.UtcNow;
        public DateTime Expiration => IssuedAt.Add(ValidFor);
        public SigningCredentials SigningCredentials { get; set; }
    }
}
