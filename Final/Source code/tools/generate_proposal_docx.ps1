$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$inputPath = Join-Path $repoRoot "docs\project_proposal_submission.md"
$outputPath = Join-Path $repoRoot "docs\project_proposal_submission.docx"
$pythonScript = Join-Path $repoRoot "tools\generate_proposal_docx.py"

if (-not (Test-Path $inputPath)) {
    throw "Missing input markdown: $inputPath"
}

$docxPackage = python -c "import importlib.util, sys; sys.exit(0 if importlib.util.find_spec('docx') else 1)"
if ($LASTEXITCODE -ne 0) {
    python -m pip install python-docx
}

python $pythonScript $inputPath $outputPath

if (-not (Test-Path $outputPath)) {
    throw "DOCX generation failed: $outputPath was not created."
}

Write-Host "Generated proposal document at $outputPath"
