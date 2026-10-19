#!/bin/zsh
set -eu
cd "${0:A:h:h}"
UNITY_ROOT="${UNITY_ROOT:-/Applications/Unity/Hub/Editor/2022.3.62f3c1/Unity.app/Contents}"
if [[ ! -x "$UNITY_ROOT/MonoBleedingEdge/bin/mono" ]]; then
  echo 'Unity Mono runtime not found. Set UNITY_ROOT to the Unity Contents directory.' >&2
  exit 1
fi
mkdir -p Build/Checks
"$UNITY_ROOT/MonoBleedingEdge/bin/mono" "$UNITY_ROOT/MonoBleedingEdge/lib/mono/4.5/mcs.exe" -define:HARBOUR_STANDALONE -out:Build/Checks/YearChecks.exe Assets/Scripts/YearModel.cs Assets/Scripts/YearRules.cs Assets/Scripts/YearLife.cs Assets/Scripts/YearEvents.cs Assets/Editor/YearChecks.cs
"$UNITY_ROOT/MonoBleedingEdge/bin/mono" Build/Checks/YearChecks.exe
