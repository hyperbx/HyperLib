# HyperLib
A work-in-progress general purpose library for software research.

# Formats

<details><summary><h2>🛠️ Barracuda</h2></summary>

### Developed by
- [Vector Unit](https://www.vectorunit.com/)
### Known for
- Hydro Thunder Hurricane
### Supported formats
Name|Type|Support|[Platforms](## "This column indicates platforms the library has been tested and confirmed working with.")|[1:1](## "Can this library generate a binary identical file from the original source?")|Description
----|----|-------|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[Archive](https://github.com/hyperbx/HyperLib/blob/main/HyperLib/Formats/Barracuda/Archive.cs)|`*.apf`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|PC, [Xbox 360](## "The format is mapped, but requires XCompression on the archive files, despite there being a flag for files being uncompressed.")|[❌](## "The data is not sorted in the same way the original archive is, but the resulting archive is read correctly by the game.")|A zlib or XCompression compressed archive format.

</details>

<details><summary><h2>🛠️ Sonic_Crytek</h2></summary>

### Developed by
- Big Red Button
- [Illfonic](https://www.illfonic.com/)
- [Crytek](https://www.crytek.com/)
### Known for
- Sonic Boom: Rise of Lyric
### Supported formats
Name|Type|Support|[Platforms](## "This column indicates platforms the library has been tested and confirmed working with.")|[1:1](## "Can this library generate a binary identical file from the original source?")|Description
----|----|-------|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[Archive](https://github.com/hyperbx/HyperLib/blob/main/HyperLib/Formats/Sonic_Crytek/Archive.cs)|`*.*.stream`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|Wii U|[❌](## "Files are always written uncompressed and are missing CRC32 hashes and some unknown flags, but the resulting archive is read correctly by the game.")|An LZSS-compressed archive format.

</details>

<details><summary><h2>🛠️ TommunismEngine</h2></summary>

### Developed by
- [Team Meat](http://www.supermeatboy.com/)
### Known for
- Super Meat Boy
### Supported formats
Name|Type|Support|[Platforms](## "This column indicates platforms the library has been tested and confirmed working with.")|[1:1](## "Can this library generate a binary identical file from the original source?")|Description
----|----|-------|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
[Archive](https://github.com/hyperbx/HyperLib/blob/main/HyperLib/Formats/TommunismEngine/Archive.cs)|`game*.dat`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|PC, [Xbox 360](## "Audio data is exported in an unknown WAV format.")|[❌](## "The table of contents is not sorted in the same way the original archive is, but the resulting archive is read correctly by the game.")|An uncompressed archive format.
[Registry](https://github.com/hyperbx/HyperLib/blob/main/HyperLib/Formats/TommunismEngine/Registry.cs)|`reg*.dat`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|PC|✔️|A basic property format used for storing user option data.
[Texture Package](https://github.com/hyperbx/HyperLib/blob/main/HyperLib/Formats/TommunismEngine/TexturePackage.cs)|`*.tp`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|PC, Xbox 360|✔️|An uncompressed texture container format.

</details>
