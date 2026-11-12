# Martin's notes

These are the facts and decisions Martin has given, so they can be checked. They are not rules: rule text changes only once Martin approves its wording.

## 2026-11-12

- **Fact (Martin, to check):** Shared Steps cannot have custom fields. The Microsoft Learn inherited-process docs list only Test Plan, Test Suite and Test Case as customisable test types. That means the ReflectedWorkItemId field cannot be added to Shared Steps on an inherited target.
- **Decision:** the migration must still find these work items again on re-runs and wire them up (Test Case step references, links). Without that, the migration is not complete.
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
