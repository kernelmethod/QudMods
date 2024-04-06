# submodule-management

**Table of contents**

- [Rationale](#rationale)
- [Usage instructions](#usage-instructions)
- [Current limitations](#current-limitations)
- [Best practices](#best-practices)

This directory contains the source for the submodule-management mod. It is
designed to be used by other modders to make it easier to toggle different
options in mods. This mod is intended to be included alongside other mods,
rather than used as a standalone mod.

## Rationale

submodule-management is a helper mod, designed to allow other modders to
conditionally load XML for their mods. The intention is to make it easier to
enable or disable object blueprint changes, population table merges, and more,
all via the options menu.

### Base game features

There are currently two outstanding feature requests functionality similar to
what this mod offers:

https://bitbucket.org/bbucklew/cavesofqud-public-issue-tracker/issues/8720

https://bitbucket.org/bbucklew/cavesofqud-public-issue-tracker/issues/9208

Depending on how these feature requests are eventually resolved, this mod may
become irrelevant in the future.

## Usage instructions

A basic example of a folder utilizing submodule-management is provided in the
[examples/](examples) directory. The [Kernel Space](../Kernel Space) mod is a
more extensive example of how to use submodule-management alongside traditional
options management.

In general, to use submodule-management, your mod should contain multiple
subdirectories with a file `Submodules.xml` in the top-level directory, e.g.:

```
examples
├── Options.xml
├── RecolorSnapjawScavengers
│   └── ObjectBlueprints.xml
├── ReplaceJoppaTile
│   └── WorldTerrain.xml
└── Submodules.xml
```

`Submodules.xml` should be structured as follows:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<submodules>
  <submodule Path="RecolorSnapjawScavengers">
    <optionenable ID="OptionRecolorSnapjawScavengers" />
  </submodule>

  <submodule Path="ReplaceJoppaTile">
    <optionenable ID="OptionReplaceJoppaTile" />
  </submodule>
</submodules>
```

Each `<submodule>` tag specifies a subdirectory that serves as a submodule. XML
files inside of that subdirectory are included or excluded, conditional on the
value of the option selected by `optionenable`.

## Current limitations

Caves of Qud does not currently offer any means of ensuring that Mod A is only
loaded if Mod B is also loaded. As a result, it is feasible that the features
offered by submodule-management may not be available.

In practice, this means that for some players, the "default" will appear to be
that all submodules are enabled.

There is currently an outstanding feature request to (hopefully, eventually) get
some mod dependence system into the game:

https://bitbucket.org/bbucklew/cavesofqud-public-issue-tracker/issues/11264

## Best practices

### C# code

**Do not include any C# code inside submodules.** Any C# code that appears
inside a submodule will be compiled regardless of whether the submodule is
enabled.  This mod is intended to enable conditional loading of XML files; I
don't currently have any plans to extend this to conditional compilation of C#.

The problem is that objects that are defined in submodules may be serialized and
saved. If a player disables a submodule that defines a serialized object and
then loads a save containing that object, they may end up permanently corrupting
their save.

If you are writing C# code and want to enable/disable behavior contingent on the
presence of certain options, you should instead use the `XRL.UI.Options` class.
For instance:

```csharp
if (XRL.UI.Options.GetOption("OptionRecolorSnapjaws").EqualsNoCase("Yes"))
    ...
```

### Mod priority

submodule-management uses a `LoadOrder` of `-5000` in order to ensure that it is
loaded before other mods that depend on it. If you need to override `LoadOrder`
you should ensure that you don't go below `-5000`, otherwise
submodule-management will be unable to properly scan your mod's files.
