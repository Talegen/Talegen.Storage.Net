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

### Markdown

Markdown is a lightweight and easy-to-use syntax for styling your writing. It includes conventions for

```markdown
Syntax highlighted code block

# Header 1
## Header 2
### Header 3

- Bulleted
- List

1. Numbered
2. List

**Bold** and _Italic_ and `Code` text

[Link](url) and ![Image](src)
```

For more details see [GitHub Flavored Markdown](https://guides.github.com/features/mastering-markdown/).

### Jekyll Themes

Your Pages site will use the layout and styles from the Jekyll theme you have selected in your [repository settings](https://github.com/Talegen/Talegen.Storage.Net/settings/pages). The name of this theme is saved in the Jekyll `_config.yml` configuration file.

### Support or Contact

Having trouble with Pages? Check out our [documentation](https://docs.github.com/categories/github-pages-basics/) or [contact support](https://support.github.com/contact) and we’ll help you sort it out.
