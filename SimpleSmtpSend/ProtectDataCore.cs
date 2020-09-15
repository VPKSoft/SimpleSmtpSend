﻿// Copyright (c) 2011 rubicon IT GmbH
using System;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleSmtpSend
{
    /// <summary>
    /// A class to store string data using <see cref="IDataProtector"/> interface.
    /// </summary>
    public class ProtectDataCore // modified sample from Microsoft: https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/using-data-protection?view=aspnetcore-3.1
    {
        /// <summary>
        /// Creates the instance of the <see cref="ProtectDataCore"/> class.
        /// </summary>
        /// <returns>An instance to the <see cref="ProtectDataCore"/> class.</returns>
        public static ProtectDataCore CreateInstance()
        {
            // add data protection services
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDataProtection();
            var services = serviceCollection.BuildServiceProvider();

            // create an instance of MyClass using the service provider
            var instance = ActivatorUtilities.CreateInstance<ProtectDataCore>(services);
            return instance;
        }

        /// <summary>
        /// Gets or sets the instance implementing the <see cref="IDataProtector"/> interface.
        /// </summary>
        /// <value>The instance implementing the <see cref="IDataProtector"/> interface.</value>
        private IDataProtector Protector { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectDataCore"/> class.
        /// </summary>
        /// <param name="provider">The provider, which is provided by the DI (Dependency Injection) in the <see cref="CreateInstance"/> method.</param>
        public ProtectDataCore(IDataProtectionProvider provider)
        {
            Protector = provider.CreateProtector("SimpleSmtpSend.ProtectDataCore");
        }

        /// <summary>
        /// Cryptographically protects a piece of plain text data.
        /// </summary>
        /// <param name="value">The plain text data to protect.</param>
        /// <returns>A protected form of the plain text data in base-64 encoded format.</returns>
        public string Protect(string value)
        {
            try
            {
                return Convert.ToBase64String(Protector.Protect(Encoding.UTF8.GetBytes(value)));
            }
            catch // on fail do not return the value which was supposed to be protected..
            {
                return null;
            }
        }

        /// <summary>
        /// Cryptographically unprotects a piece of protected data in base-64 encoded format.
        /// </summary>
        /// <param name="base64Value">The protected data to unprotect.</param>
        /// <returns>The plain text form of the protected data.</returns>
        public string Unprotect(string base64Value)
        {
            try
            {
                return Encoding.UTF8.GetString(Protector.Unprotect(Convert.FromBase64String(base64Value)));
            }
            catch // this would in most cases indicate a invalid base-64 encoding..
            {
                return base64Value;
            }
        }
    }
}
