namespace English.Website.Api.Extensions.Helpers
{
    public static class EmailTemplateHelper
    {
        public static string BuildHtmlTemplate(string title, string content)
        {
            int currentYear = DateTime.UtcNow.Year;

            return $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{title}</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #030014; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; -webkit-font-smoothing: antialiased; color: #f3f4f6;"">
    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #030014; table-layout: fixed; padding: 40px 10px;"">
        <tr>
            <td align=""center"">
                <!-- Outer Glass Panel Container -->
                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #0c0a21; border: 1px solid #231f47; border-radius: 16px; overflow: hidden; box-shadow: 0 20px 50px rgba(0, 0, 0, 0.6);"">
                    
                    <!-- Header Section with Logo ES & Engsteps Brand Name -->
                    <tr>
                        <td style=""padding: 32px 32px 24px 32px; text-align: center; border-bottom: 1px solid rgba(255, 255, 255, 0.08); background: linear-gradient(180deg, rgba(139, 92, 246, 0.12) 0%, rgba(0, 0, 0, 0) 100%);"">
                            <table border=""0"" cellpadding=""0"" cellspacing=""0"" align=""center"">
                                <tr>
                                    <!-- Logo Badge ES -->
                                    <td style=""vertical-align: middle;"">
                                        <div style=""width: 44px; height: 44px; background: linear-gradient(135deg, #3b82f6 0%, #8b5cf6 50%, #d946ef 100%); border-radius: 12px; text-align: center; line-height: 44px; color: #ffffff; font-weight: 900; font-size: 19px; letter-spacing: -0.5px; box-shadow: 0 4px 20px rgba(139, 92, 246, 0.4);"">
                                            ES
                                        </div>
                                    </td>
                                    <!-- Brand Name Engsteps -->
                                    <td style=""vertical-align: middle; padding-left: 14px; text-align: left;"">
                                        <span style=""font-size: 26px; font-weight: 800; color: #ffffff; letter-spacing: -0.5px; display: block; line-height: 1.1;"">
                                            Engsteps
                                        </span>
                                        <span style=""font-size: 11px; font-weight: 600; color: #94a3b8; text-transform: uppercase; letter-spacing: 1.5px; display: block; margin-top: 2px;"">
                                            AI-Powered English Platform
                                        </span>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Body Content Section -->
                    <tr>
                        <td style=""padding: 36px 32px; background-color: #0c0a21;"">
                            <!-- Email Title -->
                            <h1 style=""margin: 0 0 20px 0; font-size: 20px; font-weight: 700; color: #ffffff; line-height: 1.4; text-align: left;"">
                                {title}
                            </h1>
                            
                            <!-- Dynamic Content Block -->
                            <div style=""font-size: 15px; line-height: 1.6; color: #cbd5e1;"">
                                {content}
                            </div>
                        </td>
                    </tr>

                    <!-- Footer Section -->
                    <tr>
                        <td style=""padding: 24px 32px; background-color: #070514; border-top: 1px solid rgba(255, 255, 255, 0.06); text-align: center;"">
                            <p style=""margin: 0 0 8px 0; font-size: 12px; color: #64748b; line-height: 1.4;"">
                                Email này được gửi tự động từ hệ thống <strong>Engsteps</strong>. Vui lòng không trả lời trực tiếp email này.
                            </p>
                            <p style=""margin: 0; font-size: 12px; color: #475569;"">
                                &copy; {currentYear} Engsteps. All rights reserved.
                            </p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}
