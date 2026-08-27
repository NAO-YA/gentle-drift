# Gentle Drift for Windows

This WPF host turns the existing Canvas experience into a native Windows screen saver.

- `/s`: runs on every connected display and exits when the user moves the mouse or presses a key.
- `/c`: opens the normal, interactive configuration window. Its existing controls save choices locally.
- `/p <HWND>`: embeds a preview in the Windows Screen Saver Settings dialog.

The host uses WebView2 and bundles the shared `../index.html` at `Web/index.html` during publish. A Windows build is created by the GitHub Actions release workflow.

## Local Windows build

```powershell
dotnet publish .\GentleDrift.ScreenSaver.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
Copy-Item .\publish\GentleDrift.exe .\publish\GentleDrift.scr
```

Keep the generated `Web` directory beside `GentleDrift.scr`.
