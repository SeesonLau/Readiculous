# Email Setup Guide for Readiculous

## Current Status
The application is currently running in **Development Mode**, which means OTP codes are displayed in the console/terminal instead of being sent via email.

## Dedicated Sender Email Account

The application is configured to use a dedicated email account for sending OTP emails:
- **Email**: `readiculousteam@gmail.com`
- **Purpose**: Sending OTP verification emails to users
- **Service**: Gmail SMTP

## How to Enable Real Email Sending

### Step 1: Configure the Sender Email Account

1. **Use Existing Gmail Account**
   - Email: `readiculousteam@gmail.com`
   - Ensure you have access to this account
   - Complete any verification if required

### Step 2: Configure Gmail for SMTP

1. **Enable 2-Factor Authentication**
   - Go to your Google Account settings
   - Navigate to Security
   - Enable 2-Step Verification

2. **Generate App Password**
   - Go to Google Account settings → Security
   - Under "2-Step Verification", click "App passwords"
   - Select "Mail" and "Other (Custom name)"
   - Enter "Readiculous App" as the name
   - Copy the generated 16-character password

### Step 3: Set Environment Variable for SMTP Password

**IMPORTANT**: For security reasons, the SMTP password is now stored as an environment variable instead of being hardcoded in the configuration file.

#### Option A: Set Environment Variable (Recommended)

**Windows (PowerShell):**
```powershell
# Set the environment variable
$env:SMTP_PASSWORD="your-16-character-app-password"

# To make it permanent (for current user)
[Environment]::SetEnvironmentVariable("SMTP_PASSWORD", "your-16-character-app-password", "User")
```

**Windows (Command Prompt):**
```cmd
# Set the environment variable
set SMTP_PASSWORD=your-16-character-app-password

# To make it permanent (for current user)
setx SMTP_PASSWORD "your-16-character-app-password"
```

**Windows (System Properties):**
1. Right-click on "This PC" or "My Computer"
2. Select "Properties"
3. Click "Advanced system settings"
4. Click "Environment Variables"
5. Under "User variables", click "New"
6. Variable name: `SMTP_PASSWORD`
7. Variable value: `your-16-character-app-password`
8. Click "OK"

**Linux/macOS:**
```bash
# Set the environment variable
export SMTP_PASSWORD="your-16-character-app-password"

# To make it permanent, add to ~/.bashrc or ~/.zshrc
echo 'export SMTP_PASSWORD="your-16-character-app-password"' >> ~/.bashrc
source ~/.bashrc
```

#### Option B: Use User Secrets (Development Only)

For development environments, you can use .NET User Secrets:

```bash
# Navigate to the WebApp project directory
cd Readiculous.WebApp

# Set the user secret
dotnet user-secrets set "SmtpSettings:Password" "your-16-character-app-password"
```

### Step 4: Update Configuration

The `appsettings.json` file now uses an environment variable placeholder:

```json
"SmtpSettings": {
  "Host": "smtp.gmail.com",
  "Port": "587",
  "EnableSsl": "true",
  "Username": "readiculousteam@gmail.com",
  "Password": "%SMTP_PASSWORD%",
  "FromEmail": "readiculousteam@gmail.com",
  "FromName": "Readiculous Team"
}
```

**Note**: The `%SMTP_PASSWORD%` placeholder will be automatically replaced with the value from the environment variable.

### Step 5: Verify Environment Variable

You can verify that the environment variable is set correctly:

**Windows (PowerShell):**
```powershell
echo $env:SMTP_PASSWORD
```

**Windows (Command Prompt):**
```cmd
echo %SMTP_PASSWORD%
```

**Linux/macOS:**
```bash
echo $SMTP_PASSWORD
```

## Testing the Email Functionality

1. **Start the application**
2. **Go to registration page**
3. **Enter an email address**
4. **Click "Send OTP"**
5. **Check your email inbox** (or console if in development mode)

## Development Mode Features

When running locally (localhost), the application:
- Shows OTP codes in the console with clear formatting
- Displays a development mode notice on the OTP verification page
- Continues to work even without email configuration

## Troubleshooting

### Email Not Sending
- Check your SMTP settings in `appsettings.json`
- Verify the sender email account exists: `readiculousteam@gmail.com`
- Make sure you're using an App Password, not your regular password
- Check the console for error messages

### Environment Variable Issues
- Verify the environment variable is set: `echo $SMTP_PASSWORD` (Linux/macOS) or `echo %SMTP_PASSWORD%` (Windows)
- Make sure the variable name matches exactly: `SMTP_PASSWORD`
- Restart your application after setting the environment variable
- Check that the password is the 16-character App Password, not your regular Gmail password

### Gmail App Password Issues
- Make sure 2-Factor Authentication is enabled on `readiculousteam@gmail.com`
- Generate a new App Password if the current one doesn't work
- Wait a few minutes after generating a new App Password
- Ensure the App Password is for "Mail" service

### Security Notes
- Never commit real email credentials to version control
- Use environment variables or user secrets for production
- App passwords are more secure than regular passwords for SMTP
- The environment variable approach keeps sensitive data out of configuration files

## Production Deployment

For production, consider using:
- **SendGrid** - Professional email service
- **Mailgun** - Reliable email delivery
- **Amazon SES** - Cost-effective email service
- **Environment variables** for sensitive credentials
- **Azure Key Vault** or **AWS Secrets Manager** for cloud deployments

## Environment Variable Best Practices

1. **Never commit environment variables to source control**
2. **Use different values for development, staging, and production**
3. **Rotate passwords regularly**
4. **Use secure methods to distribute environment variables to team members**
5. **Consider using a secrets management service for production environments**

## Alternative Email Providers

If you prefer to use a different email provider, you can update the configuration:

#### Outlook/Hotmail Setup
```json
"SmtpSettings": {
  "Host": "smtp-mail.outlook.com",
  "Port": "587",
  "EnableSsl": "true",
  "Username": "noted2001@outlook.com",
  "Password": "your-password",
  "FromEmail": "noted2001@outlook.com",
  "FromName": "Readiculous Team"
}
```

#### Yahoo Setup
```json
"SmtpSettings": {
  "Host": "smtp.mail.yahoo.com",
  "Port": "587",
  "EnableSsl": "true",
  "Username": "noted2001@yahoo.com",
  "Password": "your-app-password",
  "FromEmail": "noted2001@yahoo.com",
  "FromName": "Readiculous Team"
}
``` 