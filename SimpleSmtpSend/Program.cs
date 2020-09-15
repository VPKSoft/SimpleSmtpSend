#region License
/*
MIT License

Copyright(c) 2020 Petteri Kautonen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using CommandLine;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace SimpleSmtpSend
{
    class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>System.Int32.</returns>
        private static int Main(string[] args)
        {
            try
            {
                // get the default Json file for the settings..
                SettingFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, SettingFileName);

                Settings = Settings.FromJson(SettingFileName);

                // get the command line arguments..
                var arguments = Parser.Default.ParseArguments<Arguments>(args)
                    .WithParsed(CheckArguments).WithNotParsed(NotParsedArguments);

                if (arguments == null)
                {
                    ShowHelp();
                    return 0;
                }

                var hideData = arguments.Value?.HideEncryptedData == true;

                // the user gave the command line argument of -d / --displayConfig to display the current configuration used ny the software..
                if (DisplayConfiguration)
                {
                    Console.WriteLine(@"SMTP configuration:");
                    Console.WriteLine($@"  User name          : {ConcealString(Settings.SmtpUser, hideData)}.");
                    Console.WriteLine($@"  Password           : {ConcealString(Settings.SmtpPassword, hideData)}.");
                    Console.WriteLine($@"  Server             : {ConcealString(Settings.SmtpHost, hideData)}.");
                    Console.WriteLine($@"  SSL/TSL            : {Settings.UseSsl}.");
                    Console.WriteLine($@"  User display name  : {ConcealString(Settings.EmailSenderName, hideData)}.");
                    Console.WriteLine($@"  Email address      : {ConcealString(Settings.EmailAddress, hideData)}.");
                    Console.WriteLine($@"  Authenticate       : {Settings.SmtpAuthenticate}.");
                    return 0;
                }

                bool mailSent = false;

                // if enough of mail send arguments were given, try to send the mail..
                if (MailToAddress != null && EmailBody != null && EmailSubject != null)
                {
                    // create a mail message..
                    var message = new MimeMessage
                    {
                        Subject = EmailSubject, 
                        Body = new TextPart("plain") {Text = EmailBody,},
                    };
                    message.From.Add(new MailboxAddress(Settings.EmailSenderName, Settings.EmailAddress));
                    message.To.Add(new MailboxAddress(MailToReceiverName, MailToAddress));

                    // inform that a message is being tried to send..
                    Console.WriteLine(
                        $@"Sending mail to: '{new MailAddress(MailToAddress, MailToReceiverName)}' with subject of '{EmailSubject}'...");

                    // if no receiver name was given just note about it (nothing serious)..
                    if (MailToReceiverName == null)
                    {
                        Console.WriteLine(@"Note, no mail receiver name:  -n, --name        The received name to use.");
                    }

                    // create a MailKit.Net.Smtp.SmtpClient instance
                    using (var client = new SmtpClient ()) 
                    {
                        // connect the SMTP client..
                        client.Connect (Settings.SmtpHost, Settings.SmtpPort, Settings.UseSsl);

                        // if the SMTP authentication is required, then authenticate..
                        if (Settings.SmtpAuthenticate)
                        {
                            client.Authenticate(Settings.SmtpUser, Settings.SmtpPassword);
                        }

                        // send the email message..
                        client.Send(message);

                        // disconnect the SMTP client..
                        client.Disconnect(true);
                    }

                    // if no exceptions occurred at this point, inform the user that the mail was successfully sent..
                    Console.WriteLine(
                        $@"Mail sent to: '{new MailAddress(MailToAddress, MailToReceiverName)}' with subject of '{EmailSubject}'.");

                    mailSent = true;
                }
                
                // if no changes were made to the settings..
                if (!SettingsChanged)
                {
                    // ..and if no email was sent..
                    if ((MailToAddress != null || EmailBody != null || EmailSubject != null) && !mailSent)
                    {
                        // ..indicate the missing arguments to the user to sent email properly..
                        if (MailToAddress == null)
                        {
                            Console.WriteLine(
                                @"Missing argument:  -r, --receiver    The received email address to use.");
                        }

                        if (EmailBody == null)
                        {
                            Console.WriteLine(@"Missing argument:  -b, --body        The mail body to use.");
                        }

                        if (EmailSubject == null)
                        {
                            Console.WriteLine(@"Missing argument:  -t, --title       The mail title to use.");
                        }
                    }
                    else if (!mailSent) // no settings were changed and no mail was sent, so do show the help..
                    {
                        ShowHelp();
                    }
                }
            }
            catch (Exception ex) // an exception occurred..
            {
                // inform the user of the exception..
                Console.WriteLine(@$"An error occurred: '{ex.Message}'.");

                // if the -v / --verbose arguments was set, also dump the stack trace..
                if (Verbose)
                {
                    Console.WriteLine();
                    Console.WriteLine(ex.StackTrace);
                }

                return -1; // indicate failure..
            }

            // save the settings to the Json file..
            Settings.ToJson(SettingFileName);

            // indicate success..
            return 0;
        }

        /// <summary>
        /// Displays the command line arguments for the software.
        /// </summary>
        private static void ShowHelp()
        {
            Parser.Default.ParseArguments<Arguments>(new[] {"--help"})
                .WithParsed(CheckArguments).WithNotParsed(NotParsedArguments);
        }

        /// <summary>
        /// Process the arguments given to the software.
        /// </summary>
        /// <param name="arguments">A class to hold the parsed arguments for the software.</param>
        static void CheckArguments(Arguments arguments)
        {
            // set the flag that no settings have been changed..
            SettingsChanged = false;

            // if an alternate settings file was defined, use that..
            if (arguments.SettingsFile != null) 
            {
                SettingFileName = arguments.SettingsFile;
                Settings = Settings.FromJson(SettingFileName);
            }

            // set the input encryption to true in case any of the settings will be changed..
            Settings.EncryptInput = true;

            // check if the SMTP host setting was changed..
            if (arguments.SmtpHost != null)
            {
                Settings.SmtpHost = arguments.SmtpHost;
                Console.WriteLine($@"Host set to: '{ConcealString(Settings.SmtpHost, arguments.HideEncryptedData)}'.");
                SettingsChanged = true; // settings were changed, so do set the flag to indicate that..
            }

            // check if the use authentication on SMTP host setting was changed..
            if (arguments.Authenticate != null)
            {
                Settings.SmtpAuthenticate = arguments.Authenticate == "1";
                Console.WriteLine($@"SMTP authentication was set to: '{Settings.SmtpAuthenticate}'.");
                SettingsChanged = true; // settings were changed, so do set the flag to indicate that..
            }

            // check if the email address setting was changed..
            if (arguments.EmailAddress != null)
            {
                Settings.EmailAddress = arguments.EmailAddress;
                Console.WriteLine($@"Email address set to: '{ConcealString(Settings.EmailAddress, arguments.HideEncryptedData)}'.");
                SettingsChanged = true; // settings were changed, so do set the flag to indicate that..
            }

            // check if the SMTP port setting was changed..
            if (arguments.SmtpPort != null)
            {
                try
                {
                    // check if the email address settings was changed..
                    Settings.SmtpPort = Convert.ToUInt16(arguments.SmtpPort);
                    Console.WriteLine($@"SMTP port set to: '{Settings.SmtpPort}'.");
                    SettingsChanged = true; // settings were changed, so do set the flag to indicate that..
                }
                catch
                {
                    Console.WriteLine($@"The given SMTP port was invalid: {arguments.SmtpPort}.");
                }
            }

            // check if the SMTP login setting was changed..
            if (arguments.SmtpLogin != null)
            {
                Settings.SmtpUser = arguments.SmtpLogin;
                Console.WriteLine($@"SMTP login set to: '{ConcealString(arguments.SmtpLogin, arguments.HideEncryptedData)}'.");
                SettingsChanged = true; // settings were changed, so do set the flag to indicate that..
            }

            // check if the display name of the email sender setting was changed..
            if (arguments.SmtpUserName != null)
            {
                Settings.EmailSenderName = arguments.SmtpUserName;
                Console.WriteLine(@$"SMTP user set to: '{ConcealString(Settings.EmailSenderName, arguments.HideEncryptedData)}'.");
                SettingsChanged = true; // settings were changed, so do set the flag to indicate that..
            }

            // check if the SMTP password setting was changed..
            if (arguments.SmtpPassword != null)
            {
                Settings.SmtpPassword = arguments.SmtpPassword;
                Console.WriteLine(@$"SMTP password was set to: '{ConcealString(Settings.SmtpPassword, arguments.HideEncryptedData)}'.");
                SettingsChanged = true; // settings were changed, so do set the flag to indicate that..
            }

            // check if the SMTP use SSL setting was changed..
            if (arguments.UseSsl != null)
            {
                Settings.UseSsl = arguments.UseSsl == "1";
                Console.WriteLine(@$"Use SSL set to: {Settings.UseSsl}.");
                SettingsChanged = true; // settings were changed, so do set the flag to indicate that..
            }


            Verbose = arguments.Verbose;
            if (arguments.Verbose)
            {
                Console.WriteLine(@$"Verbose mode: {true}.");
            }

            DisplayConfiguration = arguments.DisplayConfiguration;
            if (arguments.DisplayConfiguration)
            {
                Console.WriteLine(@$"Display configuration: {true}.");
            }

            Settings.EncryptInput = false;

            MailToAddress = arguments.ReceiverEmailAddress;
            MailToReceiverName = arguments.ReceiverName;
            EmailBody = arguments.EmailBody ?? StandardInput;
            EmailSubject = arguments.MailTitle;
        }

        /// <summary>
        /// Process the argument errors occurred.
        /// </summary>
        /// <param name="errors">The collection of argument parsing errors.</param>
        static void NotParsedArguments(IEnumerable<Error> errors)
        {
            // for future use if required..
        }

        /// <summary>
        /// Gets the standard input (stdio) in case the input is redirected.
        /// </summary>
        /// <value>The standard input as a string in case the input is redirected; otherwise null.</value>        
        public static string StandardInput
        {
            get
            {
                if (Console.IsInputRedirected)
                {
                    using var reader = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding);
                    return reader.ReadToEnd();
                }

                return null;
            }
        }

        /// <summary>
        /// Conceal a string value with a specified <paramref name="concealChar"/> if the <paramref name="conceal"/> is set to true.
        /// </summary>
        /// <param name="value">The string value to conceal.</param>
        /// <param name="conceal">A value indicating whether to return the original string or a new string concealed with repeating the specified character.</param>
        /// <param name="concealChar">The character to use to conceal the string.</param>
        /// <returns>A string length of <paramref name="value"/> concealed with the <paramref name="concealChar"/> if the <paramref name="conceal"/> is set to <c>true</c>; otherwise the original <paramref name="value"/> string.</returns>
        private static string ConcealString(string value, bool conceal, char concealChar = '*')
        {
            if (conceal)
            {
                return new string(concealChar, value.Length);
            }

            return value;
        }

        #region StaticPrivateProperties
        /// <summary>
        /// The setting file name (JSON).
        /// </summary>
        /// <value>The setting file name (JSON).</value>
        private static string SettingFileName { get; set; } = ".SimpleSmtpSend.json";

        /// <summary>
        /// Gets or sets the settings for the software.
        /// </summary>
        /// <value>The settings for the software.</value>
        private static Settings Settings { get; set; }

        /// <summary>
        /// Gets or sets the mail to address to where the email should be sent.
        /// </summary>
        /// <value>The mail to address to where the email should be sent.</value>
        private static string MailToAddress { get; set; }

        /// <summary>
        /// Gets or sets the name of the mail receiver.
        /// </summary>
        /// <value>The name of the mail receiver.</value>
        private static string MailToReceiverName { get; set; }

        /// <summary>
        /// Gets or sets the email body.
        /// </summary>
        /// <value>The email body.</value>
        private static string EmailBody { get; set; }

        /// <summary>
        /// Gets or sets the email subject.
        /// </summary>
        /// <value>The email subject.</value>
        private static string EmailSubject { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether any arguments were passed to the program which would change the settings Json file.
        /// </summary>
        /// <value><c>true</c> if any arguments were passed to the program which would change the settings Json file; otherwise, <c>false</c>.</value>
        private static bool SettingsChanged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show detailed debug information in case of an error.
        /// </summary>
        /// <value><c>true</c> if to show detailed debug information in case of an error; otherwise, <c>false</c>.</value>
        private static bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display the software configuration to the user.
        /// </summary>
        /// <value><c>true</c> if to display the software configuration to the user; otherwise, <c>false</c>.</value>
        private static bool DisplayConfiguration { get; set; }
        #endregion
    }
}
