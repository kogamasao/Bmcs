#!/bin/sh
#
# Tailwind CSS をビルドして Bmcs/wwwroot/css/app.css を生成する。
#
# 生成物はリポジトリにコミットするため、デプロイ時に Node.js もこのスクリプトも不要。
# CSSクラスを追加・変更したときだけ、ここを実行してコミットする。
#
# 初回のみ、CLI（単一バイナリ・Node.js不要）を取得する:
#   VERSION=v4.3.3
#   mkdir -p ~/.local/bin
#   curl -sL -o ~/.local/bin/tailwindcss \
#     "https://github.com/tailwindlabs/tailwindcss/releases/download/${VERSION}/tailwindcss-linux-x64"
#   chmod +x ~/.local/bin/tailwindcss
#
# 編集しながら確認する場合は --watch を付けて実行する:
#   ./build-css.sh --watch
#
set -e

CLI="${TAILWIND_CLI:-$HOME/.local/bin/tailwindcss}"

if [ ! -x "$CLI" ]; then
    echo "Tailwind CLI が見つかりません: $CLI" >&2
    echo "上記コメントの手順で取得するか、TAILWIND_CLI に実行ファイルのパスを指定してください。" >&2
    exit 1
fi

cd "$(dirname "$0")"

"$CLI" --input Bmcs/Styles/app.css --output Bmcs/wwwroot/css/app.css --minify "$@"
