using DotNetEnv;
using MimeKit;
using Npgsql;
using StudyCenter.System;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace StudyCenter.System {

public class NotificationEvent : BackgroundService
{

    private readonly Postgresql _connection;
    private readonly Mail _client;

    public NotificationEvent(Postgresql connection, Mail client)
    {
        _connection = connection;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await NotificationProcess();
        }

        catch(Exception ex)
        {
            Console.WriteLine($"{ex}, Error in start process..");
        }

        while(!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextMidNight = now.Date.AddDays(1);
            var delay = nextMidNight - now;

            Console.WriteLine($"Next process: {nextMidNight}");

            await Task.Delay(delay, stoppingToken);

            if(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine("Midnight process is started...");
                    await NotificationProcess();
                }

                catch (Exception ex)
                {
                    Console.WriteLine($"{ex}, Error in midnight process...");
                }
            }
        }
    }

    private async Task NotificationProcess()
    {
        Env.Load();

        var conn = await _connection.GetOpenConnectionAsync();
        var today = DateTime.Today;

        await using(var calendar = new NpgsqlCommand("SELECT calendar.*, users.username, users.email FROM calendar JOIN users ON users.id = calendar.users_id WHERE calendar.notification_date <= @notification_date AND calendar.date >= @date and SEEN = true", conn))
        {
            calendar.Parameters.AddWithValue("notification_date", today);
            calendar.Parameters.AddWithValue("date", today);

            await using(var reader = await calendar.ExecuteReaderAsync())
            {

            var SmtpClient = _client.CreateSMTPClient();

            while(await reader.ReadAsync())
            {
                var eventTitle = reader.GetString(reader.GetOrdinal("title"));
                var eventDesc = reader.GetString(reader.GetOrdinal("descriptions"));
                var eventDate = reader.GetDateTime(reader.GetOrdinal("date"));
                var daysLeft = (eventDate.Date - DateTime.Today).Days;

                string note = (daysLeft == 0 || daysLeft == -1)
                ? $"The {eventTitle} is today. {eventDesc} IMPORTANT! Good luck with that.\n"
                : $"The {eventTitle} is in {daysLeft} day(s). {eventDesc} IMPORTANT! This is a reminder so you don't forget.\n";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Study Center", Environment.GetEnvironmentVariable("SMTP_USERNAME")));
                message.To.Add(new MailboxAddress("", reader.GetString(reader.GetOrdinal("email"))));
                message.Subject = "Notification";
                message.Body = new TextPart("plain")
                {
                    Text = 
                    $"Hello, {reader.GetString(reader.GetOrdinal("username"))} \n" +
                    note +
                    "Have a nice day, Study Center"
                };

                SmtpClient.Send(message);
            }

            SmtpClient.Disconnect(true);
        }
    }

        await using(var wordstudy_folder = new NpgsqlCommand("SELECT wordstudy_folder.*, users.username, users.email FROM wordstudy_folder JOIN users ON users.id = wordstudy_folder.users_id WHERE wordstudy_folder.notification_date <= @notification_date AND wordstudy_folder.end_date >= @end_date and SEEN = true", conn))
        {
            wordstudy_folder.Parameters.AddWithValue("notification_date", today);
            wordstudy_folder.Parameters.AddWithValue("end_date", today);

            await using(var reader = await wordstudy_folder.ExecuteReaderAsync())
            {

                var SmtpClient = _client.CreateSMTPClient();

                while(await reader.ReadAsync())
                    {
                    
                    var email = reader.GetString(reader.GetOrdinal("email"));
                    var username = reader.GetString(reader.GetOrdinal("username"));
                    var name = reader.GetString(reader.GetOrdinal("name"));

                    var notification_date = reader.GetDateTime(reader.GetOrdinal("notification_date"));
                    var end_date = reader.GetDateTime(reader.GetOrdinal("end_date"));

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Study Center", Environment.GetEnvironmentVariable("SMTP_USERNAME")));
                    message.To.Add(new MailboxAddress("", email));
                    message.Subject = "Notification";
                    message.Body = new TextPart("plain")
                    {
                        Text = 
                    $"Hello, {username} \n" +
                    $"You are receiving this letter because you requested notification for the WordStudy folder named {name}. \n" +
                    $"You will receive this email from {notification_date} to {end_date}. If you do not wish to receive messages, please log in to the Study Center. \n" +
                    "Have a nice day, Study Center"
                    };

                    SmtpClient.Send(message);
                    }

                    SmtpClient.Disconnect(true);
                
                }
            }

        await using(var wordstudy_sett = new NpgsqlCommand("SELECT wordstudy_sett.*, users.username, users.email FROM wordstudy_sett JOIN users ON users.id = wordstudy_sett.users_id WHERE wordstudy_sett.notification_date <= @notification_date AND wordstudy_sett.end_date >= @end_date and SEEN = true", conn))
        {
            wordstudy_sett.Parameters.AddWithValue("notification_date", today);
            wordstudy_sett.Parameters.AddWithValue("end_date", today);

            await using(var reader = await wordstudy_sett.ExecuteReaderAsync())
            {

                var SmtpClient = _client.CreateSMTPClient();

                while(await reader.ReadAsync())
                    {
                    
                    var email = reader.GetString(reader.GetOrdinal("email"));
                    var username = reader.GetString(reader.GetOrdinal("username"));
                    var name = reader.GetString(reader.GetOrdinal("name"));

                    var notification_date = reader.GetDateTime(reader.GetOrdinal("notification_date"));
                    var end_date = reader.GetDateTime(reader.GetOrdinal("end_date"));

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Study Center", Environment.GetEnvironmentVariable("SMTP_USERNAME")));
                    message.To.Add(new MailboxAddress("", email));
                    message.Subject = "Notification";
                    message.Body = new TextPart("plain")
                    {
                        Text = 
                    $"Hello, {username} \n" +
                    $"You are receiving this letter because you requested notification for the WordStudy sett named {name}. \n" +
                    $"You will receive this email from {notification_date} to {end_date}. If you do not wish to receive messages, please log in to the Study Center. \n" +
                    "Have a nice day, Study Center"
                    };

                    SmtpClient.Send(message);
                    }

                    SmtpClient.Disconnect(true);
                
                }
            }

            await conn.CloseAsync();

        }
    }
}
