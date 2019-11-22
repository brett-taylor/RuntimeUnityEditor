# Runtime Unity Editor For Subnautica + QMods

### Subnautica + QMods Fork
This fork is to work with Subnautica and in particular the QMods framework. This will be maintained to stay working with the latest Subnautica / QMods updates. It will not be updated to be in parity with ManlyMarco/RuntimeUnityEditor.

### Description
In-game inspector, editor and interactive console for Subnautica. It's designed for debugging and modding Subnautica.

### Features
- Works on Subnautica via the QMods framework.
- GameObject and component browser
- Object inspector that allows modifying values of objects in real time
- REPL C# console
- All parts are integrated together (e.g. REPL console can access inspected object, inspector can focus objects on GameObject list, etc.)

![preview](https://user-images.githubusercontent.com/39247311/64476158-ce1a4c00-d18b-11e9-97d6-084452cdbf0a.PNG)


### How to use
- Build the project as a class library with .net framework 3.5
- The mcs dependency is this https://github.com/kkdevs/mcs - basically a port of roslyn-level mcs with all new language features to .Net 2.0
- Create a QMods mod folder. The entry point to the mod is `RuntimeUnityEditorForSubnautica.EntryPoint.Entry`
- Copy `mcs.dll`, `RuntimeUnityEditor.Core.dll`, `RuntimeUnityEditorForSubnautica.dll` to the mod folder.

---
##### Supporting Development
I really didn't do much, if you love this tool so much the original author had this message:
`You can support development of my plugins through my Patreon page: https://www.patreon.com/ManlyMarco`
