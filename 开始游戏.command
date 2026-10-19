#!/bin/zsh
cd "${0:A:h}"
if [[ -d 'Build/macOS/港硕这一年.app' ]]; then
  open 'Build/macOS/港硕这一年.app'
else
  python3 Tools/preview.py
fi
