## Talegen.Storage.Net Documentation

A **TRUELY SIMPLE** .NET Storage Abstraction Library for Cloud and Local Storage.

This repository ([Talegen.Storage.Net](https://github.com/Talegen/Talegen.Storage.Net)) is where we (Talegen) develop the Talegen Storage Abstraction Library. The source code here is available to everyone under the standard [Apache 2.0 license](https://github.com/talegen/Talegen.PureBlue.Models/blob/main/LICENSE).

## Why

The purpose of this library is to provide a very simplistic abstraction over various directory and file storage systems typically used by applications for storing large data files at rest locally and/or in the cloud.

There's typically a need to write applications that can provide a shared storage experience with other instances of the application, either via a multi-user shared storage experience, or a scaling scenario where multiple application instances need access to the same stored directories and files.

This can sometime fall short when using local disk storage as load-balanced instances of the application may not share the same disk storage.

.NET's [Directory](https://docs.microsoft.com/en-us/dotnet/api/system.io.directory) and [File](https://docs.microsoft.com/en-us/dotnet/api/system.io.file) classes are great abstractions over lower-level disk IO interactions, but they work with the local disk only. What if we could write a storage abstraction with similar simple methods, that allows you to do similar directory and file interactions but with a cloud provider like Azure, or an in-memory implementation for diskless unit testing?

There are plenty of storage abstraction libraries out on GitHub, but none of them took the approach of a very simplistic storage service implementation. In most cases, we can write the underlying implementation code for the simple directory and file methods found in System.IO. 

We set out to create a library that does the minimum to get the common jobs we need to get done, and to this point, **Talegen.Storage.Net** was born.

## Where

This library source code is available on GitHub to everyone under the standard [Apache 2.0 license](https://github.com/talegen/Talegen.PureBlue.Models/blob/main/LICENSE). The repository is located at Talegen.Storage.Net](https://github.com/Talegen/Talegen.Storage.Net).

## How

We really like simplicity, so there are a couple of options for implementing this storage library. For specifics on which commands are available for each implementation, you can visit the class reference documentation [here](https://talegen.github.io/Talegen.Storage.Net/ref/Talegen.Storage.Net.Core.html).

#### Interfaces

The service is comprised of two simple interface classes. [```IStorageContext```](https://talegen.github.io/Talegen.Storage.Net/ref/Talegen.Storage.Net.Core.IStorageContext.html) and [```IStorageService```](https://talegen.github.io/Talegen.Storage.Net/ref/Talegen.Storage.Net.Core.IStorageService.html). These two interfaces are used to implement a [Local Disk](https://talegen.github.io/Talegen.Storage.Net/ref/Talegen.Storage.Net.Core.Disk.html), [Memory](https://talegen.github.io/Talegen.Storage.Net/ref/Talegen.Storage.Net.Core.Memory.html), and [Azure File Share](https://talegen.github.io/Talegen.Storage.Net/ref/Talegen.Storage.Net.AzureBlobs.html) storage implementation.

#### Exceptions

The library has a single exception type, [```StorageException```](https://talegen.github.io/Talegen.Storage.Net/ref/Talegen.Storage.Net.Core.StorageException.html) that is thrown for any exception that is thrown by internal errors within the storage service. In most cases, you should catch these exceptions to log and resolve any issues that occur within the library.

## Examples

As with any flexible library, there are a couple of ways to construct your service, but you'll want to know which type of storage implementation you would like to instantiate.

```c#
// create a new local disk storage context, using a temp root path, and set unique sub-folder workspace to true. 
string rootPath = Path.Combine(Path.GetTempPath(), "StorageTesting");
IStorageContext context = new LocalStorageContext(rootPath, true); 
IStorageService service = new LocalStorageService(context);
```

Alternatively, you can create a new service using similar context parameters:

```c#
// create a new local disk storage service, using a temp root path, and set unique sub-folder workspace to true. 
string rootPath = Path.Combine(Path.GetTempPath(), "StorageTesting");
IStorageService service = new LocalStorageService(rootPath, true); 
```

The example above will create a temporary storage folder named "StorageTesting", and a sub-folder for the workspace consisting of random characters under this root working folder.

#### Creating A Directory

```c#
// create a directory named "Sub-Folder". 
string rootPath = Path.Combine(Path.GetTempPath(), "StorageTesting");
// Something like C:\Users\username\AppData\Local\Temp\StorageTesting
IStorageService service = new LocalStorageService(rootPath, true);

// Context .RootPath will be similar to C:\Users\username\AppData\Local\Temp\StorageTesting\{random-characters}

// Paths are always relative to the root working folder
string absolutePath = service.CreateDirectory("Sub-Folder");

Console.WriteLine(absolutePath);
// C:\Users\username\AppData\Local\Temp\StorageTesting\{random-characters}\Sub-Folder\
Console.WriteLine("\"{0}\" exists = \"{1}\"", absolutePath, service.DirectoryExists(absolutePath));
// "C:\Users\username\AppData\Local\Temp\StorageTesting\{random-characters}\Sub-Folder\" exists = "True"

```

#### Creating A File & Reading A File

```C#
string rootPath = Path.Combine(Path.GetTempPath(), "StorageTesting");
IStorageService service = new LocalStorageService(rootPath, true);
string testContents = "write this to storage.";
string fileName = "readme.txt";
service.WriteTextFile(fileName, testContents);

if (service.FileExists(fileName))
{
    string contentsRead = service.ReadTextFile(sourceFilePath);
    Console.WriteLine("Contents = \"{0}\"", contentsRead);
    // Contents = "write this to storage."
}

```

