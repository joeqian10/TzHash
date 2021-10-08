# TzHashCLangLibrary

The purpose of this project is the C# neo3 node can use TzHash in c language because it's almost 10 time faster than the c# version.

## Setup

### add the following two lines where you need to use TzHashCLangLibrary

 ```C#
 [DllImport("TzHashCLangLibrary", EntryPoint = "hash", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
 public static extern void Hash(byte[] data, int n, ref ulong sb);
```

### build the project to get dynamic library

#### on Windows

- install Visual Studio with C/C++ desktop development support
- open this project in Visual Studio
- set the `Configuration Type` to `Dynamic Library (.dll)` in project property
- make sure the `Solution Platforms` is `x64` based
- build the project
- copy the built TzHashCLangLibrary.dll to your app's running directory

#### on Linux (Ubuntu 18.04, other version may differ)

- download this project

```bash
git clone https://github.com/joeqian10/TzHash.git
```

- enter the source files directory

```bash
cd TzHash/TzHashCLangLibrary
```

- generate the .so file and put it into /usr/lib

```bash
sudo gcc -fPIC -mssse3 -shared -o /usr/lib/libTzHashCLangLibrary.so ./*
```
