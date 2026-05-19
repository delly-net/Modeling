---
name: git-commit
description: Automatically stage changes and create a git commit with a generated message
---

# Git Auto Commit

This skill automatically detects changes, stages them, and creates a git commit with an appropriate commit message.

## Usage

```
/git-commit
```

## What it does

1. Runs `git status` and `git diff` to analyze changes
2. Runs `git log` to understand commit message style
3. Generates a concise commit message summarizing changes
4. Stages the changed files
5. Creates the commit with the message

## Output

Returns the commit hash and summary of what was committed.