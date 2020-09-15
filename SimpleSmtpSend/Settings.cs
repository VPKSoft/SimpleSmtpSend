// Copyright (c) 2011 rubicon IT GmbH
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SimpleSmtpSend
{
    /// <summary>
    /// Settings for the application.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Reads the settings from a specified Json file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The Json file deserialized as a new <see cref="Settings"/> class instance.</returns>
        public static Settings FromJson(string fileName)
        {
            Settings NewInstance()
            {
                EncryptInput = true;
                var instance = new Settings()
                {
                    SmtpPort = 587,
                    // ReSharper disable once StringLiteralTypo
                    SmtpHost = "localhost.localdomain",
                    EmailAddress = "localhost@localdomain",
                    SmtpUser = "user",
                    EmailSenderName = "user",
                    SmtpPassword = "password",
                };
                EncryptInput = false;

                return instance;
            }

            Settings result;
            if (!File.Exists(fileName))
            {
                return NewInstance();
            }

            try
            {
                result = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(fileName));
            }
            catch
            {
                return NewInstance();
            }

            return result;
        }

        /// <summary>
        /// Serializes the specified <see cref="Settings"/> class instance to a Json and saves the data to the specified file.
        /// </summary>
        /// <param name="settings">The <see cref="Settings"/> class instance.</param>
        /// <param name="fileName">Name of the file.</param>
        public static void ToJson(Settings settings, string fileName)
        {
            EncryptOutput = true;

            try
            {
                File.WriteAllText(fileName, JsonConvert.SerializeObject(settings));
            }
            catch
            {
                // the encrypted flag must be reset..
            }

            EncryptOutput = false;
        }

        /// <summary>
        /// Converts to Json.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void ToJson(string fileName)
        {
            ToJson(this, fileName);
        }

        /// <summary>
        /// The field for the <see cref="ProtectData"/> property.
        /// </summary>
        private static ProtectDataCore _protectData;

        /// <summary>
        /// Gets the data protection class instance.
        /// </summary>
        /// <value>The data protection class instance.</value>
        public static ProtectDataCore ProtectData => _protectData ??= ProtectDataCore.CreateInstance();

        /// <summary>
        /// Gets or sets the SMTP port.
        /// </summary>
        /// <value>The SMTP port.</value>
        public ushort SmtpPort { get; set; }

        /// <summary>
        /// Gets or sets the SMTP host.
        /// </summary>
        /// <value>The SMTP host.</value>
        public string SmtpHost
        {
            get => GetProtectedValue(nameof(SmtpHost));
            set => SetProtectedValue(nameof(SmtpHost), value);
        }

        /// <summary>
        /// Gets or sets the SMTP user.
        /// </summary>
        /// <value>The SMTP user.</value>
        public string SmtpUser
        {
            get => GetProtectedValue(nameof(SmtpUser));
            set => SetProtectedValue(nameof(SmtpUser), value);
        }

        /// <summary>
        /// Gets or sets the SMTP password.
        /// </summary>
        /// <value>The SMTP password.</value>
        public string SmtpPassword
        {
            get => GetProtectedValue(nameof(SmtpPassword));
            set => SetProtectedValue(nameof(SmtpPassword), value);
        }

        /// <summary>
        /// Gets or sets the name of the email sender.
        /// </summary>
        /// <value>The name of the email sender.</value>
        public string EmailSenderName
        {
            get => GetProtectedValue(nameof(EmailSenderName));
            set => SetProtectedValue(nameof(EmailSenderName), value);
        }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        /// <value>The email address.</value>
        public string EmailAddress
        {
            get => GetProtectedValue(nameof(EmailAddress));
            set => SetProtectedValue(nameof(EmailAddress), value);
        }
                
        /// <summary>
        /// Gets or sets the value whether to use SSL for the SMTP connection.
        /// </summary>
        /// <value>The value whether to use SSL for the SMTP connection.</value>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to authenticate with the SMTP server.
        /// </summary>
        /// <value><c>true</c> if to authenticate with the SMTP server; otherwise, <c>false</c>.</value>
        public bool SmtpAuthenticate { get; set; } = true;

        /// <summary>
        /// Gets or sets the value whether the protected data should be returned as encrypted from the class properties.
        /// </summary>
        /// <value>The value whether the protected data should be returned as encrypted from the class properties.</value>
        [JsonIgnore]
        internal static bool EncryptOutput { get; set; }
        
        /// <summary>
        /// Gets or sets the value whether the protected data should be encrypted when assigning the class property values.
        /// </summary>
        /// <value>The value whether the protected data should be encrypted when assigning the class property values.</value>
        [JsonIgnore]
        internal static bool EncryptInput { get; set; }

        /// <summary>
        /// Sets the protected property value.
        /// </summary>
        /// <param name="valueName">Name of the value.</param>
        /// <param name="value">The value.</param>
        private void SetProtectedValue(string valueName, string value)
        {
            try
            {
                ProtectedValues[valueName] = EncryptInput ? ProtectData.Protect(value) : value;
            }
            catch
            {
                ProtectedValues[valueName] = null;
            }
        }

        /// <summary>
        /// Gets the protected property value.
        /// </summary>
        /// <param name="valueName">Name of the value.</param>
        /// <returns>The property value of a given property name either in encrypted format or as plain text depending on the <see cref="EncryptOutput"/> property value.</returns>
        private string GetProtectedValue(string valueName)
        {
            var value = ProtectedValues[valueName];

            return value != null ? (EncryptOutput ? value : ProtectData.Unprotect(value)) : null;
        }

        /// <summary>
        /// Gets the protected data values.
        /// </summary>
        /// <value>The protected data values.</value>
        private Dictionary<string, string> ProtectedValues { get; } = new Dictionary<string, string>();
    }
}
