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
[Archive](https://github.com/hyperbx/HyperLib/blob/main/HyperLib/Formats/Barracuda/Archive.cs)|`*.apf`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|PC, Xbox 360|[❌](## "The data is not sorted in the same way the original archive is, but the resulting archive is read correctly by the game.")|A zlib or XCompression compressed archive format.
[JSON Binary](https://github.com/hyperbx/HyperLib/blob/main/HyperLib/Formats/Barracuda/JsonBinary.cs)|`*.bin`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|PC, Xbox 360|✔️|A proprietary binary JSON format.
[Timed Event](https://github.com/hyperbx/HyperLib/blob/main/HyperLib/Formats/Barracuda/TimedEvent.cs)|`*.bin`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|PC, Xbox 360|✔️|A wrapper for the JSON binary format for storing user data for events.

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

---

<details><summary><h2>📄 <a href="https://github.com/hyperbx/HyperLib/blob/main/HyperLib/Formats/U8Archive.cs">U8 Archive</a></h2></summary>

### Developed by
- [Nintendo](https://www.nintendo.com/)
- [SEGA](https://www.sega.com/) *(compression variant for SONIC THE HEDGEHOG)*
### Used by
- F-Zero GX[<sup>[source]</sup>](https://www.gc-forever.com/forums/viewtopic.php?t=2444)
- Sonic Heroes (PlayStation 2)
- Harvest Moon: A Wonderful Life[<sup>[source]</sup>](https://hmapl.wordpress.com/2017/10/13/initial-examinations-of-clz-and-sb-files/)
- SONIC THE HEDGEHOG[<sup>[source]</sup>](https://github.com/hyperbx/Marathon/blob/master/Marathon/Formats/Archive/U8Archive.cs)
- Various Nintendo software[<sup>[source]</sup>](https://www.wiibrew.org/wiki/U8_archive)
### Supported variants
Name|Type|Support|[Platforms](## "This column indicates platforms the library has been tested and confirmed working with.")|[1:1](## "Can this library generate a binary identical file from the original source?")|Description
----|----|-------|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-----------
Big-endian|`*.arc`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|GameCube, Wii|✔️|An uncompressed archive format.
Big-endian w/ compression|`*.arc`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|PlayStation 3, Xbox 360|✔️|A zlib-compressed archive format, used for SONIC THE HEDGEHOG.
Little-endian|`*.arc`|[📜](## "Read") [💾](## "Write") [📥](## "Import") [📤](## "Export")|PlayStation 2|✔️|An uncompressed archive format.

</details>
