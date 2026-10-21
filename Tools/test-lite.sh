#!/bin/zsh
# 轻松版规则检查：编译 C# 规则并自检；有 Node.js 时再与网页版引擎逐项比对。
set -eu
cd "${0:A:h:h}"
UNITY_ROOT="${UNITY_ROOT:-/Applications/Unity/Hub/Editor/2022.3.62f3c1/Unity.app/Contents}"
if [[ ! -x "$UNITY_ROOT/MonoBleedingEdge/bin/mono" ]]; then
  echo 'Unity Mono runtime not found. Set UNITY_ROOT to the Unity Contents directory.' >&2
  exit 1
fi
mkdir -p Build/Checks
"$UNITY_ROOT/MonoBleedingEdge/bin/mono" "$UNITY_ROOT/MonoBleedingEdge/lib/mono/4.5/mcs.exe" -define:HARBOUR_STANDALONE -out:Build/Checks/LiteChecks.exe \
  Assets/Scripts/LiteData.cs Assets/Scripts/LiteJson.cs Assets/Scripts/LiteModel.cs Assets/Scripts/LiteCore.cs Assets/Scripts/LitePace.cs Assets/Editor/LiteChecks.cs
if command -v node >/dev/null && command -v python3 >/dev/null; then
  node Tools/lite-diff/engine.js Tools/lite-diff/cases.json Build/Checks/lite-js.json
  "$UNITY_ROOT/MonoBleedingEdge/bin/mono" Build/Checks/LiteChecks.exe Tools/lite-diff/cases.json Build/Checks/lite-cs.json
  python3 Tools/lite-diff/compare.py Build/Checks/lite-js.json Build/Checks/lite-cs.json | tee docs/lite-diff-results.txt
  "$UNITY_ROOT/MonoBleedingEdge/bin/mono" Build/Checks/LiteChecks.exe --roundtrip Build/Checks/lite-js.json Build/Checks/lite-roundtrip.json
  python3 Tools/lite-diff/compare.py Build/Checks/lite-js.json Build/Checks/lite-roundtrip.json
else
  "$UNITY_ROOT/MonoBleedingEdge/bin/mono" Build/Checks/LiteChecks.exe
  echo 'Node.js 或 Python 3 不可用：跳过与网页版的逐项比对。'
fi
