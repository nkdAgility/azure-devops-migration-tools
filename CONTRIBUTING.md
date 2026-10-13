## Contributor License Agreement (CLA)

Before making a contribution, all contributors must agree to the following Contributor License Agreement (CLA). By submitting a pull request or any contribution, you acknowledge and agree to the following:

1. **Grant of License**:  
   You grant us, Naked Agility Limited ("Us" or "We"), a perpetual, worldwide, non-exclusive, royalty-free, irrevocable license to use, modify, publish, and distribute your contributions under the terms of the GNU Affero General Public License (AGPL). This ensures that contributions remain aligned with the project's licensing terms.

2. **Your Rights**:  
   You retain the copyright to your contributions and are free to use them as you see fit, independent of this agreement, provided such use does not conflict with the AGPL.

3. **Warranties**:  
   You represent that:
   - You are the original author of the contributions.
   - Your contributions are not subject to any conflicting agreements or third-party rights.
   - Your contributions comply with the project's **Contributing Guidelines**.

4. **Obligation to Share**:  
   You agree that any modifications to the software made as part of your use of this project will be shared back with the community by submitting changes to the original repository.

By submitting a contribution to the project, you agree to this CLA. If you do not agree, please refrain from contributing.

---

## Contributing Guidelines

### Asking Questions or Starting Discussions

- **Use GitHub Discussions**:  
  If you have questions, ideas, or suggestions, please use the [GitHub Discussions tab](https://github.com/nkdAgility/azure-devops-migration-tools/discussions). This helps centralise knowledge and allows the community to collaborate effectively.  
  - Browse existing discussions to see if your question or idea has already been addressed.  
  - Add comments or reactions (e.g., 👍, 👎) to relevant discussions.

- **Use the Comment/Discussion Boxes on Documentation Pages**:  
  If your topic relates to an existing feature, check the relevant [documentation page](https://devopsmigration.io/) for details. Most pages include comment or discussion boxes at the bottom.  
  - Using these boxes for discussions tied to specific features helps keep the conversation contextual and accessible for others with similar questions.

---

### Submitting Issues

#### When to Submit an Issue
Issues should only be created for confirmed **bugs** or **feature requests**. Before creating an issue:
- **Search for Existing Issues**:  
  Check [open issues](https://github.com/nkdAgility/azure-devops-migration-tools/issues) to avoid duplicates. Use this [query](https://github.com/nkdAgility/azure-devops-migration-tools/issues?q=is%3Aopen+is%3Aissue+sort%3Areactions-%2B1-desc+) to find popular feature requests.  
  If your issue already exists:
  - Add comments with additional details or context.
  - Use reactions (e.g., 👍, 👎) to express your interest.

#### Writing Good Bug Reports and Feature Requests
- Submit **one issue per problem or feature**.
- Avoid combining multiple bugs or requests in a single issue.
- Include as much detail as possible to help others reproduce the issue:
  - Version of Azure DevOps Migration Tools
  - Source and target instance versions
  - Step-by-step reproduction instructions
  - The **Session ID** from your migration run (found in the log file).

---

### Submitting Pull Requests

#### Bug Fixes
- **Focus on Simplicity**: Keep changes minimal and targeted to the bug being fixed.
- **Update GitVersion.yaml**: Ensure version numbers do not collide.

#### Feature Enhancements
- Features should add or improve functionality. Clearly describe the feature and its use case in the pull request.

#### General Housekeeping
Pull requests should:
1. Be based on the **master** branch.
2. Avoid merge commits (use a linear commit history).
3. Include adequate tests:
   - Tests should fail without your changes.
   - Cover edge cases and various scenarios.
   - Maintain 100% code coverage.
4. Adhere to the coding guidelines.
5. Include clear, descriptive commit messages.

#### Coding and Git Configuration
- Follow the project's coding conventions (see [Coding Guidelines](#)).  
- Set your Git configuration to avoid line-ending issues:
  ```bash
  git config --global core.autocrlf input
  git config --global core.whitespace cr-at-eol
  ```

### Running the tests

There are three ways to run the tests. Pick the one that fits.

#### 1. Unit tests only (no setup)

```bash
dotnet test MigrationTools.sln
```

Everything runs offline. Tests that need live Azure DevOps are marked `[RequiresAzureDevOps]` and are **skipped**, not failed, with a message explaining how to enable them. This is enough for most changes.

#### 2. Live tests against your own Azure DevOps orgs

1. Create two free Azure DevOps organisations, one for the source and one for the target.
2. Sign in with an account that is an admin of both: `az login`.
3. Create the test projects and configure the variables:

   ```bash
   ./infra/test-env/Setup-TestEnvironment.ps1 -LocalOnly -SourceOrg <your-source-org> -TargetOrg <your-target-org> -SetUserEnvironment
   ```

   This creates `migrationSource1` and `migrationTarget1` and saves these user environment variables (leave out `-SetUserEnvironment` to print them instead):

   | Variable | Purpose |
   |---|---|
   | `MigrationTools__Tests__UseEntraId=true` | Use your Microsoft Entra login (`az login`, Visual Studio). No PAT needed. |
   | `MigrationTools__Tests__SourceCollection` | Source org URL, e.g. `https://dev.azure.com/my-source-org/` |
   | `MigrationTools__Tests__TargetCollection` | Target org URL (defaults to the source org) |
   | `MigrationTools__Tests__AccessToken` | Optional: a PAT instead of Entra, e.g. for orgs backed by a personal Microsoft account (alias `AZDEVOPS_SYSTEM_TEST_PAT`) |

4. Open a new terminal and run `dotnet test MigrationTools.sln`.

#### 3. Live tests on your pull request

You don't need any credentials. When you open a pull request from a fork, the `Integration tests (Azure DevOps)` workflow waits for a maintainer. They review your change and approve the `integration-tests-forks` environment, and the live tests then run against the project's test orgs. Credentials are never shared with you or exposed to your fork. A new push needs a new approval.

**Maintainers and agents:** pushes to `main`, the merge queue and same-repo branches run the live tests automatically. CI signs in with OIDC workload identity federation, so there is no secret to rotate. Locally, `az login` with an account that has access to the test orgs is enough. The environment is defined as code in `infra/test-env/` (`main.bicep` plus `Setup-TestEnvironment.ps1`).

### Troubleshooting live tests

Live runs start with `LiveEnvironment_*` preflight tests. If something is wrong with the setup, they fail with a message naming the org, the auth method in use (never the credential itself) and the fix.

| Message | Cause and fix |
|---|---|
| *The credential was rejected (HTTP 401/203/302)* | PAT expired or revoked, or the identity isn't a member of the org. A stale `AZDEVOPS_SYSTEM_TEST_PAT` overrides other settings, so remove it if you use Entra. For Entra, check that `az account show` shows the account you added to the org. |
| *Could not get a Microsoft Entra token* | Run `az login` (or sign in to Visual Studio), or switch to a PAT. |
| *Project 'migrationSource1' was not found* | Run `Setup-TestEnvironment.ps1 -LocalOnly ...` to create the projects, and check the org URL. |
| *Could not reach the org* | Wrong org URL in `MigrationTools__Tests__*Collection`, or a network problem. |
| Tests skipped with *no Azure DevOps test credentials* | Expected when nothing is configured. Follow option 2 to enable them. |

If the preflight passes but other tests fail, the environment is fine and it's a real test failure.