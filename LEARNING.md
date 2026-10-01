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

### Mental Model

```text
.editorconfig  → controls how the editor writes files
.gitattributes → controls how Git handles those files
```
