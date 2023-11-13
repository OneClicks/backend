using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace backend.Extensions
{
    public static class JWTServiceExtension
    {
        public static IServiceCollection AddJWTServices(this IServiceCollection services, IConfiguration configuration)
        {
            var key = "super secret unguessable key MYname is umer faroooooooooooooooooooooooooooooooooq";

            // Adding Authentication JWT for Session
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
            options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("ApiKeyPolicy", policy =>
            //    {
            //        policy.AddAuthenticationSchemes(new[] { JwtBearerDefaults.AuthenticationScheme });
            //        policy.Requirements.Add(new ApiKeyRequirement());
            //    });
            //});
            //services.AddScoped<IAuthorizationHandler, ApiKeyHandler>();

            return services;
        }
    }
}
