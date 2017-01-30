using System;
using System.Net;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace ServersManager
{
    public class HTTPServer
    {
       
        public static void StartServer()
        {
            var listener = new HttpListener();

            listener.Prefixes.Add("http://localhost:8080/");
            listener.Prefixes.Add("http://127.0.0.1:8080/");

            listener.Start();
            Console.WriteLine("HTTPServer :: started on 127.0.0.1:8080");
            while (true)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext(); //Block until a connection comes in
                    context.Response.StatusCode = 200;
                    context.Response.SendChunked = true;
                    string clientIP = context.Request.RemoteEndPoint.ToString();
                    AccountActivation(context);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error HTTPServer :: {0}", e);
                }
            }
        }

        private static void AccountActivation(HttpListenerContext context)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
            string activationCode = !string.IsNullOrEmpty(context.Request.QueryString["ActivationCode"]) ? context.Request.QueryString["ActivationCode"] : Guid.Empty.ToString();
            Console.WriteLine(activationCode);
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand("UPDATE accounts SET ActivationCode='VERIFIED' WHERE ActivationCode=@ActivationCode"))
                {
                    using (MySqlDataAdapter sda = new MySqlDataAdapter())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@ActivationCode", activationCode);
                        cmd.Connection = con;
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        con.Close();
                        if (rowsAffected == 1)
                        {
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes("<HTML><BODY> Account verified </BODY></HTML>");
                            context.Response.OutputStream.Write(buffer,0, buffer.Length);
                            Console.WriteLine("HTTPServer :: Account Verified");
                        }
                        else
                        {
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes("<HTML><BODY> Verification code doesn't exist </BODY></HTML>");
                            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                            Console.WriteLine("Account Failed to Verify");
                        }
                    }
                }

            }
        }
    }
}