# csgettext

This is a tool project to assist C# projects in implementing internationalization functionality.

This project will scan all `.cs` files for `private readonly` fields of types `IStringLocalizer` and `IStringLocalizer<T>`.

For more information, please refer to [Microsoft Docs](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/portable-object-localization?view=aspnetcore-8.0)

# How to use

## 1. Install csgettext

```
dotnet tool install -g csgettext
```

## 2. Run csgettext

```
// Generate .pot format file
csgettext -d <input directory> -o <output file>
// Or generate .pot format file and merge with existing .po file
csgettext -d <input directory> -o <output file> --languages <language code>
```
