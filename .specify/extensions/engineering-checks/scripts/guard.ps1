# Runs before each tool use. Reads the tool call as JSON on stdin.
# Exits 2 to block (the harness blocks only on exit 2), 0 to allow.
$ErrorActionPreference = 'Stop'
try {
    $raw = [Console]::In.ReadToEnd()
    $call = $raw | ConvertFrom-Json
} catch {
    [Console]::Error.WriteLine('guard: could not read the tool call; blocking.')
    exit 2
}
$inputs = $call.tool_input
$text = @($inputs.command, $inputs.file_path, $inputs.path) -join ' '

if ($text -match 'git\s+push\b[^|;&]*\b(origin\s+)?(main|master)\b' -or $text -match 'git\s+push\b.*--force') {
    [Console]::Error.WriteLine('guard: pushing to main or force-pushing is blocked. Merge through a PR and the merge queue.')
    exit 2
}
if ($text -match 'gh\s+issue\s+edit\b.*--(add|remove)-label\s+[''"]?state:' -or $text -match 'gh\s+project\s+item-edit\b' -or $text -match 'setIssueStatus|setIssueFieldValue|updateIssueFieldValue') {
    [Console]::Error.WriteLine('guard: board state is changed only through move-state.ps1, which checks the policy first.')
    exit 2
}
if ($call.tool_name -match 'Write|Edit' -and $text -match '\.specify[\\/]workflows[\\/]runs[\\/]') {
    [Console]::Error.WriteLine('guard: run state is written only by the workflow.')
    exit 2
}
exit 0