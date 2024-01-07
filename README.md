# HyperLib
A work-in-progress general purpose library for software research.

# Games

## TommunismEngine
The game engine developed for Super Meat Boy.

### Supported formats
Type|Support|[1:1](## "Can this library generate a binary identical file from the original source?")|Description|
----|-------|---------------------------------------------------------------------------------------|-----------|
[Archive (*.dat)](https://github.com/hyperbx/HyperLib/blob/main/HyperLib/Games/TommunismEngine/Archive.cs)|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|[⚠️](## "The table of contents is not sorted in the same way the original archive is, but the resulting file is read correctly by the game.")|An uncompressed archive format.
[Texture Package (*.tp)](https://github.com/hyperbx/HyperLib/blob/main/HyperLib/Games/TommunismEngine/TexturePackage.cs)|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|An uncompressed texture container format.
[Registry (reg*.dat)](https://github.com/hyperbx/HyperLib/blob/main/HyperLib/Games/TommunismEngine/Registry.cs)|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|✔️|A basic property format used for storing user option data.