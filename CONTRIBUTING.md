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
