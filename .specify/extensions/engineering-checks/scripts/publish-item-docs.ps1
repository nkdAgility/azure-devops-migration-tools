# Publishes the current item's own documents to its own issue, in "artifacts" mode. It needs no permission: it writes
# only to the item's issue, and only the files in the item's folder. With "files" it does nothing.
$ErrorActionPreference = 'Stop'
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
if ((Get-ItemDocsMode '.') -ne 'artifacts') { "publish-item-docs: itemDocs is 'files'; nothing to publish."; exit 0 }
$pub = @(Publish-ItemDocs '.' $id)
$drift = @(Get-ItemDocsDrift '.' $id)
if ($drift.Count) { Write-Error "#$id's artifacts still differ from the local copy: $($drift -join ', ')"; exit 1 }
if ($pub.Count) { "publish-item-docs: published to #${id}: $($pub -join ', ')" } else { "publish-item-docs: #$id's artifacts already match the local copy." }
exit 0
