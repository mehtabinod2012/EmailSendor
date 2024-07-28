using System.Net.Mail;
using System.Net;
using Oracle.ManagedDataAccess.Client;
using static EmailSender.Program;
using System.Configuration;

namespace EmailSender
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            var smtp = ConfigurationManager.AppSettings["SmtpServer"];
            Console.WriteLine("connectionString: "+ ConfigurationManager.AppSettings["connectionString"]);

            //var connectionString = "User Id=mis_ias;Password=nabil;Data Source=10.0.5.63:1521/MIS19C";
            //var senderEmail = "binod.mehta@nabilbank.com";
            //var senderName = "Binod Mehta";  // Add the sender's name here
            //var appPassword = "qztchrfwfvddblxh";
            //var logger = new Logger();

            var connectionString = ConfigurationManager.AppSettings["connectionString"];
            var senderEmail = ConfigurationManager.AppSettings["SenderEmail"];
            var senderName = ConfigurationManager.AppSettings["senderName"];  // Add the sender's name here
            var appPassword = ConfigurationManager.AppSettings["appPassword"]; 
            var logger = new Logger();
            var oracl_qry = ConfigurationManager.AppSettings["ora_email_qry"];


            try
            {
                var emailData = await GetEmailDataFromDatabase(connectionString, logger, oracl_qry);

                logger.Log($"connecion string is: {connectionString}");
                logger.Log($"Oracle Qurery is : {oracl_qry}");
                foreach (var data in emailData)
                {
                    await SendEmail(senderEmail, senderName, appPassword, data.RecipientEmail, data.CcEmail, data.BccEmail, data.Subject, data.Body, logger);
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error in Main: {ex.Message}");
            }
        }

        static async Task<List<EmailData>> GetEmailDataFromDatabase(string connectionString, Logger logger, string oracl_qry)
        {
            var emailDataList = new List<EmailData>();

            try
            {
                using (var connection = new OracleConnection(connectionString))
                {
                    await connection.OpenAsync();
                    logger.Log("Connected to the database.");

                    //using (var command = new OracleCommand("SELECT recipient_email, cc_email, bcc_email, email_subject, email_body FROM your_table", connection))
                    using (var command = new OracleCommand(oracl_qry, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var recipientEmail = reader.GetString(0);
                                //var ccEmail = reader.IsDBNull(1) ? null : reader.GetString(1);
                                var ccEmail = ConfigurationManager.AppSettings["ccEmail"];
                                var bccEmail = ConfigurationManager.AppSettings["bccEmail"];
                                var subject = reader.GetString(1);
                                var body = reader.GetString(2);
                                emailDataList.Add(new EmailData
                                {
                                    RecipientEmail = recipientEmail,
                                    CcEmail = ccEmail,
                                    BccEmail = bccEmail,
                                    Subject = subject,
                                    Body = body
                                });
                            }
                        }
                    }
                }
                logger.Log("Email data retrieved from the database.");
            }
            catch (Exception ex)
            {
                logger.Log($"Error in GetEmailDataFromDatabase: {ex.Message}");
            }

            return emailDataList;
        }

        static async Task SendEmail(string senderEmail, string senderName, string appPassword, string recipientEmail, string ccEmail, string bccEmail, string subject, string body, Logger logger)
        {
            try
            {
                using (var client = new SmtpClient("smtp.office365.com", 587))
                {
                    client.Credentials = new NetworkCredential(senderEmail, appPassword);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderName),  // Set sender email and name
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,
                    };

                    mailMessage.To.Add(recipientEmail);

                    if (!string.IsNullOrEmpty(ccEmail))
                    {
                        mailMessage.CC.Add(ccEmail);
                    }

                    if (!string.IsNullOrEmpty(bccEmail))
                    {
                        mailMessage.Bcc.Add(bccEmail);
                    }

                    await client.SendMailAsync(mailMessage);
                    logger.Log($"Email sent successfully to {recipientEmail}.");
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error sending email to {recipientEmail}: {ex.Message}");
            }
        }

        public class EmailData
        {
            public string RecipientEmail { get; set; }
            public string CcEmail { get; set; }
            public string BccEmail { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }

        public class Logger
        {
            private readonly string logFilePath;

            public Logger()
            {
                var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"log_{currentDate}.txt");
            }

            public void Log(string message)
            {
                var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                Console.WriteLine(logMessage);
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            }
        }
    }
}
        //static async Task Main(string[] args)
        //{
        //    var senderEmail = "binod.mehta@nabilbank.com";
        //    var senderName = "Binod Mehta";  // Add the sender's name here

        //    var appPassword = "qztchrfwfvddblxh";
        //    var recipientEmail = "mehtabinod2012@gmail.com, sahen.manandhar15@gmail.com ";
        //    var subject = "Test Email";
        //    var body = "This is a test email sent from .NET Core Console App using Microsoft 365.";

        //    try
        //    {
        //        using (var client = new SmtpClient("smtp.office365.com", 587))
        //        {
        //            client.Credentials = new NetworkCredential(senderEmail, appPassword);
        //            client.EnableSsl = true;

        //            var mailMessage = new MailMessage
        //            {
        //                From = new MailAddress(senderEmail, senderName),
        //                Subject = subject,
        //                Body = body,
        //                IsBodyHtml = true,
        //            };

        //            mailMessage.To.Add(recipientEmail);

        //            await client.SendMailAsync(mailMessage);
        //            Console.WriteLine("Email sent successfully.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error sending email: {ex.Message}");
        //    }
        //}


//    }
//}