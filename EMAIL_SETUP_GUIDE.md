# Email Setup Guide for Readiculous

## Current Status
The application is currently running in **Development Mode**, which means OTP codes are displayed in the console/terminal instead of being sent via email.

## Dedicated Sender Email Account

The application is configured to use a dedicated email account for sending OTP emails:
- **Email**: `noted2001@gmail.com`
- **Purpose**: Sending OTP verification emails to users
- **Service**: Gmail SMTP

## How to Enable Real Email Sending

### Step 1: Configure the Sender Email Account

1. **Use Existing Gmail Account**
   - Email: `noted2001@gmail.com`
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

### Step 3: Update Configuration

Update `appsettings.json` with the real App Password:
```json
"SmtpSettings": {
  "Host": "smtp.gmail.com",
  "Port": "587",
  "EnableSsl": "true",
  "Username": "noted2001@gmail.com",
  "Password": "your-16-character-app-password",
  "FromEmail": "noted2001@gmail.com",
  "FromName": "Readiculous Team"
}
```

1. **Enable 2-Factor Authentication**
   - Go to your Google Account settings
   - Navigate to Security
   - Enable 2-Step Verification

2. **Generate App Password**
   - Go to Google Account settings → Security
   - Under "2-Step Verification", click "App passwords"
   - Select "Mail" and "Other (Custom name)"
   - Enter "Readiculous" as the name
   - Copy the generated 16-character password

3. **Update appsettings.json**
   ```json
   "SmtpSettings": {
     "Host": "smtp.gmail.com",
     "Port": "587",
     "EnableSsl": "true",
     "Username": "your-actual-email@gmail.com",
     "Password": "your-16-character-app-password",
     "FromEmail": "your-actual-email@gmail.com"
   }
   ```

### Alternative Email Providers

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
- Verify the sender email account exists: `noted2001@gmail.com`
- Make sure you're using an App Password, not your regular password
- Check the console for error messages

### Gmail App Password Issues
- Make sure 2-Factor Authentication is enabled on `noted2001@gmail.com`
- Generate a new App Password if the current one doesn't work
- Wait a few minutes after generating a new App Password
- Ensure the App Password is for "Mail" service

### Security Notes
- Never commit real email credentials to version control
- Use environment variables or user secrets for production
- App passwords are more secure than regular passwords for SMTP

## Production Deployment

For production, consider using:
- **SendGrid** - Professional email service
- **Mailgun** - Reliable email delivery
- **Amazon SES** - Cost-effective email service
- **Environment variables** for sensitive credentials 