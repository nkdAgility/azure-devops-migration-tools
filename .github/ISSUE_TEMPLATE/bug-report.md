---
name: Bug Report
about: File a bug report
title: "[Bug]: "
labels: bug, triage
assignees: ''

---

- type: markdown
    attributes:
      value: |
        Thanks for taking the time to fill out this bug report! If you have questions or want to discus how the tool works then please [open a discussion](https://github.com/nkdAgility/azure-devops-migration-tools/discussions) instead of filing a bug report.
  - type: checkboxes
    attributes:
      label: Version
      description: Are you using the latest version of our software?
      options:
        - label: I confirm that I am using the latest version
          required: true
  - type: dropdown
    id: sourceVersion
    attributes:
      label: Source Version
      description: What version of Azure DevOps / TFS are you pulling data from?
      options:
        - Azure DevOps Service
        - Azure DevOps Server 2020
        - Azure DevOps Server 2019
        - Team Foundation Server 2017
        - Team Foundation Server 2015
        - Team Foundation Server 2013
        - Team Foundation Server 2010
        - Team Foundation Server 2008
    validations:
      required: true
  - type: dropdown
    id: targetVersion
    attributes:
      label: Target Version
      description: What version of Azure DevOps / TFS are you pushing data to?
      options:
        - Azure DevOps Service
        - Azure DevOps Server 2020
        - Azure DevOps Server 2019
        - Team Foundation Server 2017
        - Team Foundation Server 2015
        - Team Foundation Server 2013
        - Team Foundation Server 2010
        - Team Foundation Server 2008
    validations:
      required: true
  - type: textarea
    id: configuration
    attributes:
      label: Relevant configuration
      description: Please copy and paste your configuration file used. This will be automatically formatted into code, so no need for backticks.
      render: shell
  - type: textarea
    id: logs
    attributes:
      label: Relevant log output
      description: Please copy and paste any relevant log output. This will be automatically formatted into code, so no need for backticks.
      render: shell
  - type: textarea
    id: what-happened
    attributes:
      label: What happened?
      description: Also tell us, what did you expect to happen?
      placeholder: Tell us what you see!
      value: "A bug happened!"
    validations:
      required: true
  - type: checkboxes
    id: debug
    attributes:
      label: Debug in Visual Studio
      description: I am able to debug in Visual Studio
      options:
        - label: Visual Studio Debug
          required: true
