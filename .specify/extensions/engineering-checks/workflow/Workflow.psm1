# The engineering Definition of Workflow, run as code.
# Board adapters, policy checks, the policy-checked state move, and the manager (pulls, auto-advance, watchdog, attention).
Set-StrictMode -Version Latest

$script:MoveMarker = 'nkda-move'

# ---------------------------------------------------------------- configuration

function Get-WorkflowConfig {
    param([string]$Root = '.')
    $repoCfg = Join-Path $Root '.specify/memory/workflow.json'
    $path = if (Test-Path $repoCfg) { $repoCfg } else { Join-Path $PSScriptRoot 'workflow.default.json' }
    $cfg = Get-Content $path -Raw | ConvertFrom-Json -AsHashtable
    $cfg['order'] = if ($cfg.validatingBeforeMerge) { $cfg.orderBeforeMerge } else { $cfg.orderAfterMerge }
    $cfg
}

function Test-IsParent($Item) { $Item.type -eq 'Package' -or ($Item.ContainsKey('children') -and @($Item.children).Count -gt 0) }

function Get-ItemOrder($Config, $Item) { if ((Test-IsParent $Item) -and $Config.ContainsKey('parentOrder')) { $Config.parentOrder } else { $Config.order } }

function Get-StateDef($Config, [string]$Name) {
    $s = $Config.states | Where-Object { $_.name -eq $Name } | Select-Object -First 1
    if (-not $s) { throw "Unknown state '$Name'." }
    $s
}

# ---------------------------------------------------------------- board adapters
# An adapter is a hashtable of scriptblocks:
#   GetItems() -> items;  GetItem(id) -> item;  SetState(item, state);  AddComment(item, text);  GetPrState(item) -> 'MERGED' | 'OPEN' | 'CLOSED' | $null
# An item: id, title, type, state, tags (array), createdAt, comments (array of @{at; body}), body, pr

function New-FileBoardAdapter {
    param([Parameter(Mandatory)][string]$Path)
    $load = { (Get-Content $Path -Raw | ConvertFrom-Json -AsHashtable) }.GetNewClosure()
    $save = { param($b) $b | ConvertTo-Json -Depth 10 | Set-Content $Path -Encoding utf8 }.GetNewClosure()
    @{
        Name       = 'file'
        GetItems   = { (& $load).items }.GetNewClosure()
        GetItem    = { param($id) (& $load).items | Where-Object { "$($_.id)" -eq "$id" } | Select-Object -First 1 }.GetNewClosure()
        SetState   = { param($item, $state) $b = & $load; ($b.items | Where-Object { "$($_.id)" -eq "$($item.id)" }).state = $state; & $save $b }.GetNewClosure()
        AddComment = { param($item, $text) $b = & $load; $i = $b.items | Where-Object { "$($_.id)" -eq "$($item.id)" }; if (-not $i.comments) { $i.comments = @() }; $i.comments = @($i.comments) + @(@{ at = (Get-Date).ToUniversalTime().ToString('o'); body = $text }); & $save $b }.GetNewClosure()
        GetPrState = { param($item) if ($item.pr) { $item.pr.state } else { $null } }.GetNewClosure()
        SetTag     = { param($item, $tag, [bool]$on) $b = & $load; $i = $b.items | Where-Object { "$($_.id)" -eq "$($item.id)" }; $t = @($i.tags | Where-Object { $_ -ne $tag }); if ($on) { $t += $tag }; $i.tags = $t; & $save $b }.GetNewClosure()
    }
}

function New-GitHubBoardAdapter {
    # State is the issue's built-in Status (state labels are hard denied). StatusNames maps a workflow state to the
    # Status option that holds it; a state not listed uses its own name. An issue whose Status names no workflow
    # state is read as Proposed, with that Status kept as oldStatus for adoption.
    param([string]$Repo, [hashtable]$StatusNames = @{}, [string[]]$States = @(), [string]$StateField, [hashtable]$CategoryStatus = @{}, [hashtable]$StateCategory = @{})
    $nwo = if ($Repo) { $Repo } else { (gh repo view --json nameWithOwner | ConvertFrom-Json).nameWithOwner }
    $owner, $name = $nwo -split '/'
    $r = @('-R', $nwo)
    $toStatus = { param($state) if ($StatusNames.ContainsKey($state)) { $StatusNames[$state] } else { $state } }.GetNewClosure()
    $fromStatus = { param($status) foreach ($s in $States) { if ((& $toStatus $s) -eq $status) { return $s } }; $null }.GetNewClosure()
    $statusOf = {
        param([int[]]$numbers)
        $map = @{}
        foreach ($chunk in ($numbers | ForEach-Object -Begin { $b = @() } -Process { $b += $_; if ($b.Count -eq 50) { , $b; $b = @() } } -End { if ($b) { , $b } })) {
            $fields = ($chunk | ForEach-Object { "i$($_): issue(number:$_){number status{name} issueFieldValues(first:30){nodes{... on IssueFieldSingleSelectValue{name field{... on IssueFieldSingleSelect{name}}}}}}" }) -join ' '
            $res = gh api graphql -f query="{repository(owner:`"$owner`",name:`"$name`"){$fields}}" | ConvertFrom-Json
            foreach ($p in $res.data.repository.PSObject.Properties) { if ($p.Value) {
                $fv = if ($StateField) { @($p.Value.issueFieldValues.nodes | Where-Object { $_.field -and $_.field.name -eq $StateField } | ForEach-Object { $_.name }) | Select-Object -First 1 } else { $null }
                $map[[int]$p.Value.number] = @{ status = $p.Value.status.name; field = $fv } } }
        }
        $map
    }.GetNewClosure()
    $toItem = {
        param($i, $st)
        $status = if ($st) { $st.status } else { $null }
        $state = if ($StateField) { if ($st -and $st.field -in $States) { $st.field } else { $null } } else { & $fromStatus $status }
        @{
            id        = $i.number
            title     = $i.title
            type      = if ($i.PSObject.Properties['issueType'] -and $i.issueType) { $i.issueType.name } else { $null }
            state     = if ($state) { $state } elseif ($i.state -eq 'CLOSED') { 'Closed' } else { 'Proposed' }
            oldStatus = if ($state) { $null } else { $status }
            stateSet  = [bool]$state -or $i.state -eq 'CLOSED'
            tags      = @($i.labels | ForEach-Object { $_.name })
            createdAt = $i.createdAt
            body      = $i.body
            comments  = @($i.comments | ForEach-Object { @{ at = $_.createdAt; body = $_.body; author = $_.author.login } })
            pr        = $null
        }
    }.GetNewClosure()
    $fields = 'number,title,labels,body,createdAt,comments,state,issueType'
    @{
        Name       = 'github'
        GetItems   = {
            $list = @(gh issue list @r --state open --limit 5000 --json $fields | ConvertFrom-Json)
            if ($LASTEXITCODE -ne 0) { throw 'gh issue list failed' }
            if ($list.Count -ge 5000) { throw 'more than 5000 open issues; the inventory would be incomplete' }
            $st = if ($list) { & $statusOf @($list | ForEach-Object { [int]$_.number }) } else { @{} }
            $list | ForEach-Object { & $toItem $_ $st[[int]$_.number] }
        }.GetNewClosure()
        GetItem    = {
            param($id)
            $i = gh issue view $id @r --json $fields | ConvertFrom-Json
            $it = & $toItem $i (& $statusOf @([int]$id))[[int]$id]
            $it.children = @(gh api "repos/$nwo/issues/$id/sub_issues" --jq '.[].number' 2>$null | Where-Object { $_ })
            $it
        }.GetNewClosure()
        SetState   = {
            param($item, $state)
            # With a state field, the built-in Status shows the state's category (a state listed by name overrides it).
            $want = if (-not $StateField) { & $toStatus $state } elseif ($CategoryStatus.ContainsKey($state)) { $CategoryStatus[$state] } else { $CategoryStatus[$StateCategory[$state]] }
            $q = gh api graphql -f query="{repository(owner:`"$owner`",name:`"$name`"){issue(number:$($item.id)){id} planningStatuses(first:50){nodes{id name}} issueFields(first:30){nodes{... on IssueFieldSingleSelect{id name options{id name}}}}}}" | ConvertFrom-Json
            if ($StateField) {
                $f = @($q.data.repository.issueFields.nodes | Where-Object { $_.name -eq $StateField }) | Select-Object -First 1
                if (-not $f) { throw "the repo has no issue field named '$StateField'" }
                $oid = @($f.options | Where-Object { $_.name -eq $state } | ForEach-Object { $_.id }) | Select-Object -First 1
                if (-not $oid) { throw "'$StateField' has no option named '$state'" }
                $fr = gh api graphql -f query="mutation{setIssueFieldValue(input:{issueId:`"$($q.data.repository.issue.id)`",issueFields:[{fieldId:`"$($f.id)`",singleSelectOptionId:`"$oid`"}]}){clientMutationId}}"
                if ($LASTEXITCODE -ne 0 -or "$fr" -match '"errors"') { throw "setting $StateField failed: $fr" }
            }
            $sid = @($q.data.repository.planningStatuses.nodes | Where-Object { $_.name -eq $want } | ForEach-Object { $_.id }) | Select-Object -First 1
            if (-not $sid) { throw "the repo has no Status named '$want'; add it in the repo's settings" }
            $res = gh api graphql -f query="mutation{setIssueStatus(input:{issueId:`"$($q.data.repository.issue.id)`",statusId:`"$sid`"}){clientMutationId}}"
            if ($LASTEXITCODE -ne 0 -or "$res" -match '"errors"') { throw "setting Status failed: $res" }
            if ($state -eq 'Closed') { gh issue close $item.id @r --reason completed | Out-Null; if ($LASTEXITCODE -ne 0) { throw "closing #$($item.id) failed" } }
            if ($state -eq 'Removed') { gh issue close $item.id @r --reason 'not planned' | Out-Null; if ($LASTEXITCODE -ne 0) { throw "closing #$($item.id) failed" } }
        }.GetNewClosure()
        GetStatuses = { (gh api graphql -f query="{repository(owner:`"$owner`",name:`"$name`"){planningStatuses(first:50){nodes{name}}}}" | ConvertFrom-Json).data.repository.planningStatuses.nodes.name }.GetNewClosure()
        ToStatus   = $toStatus
        AddComment = { param($item, $text) gh issue comment $item.id @r --body $text | Out-Null; if ($LASTEXITCODE -ne 0) { throw "commenting on #$($item.id) failed" } }.GetNewClosure()
        SetTag     = { param($item, $tag, [bool]$on) gh label create $tag @r --force | Out-Null; if ($on) { gh issue edit $item.id @r --add-label $tag | Out-Null } else { gh issue edit $item.id @r --remove-label $tag | Out-Null } }.GetNewClosure()
        GetPrState = { param($item) $pr = gh pr list @r --state all --search "$($item.id) in:body" --json number,state --limit 1 | ConvertFrom-Json; if ($pr) { $pr[0].state } else { $null } }.GetNewClosure()
    }
}
function Get-BoardAdapter {
    param($Config, [string]$Root = '.')
    switch ($Config.board.adapter) {
        'file'   { New-FileBoardAdapter -Path (Join-Path $Root $Config.board.path) }
        'github' {
            $b = $Config.board
            $sn = if ($b.ContainsKey('statusNames') -and $b.statusNames) { $b.statusNames } else { @{} }
            $cs = if ($b.ContainsKey('categoryStatus') -and $b.categoryStatus) { $b.categoryStatus } else { @{} }
            $sc = @{}; foreach ($s in $Config.states) { $sc[$s.name] = $s.category }
            New-GitHubBoardAdapter -Repo $b['repo'] -StatusNames $sn -States @($Config.states | ForEach-Object { $_.name }) -StateField $b['stateField'] -CategoryStatus $cs -StateCategory $sc
        }
        default  { throw "Unknown board adapter '$($Config.board.adapter)'." }
    }
}

# ---------------------------------------------------------------- evidence and checks

function Get-ItemEvidence {
    param($Item, [string]$Root = '.')
    $parts = @("$($Item.body)") + @($Item.comments | ForEach-Object { "$($_.body)" })
    $wkr = Join-Path $Root ".specify/wkr/$($Item.id).md"
    if (Test-Path $wkr) { $parts += Get-Content $wkr -Raw }
    ($parts -join "`n`n") -replace "`r`n", "`n"
}

function Get-OutcomeProblem([string]$Text, $Id, [string]$Head) {
    # Why the outcome evidence does not stand for this item now, or $null when it does. Every attempt leaves a
    # marker: "Automated outcome checks (all passed) for #<id> at <sha> attempt <a>:" or
    # "Outcome checks failed for #<id> at <sha> attempt <a>: <why>". The last marker for this item must be a pass
    # at the current commit; a pass for another item, an older commit, or one followed by a failed attempt does not count.
    # Each attempt is also opened with "Outcome checks started for ...": the pass must close the latest started
    # attempt, so an attempt that stopped before its result (incomplete) outranks an earlier pass.
    $marks = @([regex]::Matches($Text, '(?m)^(?<kind>Automated outcome checks \(all passed\)|Outcome checks failed|Outcome checks started) for #(?<id>\d+) at (?<sha>[0-9a-f]{40}) attempt (?<att>[0-9a-f]+):') |
        Where-Object { $_.Groups['id'].Value -eq "$Id" })
    if (-not $marks) { return "no outcome check result for #$Id" }
    $pass = 'Automated outcome checks (all passed)'
    $last = $marks | Where-Object { $_.Groups['kind'].Value -ne 'Outcome checks started' } | Select-Object -Last 1
    $lastStart = $marks | Where-Object { $_.Groups['kind'].Value -eq 'Outcome checks started' } | Select-Object -Last 1
    if (-not $last) { return "the last outcome check attempt for #$Id did not finish" }
    if ($last.Groups['kind'].Value -ne $pass) { return "the last outcome check attempt for #$Id failed" }
    if (-not $lastStart) { return "the outcome check pass for #$Id has no recorded start: it is not proof of an attempt" }
    if ($lastStart.Groups['att'].Value -ne $last.Groups['att'].Value) {
        return "the latest outcome check attempt for #$Id ($($lastStart.Groups['att'].Value)) did not finish with a pass"
    }
    if ($last.Groups['sha'].Value -ne $Head) { return "the outcome checks passed at $($last.Groups['sha'].Value), not the current commit $Head" }
    $null
}

function Get-SpecAgreed([string]$Text) {
    # Martin's own words on the last "Spec agreed:" line, or $null: at least three characters, not the stock phrase.
    $m = [regex]::Matches($Text, '(?m)^Spec agreed:[ \t]*(.*)$') | Select-Object -Last 1
    if (-not $m) { return $null }
    $words = $m.Groups[1].Value.Trim()
    if ($words.Length -lt 3 -or $words -match '(?i)^approved by Martin at the review gate') { return $null }
    $words
}

function Get-SkillText([string]$Extension, [string]$CommandFile) {
    # The agent skill Spec Kit generates from an extension command: the same text, with the skill front matter.
    $t = (Get-Content $CommandFile -Raw) -replace "`r`n", "`n"
    $m = [regex]::Match($t, '(?s)\A---\n(.*?)\n---\n(.*)\z')
    $desc = [regex]::Match($m.Groups[1].Value, '(?m)^description:\s*(.+)$').Groups[1].Value.Trim().Trim('"')
    if ($desc -match ": |^[\[\]{}&*!|>%@``#,?'""-]") { $desc = "'" + $desc.Replace("'", "''") + "'" }
    $name = "speckit-$Extension-$([IO.Path]::GetFileNameWithoutExtension($CommandFile))"
    "---`nname: $name`ndescription: $desc`ncompatibility: Requires spec-kit project structure with .specify/ directory`nmetadata:`n  author: nkdAgility`n  source: ${Extension}:commands/$(Split-Path $CommandFile -Leaf)`n---`n" + $m.Groups[2].Value
}

function Get-SkillCopies([string]$Target, [string]$Extension, [string]$Name) {
    # Every generated copy of one skill in a target repo: the extension's .specify-dev/agent-commands/<agent>/ copy,
    # and each agent's own skills folder (a dot folder at the repo root). Copies that are links are left out.
    $found = @(Get-ChildItem (Join-Path $Target ".specify/extensions/$Extension/.specify-dev/agent-commands") -Directory -ErrorAction SilentlyContinue |
        ForEach-Object { Join-Path $_.FullName "$Name/SKILL.md" })
    $found += @(Get-ChildItem $Target -Directory -Force -Filter '.*' -ErrorAction SilentlyContinue |
        ForEach-Object { Join-Path $_.FullName "skills/$Name/SKILL.md" })
    @($found | Where-Object { (Test-Path $_) -and -not (((Get-Content $_ -Raw) -replace "`r`n", "`n") -match '^\.\./') })
}

function Get-SharedDrift([string]$Source, [string]$Target) {
    # Every shared file in a target repo that is missing or differs from nkda-speckit: the extensions as copied,
    # and the skills generated from their commands.
    $drift = [Collections.Generic.List[string]]::new()
    $srcExt = (Resolve-Path (Join-Path $Source 'extensions')).Path
    $root = (Resolve-Path $Target).Path.TrimEnd('\', '/')
    foreach ($f in Get-ChildItem $srcExt -Recurse -File) {
        $rel = $f.FullName.Substring($srcExt.Length).TrimStart('\', '/').Replace('\', '/')
        $copy = Join-Path $root ".specify/extensions/$rel"
        if (-not (Test-Path $copy)) { $drift.Add(".specify/extensions/$rel (missing)"); continue }
        if (((Get-Content $f.FullName -Raw) -replace "`r`n", "`n") -ne ((Get-Content $copy -Raw) -replace "`r`n", "`n")) { $drift.Add(".specify/extensions/$rel") }
    }
    foreach ($e in Get-ChildItem $srcExt -Directory) {
        foreach ($c in Get-ChildItem (Join-Path $e.FullName 'commands') -Filter *.md -ErrorAction SilentlyContinue) {
            $n = "speckit-$($e.Name)-$($c.BaseName)"; $want = Get-SkillText $e.Name $c.FullName
            foreach ($p in Get-SkillCopies $root $e.Name $n) {
                if ((((Get-Content $p -Raw) -replace "`r`n", "`n")) -ne $want) { $drift.Add($p.Substring($root.Length + 1).Replace('\', '/')) }
            }
            # A skills folder that holds this extension's other skills must hold this one too.
            $folders = @(Get-ChildItem (Join-Path $root ".specify/extensions/$($e.Name)/.specify-dev/agent-commands") -Directory -ErrorAction SilentlyContinue | ForEach-Object FullName) +
                @(Get-ChildItem $root -Directory -Force -Filter '.*' -ErrorAction SilentlyContinue | ForEach-Object { Join-Path $_.FullName 'skills' } | Where-Object { Test-Path $_ })
            foreach ($dir in $folders) {
                if ((Get-ChildItem $dir -Directory -Filter "speckit-$($e.Name)-*" -ErrorAction SilentlyContinue) -and -not (Test-Path (Join-Path $dir "$n/SKILL.md"))) {
                    $drift.Add("$((Join-Path $dir $n).Substring($root.Length + 1).Replace('\', '/'))/SKILL.md (missing)")
                }
            }
        }
    }
    , $drift
}

function Get-Section([string]$Text, [string]$Name) {
    # The evidence is the body, then comments, then the WKR, so the last section with the name is the current one.
    # A section ends only at a heading of its own level or higher, so its sub-headings stay inside it.
    $m = @(foreach ($n in 2..4) { [regex]::Matches($Text, "(?ms)^#{$n}[ \t]*$([regex]::Escape($Name))[ \t]*\n(.*?)(?=^#{1,$n}\s|\z)") }) | Sort-Object Index | Select-Object -Last 1
    if ($m) { $m.Groups[1].Value.Trim() } else { $null }
}

function Get-ItemMoves($Item) {
    @($Item.comments | ForEach-Object {
        $m = [regex]::Match("$($_.body)", "<!--\s*$script:MoveMarker\s+(\{.*?\})\s*-->")
        if ($m.Success) { $m.Groups[1].Value | ConvertFrom-Json -AsHashtable }
    })
}

function Get-StateSince($Item) {
    $moves = Get-ItemMoves $Item | Where-Object { $_.to -eq $Item.state }
    if ($moves) { [datetime]($moves | Select-Object -Last 1).at } else { [datetime]$Item.createdAt }
}

function Get-LastActivity($Item, [string]$Root = '.') {
    $times = @(Get-StateSince $Item) + @($Item.comments | ForEach-Object { [datetime]$_.at })
    $wkr = Join-Path $Root ".specify/wkr/$($Item.id).md"
    if (Test-Path $wkr) {
        $times += (Get-Item $wkr).LastWriteTimeUtc
        $run = [regex]::Match((Get-Content $wkr -Raw), '(?m)^Run:\s*(\S+)')
        if ($run.Success) {
            $state = Join-Path $Root ".specify/workflows/runs/$($run.Groups[1].Value)/state.json"
            if (Test-Path $state) { $times += (Get-Item $state).LastWriteTimeUtc }
        }
    }
    ($times | ForEach-Object { $_.ToUniversalTime() } | Sort-Object | Select-Object -Last 1)
}

function Test-Check {
    param($Check, $Item, [string]$Evidence, $Adapter, [string]$Root = '.', [switch]$SkipScripts, $Config)
    if ($Check.ContainsKey('types') -and $Check.types -and ($Item.type -notin $Check.types)) { return $null }
    if ($Check.ContainsKey('source') -and $Check.source -eq 'wkr') {
        $wkr = Join-Path $Root ".specify/wkr/$($Item.id).md"
        $Evidence = if (Test-Path $wkr) { (Get-Content $wkr -Raw) -replace "`r`n", "`n" } else { '' }
    }
    $ok = switch ($Check.type) {
        'pattern'         { [regex]::IsMatch($Evidence, $Check.pattern) }
        'section'         { $s = Get-Section $Evidence $Check.name; $null -ne $s -and $s -ne '' -and -not [regex]::IsMatch($s, '(?im)^\s*(status:\s*)?(open|tbd|todo)\b(?!\s+findings:\s*0\s*$)') }
        'specAgreed'      { $null -ne (Get-SpecAgreed $Evidence) }
        'outcomeCurrent'  {
            $head = if ($Check.ContainsKey('head')) { $Check.head } else { git -C $Root rev-parse HEAD 2>$null }
            (-not (Test-Path (Join-Path $Root '.specify/reviews/outcome-attempt-pending'))) -and $null -eq (Get-OutcomeProblem $Evidence $Item.id "$head")
        }
        'noOpen'          { $s = Get-Section $Evidence $Check.name; $null -eq $s -or -not [regex]::IsMatch($s, '(?i)\bopen\b') }
        'noOpenQuestions' { @(Get-OpenQuestions $Evidence).Count -eq 0 }
        'confidence'      {
            $s = Get-Section $Evidence $Check.name
            $min = if ($Check.ContainsKey('min')) { $Check.min } elseif ($Config -and $Config.ContainsKey('minConfidence')) { $Config.minConfidence } else { 85 }
            $f = if ($s) { [regex]::Match($s, '(?im)^Confidence:\s*(\d{1,3})\s*%') } else { $null }
            $why = if ($s) { [regex]::IsMatch($s, '(?ims)^Reasons:\s*\n\s*-\s*\S') } else { $false }
            $f -and $f.Success -and [int]$f.Groups[1].Value -ge $min -and $why
        }
        'productOutcome'  {
            # The current product outcome, in Martin's words: .specify/memory/product-outcome.md with
            # "Outcome: <what>" and "Words: <what Martin said>".
            $f = Join-Path $Root '.specify/memory/product-outcome.md'
            if (-not (Test-Path $f)) { $false } else {
                $t = Get-Content $f -Raw
                [regex]::IsMatch($t, '(?m)^\W*Outcome:\W*\s*\S.{2,}$') -and [regex]::IsMatch($t, '(?m)^\W*Words:\W*\s*\S.{2,}$')
            }
        }
        'hasChildren'     { @($Item['children']).Count -gt 0 }
        'childrenDone'    {
            $kids = @($Item['children'] | ForEach-Object { & $Adapter.GetItem $_ })
            $kids.Count -gt 0 -and -not ($kids | Where-Object { $_.state -notin 'Closed', 'Removed' }) -and ($kids | Where-Object { $_.state -eq 'Closed' })
        }
        'prMerged'        { (& $Adapter.GetPrState $Item) -eq 'MERGED' }
        'script'          {
            if ($SkipScripts) { return @{ skipped = $true; message = $Check.message } }
            Push-Location $Root; try { & pwsh -NoProfile -Command $Check.run *> $null; $LASTEXITCODE -eq 0 } finally { Pop-Location }
        }
        default           { throw "Unknown check type '$($Check.type)'." }
    }
    if ($ok) { $null } else { @{ message = $Check.message } }
}

# ---------------------------------------------------------------- the move

function Get-ShiftKey($Config, [string]$From, [string]$To) {
    if ($To -eq 'Removed') { return '*>Removed' }
    "$From>$To"
}

function Test-Move {
    # Returns @{ ok; shift; failures; skipped } without changing anything.
    param($Config, $Item, [Parameter(Mandatory)][string]$To, $Adapter, [string]$Root = '.', [object[]]$AllItems, [switch]$SkipScripts)
    $from = $Item.state
    $failures = [Collections.Generic.List[string]]::new()
    $skipped = [Collections.Generic.List[string]]::new()
    $fromDef = Get-StateDef $Config $from
    $toDef = Get-StateDef $Config $To

    # Legal route: the next state in the order, a declared back move, or Removed from any unfinished state.
    $order = Get-ItemOrder $Config $Item
    $next = if ($order.IndexOf($from) -ge 0 -and $order.IndexOf($from) -lt $order.Count - 1) { $order[$order.IndexOf($from) + 1] } else { $null }
    $isBack = [bool]($Config.backMoves | Where-Object { $_.from -eq $from -and $_.to -eq $To })
    if ($fromDef.kind -eq 'end') { $failures.Add("$from is finished; it cannot move") }
    elseif (-not ($To -eq $next -or $isBack -or $To -eq 'Removed')) { $failures.Add("$from cannot move to $To; the next state is $next") }

    # WIP limit of the state being entered.
    if ($toDef.wipLimit -and $AllItems) {
        $inTarget = @($AllItems | Where-Object { $_.state -eq $To -and "$($_.id)" -ne "$($Item.id)" }).Count
        if ($inTarget -ge $toDef.wipLimit) { $failures.Add("$To is full ($inTarget of $($toDef.wipLimit))") }
    }

    $key = Get-ShiftKey $Config $from $To
    $shift = if ($isBack) { @{ name = 'Back'; checks = @() } } elseif ($Config.shifts.ContainsKey($key)) { $Config.shifts[$key] } else { $null }
    if (-not $shift -and $failures.Count -eq 0) { $failures.Add("no policy is defined for $key") }
    if ($shift) {
        $evidence = Get-ItemEvidence $Item $Root
        foreach ($c in $shift.checks) {
            $r = Test-Check -Check $c -Item $Item -Evidence $evidence -Adapter $Adapter -Root $Root -SkipScripts:$SkipScripts -Config $Config
            if ($r -and $r.ContainsKey('skipped')) { $skipped.Add($r.message) }
            elseif ($r) { $failures.Add($r.message) }
        }
    }
    @{ ok = ($failures.Count -eq 0 -and $skipped.Count -eq 0); shift = $shift; failures = @($failures); skipped = @($skipped) }
}

function Move-WorkItem {
    # The only way an item's state changes. Checks the policy, then sets the state and records the move and its reason.
    param($Config, [Parameter(Mandatory)]$Id, [Parameter(Mandatory)][string]$To, [Parameter(Mandatory)][string]$Reason,
          $Adapter, [string]$Root = '.', [switch]$WhatIf)
    if (-not $Adapter) { $Adapter = Get-BoardAdapter $Config $Root }
    $item = & $Adapter.GetItem $Id
    if (-not $item) { throw "Item $Id not found." }
    if ($item.state -eq $To) { return @{ ok = $true; moved = $false; message = "already $To" } }
    $all = @(& $Adapter.GetItems)
    $t = Test-Move -Config $Config -Item $item -To $To -Adapter $Adapter -Root $Root -AllItems $all
    if (-not $t.ok) { return @{ ok = $false; moved = $false; from = $item.state; to = $To; failures = $t.failures } }
    if ($WhatIf) { return @{ ok = $true; moved = $false; from = $item.state; to = $To; shift = $t.shift.name } }

    $fromCat = (Get-StateDef $Config $item.state).category
    $toCat = (Get-StateDef $Config $To).category
    $record = [ordered]@{ from = $item.state; to = $To; shift = $t.shift.name; at = (Get-Date).ToUniversalTime().ToString('o'); reason = $Reason }
    if ($fromCat -eq 'Proposed' -and $toCat -eq 'In Progress') { $record.records = 'started' }
    if ($toCat -in 'Completed', 'Removed') { $record.records = 'finished' }
    $json = $record | ConvertTo-Json -Compress
    $text = "Moved from **$($item.state)** to **$To** ($($t.shift.name)): $Reason`n`n<!-- $script:MoveMarker $json -->"
    & $Adapter.SetState $item $To
    if ($Config.board.moveComments -ne $false) { & $Adapter.AddComment $item $text }
    @{ ok = $true; moved = $true; from = $item.state; to = $To; shift = $t.shift.name; records = $record['records'] }
}

# ---------------------------------------------------------------- watching and questions

function Get-OpenQuestions([string]$Evidence) {
    $s = Get-Section $Evidence 'Questions'
    if (-not $s) { return @() }
    @([regex]::Matches($s, '(?im)^\s*-\s*(Q-\d+)\b(.*?)\bopen\s*$') | ForEach-Object { @{ id = $_.Groups[1].Value; text = $_.Groups[2].Value.Trim() } })
}

function Sync-ItemWatch {
    # Run by a WKR's watch. Finds what changed on the item since it last looked, records answers to open
    # questions (a reply that names Q-n), and sets or clears the blocked tag. Returns what changed.
    param($Config, [Parameter(Mandatory)]$Id, $Adapter, [string]$Root = '.')
    if (-not $Adapter) { $Adapter = Get-BoardAdapter $Config $Root }
    $item = & $Adapter.GetItem $Id
    $seenPath = Join-Path $Root ".specify/wkr/$Id.seen.json"
    $seen = if (Test-Path $seenPath) { Get-Content $seenPath -Raw | ConvertFrom-Json -AsHashtable } else { @{ comments = 0; state = $item.state; tags = @($item.tags) } }
    $comments = @($item.comments)
    $new = @($comments | Select-Object -Skip ([int]$seen.comments) | Where-Object { "$($_.body)" -notmatch "<!--\s*$script:MoveMarker" })
    $changes = [ordered]@{ id = $Id; newComments = $new.Count; stateChanged = ($seen.state -ne $item.state); tagsChanged = ((@($seen.tags) -join ',') -ne (@($item.tags) -join ',')); answered = @(); blocked = $false }

    $wkr = Join-Path $Root ".specify/wkr/$Id.md"
    if (-not (Test-Path $wkr)) { New-Item -ItemType Directory -Force (Split-Path $wkr) | Out-Null; "# WKR-$Id`n" | Set-Content $wkr -Encoding utf8 }
    $wkrText = (Get-Content $wkr -Raw) -replace "`r`n", "`n"
    foreach ($c in $new) {
        foreach ($m in [regex]::Matches("$($c.body)", '(?m)\b(Q-\d+)\s*:\s*(.+)$')) {
            $q = $m.Groups[1].Value; $answer = $m.Groups[2].Value.Trim()
            $who = if ($c.ContainsKey('author') -and $c.author) { $c.author } else { 'reply on the item' }
            $line = [regex]::Match($wkrText, "(?im)^(\s*-\s*$q\b.*?)\bopen\s*$")
            if ($line.Success) {
                $wkrText = $wkrText.Replace($line.Value, "$($line.Groups[1].Value.TrimEnd()) answered: ""$answer"" ($who, $($c.at))")
                $changes.answered += $q
            }
        }
    }
    # Write only when an answer was recorded, so an unchanged poll does not count as activity on the WKR.
    if ($changes.answered.Count) { Set-Content $wkr $wkrText -NoNewline -Encoding utf8 }

    $item = & $Adapter.GetItem $Id
    $open = @(Get-OpenQuestions (Get-ItemEvidence $item $Root))
    $tag = $Config.tags.blocked
    $isBlocked = $item.tags -contains $tag
    if ($open.Count -and -not $isBlocked) { & $Adapter.SetTag $item $tag $true }
    if (-not $open.Count -and $isBlocked) { & $Adapter.SetTag $item $tag $false }
    $changes.blocked = [bool]$open.Count
    $item = & $Adapter.GetItem $Id
    # The cursor is the snapshot this pass processed, so a reply that arrived meanwhile is read next time.
    @{ comments = $comments.Count; state = $item.state; tags = @($item.tags) } | ConvertTo-Json | Set-Content $seenPath -Encoding utf8
    $changes
}

# ---------------------------------------------------------------- adoption

function Get-SupportedState {
    # Walks an item forward from Proposed along its route, through each move's policy (no WIP, no scripts),
    # and returns the furthest state its evidence supports, up to Target. Also returns what stopped it.
    param($Config, $Item, [string]$Target, $Adapter, [string]$Root = '.')
    $order = Get-ItemOrder $Config $Item
    if ($Target -in 'Closed', 'Removed') { return @{ state = $Target; missing = @() } }
    $stop = $order.IndexOf($Target); if ($stop -lt 0) { return @{ state = 'Proposed'; missing = @("'$Target' is not on this item's route") } }
    $probe = @{} + $Item; $probe.state = 'Proposed'
    for ($i = 0; $i -lt $stop; $i++) {
        $t = Test-Move -Config $Config -Item $probe -To $order[$i + 1] -Adapter $Adapter -Root $Root -SkipScripts
        $real = @($t.failures | Where-Object { $_ -notmatch ' is full \(' })
        if ($real.Count) { return @{ state = $order[$i]; missing = $real } }
        $probe.state = $order[$i + 1]
    }
    @{ state = $Target; missing = @() }
}

function Get-AdoptionPlan {
    # The reconcile dry run: for each open item, the state its old labels map to, and the state its evidence supports.
    param($Config, $Adapter, [string]$Root = '.')
    $map = $Config.adopt.map
    $plan = foreach ($item in @(& $Adapter.GetItems)) {
        $old = @($item.tags | Where-Object { $map.ContainsKey($_) }) + @(if ($item.ContainsKey('oldStatus') -and $item.oldStatus -and $Config.adopt.statusMap.ContainsKey($item.oldStatus)) { "status:$($item.oldStatus)" })
        $map = @{} + $Config.adopt.map; foreach ($k in $Config.adopt.statusMap.Keys) { $map["status:$k"] = $Config.adopt.statusMap[$k] }
        $mapped = if ($old) { ($old | ForEach-Object { $map[$_] } | Sort-Object { $Config.order.IndexOf($_) } | Select-Object -Last 1) } elseif ($item.state -and $item.state -ne $Config.adopt.default) { $item.state } else { $Config.adopt.default }
        $s = Get-SupportedState -Config $Config -Item $item -Target $mapped -Adapter $Adapter -Root $Root
        [ordered]@{ id = $item.id; title = $item.title; type = $item.type; old = (@($old) -join ', '); mapped = $mapped; new = $s.state; missing = @($s.missing); removeTags = @($old) }
    }
    @($plan)
}

function Format-AdoptionPlan([object[]]$Plan) {
    $lines = @('# Adoption: reconcile dry run', '', '| Item | Type | Old | Maps to | New state | Why not further |', '|---|---|---|---|---|---|')
    foreach ($p in $Plan) { $lines += "| #$($p.id) $($p.title) | $($p.type) | $($p.old) | $($p.mapped) | **$($p.new)** | $(($p.missing -join '; ')) |" }
    $lines -join "`n"
}

function Invoke-AdoptionApply {
    # Applies an approved plan: sets each item's state, records "adopted from <old>", then removes the old labels.
    param($Config, $Adapter, [object[]]$Plan, [string]$Root = '.')
    foreach ($p in $Plan) {
        $item = & $Adapter.GetItem $p.id
        if ($item.state -ne $p.new) {
            $rec = [ordered]@{ from = $item.state; to = $p.new; shift = 'Adopted'; at = (Get-Date).ToUniversalTime().ToString('o'); reason = "adopted from $(if ($p.old) { $p.old } else { 'no old state' })" }
            & $Adapter.SetState $item $p.new
            & $Adapter.AddComment $item "Adopted onto the workflow in **$($p.new)** ($($rec.reason)).`n`n<!-- $script:MoveMarker $($rec | ConvertTo-Json -Compress) -->"
        }
        foreach ($t in @($p.removeTags | Where-Object { $_ -notlike 'status:*' })) { & $Adapter.SetTag (& $Adapter.GetItem $p.id) $t $false }
    }
}

# ---------------------------------------------------------------- the manager

function Get-PullOrder($Config, [object[]]$Items) {
    $exp = $Config.tags.expedite
    $Items | Sort-Object @{ Expression = { if ($_.tags -contains $exp) { 0 } else { 1 } } }, @{ Expression = { Get-StateSince $_ } }
}

function Invoke-WorkflowManager {
    # One pass: pull from wait columns into free slots, move work forward where its policy passes,
    # find hung work, and list what needs Martin. Returns a report; changes only states and move records.
    param($Config, $Adapter, [string]$Root = '.', [datetime]$Now = (Get-Date).ToUniversalTime(), [switch]$WhatIf)
    if (-not $Adapter) { $Adapter = Get-BoardAdapter $Config $Root }
    $report = [ordered]@{ at = $Now.ToString('o'); pulls = @(); moves = @(); actions = @(); stuck = @(); attention = @() }
    $order = $Config.order
    $blocked = $Config.tags.blocked

    # 1. Move work forward where its policy passes (script checks are left to the run itself).
    foreach ($item in @(& $Adapter.GetItems)) {
        $def = Get-StateDef $Config $item.state
        $parentReady = (Test-IsParent $item) -and $item.state -eq 'Refined'
        if (($def.kind -notin 'work', 'wait-children' -and -not $parentReady) -or $item.tags -contains $blocked) { continue }
        $full = & $Adapter.GetItem $item.id
        $iorder = Get-ItemOrder $Config $full
        $i = $iorder.IndexOf($item.state); if ($i -lt 0 -or $i -ge $iorder.Count - 1) { continue }
        $to = $iorder[$i + 1]
        $all = @(& $Adapter.GetItems)
        $t = Test-Move -Config $Config -Item $full -To $to -Adapter $Adapter -Root $Root -AllItems $all -SkipScripts
        if ($t.ok) {
            $r = Move-WorkItem -Config $Config -Id $item.id -To $to -Reason "its $($t.shift.name) policy passed" -Adapter $Adapter -Root $Root -WhatIf:$WhatIf
            if ($r.ok) { $report.moves += @{ id = $item.id; from = $item.state; to = $to; shift = $t.shift.name } }
        }
    }

    # 2. Pull from each wait column into the next state while it has free slots.
    foreach ($w in @($Config.states | Where-Object { $_.kind -eq 'wait' })) {
        $i = $order.IndexOf($w.name); if ($i -lt 0 -or $i -ge $order.Count - 1) { continue }
        $next = Get-StateDef $Config $order[$i + 1]
        $all = @(& $Adapter.GetItems)
        $free = if ($next.wipLimit) { $next.wipLimit - @($all | Where-Object { $_.state -eq $next.name }).Count } else { [int]::MaxValue }
        foreach ($cand in @(Get-PullOrder $Config @($all | Where-Object { $_.state -eq $w.name -and $_.tags -notcontains $blocked -and -not ($w.name -eq 'Refined' -and (Test-IsParent $_)) }))) {
            if ($free -le 0) { break }
            if ($w.name -eq 'Approved' -and (Test-Check -Check @{ type = 'pattern'; pattern = '(?m)^WKR:\s*\S'; message = '' } -Item $cand -Evidence (Get-ItemEvidence $cand $Root) -Adapter $Adapter)) {
                $wkr = "WKR-$($cand.id)"
                if (-not $WhatIf) { & $Adapter.AddComment $cand "WKR: $wkr" }
                $cand = & $Adapter.GetItem $cand.id
                $report.actions += @{ action = 'start-wkr'; id = $cand.id; wkr = $wkr; state = $next.name }
            }
            else { $named = [regex]::Match((Get-ItemEvidence $cand $Root), '(?m)^WKR:\s*(\S+)'); if (-not $named.Success -and $cand.ContainsKey('parent') -and $cand.parent) { $named = [regex]::Match((Get-ItemEvidence (& $Adapter.GetItem $cand.parent) $Root), '(?m)^WKR:\s*(\S+)') }; $report.actions += @{ action = 'resume-wkr'; id = $cand.id; wkr = $named.Groups[1].Value; state = $next.name } }
            $r = Move-WorkItem -Config $Config -Id $cand.id -To $next.name -Reason "pulled from $($w.name) into a free $($next.name) slot" -Adapter $Adapter -Root $Root -WhatIf:$WhatIf
            if ($r.ok) { $report.pulls += @{ id = $cand.id; from = $w.name; to = $next.name }; $free-- }
            else { $report.attention += @{ kind = 'pull-refused'; id = $cand.id; state = $w.name; detail = ($r.failures -join '; ') } }
        }
    }

    # 3. Watchdog and attention list.
    foreach ($item in @(& $Adapter.GetItems)) {
        $def = Get-StateDef $Config $item.state
        $since = Get-StateSince $item
        $waited = [math]::Round(($Now - $since.ToUniversalTime()).TotalHours, 1)
        if ($item.tags -contains $blocked) { $report.attention += @{ kind = 'blocked'; id = $item.id; state = $item.state; waitedHours = $waited } }
        elseif ($def.kind -eq 'wait-martin' -or ((Test-IsParent $item) -and $item.state -eq 'Refined')) { $report.attention += @{ kind = 'decision'; id = $item.id; state = $item.state; waitedHours = $waited } }
        foreach ($oq in @(Get-OpenQuestions (Get-ItemEvidence $item $Root))) { $report.attention += @{ kind = 'question'; id = $item.id; question = $oq.id; state = $item.state } }
        if ($def.kind -eq 'work' -and $def.stuckAfterMinutes -and $item.tags -notcontains $blocked) {
            $idle = ($Now - (Get-LastActivity $item $Root)).TotalMinutes
            if ($idle -gt $def.stuckAfterMinutes) {
                $restarts = @(Get-ItemMoves $item | Where-Object { $_['restart'] }).Count
                $s = @{ id = $item.id; state = $item.state; idleMinutes = [math]::Round($idle) }
                $report.stuck += $s
                if ($restarts -eq 0) { $report.actions += @{ action = 'restart-step'; id = $item.id; state = $item.state } }
                else { $report.attention += @{ kind = 'hung'; id = $item.id; state = $item.state; detail = 'hung again after a restart' } }
            }
        }
    }
    $report.attention = @($report.attention | Sort-Object { -[double]($_['waitedHours'] ?? 0) })
    $report
}

function Format-AttentionList($Report) {
    $lines = @("# Waiting on Martin ($($Report.at))", '')
    if (-not $Report.attention) { return ($lines + 'Nothing is waiting on you.') -join "`n" }
    foreach ($a in $Report.attention) {
        $lines += switch ($a.kind) {
            'decision'     { "- #$($a.id) is in **$($a.state)**, waiting for your decision ($($a.waitedHours) h)." }
            'blocked'      { "- #$($a.id) is **blocked** in $($a.state) ($($a.waitedHours) h)." }
            'question'     { "- #$($a.id) has open question **$($a.question)**." }
            'hung'         { "- #$($a.id) is hung in $($a.state): $($a.detail)." }
            'pull-refused' { "- #$($a.id) could not be pulled from $($a.state): $($a.detail)." }
            default        { "- #$($a.id): $($a.kind)" }
        }
    }
    $lines -join "`n"
}

Export-ModuleMember -Function Get-SupportedState, Get-AdoptionPlan, Format-AdoptionPlan, Invoke-AdoptionApply, Test-IsParent, Get-ItemOrder, Get-WorkflowConfig, Get-StateDef, New-FileBoardAdapter, New-GitHubBoardAdapter, Get-BoardAdapter,
    Get-ItemEvidence, Get-Section, Get-ItemMoves, Get-StateSince, Test-Check, Test-Move, Move-WorkItem, Invoke-WorkflowManager, Format-AttentionList, Get-OpenQuestions, Sync-ItemWatch,
    Get-SpecAgreed, Get-OutcomeProblem, Get-SkillText, Get-SkillCopies, Get-SharedDrift
