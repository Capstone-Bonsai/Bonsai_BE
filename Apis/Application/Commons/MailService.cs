using Domain.Entities;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commons
{
    public class MailService
    {
        public bool SendEmail(string toMail, string subject, string content)
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress("Thanh Sơn Garden", "tramygeo@gmail.com"));
            email.To.Add(MailboxAddress.Parse(toMail));
            email.Subject = subject;

            var builder = new BodyBuilder();

            builder.HtmlBody = "<div style=\"font-family: 'Arial', sans-serif;\r\nbackground-color: #f4f4f4;\r\nmargin: 0;\r\npadding: 0;\r\ndisplay: flex;" +
                "\r\njustify-content: center;\r\nalign-items: center;\r\nheight: 100vh;\">\r\n  <div style=\" background-color: #fff;\r\n  color: #333;\r\n " +
                " border-radius: 15px;\r\n  box-shadow: 0 0 20px rgba(0, 0, 0, 0.2);\r\n  overflow: hidden;\r\n  width: 80%;\r\n  max-width: 500px;\r\n " +
                " text-align: left; padding: 30px;\">" + content +
                "   <p style=\"margin-bottom: 10px;\r\n   " +
                " text-align: left;\">Trân trọng,</p>\r\n    <p style=\"font-weight: bold; color: #009c11;\">Thanh Sơn Garden</p>\r\n  " +
                "  <img class=\"logo\" style=\"max-width: 50%;\r\n    height: auto;\r\n    margin-bottom: 20px;\"\r\n    " +
                "  src=\"https://firebasestorage.googleapis.com/v0/b/rentalhome-df2d1.appspot.com/o/logo_black_ver.png?alt=media&token=84a26698-9097-44a4-b80c-2de39def8dfb\"\r\n    " +
                "  alt=\"Logo Thanh Sơn Garden\">  <div style=\"margin-top: 20px;\r\n    text-align: left;\">\r\n    " +
                "  <p style=\"margin-bottom: 10px;\r\n text-align: left; margin: 5px 0;  color: #009c11;\"><strong>Liên hệ với chúng tôi để được hỗ trợ nhiều\r\n hơn</strong>" +
                "</p>\r\n <p style=\"margin-bottom: 10px;\r\n text-align: left; margin: 5px 0;\"><strong>Email:</strong> hotro@thanhson.vn</p>\r\n " +
                " <p style=\"margin-bottom: 10px;\r\n      text-align: left; margin: 5px 0;\"><strong>Hotline:</strong> (024) 6680 5588</p>\r\n    </div>\r\n  </div>\r\n</div>";
            email.Body = builder.ToMessageBody();
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
            smtp.Authenticate("tramygeo@gmail.com", "shvmqyxqovhiapgh");
            try
            {
                smtp.Send(email);
            }
            catch (SmtpCommandException ex)
            {
                return false;
            }
            smtp.Disconnect(true);
            return true;
        }


    }
}
