# Talegen.Storage.Net
A **TRUELY SIMPLE** .NET Storage Abstraction Library for Cloud and Local Storage.

This repository ([Talegen.Storage.Net](https://github.com/Talegen/Talegen.Storage.Net)) is where we (Talegen) develop the Talegen Storage Abstraction Library. The source code here is available to everyone under the standard [Apache 2.0 license](https://github.com/talegen/Talegen.PureBlue.Models/blob/main/LICENSE).

## Why

The purpose of this library is to provide a very simplistic abstraction over various directory and file storage systems typically used by applications for storing large data files at rest locally and/or in the cloud.

There's typically a need to write applications that can provide a shared storage experience with other instances of the application, either via a multi-user shared storage experience, or a scaling scenario where multiple application instances need access to the same stored directories and files.

This can sometime fall short when using local disk storage as load-balanced instances of the application may not share the same disk storage.

.NET's [Directory](https://docs.microsoft.com/en-us/dotnet/api/system.io.directory) and [File](https://docs.microsoft.com/en-us/dotnet/api/system.io.file) classes are great abstractions over lower-level disk IO interactions, but they work with the local disk only. What if we could write a storage abstraction with similar simple methods, that allows you to do similar directory and file interactions but with a cloud provider like Azure, or an in-memory implementation for diskless unit testing?

There are plenty of storage abstraction libraries out on GitHub, but none of them took the approach of a very simplistic storage service implementation. In most cases, we can write the underlying implementation code for the simple directory and file methods found in System.IO. 

We set out to create a library that does the minimum to get the common jobs we need to get done, and to this point, **Talegen.Storage.Net** was born.

## Documentation

For a general overview of the library, please visit the [Talegen.Storage.Net documentation online](https://talegen.github.io/Talegen.Storage.Net/).

## Contributing

There are many ways in which you can participate in the project, for example:

 - Submit bugs and feature requests, and help us verify as they are checked in.
 - Review source code changes.
 - Review the documentation and make pull requests for anything from typos to new content. 

If you are interested in fixing issues and contributing directly to the code base, please see the document How to Contribute, which covers the following:

 - How to build and run from source
 - The development workflow, including debugging and running tests.
 - Coding and style guidelines
 - Submitting code changes (pull requests)
 - Contributing to translations

## Feedback

 - Ask a question on [Stack Overflow](https://stackoverflow.com/questions/tagged/Talegen)
 - [Request a new feature](https://github.com/talegen/Talegen.Storage.Net/blob/main/CONTRIBUTING.md).
 - Up vote [popular feature requests](https://github.com/talegen/Talegen.Storage.Net/issues?q=is:open%20is:issue%20label:feature-request%20sort:reactions-%2b1-desc).
 - [File an issue](https://github.com/talegen/Talegen.Storage.Net/issues).
 - Follow [@TalegenInc](https://twitter.com/TalegenInc) and let us know what you think!

## Related Projects

Many projects and products rely on this Talegen Backchannel library. Many of these projects live in their own repositories on GitHub. 

## Code of Conduct

This project has adopted the [Talegen Open Source Code of Conduct](https://talegen.com/open-source-code-of-conduct/). For more information see the Code of Conduct FAQ or [contact us](https://talegen.com/contact/) with additional questions or comments.

## License

Copyright &copy; Talegen, LLC. All rights reserved.
