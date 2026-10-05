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

latestPatch is a band that only changes the last two digits of the version whic hare the patch update number.

## Python version

Python interpreter version can be pinned and is different from allowed range of version that users can install  

### Architecture Design

ADRs are immutable. Supersede, don't edit.
