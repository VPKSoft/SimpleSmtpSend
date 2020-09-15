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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SimpleSmtpSend
{
    /// <summary>
    /// A class to encrypt or decrypt strings using <see cref="AesManaged"/> using base-64 encoding for the encrypted data.
    /// </summary>
    public class EncryptDecryptTextAesBase64
    {
        /// <summary>
        /// Encrypts a string using <see cref="AesManaged"/> class with padding mode of <see cref="PaddingMode.ISO10126"/>.
        /// </summary>
        /// <param name="value">The string value to encrypt.</param>
        /// <param name="password">The password to use for the encryption.</param>
        /// <returns>A string containing the data and the initialization vector in base-64 encoded binary format.</returns>
        public static string EncryptString(string value, string password)
        {
            if (password.Length < 6)
            {
                throw new ArgumentOutOfRangeException(nameof(password), @"At least six digits is required for the password.");
            }

            while (password.Length < 32)
            {
                password += " ";
            }

            using var aes = new AesManaged {Padding = PaddingMode.ISO10126, KeySize = 256};

            aes.GenerateIV();
            aes.Key = Encoding.UTF8.GetBytes(password.ToCharArray(), 0, 32);
            
            var ivString = Convert.ToBase64String(aes.IV);

            using var memoryStream = new MemoryStream();

            // ReSharper disable once IdentifierTypo
            using var encryptor = aes.CreateEncryptor();

            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

            cryptoStream.Write(Encoding.UTF8.GetBytes(value));
            cryptoStream.FlushFinalBlock();

            return ivString + "|" +
                   Convert.ToBase64String(memoryStream.ToArray());
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="base64Value"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string DecryptString(string base64Value, string password)
        {
            if (password.Length < 6)
            {
                throw new ArgumentOutOfRangeException(nameof(password), @"At least six digits is required for the password.");
            }

            while (password.Length < 32)
            {
                password += " ";
            }

            var ivBase64 = base64Value.Split('|').FirstOrDefault();
            var valueBase64 = base64Value.Split('|').LastOrDefault();

            if (ivBase64 == null || valueBase64 == null) // the data provided was invalid..
            {
                throw new ArgumentNullException(nameof(base64Value));
            }

            var iv = Convert.FromBase64String(ivBase64);
            var value = Convert.FromBase64String(valueBase64);

            using var aes = new AesManaged
            {
                Key = Encoding.UTF8.GetBytes(password.ToCharArray(), 0, 32),
                IV = iv,
                Padding = PaddingMode.ISO10126,
                KeySize = 256,
            };


            using var memoryStream = new MemoryStream();
            memoryStream.Write(value.AsSpan());
            memoryStream.Position = 0;

            // ReSharper disable once IdentifierTypo
            using var decryptor = aes.CreateDecryptor();

            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            var bytes = new byte[1000000]; // this should be enough for this purpose..

            var readAmount = cryptoStream.Read(bytes, 0, 1000000);

            var result =  Encoding.UTF8.GetString(bytes, 0, readAmount);

            return result;

//            return Convert.FromBase64String(cryptoStream.r)
        }
        
    }
}
