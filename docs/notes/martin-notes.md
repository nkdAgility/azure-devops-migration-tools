# Martin's notes

These are the facts and decisions Martin has given, so they can be checked. They are not rules: rule text changes only once Martin approves its wording.

## 2026-11-12

- **Fact (Martin, to check):** Shared Steps cannot have custom fields. The Microsoft Learn inherited-process docs list only Test Plan, Test Suite and Test Case as customisable test types. That means the ReflectedWorkItemId field cannot be added to Shared Steps on an inherited target.
- **Decision:** the migration must still find these work items again on re-runs and wire them up (Test Case step references, links). Without that, the migration is not complete.
- **Finding (checked 2026-11-12, read-only, target org `nkdagility-adomt-target`):** confirmed. The inherited process `amte2e-agile` lists Test Case, Test Plan and Test Suite (plus the backlog types) as its work item types; Shared Steps, Shared Parameter, Code Review Request/Response and Feedback Request/Response are not listed, so they cannot be customised. In a project on that process, none of Shared Steps, Shared Parameter, Code Review Request or Feedback Request has `Custom.ReflectedWorkItemId`. All of them have `System.HyperLinkCount`. WIQL can filter on `System.HyperLinkCount` but not on a hyperlink's URL.
- **Design as built:** a target type without the reflected field gets a hyperlink to the source work item, with comment `Migrated from (ReflectedWorkItemId)`. The lookup queries candidates of those types with `[System.HyperLinkCount] > 0` once per run, indexes them by link URL, and adds items as they are saved. The ExportWorkItemMappingTool file is not used as a lookup: it can be stale or missing, and the index costs one query.
- **Decision (Martin):** use `Microsoft.VSTS.Build.IntegrationBuild` to hold the reflected work item ID, and only for the specific list of work item types that cannot be customised (Code Review Request, Code Review Response, Feedback Request, Feedback Response, Shared Parameter, Shared Steps). Not for any other type that lacks the field. This replaces the hyperlink lookup, which loads every candidate and does not scale (for example to a million items).
- **Finding (checked read-only, target org, project `migrationTarget1`):** `Microsoft.VSTS.Build.IntegrationBuild` is the only writable general-purpose string field present on all six of those types.
- **Instruction (Martin), for each worker:** run the e2e scenarios already in use on the test orgs, plus the issue's own scenario, on both `main` and the branch, including failure paths; put the output and exit codes in the PR; use only its own `amt<issue>-*` scratch projects and processes and clean them up afterwards; follow the Definition of Done and report confidence (auto-merge only at 90% or higher); do not comment on the GitHub issues. The e2e harness only accepts names starting `amte2e-`, so this PR uses `amte2e-3245-*`.
- **Instruction (Martin):** for this fix, create a GitHub issue if there isn't one (created #3248, linked from PR #3245), remove the triage label if it has one (#3248 was created with `bug` only), add a comment that Martin is working on it, and keep that comment updated with the latest findings and the status of the fix. This overrides "do not comment on the GitHub issues" for this issue.
- **Decision:** spawn one worker each for #3240, #3241 and #3242. Each must run the end-to-end tests on the test orgs (tools/e2e/Invoke-MigrationE2E.ps1) against main and the branch.
- **Decision:** no changes to the way of working until Martin approves them.
- **Decision:** in this repo the GitHub issue is the work package. There is no separate WKR-{id}.md file.
- **Decision:** adapt the nkda-engineering-delivery stage-gate tool to store a package's state in its GitHub issue (the issue has the original description, then comments).
- **Decision:** for #3242, remove the issue's `triage` label and post a comment saying Martin is working on it. Keep that comment updated with the latest findings and the status of the fix (PR #3246). This supersedes the earlier "do not comment on the GitHub issue" for #3242.
- **Decision:** issue workers use their own `amt<issue>-*` scratch processes and projects, and never modify an existing process.
- **Decision:** on #3241, remove the `triage` label and post a comment saying Martin is working on it. Keep that comment updated with the latest discovery and the status of the fix. This replaces the earlier "do not comment on the issue" instruction for #3241.
- **Decision:** the stage-gate tool keeps a package's state as a `state:*` label on its issue (one at a time, changed only by the tool) and posts a short comment for each stage shift with its reason, worded for a customer to read. The issue description is never edited.
- **Decision:** the nkda-engineering-delivery plugin must support the workflow this repo has chosen, not impose its own.
- **Decision:** NKDAEngineering-Delivery provides the tools and the wrapper; the workflow itself is defined by this repo.
- **Decision:** any change that affects NKDAContent-Hub's setup (including the plugin's built-in states and checks) is agreed with the CONTROL-HUB session first.
- **Decision:** document in NKDAEngineering-Delivery the prompts to use to integrate a new engineering flow, to add new tools, and to create a new product's workflow.
- **Decision:** ADMT records which worker chat owns an issue as a hidden tag (an HTML comment such as `<!-- worker: ... -->`) in the issue's status comment, written only by the stage-gate tool.
- **Fact (Martin, to check):** the "azure-devops-migration-platform" repository comes in next for this way of working. It includes a number of new tools and workflows.
- **Decision:** the plugin's tools can be skills, workflows, agents or anything else. A repository's Definition of Workflow drives them, directly or indirectly: it combines the available tools into what that repository uses, so new skills can be added to the plugin and each repository chooses whether to use them.
- **Decision (given in CONTROL-HUB, relayed):** any change to the plugin, or to how we interact with it, is worked through with every active CONTROL-* chat to ensure compatibility before it is pushed.
- **Decision (given in CONTROL-HUB, relayed):** the machine-readable workflow lives in a file of the same name with a .json extension, beside the prose: `platform/development/definition-of-workflow.md` and `platform/development/definition-of-workflow.json`. It is not embedded in the Markdown. ADMT needs its own .json.
- **Decision:** ADMT's WIP limit is 3 issues in fixing and in review together.

## 2026-11-13

- **Decision:** Martin approved the "Issue status comment" wording for CLAUDE.md and AGENTS.md (workers update the status comment only with `Test-StageGate.ps1 -Status`), and the matching sentence in definition-of-workflow.md. It is pushed together with the plugin's status-comment hook.
- **Finding:** sessions run a cached copy of nkda-engineering-delivery 0.1.0 (from 2026-11-12 12:22), because the plugin's version never changed. None of the 2026-11-12 plugin changes (definition-of-workflow.json, issue storage, -Status, the gh hook) are live yet.
- **Decision:** the plugin is versioned with GitVersion, like every other tool Martin uses.
- **Decision (given in CONTROL-HUB, relayed):** plugins are installed machine-wide only (managed settings), never at user scope. The plugin's build.ps1 installs nothing and prints the steps for Martin.
- **Fact (Martin):** in this repo the backlog and the options are stored in GitHub issues.
- **Fact (Martin):** you can't push on main. Checked: main's ruleset requires a pull request, status checks and the merge queue. The earlier direct pushes (22202606, 3db6b3fb, 8af3e1bc on 2026-11-12/13) went around that rule.

## 2026-11-16

- **Instruction (Martin):** bring the documentation, including README.md, up to date with main (issue #3250). Rule text in `.github/copilot-instructions.md` and `docs/copilot-instructions.md` changes only once Martin approves the wording proposed in the PR. The stale `docs/data/releases*.json` is mentioned in the PR, not deleted.
- **Reminder (Martin):** much of the docs runs through the generator, so fix XML comments and appsettings.json samples and regenerate rather than hand-editing generated output.

## 2026-11-13

- **Decision:** on PR #3249 (#3241), the two failure paths that can't be forced on the test organisations stay untested live: the read-back of a new field failing after a successful create, and the service rejecting an update to an existing picklist. They are covered by code review and unit tests and listed under "Not covered" in the PR.
