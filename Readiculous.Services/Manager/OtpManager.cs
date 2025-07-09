using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Readiculous.Services.Manager
{
    public class OtpManager
    {
        private static readonly ConcurrentDictionary<string, OtpData> _otpStorage = new ConcurrentDictionary<string, OtpData>();
        private static readonly Random _random = new Random();

        public static string GenerateOtp()
        {
            return _random.Next(100000, 999999).ToString();
        }

        public static string GenerateTempPassword(int length = 10)
        {
            if (length < 8) length = 8;
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string digits = "23456789";
            const string specials = "!@#$%^&*";
            const string all = lower + upper + digits + specials;
            var rand = _random;
            // Ensure at least one of each required type
            char[] password = new char[length];
            password[0] = lower[rand.Next(lower.Length)];
            password[1] = upper[rand.Next(upper.Length)];
            password[2] = digits[rand.Next(digits.Length)];
            password[3] = specials[rand.Next(specials.Length)];
            for (int i = 4; i < length; i++)
                password[i] = all[rand.Next(all.Length)];
            // Shuffle to avoid predictable positions
            for (int i = password.Length - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                var temp = password[i];
                password[i] = password[j];
                password[j] = temp;
            }
            return new string(password);
        }

        public static void StoreOtpAndPassword(string email, string otp, string tempPassword, int expirationMinutes = 10)
        {
            var otpData = new OtpData
            {
                Otp = otp,
                TempPassword = tempPassword,
                ExpirationTime = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };
            _otpStorage.AddOrUpdate(email.ToLower(), otpData, (key, oldValue) => otpData);
        }

        public static (string Otp, string TempPassword)? GetOtpAndPassword(string email)
        {
            if (_otpStorage.TryGetValue(email.ToLower(), out var otpData))
            {
                return (otpData.Otp, otpData.TempPassword);
            }
            return null;
        }

        public static bool ValidateOtp(string email, string otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
                return false;

            if (_otpStorage.TryGetValue(email.ToLower(), out var otpData))
            {
                if (DateTime.UtcNow <= otpData.ExpirationTime && otpData.Otp == otp)
                {
                    // Remove the OTP after successful validation
                    _otpStorage.TryRemove(email.ToLower(), out _);
                    return true;
                }
            }

            return false;
        }

        public static string GetTempPassword(string email)
        {
            if (_otpStorage.TryGetValue(email.ToLower(), out var otpData))
            {
                return otpData.TempPassword;
            }
            return null;
        }

        public static void RemoveOtp(string email)
        {
            _otpStorage.TryRemove(email.ToLower(), out _);
        }

        private class OtpData
        {
            public string Otp { get; set; }
            public string TempPassword { get; set; }
            public DateTime ExpirationTime { get; set; }
        }
    }
} 