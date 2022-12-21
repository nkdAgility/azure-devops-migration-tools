## Contributing Issues

### Before Submitting an Issue
First, please do a search in [open issues](https://github.com/nkdAgility/azure-devops-migration-tools/issues) to see if the issue or feature request has already been filed. Use this [query](https://github.com/nkdAgility/azure-devops-migration-tools/issues?q=is%3Aopen+is%3Aissue+sort%3Areactions-%2B1-desc+) to search for the most popular feature requests.

If you find your issue already exists, make relevant comments and add your [reaction](https://github.com/blog/2119-add-reactions-to-pull-requests-issues-and-comments). Use a reaction in place of a "+1" comment.

👍 - upvote

👎 - downvote

The entire codebase for the Azure DevOps Migration Tools project is stored in this repository.

If your issue is a question then please ask the question on [Stack Overflow](https://stackoverflow.com/questions/tagged/azure-devops-migration-tools) using the tag `azure-devops-migration-tools`.

If you cannot find an existing issue that describes your bug or feature, submit an issue using the guidelines below.

## Writing Good Bug Reports and Feature Requests

File a single issue per problem and feature request.

* Do not enumerate multiple bugs or feature requests in the same issue.
* Do not add your issue as a comment to an existing issue unless it's for the identical input. Many issues look similar, but have different causes.

The more information you can provide, the more likely someone will be successful reproducing the issue and finding a fix. 

Please include the following with each issue. 

* Version of Azure DevOps Migration Tools
* Version of the source and target ADO/VSTS/TFS instances 

* Reproducible steps (1... 2... 3...) and what you expected versus what you actually got.
* The Session ID from your migration run that had a problem (this lets us look up telemitery).

> **Note:** Session ID is in your Log file for that run.

Please remember to do the following:

* Search the issue repository to see if there exists a duplicate. 

Don't feel bad if we can't reproduce the issue and ask for more information!

## Contributing Fixes

Azure DevOps Migration Tools accepts both bug fixes and feature enhancements as Pull Requests. When contibuting please try to minimise the number of files that you touch, and do not arbitrarily change large amounts of the files without justification. This will help keep the fix or feature as simple and easy as posible to review and incorporate. If you are a long term contributor you may request access to the git repository that houses the core code and provide direct contributions there.

# Instructions for Contributing Code

All fixed require to update GitVersion.yaml so that we dont have colliding version numbers.

## Contributing bug fixes

Bugs are things that cause exceptions and failures. Please keep fixes simple and minimal so that we can incorporate them quickly.

## Contributing features

Features (things that add new or improved functionality to Azure DevOps Migration Tools) may be accepted.

## Housekeeping

Your pull request should: 

* Include a description of what your change intends to do
* Be a child commit of a reasonably recent commit in the **master** branch 
    * Requests need not be a single commit, but should be a linear sequence of commits (i.e. no merge commits in your PR)
* It is desirable, but not necessary, for the tests to pass at each commit
* Have clear commit messages 
    * e.g. "Refactor feature", "Fix issue", "Add tests for issue"
* Include adequate tests 
    * At least one test should fail in the absence of your non-test code changes. If your PR does not match this criteria, please specify why
    * Tests should include reasonable permutations of the target fix/change
    * Include baseline changes with your change
    * All changed code must have 100% code coverage
* Follow the code conventions described in _Coding Guidelines_ (link lost)
* To avoid line ending issues, set `autocrlf = input` and `whitespace = cr-at-eol` in your git configuration