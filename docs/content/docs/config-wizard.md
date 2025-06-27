---
title: Interactive Configuration Wizard
description: Build your Azure DevOps Migration Tools configuration with our interactive step-by-step wizard supporting three migration type architectures
weight: 50
date: 2025-06-26T12:00:00Z
discussionId:
aliases:
  - /config-wizard/
  - /wizard/
  - /configure/
layout: config-wizard
cascade:
  - build:
      list: never
      render: never
    target:
      environment: production
---

Use this interactive wizard to build your Azure DevOps Migration Tools configuration. The wizard supports three primary migration types:

- **Work Items**: Standard work item migration with pre-configured TfsWorkItemEndpoint and TfsWorkItemMigrationProcessor
- **Pipelines**: Pipeline migration with pre-configured AzureDevOpsEndpoint and AzureDevOpsPipelineProcessor
- **Custom**: Advanced scenarios with complete flexibility to configure multiple endpoints and processors

The wizard will guide you through each step and generate a complete configuration file based on your selections and the migration type constraints.
