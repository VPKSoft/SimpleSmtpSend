# SimpleSmtpSend
A .NET Core command line program to send email using SMTP. Works under Linux and Windows (propably Mac too).

The server settings are encrypted in a Json file.

## Sample:
*./SimpleSmtpSend  -n "VPKSoft" -r address@vpksoft.net -t "Hello!" < EmailBodyPipeLoremIpsum.txt*
