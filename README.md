# play-with-tree-sitter
Tree-sitter を使ったコード解析の実験用リポジトリです。  
Qiita 記事のサンプルコードや補助ツールを置いています。

## Contents
- libs/ : tree-sitterのdll
- tree-sitter-scm/ : Tree-sitter のscm
- result/ : サンプルアプリで生成したファイル類

## Runtime Requirements
ファイルは以下のように配置して下さい

The following files must be placed in the same directory as the executable:
```
<exe directory>/
  ├ Libs/*.dll              # Tree-sitter native DLLs
  └ tree-sitter/*.scm       # Tree-sitter query files

Example:
C:/my/src/TreeSitterTest/bin/Debug/
  ├ TreeSitterTest.exe
  ├ Libs/
  │   ├ tree-sitter.dll
  │   ├ tree-sitter-html.dll
  │   └ ...
  └ tree-sitter/
      ├ html.scm
      ├ javascript.scm
      └ ...
```

