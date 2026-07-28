using MMAC.DTOS;
using MMAC.Services.AuditLogService;
using QRCoder;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System.Drawing;
using System.Net;
using System.Net.Mail;

namespace MMAC.Services.PdfService
{
    public class PdfService : IPdfService
    {
        private readonly IConfiguration _configuration;
        private readonly IAuditLogService _auditLogService;
        private readonly IServiceScopeFactory _scopeFactory;

        public PdfService(IConfiguration configuration, IAuditLogService auditLogService, IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _auditLogService = auditLogService;
            _scopeFactory = scopeFactory;
        }

        public async Task<byte[]> GenerateArrivalPdfAsync(CompleteArrivalDTO model, Guid applicationNo, string referenceNo, string countryName, string fullAddress)
        {
            return await Task.Run(() =>
            {
                PdfDocument doc = new PdfDocument();
                PdfPageBase page = doc.Pages.Add(PdfPageSize.A4, new PdfMargins(40));

                // Font definitions
                PdfFont titleFont = new PdfFont(PdfFontFamily.Helvetica, 16f, PdfFontStyle.Bold);
                PdfFont subTitleFont = new PdfFont(PdfFontFamily.Helvetica, 11f, PdfFontStyle.Regular);
                PdfFont sectionFont = new PdfFont(PdfFontFamily.Helvetica, 11f, PdfFontStyle.Bold);
                PdfFont labelFont = new PdfFont(PdfFontFamily.Helvetica, 9f, PdfFontStyle.Bold);
                PdfFont valueFont = new PdfFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);

                Color themeColor = ColorTranslator.FromHtml("#004578");
                PdfSolidBrush themeBrush = new PdfSolidBrush(themeColor);
                PdfPen themePen = new PdfPen(themeColor, 1f);

                PdfBrush blackBrush = PdfBrushes.Black;
                PdfBrush grayBrush = PdfBrushes.DimGray;
                PdfBrush lightGrayBrush = new PdfSolidBrush(Color.FromArgb(245, 247, 250));

                float currentY = 15f;
                float pageWidth = page.Canvas.ClientSize.Width;
                float col2X = (pageWidth / 2) + 10f; // ညာဘက် Column အတွက် X တည်နေရာ

                // --- HEADER ---
                PdfStringFormat centerFormat = new PdfStringFormat(PdfTextAlignment.Center);
                page.Canvas.DrawString("REPUBLIC OF THE UNION OF MYANMAR", titleFont, themeBrush, pageWidth / 2, currentY, centerFormat);
                currentY += 22f;
                page.Canvas.DrawString("Electronic Arrival Declaration (e-Arrival)", subTitleFont, grayBrush, pageWidth / 2, currentY, centerFormat);
                currentY += 35f;

                // --- QR CODE & DE NUMBER ---
                byte[] qrBytes = GenerateQrCodeBytes(applicationNo.ToString());
                using (MemoryStream qrStream = new MemoryStream(qrBytes))
                {
                    PdfImage qrImage = PdfImage.FromStream(qrStream);
                    page.Canvas.DrawImage(qrImage, pageWidth - 110, currentY, 110, 110);
                }

                page.Canvas.DrawString("Arrival Approval ID (DE Number):", labelFont, blackBrush, 10, currentY + 5f);
                currentY += 18f;
                page.Canvas.DrawString(referenceNo.ToString(), new PdfFont(PdfFontFamily.Helvetica, 15f, PdfFontStyle.Bold), themeBrush, 10, currentY + 5f);
                currentY += 30f;

                string infoText = "Please present this approval code and QR code, along with your valid passport and visa (if applicable), to the Myanmar Immigration Officer upon arrival. Valid for a single entry.";
                PdfTextWidget textWidget = new PdfTextWidget(infoText, new PdfFont(PdfFontFamily.Helvetica, 9f, PdfFontStyle.Regular), grayBrush);
                PdfTextLayout textLayout = new PdfTextLayout();
                textLayout.Layout = PdfLayoutType.Paginate;

                textWidget.Draw(page, new RectangleF(10, currentY, pageWidth - 130, 60), textLayout);
                currentY += 65f;

                // --- HELPER FUNCTION FOR MASKING ---
                string MaskPassportNumber(string passportNo)
                {
                    if (string.IsNullOrEmpty(passportNo)) return string.Empty;
                    if (passportNo.Length <= 4) return passportNo;

                    string firstTwo = passportNo.Substring(0, 2);
                    string lastTwo = passportNo.Substring(passportNo.Length - 2);
                    string middleMask = new string('*', passportNo.Length - 4);

                    return $"{firstTwo}{middleMask}{lastTwo}";
                }

                // Gender Mapping (M = Male, F = Female)
                string formattedGender = model.Gender?.ToUpper() switch
                {
                    "M" => "Male",
                    "F" => "Female",
                    _ => model.Gender ?? "N/A"
                };

                // --- PART I: Personal Particulars ---
                page.Canvas.DrawRectangle(themePen, lightGrayBrush, new RectangleF(10, currentY, pageWidth - 20, 22));
                page.Canvas.DrawString("PART I: Personal Particulars", sectionFont, themeBrush, 18, currentY + 4);
                currentY += 32f;

                // Row 1: FULL NAME & DATE OF BIRTH
                DrawField(page, "FULL NAME", model.FullName, labelFont, valueFont, 10, currentY);
                DrawField(page, "DATE OF BIRTH", model.DOB.ToString("yyyy-MM-dd"), labelFont, valueFont, col2X, currentY);
                currentY += 38f;

                // Row 2: COUNTRY & GENDER
                DrawField(page, "COUNTRY", countryName, labelFont, valueFont, 10, currentY);
                DrawField(page, "GENDER", formattedGender, labelFont, valueFont, col2X, currentY);
                currentY += 38f;

                // Row 3: PASSPORT NUMBER & PASSPORT EXPIRE DATE
                DrawField(page, "PASSPORT NUMBER", MaskPassportNumber(model.PassportNo), labelFont, valueFont, 10, currentY);
                DrawField(page, "PASSPORT EXPIRE DATE", model.ExpiryDate.ToString("yyyy-MM-dd"), labelFont, valueFont, col2X, currentY);
                currentY += 48f;

                // --- PART II: Trip Details ---
                page.Canvas.DrawRectangle(themePen, lightGrayBrush, new RectangleF(10, currentY, pageWidth - 20, 22));
                page.Canvas.DrawString("PART II: Trip Details", sectionFont, themeBrush, 18, currentY + 4);
                currentY += 32f;

                // Row 1: DATE OF ARRIVAL & PURPOSE OF VISIT
                DrawField(page, "DATE OF ARRIVAL", model.ArrivalDate.ToString("dd MMM yyyy"), labelFont, valueFont, 10, currentY);
                DrawField(page, "PURPOSE OF VISIT", model.PurposeOfVisit, labelFont, valueFont, col2X, currentY);
                currentY += 38f;

                // Row 2: PREVIOUS CITY & ACCOMMODATION
                DrawField(page, "PREVIOUS CITY", model.PreviousCity, labelFont, valueFont, 10, currentY);
                //DrawField(page, "ACCOMMODATION", model.Accommodation, labelFont, valueFont, col2X, currentY);
                currentY += 38f;

                // Row 3: ADDRESS IN MYANMAR
                DrawField(page, "ADDRESS IN MYANMAR", fullAddress, labelFont, valueFont, 10, currentY);
                currentY += 52f;

                // --- PART III: Health & Customs Declaration ---
                page.Canvas.DrawRectangle(themePen, lightGrayBrush, new RectangleF(10, currentY, pageWidth - 20, 22));
                page.Canvas.DrawString("PART III: Health & Customs Declaration", sectionFont, themeBrush, 18, currentY + 4);
                currentY += 32f;

                PdfTextWidget noticeWidget = new PdfTextWidget("NOTICE: False declarations are subject to prosecution under the laws of the Republic of the Union of Myanmar.", new PdfFont(PdfFontFamily.Helvetica, 8.5f, PdfFontStyle.Bold), PdfBrushes.DarkGoldenrod);
                noticeWidget.Draw(page, new RectangleF(10, currentY, pageWidth - 20, 25), textLayout);
                currentY += 28f;

                string q1 = "Do you currently have or have you had in the past 14 days any of the following symptoms: fever, cough, sore throat, or shortness of breath?";
                PdfTextWidget q1Widget = new PdfTextWidget(q1, valueFont, blackBrush);
                q1Widget.Draw(page, new RectangleF(10, currentY, pageWidth - 60, 30), textLayout);
                page.Canvas.DrawString(model.HealthDeclaration, labelFont, themeBrush, pageWidth - 40, currentY + 5f);
                currentY += 35f;

                string q2 = "Are you carrying any prohibited or restricted items such as plants, seeds, unprocessed foods, meats, endangered animal products, or illegal drugs?";
                PdfTextWidget q2Widget = new PdfTextWidget(q2, valueFont, blackBrush);
                q2Widget.Draw(page, new RectangleF(10, currentY, pageWidth - 60, 30), textLayout);
                page.Canvas.DrawString(model.DigitalDeclarations, labelFont, themeBrush, pageWidth - 40, currentY + 5f);
                currentY += 45f;

                // --- FOOTER ---
                page.Canvas.DrawLine(PdfPens.LightGray, 10, currentY, pageWidth - 10, currentY);
                currentY += 12f;
                string footerText = "IMPORTANT: This acknowledgment does not guarantee entry into Myanmar. The Department of Immigration and Population officers will assess your eligibility for entry upon arrival. Ensure your passport is valid for at least 6 months and you possess a valid visa if required. This document was generated electronically.";
                PdfTextWidget footerWidget = new PdfTextWidget(footerText, new PdfFont(PdfFontFamily.Helvetica, 8f, PdfFontStyle.Regular), grayBrush);
                footerWidget.Draw(page, new RectangleF(10, currentY, pageWidth - 20, 50), textLayout);

                using MemoryStream outputStream = new MemoryStream();
                doc.SaveToStream(outputStream, FileFormat.PDF);
                doc.Close();

                return outputStream.ToArray();
            });
        }
        private void DrawField(PdfPageBase page, string label, string value, PdfFont labelFont, PdfFont valueFont, float x, float y)
        {
            page.Canvas.DrawString(label, labelFont, new PdfSolidBrush(Color.Gray), x, y);
            page.Canvas.DrawString(string.IsNullOrEmpty(value) ? "N/A" : value, valueFont, PdfBrushes.Black, x, y + 14f);
        }

        private byte[] GenerateQrCodeBytes(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }

        public void SendPdfEmailInBackground(string toEmail, string applicationId, byte[] pdfBytes, string referenceNo, Guid travellerId)
        {
            Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var scopedAuditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();

                try
                {
                    string senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "ath7107@gmail.com";
                    string senderName = _configuration["EmailSettings:SenderName"] ?? "MMAC Arrival System";
                    string appPassword = _configuration["EmailSettings:AppPassword"] ?? "";

                    var fromAddress = new MailAddress(senderEmail, senderName);
                    var toAddress = new MailAddress(toEmail);
                    string subject = "Your e-Arrival Form Submission";
                    string body = "Dear Applicant,\n\nYour application has been submitted successfully. Please find your official e-Arrival Form PDF attached below.";

                    using var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, appPassword)
                    };

                    using var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body
                    };

                    using var ms = new MemoryStream(pdfBytes);
                    message.Attachments.Add(new Attachment(ms, $"MM_ArrivalForm_{referenceNo}.pdf", "application/pdf"));

                    await smtp.SendMailAsync(message);

                    var successLogObj = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "To", toEmail },
                        { "AppId", applicationId },
                        { "Status", "Success" }
                    };

                    await scopedAuditLogService.LogAsync("EMAIL_SENT_SUCCESS", successLogObj, travellerId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SMTP Email Error]: {ex.Message}");
                    try
                    {
                        var errorLogObj = new System.Collections.Generic.Dictionary<string, string>
                        {
                            { "To", toEmail },
                            { "ErrorMessage", ex.Message }
                        };

                        await scopedAuditLogService.LogAsync("EMAIL_SENT_FAILED", errorLogObj, travellerId);
                    }
                    catch (Exception logEx)
                    {
                        Console.WriteLine($"[Critical Audit Log Error]: {logEx.Message}");
                    }
                }
            });
        }

    }
}