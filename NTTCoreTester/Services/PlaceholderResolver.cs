//using NTTCoreTester.Core;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace NTTCoreTester.Services
//{
//    public class PlaceholderResolver 
//    {
//        private readonly PlaceholderCache _cache;

//        public PlaceholderResolver(PlaceholderCache cache)
//        {
//            _cache = cache;
//        }

//        public async Task<string?> ResolvePlaceholders(string template, string endpoint)
//        {
//            var matches = Regex.Matches(template, @"\{\{(\w+)\}\}");
//            var placeholders = matches.Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToList();

//            if (placeholders.Count == 0)
//            {
//                Console.WriteLine("  No placeholders found in body");
//                return template;
//            }

//            Console.WriteLine($"\n Found {placeholders.Count} placeholder(s) in body: {string.Join(", ", placeholders)}");

//            string result = template;

//            foreach (var placeholder in placeholders)
//            {
//                string value = await GetPlaceholderValue(placeholder, endpoint);
//                if (value == null)
//                {
//                    Console.WriteLine($" Failed to resolve: {placeholder}");
//                    return null;
//                }
//                result = result.Replace($"{{{{{placeholder}}}}}", value);
//            }

//            return result;
//        }

//        public async Task<string?> ResolvePlaceholderValue(string value, string endpoint)
//        {
//            var match = Regex.Match(value, @"\{\{(\w+)\}\}");
//            if (!match.Success)
//                return value;

//            string placeholder = match.Groups[1].Value;
//            string resolvedValue = await GetPlaceholderValue(placeholder, endpoint);

//            if (resolvedValue == null)
//                return null;

//            return value.Replace($"{{{{{placeholder}}}}}", resolvedValue);
//        }

//        private async Task<string?> GetPlaceholderValue(string placeholder, string endpoint)
//        {
//            if (placeholder == "token")
//            {
//                string token = _cache.Get("token");
//                if (string.IsNullOrEmpty(token))
//                {
//                    Console.WriteLine($" No active session. Please login first.");
//                    return null;
//                }
//                Console.WriteLine($"   {{{{token}}}} from session");
//                return token;
//            }

//            if (placeholder == "userId")
//            {
//                string userId = _cache.Get("userId");
//                if (string.IsNullOrEmpty(userId))
//                {
//                    Console.WriteLine($" No active session. Please login first.");
//                    return null;
//                }
//                Console.WriteLine($"   {{{{userId}}}} {userId} from session");
//                return userId;
//            }

//            if (placeholder == "userName")
//            {
//                string userName = _cache.Get("userName");
//                if (string.IsNullOrEmpty(userName))
//                {
//                    Console.WriteLine($" No active session. Please login first.");
//                    return null;
//                }
//                Console.WriteLine($"   {{{{userName}}}} {userName} from session");
//                return userName;
//            }

//            // Never-cached placeholders (always prompt)
//            if (placeholder == "otp" || placeholder == "newPwd" || placeholder == "logintoken")
//            {
//                Console.Write($"   Enter {placeholder}: ");
//                string value = Console.ReadLine();
//                return value;
//            }

//            if (_cache.Has(placeholder))
//            {
//                string cached = _cache.Get(placeholder);
//                Console.WriteLine($"   {{{{{placeholder}}}}} from cache");
//                return cached;
//            }

//            Console.Write($"   Enter {placeholder}: ");
//            string input = Console.ReadLine();
//            _cache.Set(placeholder, input);
//            return input;
//        }
//    }
//}
