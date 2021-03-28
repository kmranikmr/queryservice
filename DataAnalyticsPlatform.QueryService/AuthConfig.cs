/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;

namespace DataAnalyticsPlatform.QueryService
{
    public static class AuthConfig
    {
        public static void ConfigureAuth(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtOptions = configuration.GetSection("JwtOptions");

            var secret = Convert.FromBase64String(jwtOptions["Secret"]);
            var key = new SymmetricSecurityKey(secret);

            services.Configure<JwtOptions>(options =>
            {
                int.TryParse(jwtOptions["Expire"], out var minutes);
                if (minutes > 0)
                {
                    options.ValidFor = TimeSpan.FromMinutes(minutes);
                }

                options.Issuer = jwtOptions["Issuer"];
                options.Audience = jwtOptions["Audience"];
                options.Secret = secret;
                options.SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtOptions["Issuer"];
                configureOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtOptions["Issuer"],
                    ValidAudience = jwtOptions["Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        }
    }
}
