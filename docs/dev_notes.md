# Dev Notes

## Regenerate Proposal Word Document

The editable source remains `docs/project_proposal_submission.md`.

To regenerate the submission-ready Word file, run:

- `powershell -ExecutionPolicy Bypass -File tools/generate_proposal_docx.ps1`

The script will:

- verify the markdown source exists
- install `python-docx` if it is missing
- generate `docs/project_proposal_submission.docx`

The conversion uses `tools/generate_proposal_docx.py`, which applies:

- cover section
- Times New Roman, size 12
- line spacing 1.5
- 2.5 cm page margins
- heading and table formatting for Word submission
