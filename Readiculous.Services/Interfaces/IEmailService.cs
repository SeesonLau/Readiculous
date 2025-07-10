using System.Threading.Tasks;

namespace Readiculous.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string email, string otp, string tempPassword);
        Task<bool> SendTempPasswordEmailAsync(string email, string tempPassword);
        Task<bool> SendOtpForForgotPasswordEmailAsync(string email, string otp);
    }
} 