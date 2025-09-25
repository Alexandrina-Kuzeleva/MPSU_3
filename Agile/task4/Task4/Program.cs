using System;

namespace NotificationsSystem
{
    public class Notifier
    {
        private string _sender;
        private bool _enabled;

        public string Sender
        {
            get { return _sender; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Sender cannot be empty or null");
                _sender = value;
            }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set 
            { 
                _enabled = value; 
            }
        }

        public Notifier(string sender, bool enabled = true)
        {
            Sender = sender;
            Enabled = enabled;
        }

        public void Enable()
        {
            Enabled = true;
        }

        public void Disable()
        {
            Enabled = false;
        }
        
        public virtual string Send(string to, string text)
        {
            if (!Enabled)
                return "Disabled";

            return $"Base send to {to}: {text}";
        }

        public string GetStatus()
        {
            return $"Sender: {Sender}, Enabled: {Enabled}";
        }
    }

    public class EmailNotifier : Notifier
    {
        private string _smtpHost;

        public string SmtpHost
        {
            get { return _smtpHost; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("SMTP host cannot be empty");
                _smtpHost = value;
            }
        }

        public EmailNotifier(string sender, string smtpHost, bool enabled = true) 
            : base(sender, enabled)
        {
            SmtpHost = smtpHost;
        }

        public void Configure(string host)
        {
            SmtpHost = host;
            Console.WriteLine($"Email notifier configured with SMTP host: {host}");
        }

        public override string Send(string to, string text)
        {
            if (!Enabled)
                return "Email notifier disabled";

            if (string.IsNullOrWhiteSpace(to) || !to.Contains("@"))
                return "Invalid email address";

            return $"[EMAIL via {SmtpHost}] From: {Sender}, To: {to}, Message: {text}";
        }

        public string GetEmailConfig()
        {
            return $"Email notifier - Sender: {Sender}, SMTP: {SmtpHost}, Enabled: {Enabled}";
        }
    }

    public class SmsNotifier : Notifier
    {
        private string _provider;

        public string Provider
        {
            get { return _provider; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Provider cannot be empty");
                _provider = value;
            }
        }

        public SmsNotifier(string sender, string provider, bool enabled = true) 
            : base(sender, enabled)
        {
            Provider = provider;
        }

        public void SetProvider(string provider)
        {
            Provider = provider;
            Console.WriteLine($"SMS provider changed to: {provider}");
        }

        public string GetSmsInfo()
        {
            return $"SMS notifier - Provider: {Provider}, Sender: {Sender}, Enabled: {Enabled}";
        }
    }

    public class SecureSmsNotifier : SmsNotifier
    {
        private bool _encrypted;

        public bool Encrypted
        {
            get { return _encrypted; }
            set { _encrypted = value; }
        }

        public SecureSmsNotifier(string sender, string provider, bool encrypted = false, bool enabled = true) 
            : base(sender, provider, enabled)
        {
            Encrypted = encrypted;
        }

        public void EnableEncryption(bool on)
        {
            Encrypted = on;
            Console.WriteLine($"Encryption {(on ? "enabled" : "disabled")} for secure SMS");
        }

        public override string Send(string to, string text)
        {
            if (!Enabled)
                return "Secure SMS notifier disabled";

            if (string.IsNullOrWhiteSpace(to) || to.Length < 5)
                return "Invalid phone number";

            string encryptionStatus = Encrypted ? "[ENCRYPTED] " : "";
            return $"{encryptionStatus}[SECURE SMS via {Provider}] From: {Sender}, To: {to}, Message: {text}";
        }

        public string GetSecurityInfo()
        {
            return $"Secure SMS - Provider: {Provider}, Encrypted: {Encrypted}, Sender: {Sender}";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Notification System Demo");

            Console.WriteLine("1. Base Notifier:");
            Notifier baseNotifier = new Notifier("System Admin");
            Console.WriteLine(baseNotifier.GetStatus());
            Console.WriteLine(baseNotifier.Send("user@test.com", "Hello from base"));
            baseNotifier.Disable();
            Console.WriteLine(baseNotifier.Send("user@test.com", "This should be disabled"));
            Console.WriteLine();

            Console.WriteLine("2. Email Notifier:");
            EmailNotifier emailNotifier = new EmailNotifier("noreply@company.com", "smtp.company.com");
            emailNotifier.Configure("new-smtp.company.com");
            Console.WriteLine(emailNotifier.GetEmailConfig());
            Console.WriteLine(emailNotifier.Send("client@example.com", "Welcome email"));
            Console.WriteLine();

            Console.WriteLine("3. SMS Notifier:");
            SmsNotifier smsNotifier = new SmsNotifier("Bank", "Twilio");
            smsNotifier.SetProvider("Nexmo");
            Console.WriteLine(smsNotifier.GetSmsInfo());
            Console.WriteLine(smsNotifier.Send("+1234567890", "Your code is 1234"));
            Console.WriteLine();

            Console.WriteLine("4. Secure SMS Notifier:");
            SecureSmsNotifier secureSms = new SecureSmsNotifier("Secure Bank", "Vonage", false);
            secureSms.EnableEncryption(true);
            Console.WriteLine(secureSms.GetSecurityInfo());
            Console.WriteLine(secureSms.Send("+0987654321", "Secret transaction code"));
            secureSms.EnableEncryption(false);
            Console.WriteLine(secureSms.Send("+0987654321", "Normal message"));
            Console.WriteLine();

            Console.WriteLine("5. Polymorphism Demo:");
            Notifier[] notifiers = {
                new Notifier("Generic"),
                new EmailNotifier("mail@test.com", "smtp.test.com"),
                new SmsNotifier("SMS Service", "ProviderX"),
                new SecureSmsNotifier("Secure Service", "ProviderY", true)
            };

            foreach (var notifier in notifiers)
            {
                Console.WriteLine(notifier.Send("recipient@test.com", "Polymorphic message"));
            }
        }
    }
}