<#
    Script description.

    Some notes.
#>
# Helpers
enum ExecutionMode {
    log = 0
    diff = 1
}

# Define Parameters
function Get-ReleaseDescription {
    param (

        [Parameter(Mandatory=$false)]
        [string]$compairFrom,
        
        # name of the output folder
        [Parameter(Mandatory=$false)]
        [string]$compairTo,
    
        # name of the output folder
        [Parameter(Mandatory=$true)]
        [ExecutionMode]$mode,
    
        # name of the output folder
        [Parameter(Mandatory=$true)]
        [string]$OPEN_AI_KEY
    )
    Write-Host "========================================="
    Write-Host "Azure DevOps Migration Tools (Release) Description"
    Write-Host "========================================="
    Write-Host "Mode: $mode"
    if ([string]::IsNullOrEmpty($compairFrom) ){
        $lastRelease = gh release list --exclude-pre-releases --json name,tagName,publishedAt --limit 1 | ConvertFrom-Json
        $compairFrom = $lastRelease.tagName
    }
    If ([string]::IsNullOrEmpty($compairTo) ) {
        $compairTo = "main"
    }
    Write-Host "Comparing: $compairFrom...$compairTo"
    Write-Host "-----------------------------------------"    
    switch ($mode)
    {
        "log" {
            Write-Host "Running: git log"
            $result = git log --pretty=format:'{\"hash\": \"%H\", \"author\": \"%an\", \"date\": \"%ad\", \"message\": \"%s\"}' "$compairFrom...$compairTo" #
            Write-Host "Complete: git log"
        }
        "diff" {
            Write-Host "Running: git diff"
            $diffOutPut  = git diff "$compairFrom...$compairTo"
            $joineddiffOutPut = $diffOutPut -join "`n"
            $splitdiffOutPut = $joineddiffOutPut -split "diff --git"
            $result = $splitdiffOutPut | ConvertTo-Json
            Write-Host "Complete: git diff"
        }
        default {
            Write-Host "Invalid mode"
            return
        }
    }
    Write-Host "-----------------------------------------"
    If ([string]::IsNullOrEmpty($result) ) {
        Write-Host "No changes found"
        return
    } else {
        Write-Host "Changes found to create comment with!"
    }
    
    $prompt = @"
    
    Your mission is to create clean and comprehensive overview of the changes as per the GitMoji specification conventional commit convention and explain:
    
    - **WHAT** were the changes
    - **WHY** the changes were done
    
    I'll send you an output of the `git diff` command, and you are to convert it into a list of commits .For each commit Use GitMoji convention to preface each commit. Here are some help to choose the right emoji (emoji, description):
    
    - 🐛, Fix a bug
    - ✨, Introduce new features
    - 📝, Add or update documentation
    - 🚀, Deploy stuff
    - ✅, Add, update, or pass tests
    - ♻️, Refactor code
    - ⬆️, Upgrade dependencies
    - 🔧, Add or update configuration files
    - 🌐, Internationalization and localization
    - 💡, Add or update comments in source code
    - 🎨, Improve structure/format of the code
    - ⚡️, Improve performance
    - 🔥, Remove code or files
    - 🚑️, Critical hotfix
    - 💄, Add or update the UI and style files
    - 🎉, Begin a project
    - 🔒️, Fix security issues
    - 🔐, Add or update secrets
    - 🔖, Release / Version tags
    - 🚨, Fix compiler / linter warnings
    - 🚧, Work in progress
    - 💚, Fix CI Build
    - ⬇️, Downgrade dependencies
    - 📌, Pin dependencies to specific versions
    - 👷, Add or update CI build system
    - 📈, Add or update analytics or track code
    - ➕, Add a dependency
    - ➖, Remove a dependency
    - 🔨, Add or update development scripts
    - ✏️, Fix typos
    - 💩, Write bad code that needs to be improved
    - ⏪️, Revert changes
    - 🔀, Merge branches
    - 📦️, Add or update compiled files or packages
    - 👽️, Update code due to external API changes
    - 🚚, Move or rename resources (e.g., files, paths, routes)
    - 📄, Add or update license
    - 💥, Introduce breaking changes
    - 🍱, Add or update assets
    - ♿️, Improve accessibility
    - 🍻, Write code drunkenly
    - 💬, Add or update text and literals
    - 🗃️, Perform database related changes
    - 🔊, Add or update logs
    - 🔇, Remove logs
    - 👥, Add or update contributor(s)
    - 🚸, Improve user experience / usability
    - 🏗️, Make architectural changes
    - 📱, Work on responsive design
    - 🤡, Mock things
    - 🥚, Add or update an easter egg
    - 🙈, Add or update a .gitignore file
    - 📸, Add or update snapshots
    - ⚗️, Perform experiments
    - 🔍️, Improve SEO
    - 🏷️, Add or update types
    - 🌱, Add or update seed files
    - 🚩, Add, update, or remove feature flags
    - 🥅, Catch errors
    - 💫, Add or update animations and transitions
    - 🗑️, Deprecate code that needs to be cleaned up
    - 🛂, Work on code related to authorization, roles and permissions
    - 🩹, Simple fix for a non-critical issue
    - 🧐, Data exploration/inspection
    - ⚰️, Remove dead code
    - 🧪, Add a failing test
    - 👔, Add or update business logic
    - 🩺, Add or update healthcheck
    - 🧱, Infrastructure related changes
    - 🧑‍💻, Improve developer experience
    - 💸, Add sponsorships or money related infrastructure
    - 🧵, Add or update code related to multithreading or concurrency
    - 🦺, Add or update code related to validation
    
    Conventional commit keywords: fix, feat, build, chore, ci, docs, style, refactor, perf, test.
    
    - On the same line include a short description of **WHY** the changes are done after the commit message.
    - Don't start it with "This commit", just describe the changes.
    - Don't add any descriptions to the commit, only commit message.
    
    
    At the beginning of the list of changes, add an overview of the changes in the following format:
    
    - Look at the overall archtecture and changes and describe the value of this release in a single paragraph.
    - Do not add a title.
    
    Aditional information:
    
    - Use the present tense.
    - Lines must not be longer than 74 characters.
    - Use en-gb for the commit message.
"@

$prompt2 = @"
    You are an expert in software development and business communication. You are given the JSON output of a git log or git diff command, capturing changes made between the last software release and the current one. Your task is to create a clear, detailed, and business-friendly summary of these changes.

Instructions:

1. Group by Intent: Organize the changes into categories based on the intent of the changes. Possible categories include  (title, emoji, description):
    - New Features, ✨, Describe any new functionalities added.
    - Enhancements, 💡,Outline improvements to existing features.
    - Bug Fixes, 🐛,Summarize issues resolved.
    - Performance Improvements, ⚡️,Note optimizations made for better performance.
    - Refactoring, ♻️,Explain changes aimed at code structure improvement without altering functionality.
    - Security Updates, 🔒️,Highlight security-related changes.
    - Documentation Updates, 📝, Mention improvements or additions to documentation.
    - Dependency Updates, 📌,Specify updates to libraries, packages, or tools.
    - Other, 🧱,Capture any other changes that don’t fit the above categories.
2. Link to Pull Requests: Where applicable, reference the relevant Pull Requests (PRs) by number or link, providing context on the reason for the changes.
3. Provide Context: For each category, offer a brief explanation of why these changes were necessary or beneficial from a business perspective.
4. Output Structure:
    - Overview: Begin with a high-level summary of the release.
    - Detailed Changes: Break down the changes by the categories above, including a description and the impact of each change.
    - Conclusion: Finish with a conclusion that provides a brief outlook on the impact these changes are expected to have on the overall project or business goals.
5. Tone: The summary should be written in a professional tone that is accessible to non-technical stakeholders but detailed enough for technical team members.
6. Aditional information:
    - Use the present tense.
    - Use en-gb for the commit message.
    - Use https://github.com/nkdAgility/azure-devops-migration-tools as the github repository
"@
    
    # Set the API endpoint and API key
    $apiUrl = "https://api.openai.com/v1/chat/completions"
    
    # Convert $resultItems to a single string
    #$resultItemsString = $resultItems -join "`n"
    
    # Prepare the full prompt with the git diff results appended
    $fullPrompt = $prompt2 + "`n`nUse the folowing json:`n`n" + $result
    
    Write-Host "-----------------------------------------"
    Write-Host "Prompt:"
    Write-Host $fullPrompt
    
    Write-Host "-----------------------------------------"
    # Create the body for the API request
    Write-Host "Create the body for the API request..."
    $body = @{
        "model"       = "gpt-4-turbo"
        "messages"    = @(
            @{
                "role"    = "system"
                "content" = "You are an expert assistant that generates high-quality, structured content based on Git diffs using the GitMoji specification. You follow UK English conventions and keep lines under 74 characters."
            },
            @{
                "role"    = "user"
                "content" = $fullPrompt
            }
        )
        "temperature" = 0
        "max_tokens"  = 2048
    } | ConvertTo-Json
    Write-Host "Body:"
    Write-Host $body


    Write-Host "-----------------------------------------"
    Write-Host "Sending request to the ChatGPT API..."
    # Send the request to the ChatGPT API
    $response = Invoke-RestMethod -Uri $apiUrl -Method Post -Headers @{
        "Content-Type" = "application/json"
        "Authorization" = "Bearer $OPEN_AI_KEY"
    } -Body $body
    
    Write-Host "-----------------------------------------"
    Write-Host "Extracting Output.."
    # Extract and display the response content
    $result = $response.choices[0].message.content
    Write-Host "-----------------------------------------"
    Write-Host "result:"
    Write-Host $result
    Write-Host "-----------------------------------------"
    Write-Host "-----------------------------------------"
    Write-Host "Returning.."
    return $result
}