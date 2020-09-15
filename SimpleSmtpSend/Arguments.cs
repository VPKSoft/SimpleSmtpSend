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

using CommandLine;

namespace SimpleSmtpSend
{
    /// <summary>
    /// A class for the command line arguments.
    /// </summary>
    public class Arguments
    {
        /// <summary>
        /// Gets or sets the SMTP password.
        /// </summary>
        /// <value>The SMTP password.</value>
        [Option('w', "password", Required = false, HelpText = "Set the password of the email account to the settings.json file.")]
        public string SmtpPassword { get; set; }

        /// <summary>
        /// Gets or sets the SMTP host.
        /// </summary>
        /// <value>The SMTP host.</value>
        [Option('h', "host", Required = false, HelpText = "Set the host of the email account to the settings.json file.")]
        public string SmtpHost { get; set; }

        /// <summary>
        /// Gets or sets the SMTP port.
        /// </summary>
        /// <value>The SMTP port.</value>
        [Option('p', "port", Required = false, HelpText = "Set the port of the email account to the settings.json file.")]
        public string SmtpPort { get; set; }

        /// <summary>
        /// Gets or sets the name of the SMTP user.
        /// </summary>
        /// <value>The name of the SMTP user.</value>
        [Option('u', "user", Required = false, HelpText = "Set the user display name of the email account to the settings.json file.")]
        public string SmtpUserName { get; set; }

        /// <summary>
        /// Gets or sets the name of the SMTP user.
        /// </summary>
        /// <value>The name of the SMTP user.</value>
        [Option('l', "login", Required = false, HelpText = "Set the login of the email account to the settings.json file.")]
        public string SmtpLogin { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        /// <value>The email address.</value>
        [Option('e', "email", Required = false, HelpText = "Set the email address of the email account to the settings.json file.")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the settings file.
        /// </summary>
        /// <value>The settings file.</value>
        [Option('s', "settings", Required = false, HelpText = "An alternate settings json file to use.")]
        public string SettingsFile { get; set; }

        /// <summary>
        /// Gets or sets the settings file.
        /// </summary>
        /// <value>The settings file.</value>
        [Option('S', "ssl", Required = false, HelpText = "A value indicating whether to use SSL for sending mail (0/1).")]
        public string UseSsl { get; set; }

        /// <summary>
        /// Gets or sets the mail title.
        /// </summary>
        /// <value>The mail title.</value>
        [Option('t', "title", Required = false, HelpText = "The mail title to use.")]
        public string MailTitle { get; set; }

        /// <summary>
        /// Gets or sets the email body.
        /// </summary>
        /// <value>The email body.</value>
        [Option('b', "body", Required = false, HelpText = "The mail body to use.")]
        public string EmailBody { get; set; }

        /// <summary>
        /// Gets or sets the receiver email address.
        /// </summary>
        /// <value>The receiver email address.</value>
        [Option('r', "receiver", Required = false, HelpText = "The received email address to use.")]
        public string ReceiverEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the name of the receiver.
        /// </summary>
        /// <value>The name of the receiver.</value>
        [Option('n', "name", Required = false, HelpText = "The received name to use.")]
        public string ReceiverName { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether to show detailed debug information.
        /// </summary>
        /// <value>The value indicating whether to show detailed debug information.</value>
        [Option('v', "verbose", Default = false, Required = false, HelpText = "Whether to show detailed debug information.")]
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether display configuration to the user.
        /// </summary>
        /// <value><c>null</c> if display configuration to the user, <c>true</c> if [display configuration]; otherwise, <c>false</c>.</value>
        [Option('d', "displayConfig", Default = false, Required = false, HelpText = "Display the current application configurationThe application is asked to display the configuration it has including login information in plain text.")]
        public bool DisplayConfiguration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the SMTP mail send should authenticate with the server.
        /// </summary>
        /// <value><c>true</c> if the SMTP mail send should authenticate with the server; otherwise, <c>false</c>.</value>
        [Option('a', "authenticate", Required = false, HelpText = "An option whether to use authentication with the SMTP server (0/1).")]
        public string Authenticate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide encrypted data on output to console.
        /// </summary>
        /// <value><c>true</c> if [hide encrypted data]; otherwise, <c>false</c>.</value>
        [Option('c', "conceal", Default = false, Required = false, HelpText = "Use this argument to hide also the protected data of the program settings upon setting a setting value or upon displaying the configuration data with the -d argument.")]
        public bool HideEncryptedData { get; set; }
    }
}
