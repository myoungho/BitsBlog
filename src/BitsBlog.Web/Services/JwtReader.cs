using System;
using System.Text;
using System.Text.Json;

namespace BitsBlog.Web.Services
{
    public static class JwtReader
    {
        public static string? TryGetDisplayName(string? jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt)) return null;
            try
            {
                var parts = jwt.Split('.');
                if (parts.Length < 2) return null;
                var payload = parts[1];
                var json = Encoding.UTF8.GetString(Base64UrlDecode(payload));
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Common keys for display name
                string[] keys = new[]
                {
                    "name",
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
                    "unique_name"
                };
                foreach (var k in keys)
                {
                    if (root.TryGetProperty(k, out var val) && val.ValueKind == JsonValueKind.String)
                    {
                        var s = val.GetString();
                        if (!string.IsNullOrWhiteSpace(s)) return s;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static string? TryGetRole(string? jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt)) return null;
            try
            {
                var parts = jwt.Split('.');
                if (parts.Length < 2) return null;
                var payload = parts[1];
                var json = Encoding.UTF8.GetString(Base64UrlDecode(payload));
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                string[] keys = new[]
                {
                    "role",
                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role"
                };
                foreach (var k in keys)
                {
                    if (root.TryGetProperty(k, out var val) && val.ValueKind == JsonValueKind.String)
                    {
                        var s = val.GetString();
                        if (!string.IsNullOrWhiteSpace(s)) return s;
                    }
                }
                return null;
            }
            catch { return null; }
        }

        private static byte[] Base64UrlDecode(string input)
        {
            string s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }
    }
}
