dotNETerrorReporter
===================

Library for .NET application, written with Visual Studio 2010 and C#.

Allows a user to save all exceptions' data (and any additional information specified by the user) and immediately send them to external service (website) with use of HTTP request.
The library has VS intellisense documentation, so you should not have any problems with using.

Directories:
------------
"ErrorReporter" - contains library for reporting exceptions to an external service.

"xmlConfigurator" - contains example of using the library.

"ErrorReporter\ErrorReporter\bin\Release\" - the library in .dll format, which can be used with any .NET project

Adding library to a project:
----------------------------
1. After opening your project go to "Solution Explorer" view.
2. There, in a tree of directories find "References" directory, click with right mouse button and choose "Add reference..."
3. In the new window go to "Browse" tab and select "ErrorReporter.dll" file from ErrorReporter\ErrorReporter\bin\Release\ directory of the library
4. Confirm with "OK" button and rebuild your project.
5. The library is now ready to use
