# Learnings

## `.gitattributes`

- Controls how Git handles files.
- Commonly used for:
  - Line-ending normalization
  - File type handling
  - Merge behaviour
  - Diff behaviour

## `.editorconfig`

- Controls how editors such as VS Code format and save files.
- Commonly used for:
  - Line endings
  - Indentation
  - Character encoding
  - Trailing whitespace
  - Final newlines

## Important

`.gitattributes` and `.editorconfig` should use compatible settings.

For example:

- `.editorconfig` may tell VS Code to save files using `LF`.
- `.gitattributes` may tell Git to normalize files to `LF`.

If they are configured inconsistently, files can appear as modified because the editor and Git are handling line endings differently.

## Mental Model

```text
.editorconfig  → controls how the editor writes files
.gitattributes → controls how Git handles those files
```

## Mistakes made

```text
eol=LF in uppercase, it's silently ignored
.cmd files are stored as LF in repo but disk gets CRLF
*.png text would corrupt every file by changing top level header bytes 10-13
```

## `global.json`

- Pinning to whatever version is installed is not a decision. Check that the version is still supported and patched.

## SDK feature bands

latestPatch keeps the version strictly within the same feature band, for example if we have version 4.0.100, it will rollForward to 4.0.105 but won't do 4.0.200

latestFeature instead will rollForward to 4.0.200 or 4.0.305, as long as it stays within that minor version and if the version is already installed.

4 is major, 0 is minor, Feature Band is 1 and Patch 05. 4.0.105

When no version in the band is installed, the build fails loudly. 

## .python-version vs requires-python

Python interpreter version can be pinned and is different from allowed range of version that users can install  

## Architecture Decision Record

ADRs are immutable. Supersede, don't edit the current version. We take the template and write the new updates on it, in the end we have multiple version of each major decision and why we made such decision.

## Store first, then return 2xx

The idea is that we must store the event first before returning a positive http code. If we respond before saving, the event can be lost.
