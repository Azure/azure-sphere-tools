
$PythonRoot = Join-Path $PSScriptRoot ".." "Manufacturing" "src" "Python"

$PythonPackage = Join-Path $PythonRoot "package"

& python3 -m pip install --upgrade $PythonPackage
& pip install build
& python3 -m build $PythonPackage