# HyperLib
A work-in-progress general purpose library for software research.

# Frameworks

## TommunismEngine
### Used by
- Super Meat Boy

### Supported formats
Type|Support|[Platforms](## "This column indicates platforms the library has been tested and confirmed working with.")|[1:1](## "Can this library generate a binary identical file from the original source?")|Description
----|-------|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[Archive (*.dat)](https://github.com/hyperbx/HyperLib/blob/main/HyperLib/Games/TommunismEngine/Archive.cs)|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|PC, [Xbox 360](## "WAV audio data is exported in an unknown format.")|[⚠️](## "The table of contents is not sorted in the same way the original archive is, but the resulting file is read correctly by the game.")|An uncompressed archive format.
[Registry (reg*.dat)](https://github.com/hyperbx/HyperLib/blob/main/HyperLib/Games/TommunismEngine/Registry.cs)|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|PC|✔️|A basic property format used for storing user option data.
[Texture Package (*.tp)](https://github.com/hyperbx/HyperLib/blob/main/HyperLib/Games/TommunismEngine/TexturePackage.cs)|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|PC, Xbox 360|✔️|An uncompressed texture container format.