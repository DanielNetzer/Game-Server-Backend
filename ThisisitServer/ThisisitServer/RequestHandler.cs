using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;

public static class RequestHandler
{

    private static MySqlDataReader mysqlReader = null;

    public static void Register(string registerString, Socket socket, MySqlConnection mysqlCon)
    {

        // At 0: Username, At 1: Password, At 2:Email, at 3:ActivationCode
        string[] data = registerString.Split('|');
        // Check for duplicates, if none found register user.
        if (!isExist("Username", data[0], mysqlCon) && !isExist("Email", data[2], mysqlCon))
        {
            try
            {
                string salt = GenerateSalt();
                string activationCode = Guid.NewGuid().ToString();
                const string cmdString = "INSERT INTO accounts ( Username, Password, Salt, Email, ActivationCode )" +
                    "VALUES" +
                    "(@Username, @Password, @Salt, @Email, @ActivationCode)";
                MySqlCommand cmd = new MySqlCommand(cmdString, mysqlCon);
                cmd.Parameters.AddWithValue("@Username", data[0]);
                cmd.Parameters.AddWithValue("@Password", StringToMD5(data[1], salt));
                cmd.Parameters.AddWithValue("@Salt", salt);
                cmd.Parameters.AddWithValue("@Email", data[2]);
                cmd.Parameters.AddWithValue("@ActivationCode", activationCode);
                cmd.Prepare();

                mysqlReader = cmd.ExecuteReader();
                Console.WriteLine("Registrated succesfully, account validation email will be sent shortly.");
                sendResponse("2|1", socket);
                SendActivationEmail(data[2], data[0], activationCode);
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Error: {0}", e.ToString());
            }
            finally
            {
                if (mysqlReader != null)
                    mysqlReader.Close();
            }
        }
        else if (isExist("Username", data[0], mysqlCon))
        {
            Console.WriteLine("Error: Username {0} already exists!", data[0]);
            sendResponse("2|0|1", socket);
        }
        else {
            Console.WriteLine("Error: Email {0} already exists!", data[0]);
            sendResponse("2|0|0", socket);
        }

    }

    public static void Login(string loginString, Socket socket, MySqlConnection mysqlCon)
    {

        // At 0: Username, At 1: Password, At 2: Email, At 3: ActivationCode
        string[] data = loginString.Split('|');
        // Check the login request
        try
        {
            const string cmdString = "SELECT * FROM accounts WHERE Username=@Username";
            MySqlCommand cmd = new MySqlCommand(cmdString, mysqlCon);
            cmd.Parameters.AddWithValue("@Username", data[0]);
            cmd.Prepare();
            mysqlReader = cmd.ExecuteReader();
            if (mysqlReader.HasRows)
            {
                while (mysqlReader.Read())
                {
                    // mysqlReader :: 0->ID (int), 1->Username (string), 2->hash (string), 3->salt (string), 4-> email (string), 5-> Activation Field (String) 
                    if (mysqlReader.GetString(2) == (StringToMD5(data[1], mysqlReader.GetString(3)))
                        && mysqlReader.GetString(5) == "VERIFIED")
                    {
                        Console.WriteLine("Login Succesful.");
                        sendResponse("0|1", socket);
                    }
                    else if (mysqlReader.GetString(2) != StringToMD5(data[1], mysqlReader.GetString(3)))
                    {
                        Console.WriteLine("Error: Password {0} is wrong!", data[1]);
                        sendResponse("0|0|1", socket);
                    }
                    else
                    {
                        Console.WriteLine("Error: Account {0} is not activated", data[0]);
                        sendResponse("0|0|2", socket);
                    }
                }
            }
            else
            {
                Console.WriteLine("Error: User {0} does not exist!", data[0]);
                sendResponse("0|0|0", socket);
            }
        }
        catch (MySqlException e)
        {
            Console.WriteLine("Error: {0}", e.ToString());
        }
        finally
        {
            if (mysqlReader != null)
                mysqlReader.Close();
        }
    }


    private static void sendResponse(string str, Socket s)
    {
        byte[] responseString = Encoding.ASCII.GetBytes(str);
        s.Send(responseString);
    }

    private static void SendActivationEmail(string email, string userName, string activationCode)
    {
        using (MailMessage mm = new MailMessage("testthisisit@gmail.com", email))
        {
            string acUrl = ("http://localhost:8080/Thisisitac?ActivationCode=" + activationCode);
            MailMessage message = new MailMessage();
            string fromEmail = "testthisisit@gmail.com";
            string password = "1324hzyi";
            string toEmail = email;
            message.From = new MailAddress(fromEmail);
            message.To.Add(toEmail);
            message.Subject = "Thisisit Account Activation";
            string body = "<body>Hello " + userName + ","
                        + "<br /><br />Please click the following link to activate your account"
                        + "<br /><a href = " + acUrl + ">Click here to activate your account.</a>"
                        + "<br /><br />Thanks</body>";
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(body, new ContentType("text/html"));
            message.AlternateViews.Add(htmlView);
            message.IsBodyHtml = true;
            message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                smtpClient.EnableSsl = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(fromEmail, password);

                try
                {
                    smtpClient.Send(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: Failed to send email, {0}", e);
                }
            }
        }
    }

    // Check if userInput does not exist in the database already pre registration
    private static bool isExist(string DBField, string userInput, MySqlConnection mysqlCon)
    {
        bool isExistFlag = false;
        try
        {
            string cmdString = "SELECT COUNT(*) FROM accounts WHERE " + DBField + "= @userInput";
            MySqlCommand cmd = new MySqlCommand(cmdString, mysqlCon);
            cmd.Parameters.AddWithValue("@userInput", userInput);
            cmd.Prepare();
            // Returns 0 if not exists, 1 for duplicate.
            int trueFalse = Convert.ToInt32(cmd.ExecuteScalar());
            if (trueFalse > 0)
            {
                isExistFlag = true;
            }
            else {
                isExistFlag = false;
            }
        }
        catch (MySqlException e)
        {
            Console.WriteLine("Error: {0}", e.ToString());
        }
        return isExistFlag;
    }

    // Encryption from string to MD5 Hash
    private static string StringToMD5(string pass, string salt)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(pass + salt));
        string result = BitConverter.ToString(bytes).Replace("-", String.Empty);
        return result;
    }

    // RNGEncryption generating random Salt
    private static string GenerateSalt()
    {
        using (RNGCryptoServiceProvider salt = new RNGCryptoServiceProvider())
        {
            byte[] data = new byte[4];
            salt.GetBytes(data);
            string result = BitConverter.ToString(data, 0);
            return result;
        }
    }
}

