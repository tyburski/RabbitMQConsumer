using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "orders",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    var order = JsonConvert.DeserializeObject<Order>(message);

    if(order.isCompleted == true && order.isSent == false)
    {
        Console.WriteLine($"Zamówienie [{order.Id}] jest w trakcie realizacji.");

        SendEmail(order.Email, 
            $"Twoje zamówienie o numerze {order.Id} jest w trakcie realizacji. Powiadomimy Cię gdy zostanie wysłane.");
        Console.WriteLine($"Status został wysłany na : {order.Email}");
    }
    if (order.isCompleted == true && order.isSent == true)
    {
        Console.WriteLine($"Zamówienie [{order.Id}] zostało wysłane.");
        SendEmail(order.Email,
            $"Twoje zamówienie o numerze {order.Id} zostało wysłane. Dziękujemy za skorzystanie z naszych usług.");
        Console.WriteLine($"Status został wysłany na : {order.Email}");
    }
};
channel.BasicConsume(queue: "orders",
                     autoAck: true,
                     consumer: consumer);

Console.WriteLine(" Naciśnij [Enter] aby wyjść.");
Console.ReadLine();


void SendEmail(string email, string message)
{
    var fromAddress = new MailAddress("tyburskid7@gmail.com");
    var toAddress = new MailAddress(email);
    const string fromPassword = ".";
    const string subject = "Sklep";

    var smtp = new SmtpClient
    {
        Host = "smtp.gmail.com",
        Port = 587,
        EnableSsl = true,
        DeliveryMethod = SmtpDeliveryMethod.Network,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
    };
    using (var mailMessage = new MailMessage(fromAddress, toAddress)
    {
        Subject = subject,
        Body = message
    })
    {
        smtp.Send(mailMessage);
    }
}