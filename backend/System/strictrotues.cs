using Microsoft.AspNetCore.Mvc;
using System;

namespace StudyCenter.System {

    public static class HttpRequestExtensions
    {

        public static string GetBearerToken(this HttpRequest request) 
        {
            var authHeader = request.Headers["Authorization"].ToString();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        return null;
        
        }

    }
}