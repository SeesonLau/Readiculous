using Readiculous.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Readiculous.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        
        private string ResolveSmtpPassword(IConfigurationSection smtpSettings)
        {
            var password = smtpSettings["Password"];
            
            
            if (password != null && password.StartsWith("%") && password.EndsWith("%"))
            {
                var envVarName = password.Substring(1, password.Length - 2);
                var envPassword = Environment.GetEnvironmentVariable(envVarName);
                
                if (string.IsNullOrEmpty(envPassword))
                {
                    throw new InvalidOperationException($"Environment variable '{envVarName}' is not set. Please set the SMTP password environment variable.");
                }
                
                return envPassword;
            }
            
            return password;
        }

        public async Task<bool> SendOtpEmailAsync(string email, string otp, string tempPassword)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var resolvedPassword = ResolveSmtpPassword(smtpSettings);
                var fromEmail = smtpSettings["FromEmail"] ?? "no-reply@readiculous.com";
                var fromName = smtpSettings["FromName"] ?? "Readiculous Team";
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = "Your OTP - Readiculous",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='background-color: #f8f9fa; padding: 30px; border-radius: 10px; text-align: center;'>
                                <h2 style='color: #007bff; margin-bottom: 20px;'>Welcome to Readiculous!</h2>
                                <div style='background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                                    <p style='margin-bottom: 10px;'>Your verification code is: <b>{otp}</b></p>
                                    <p style='color: #d63384; font-weight: bold; margin-top: 18px;'>DO NOT REPLY TO THIS EMAIL.</p>
                                </div>
                                <p style='color: #6c757d; font-size: 14px;'>This code and password will expire in 10 minutes.</p>
                                <p style='color: #6c757d; font-size: 14px;'>If you didn't request this code, please ignore this email.</p>
                                <hr style='margin: 30px 0; border: none; border-top: 1px solid #dee2e6;'>
                                <p style='color: #6c757d; font-size: 14px;'>Best regards,<br>The Readiculous Team</p>
                            </div>
                        </body>
                        </html>",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);
                var smtpClient = new SmtpClient
                {
                    Host = smtpSettings["Host"],
                    Port = int.Parse(smtpSettings["Port"]),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"]),
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpSettings["Username"], resolvedPassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 10000 
                };
                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"✅ Email sent successfully to {email}");
                return true;
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"❌ Failed to send email: {ex.Message}");
                return false; 
            }
        }

        public async Task<bool> SendTempPasswordEmailAsync(string email, string tempPassword)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var resolvedPassword = ResolveSmtpPassword(smtpSettings);
                var fromEmail = smtpSettings["FromEmail"] ?? "no-reply@readiculous.com";
                var fromName = smtpSettings["FromName"] ?? "Readiculous Team";
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = "Your Temporary Password - Readiculous",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='background-color: #f8f9fa; padding: 30px; border-radius: 10px; text-align: center;'>
                                <h2 style='color: #007bff; margin-bottom: 20px;'>Welcome to Readiculous!</h2>
                                <div style='background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                                    <p style='margin-bottom: 10px;'>Your temporary password is: <b>{tempPassword}</b></p>
                                    <p style='color: #d63384; font-weight: bold; margin-top: 18px;'>DO NOT REPLY TO THIS EMAIL.</p>
                                </div>
                                <p style='color: #6c757d; font-size: 14px;'>Use this password to log in. You will be prompted to change it after your first login.</p>
                                <hr style='margin: 30px 0; border: none; border-top: 1px solid #dee2e6;'>
                                <p style='color: #6c757d; font-size: 14px;'>Best regards,<br>The Readiculous Team</p>
                            </div>
                        </body>
                        </html>",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);
                var smtpClient = new SmtpClient
                {
                    Host = smtpSettings["Host"],
                    Port = int.Parse(smtpSettings["Port"]),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"]),
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpSettings["Username"], resolvedPassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 10000 
                };
                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"✅ Temp password email sent successfully to {email}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send temp password email: {ex.Message}");
                return false; 
            }
        }

        public async Task<bool> SendOtpForForgotPasswordEmailAsync(string email, string otp)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var resolvedPassword = ResolveSmtpPassword(smtpSettings);
                
                var smtpClient = new SmtpClient
                {
                    Host = smtpSettings["Host"],
                    Port = int.Parse(smtpSettings["Port"]),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"]),
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpSettings["Username"], resolvedPassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 10000 
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromEmail"], smtpSettings["FromName"] ?? "Readiculous Team"),
                    Subject = "Password Reset OTP - Readiculous",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='background-color: #f8f9fa; padding: 30px; border-radius: 10px; text-align: center;'>
                                <h2 style='color: #007bff; margin-bottom: 20px;'>Password Reset Request</h2>
                                <div style='background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                                    <p style='margin-bottom: 10px;'>Your password reset verification code is: <b>{otp}</b></p>
                                    <p style='color: #d63384; font-weight: bold; margin-top: 18px;'>DO NOT REPLY TO THIS EMAIL.</p>
                                </div>
                                <p style='color: #6c757d; font-size: 14px;'>This code will expire in 10 minutes.</p>
                                <p style='color: #6c757d; font-size: 14px;'>If you didn't request this password reset, please ignore this email.</p>
                                <hr style='margin: 30px 0; border: none; border-top: 1px solid #dee2e6;'>
                                <p style='color: #6c757d; font-size: 14px;'>Best regards,<br>The Readiculous Team</p>
                            </div>
                        </body>
                        </html>",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                return false; 
            }
        }
    }
} 